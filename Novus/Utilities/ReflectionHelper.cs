using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Novus.Memory;
// ReSharper disable InconsistentNaming

// ReSharper disable UnusedTypeParameter
// ReSharper disable UnusedMember.Global
#pragma warning disable	IDE1006

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


		public static IEnumerable<MemberInfo> GetAllMembers(this Type t) => t.GetMembers(ALL_FLAGS);

		public static IEnumerable<MemberInfo> GetAnyMember(this Type t, string name) => t.GetMember(name, ALL_FLAGS);

		public static IEnumerable<FieldInfo> GetAllFields(this Type t) => t.GetFields(ALL_FLAGS);

		public static FieldInfo GetAnyField(this Type t, string name) => t.GetField(name, ALL_FLAGS);

		public static IEnumerable<MethodInfo> GetAllMethods(this Type t) => t.GetMethods(ALL_FLAGS);

		public static MethodInfo GetAnyMethod(this Type t, string name) => t.GetMethod(name, ALL_FLAGS);


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

		private const string BACKING_FIELD = "k__BackingField";

		public static IEnumerable<FieldInfo> GetAllBackingFields(this Type t)
		{
			var rg = t.GetRuntimeFields().Where(f => f.Name.Contains(BACKING_FIELD)).ToArray();


			return rg;
		}

		public static FieldInfo GetBackingField(this Type t, string name)
		{
			var fi = t.GetRuntimeFields()
				.FirstOrDefault(a => Regex.IsMatch(a.Name, $"\\A<{name}>{BACKING_FIELD}\\Z"));

			return fi;
		}


		internal static (TAttr Attribute, MemberInfo Member)[] GetAnnotated<TAttr>(this Type t) where TAttr : Attribute
		{
			return (from member in t.GetAllMembers()
				where Attribute.IsDefined(member, typeof(TAttr))
				select (member.GetCustomAttribute<TAttr>(),member)).ToArray();
		}

		public static bool ImplementsInterface(this Type type, string interfaceName) =>
			type.GetInterface(interfaceName) != null;

		public static bool ImplementsGenericInterface(this Type type, Type genericType)
		{
			bool IsMatch(Type t)
			{
				return t.IsGenericType && t.GetGenericTypeDefinition() == genericType;
			}

			return type.GetInterfaces().Any(IsMatch);
		}

		public static bool IsNumeric(this Type t) => t.IsReal() || t.IsInteger();

		public static bool IsInteger(this Type t)
		{
			int c = (int) Type.GetTypeCode(t);

			const int INT_MIN = (int) TypeCode.SByte;
			const int INT_MAX = (int) TypeCode.UInt64;

			return c <= INT_MAX && c >= INT_MIN;
		}

		public static bool IsReal(this Type t)
		{
			int c = (int) Type.GetTypeCode(t);

			const int REAL_MIN = (int) TypeCode.Single;
			const int REAL_MAX = (int) TypeCode.Decimal;

			var case1 = c <= REAL_MAX && c >= REAL_MIN;

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

		public static bool IsAnyPointer(this Type t)
		{
			bool isPointer = t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Pointer<>);

			bool isIntPtr = t == typeof(IntPtr) || t == typeof(UIntPtr);

			return t.IsPointer || isPointer || isIntPtr;
		}

		public static bool IsEnumerableType(this Type type) => type.ImplementsInterface(nameof(IEnumerable));

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


		public static FieldInfo fieldof<T>(Expression<Func<T>> expression)
		{
			var body = (MemberExpression) expression.Body;
			return (FieldInfo) body.Member;
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