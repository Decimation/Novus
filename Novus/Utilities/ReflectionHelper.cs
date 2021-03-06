﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Novus.Memory;
using SimpleCore.Utilities;
using System.Reflection.PortableExecutable;
using Novus.Runtime;
using Novus.Runtime.Meta;
using SimpleCore.Diagnostics;

// ReSharper disable InconsistentNaming

// ReSharper disable UnusedTypeParameter
// ReSharper disable UnusedMember.Global
#pragma warning disable IDE1006

namespace Novus.Utilities
{
	public static class ReflectionHelper
	{
		/// <summary>
		///     <see cref="ALL_INSTANCE_FLAGS" /> and <see cref="BindingFlags.Static" />
		/// </summary>
		public const BindingFlags ALL_FLAGS = ALL_INSTANCE_FLAGS | BindingFlags.Static;

		/// <summary>
		///     <see cref="BindingFlags.Public" />, <see cref="BindingFlags.Instance" />,
		///     and <see cref="BindingFlags.NonPublic" />
		/// </summary>
		public const BindingFlags ALL_INSTANCE_FLAGS =
			BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;


		#region Members

		public static IEnumerable<MemberInfo> GetAllMembers(this Type t) => t.GetMembers(ALL_FLAGS);

		public static IEnumerable<MemberInfo> GetAnyMember(this Type t, string name) => t.GetMember(name, ALL_FLAGS);

		public static FieldInfo GetAnyField(this Type t, string name) => t.GetField(name, ALL_FLAGS);

		public static MethodInfo GetAnyMethod(this Type t, string name) => t.GetMethod(name, ALL_FLAGS);

		public static PropertyInfo GetAnyProperty(this Type t, string name) => t.GetProperty(name, ALL_FLAGS);


		public static FieldInfo GetResolvedField(this Type t, string fname)
		{
			var member = t.GetAnyMember(fname).First();


			var field = member.MemberType == MemberTypes.Property
				? t.GetBackingField(fname)
				: member as FieldInfo;

			return field;
		}

		public static IEnumerable<FieldInfo> GetAllBackingFields(this Type t)
		{
			var rg = t.GetRuntimeFields().Where(f => f.Name.Contains(BACKING_FIELD_NAME)).ToArray();


			return rg;
		}

		public static FieldInfo GetBackingField(this MemberInfo m)
		{
			var fv = m.DeclaringType.GetResolvedField(m.Name);

			return fv;
		}

		public static FieldInfo GetBackingField(this Type t, string name)
		{
			var fi = t.GetRuntimeFields()
			          .FirstOrDefault(a => Regex.IsMatch(a.Name, $@"\A<{name}>{BACKING_FIELD_NAME}\Z"));

			return fi;
		}


		private const string BACKING_FIELD_NAME = "k__BackingField";

		public static (TAttribute Attribute, MemberInfo Member)[] GetAnnotated<TAttribute>(this Type t)
			where TAttribute : Attribute
		{
			return (from member in t.GetAllMembers()
			        where Attribute.IsDefined(member, typeof(TAttribute))
			        select (member.GetCustomAttribute<TAttribute>(), member)).ToArray();
		}

		#endregion


		#region Properties

		public static Type[] GetAllSubclasses(this Type superType) =>
			GetAllWhere(superType, myType => myType.ExtendsType(superType));


		public static Type[] GetAllImplementations(this Type interfaceType) =>
			GetAllWhere(interfaceType, myType => myType.ImplementsInterface(interfaceType));

		public static bool ExtendsType(this Type myType, Type superType)
		{
			return myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(superType);
		}

		public static bool ImplementsInterface(this Type type, Type interfaceType) =>
			type.ImplementsInterface(interfaceType.Name);

		public static bool ImplementsInterface(this Type type, string interfaceName) =>
			type.GetInterface(interfaceName) != null;

		public static bool ImplementsGenericInterface(this Type type, Type genericType)
		{
			return type.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == genericType);
		}

		public static bool IsSigned(this Type t)
		{
			return t.IsNumeric() && (int) Type.GetTypeCode(t) % 2 == 1;
		}

		public static bool IsUnsigned(this Type t) => !t.IsSigned();

		public static bool IsNumeric(this Type t) => t.IsReal() || t.IsInteger();

		public static bool IsInteger(this Type t)
		{
			var c = Type.GetTypeCode(t);

			return c is <= TypeCode.UInt64 and >= TypeCode.SByte;
		}

		public static bool IsReal(this Type t)
		{
			var c = Type.GetTypeCode(t);

			var case1 = c is <= TypeCode.Decimal and >= TypeCode.Single;

			// Special case (?)
			var case2 = t == typeof(Half);

			return case1 || case2;
		}

		/// <summary>
		///     Dummy class for use with <see cref="IsUnmanaged" /> and <see cref="IsUnmanaged" />
		/// </summary>
		private sealed class U<T> where T : unmanaged { }

		/// <summary>
		///     Determines whether this type fits the <c>unmanaged</c> type constraint.
		/// </summary>
		public static bool IsUnmanaged(this Type t)
		{
			try {
				// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
				typeof(U<>).MakeGenericType(t);
				return true;
			}
			catch {
				return false;
			}
		}

		public static bool CanBePointerSurrogate(this Type t)
		{
			return t.IsValueType && (t.IsAnyPointer() || t.AsMetaType().NativeSize == Mem.Size);
		}

		public static bool IsAnyPointer(this Type t)
		{
			bool isPointer = t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Pointer<>);

			bool isIntPtr = t == typeof(IntPtr) || t == typeof(UIntPtr);

			return t.IsPointer || isPointer || isIntPtr;
		}

		public static bool IsEnumerableType(this Type type) => type.ImplementsInterface(nameof(IEnumerable));

		#endregion


		#region Invocation

		/// <summary>
		///     Executes a generic method
		/// </summary>
		/// <param name="method">Method to execute</param>
		/// <param name="args">Generic type parameters</param>
		/// <param name="value">Instance of type; <c>null</c> if the method is static</param>
		/// <param name="fnArgs">Method arguments</param>
		/// <returns>Return value of the method specified by <paramref name="method"/></returns>
		public static object CallGeneric(MethodInfo method, Type[] args, object value, params object[] fnArgs)
		{
			return method.MakeGenericMethod(args).Invoke(value, fnArgs);
		}

		public static object CallGeneric(MethodInfo method, Type arg, object value, params object[] fnArgs)
		{
			return CallGeneric(method, new[] {arg}, value, fnArgs);
		}

		/// <summary>
		///     Runs a constructor whose parameters match <paramref name="args" />
		/// </summary>
		/// <param name="value">Instance</param>
		/// <param name="args">Constructor arguments</param>
		/// <returns>
		///     <c>true</c> if a matching constructor was found and executed;
		///     <c>false</c> if a constructor couldn't be found
		/// </returns>
		public static bool CallConstructor<T>(T value, params object[] args)
		{
			/*
			 * https://stackoverflow.com/questions/142356/most-efficient-way-to-get-default-constructor-of-a-type
			 */

			var ctors    = value.GetType().GetConstructors();
			var argTypes = args.Select(x => x.GetType()).ToArray();

			foreach (var ctor in ctors) {
				var paramz = ctor.GetParameters();


				if (paramz.Length == args.Length) {
					if (paramz.Select(x => x.ParameterType).SequenceEqual(argTypes)) {
						ctor.Invoke(value, args);
						return true;
					}
				}
			}

			return false;
		}

		#endregion

		#region Assemblies

		private static Type[] GetAllWhere(Type t, Func<Type, bool> fn)
		{
			var rg = new List<Type>();

			var assembly = Assembly.GetAssembly(t);
			Guard.AssertNotNull(assembly);

			var asmTypes = assembly.GetTypes();

			var types = asmTypes.Where(fn);

			rg.AddRange(types);

			return rg.ToArray();
		}

		public static HashSet<AssemblyName> DumpDependencies()
		{
			var rg = new[]
			{
				//
				//typeof(Global).Assembly,
				//Assembly.GetExecutingAssembly(),
				//
				Assembly.GetCallingAssembly()
			};

			var asm = new HashSet<AssemblyName>();

			foreach (var assembly in rg) {

				var dependencies = GetUserDependencies(assembly);

				asm.UnionWith(dependencies);

			}

			return asm;
		}

		public static Assembly GetAssemblyByName(string name)
		{
			return AppDomain.CurrentDomain.GetAssemblies()
			                .SingleOrDefault(assembly => assembly.GetName().FullName.Contains(name));
		}

		public static IEnumerable<AssemblyName> GetUserDependencies(Assembly asm)
		{
			const string SYSTEM = "System";

			return asm.GetReferencedAssemblies().Where(a => a.Name != null && !a.Name.Contains(SYSTEM));
		}

		#endregion

		public static T Consolidate<T>(T current, IList<T> values)
		{
			var fields = typeof(T).GetRuntimeFields().Where(f => !f.IsStatic);

			foreach (var field in fields) {

				foreach (var value in values) {
					object fieldVal        = field.GetValue(value);
					object currentFieldVal = field.GetValue(current);


					/*
					 * (fieldVal != null || (fieldVal is string str && !string.IsNullOrWhiteSpace(str))) &&
					    (currentFieldVal == null || currentFieldVal is string str2 && string.IsNullOrWhiteSpace(str2))
					 */

					if (fieldVal != null && currentFieldVal == null) {
						field.SetValue(current, fieldVal);
					}
				}

			}

			return current;
		}
	}

	public static class ReflectionOperatorHelpers
	{
		
		public static MemberInfo memberof<T>(Expression<Func<T>> expression)
		{
			var body = (MemberExpression) expression.Body;
			return body.Member;
		}

		public static FieldInfo fieldof<T>(Expression<Func<T>> expression)
		{
			return (FieldInfo) memberof(expression);
		}

		public static PropertyInfo propertyof<T>(Expression<Func<T>> expression)
		{
			return (PropertyInfo) memberof(expression);
		}

		public static MethodInfo methodof<T>(Expression<Func<T>> expression)
		{
			var body = (MethodCallExpression) expression.Body;
			return body.Method;
		}

		public static MethodInfo methodof(Expression<Action> expression)
		{
			var body = (MethodCallExpression) expression.Body;
			return body.Method;
		}
	}
}