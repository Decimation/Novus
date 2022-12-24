#pragma warning disable IDE1006
// ReSharper disable RedundantUsingDirective.Global
global using MI = System.Reflection.MethodInfo;
global using PI = System.Reflection.PropertyInfo;
global using MMI = System.Reflection.MemberInfo;
global using FI = System.Reflection.FieldInfo;
global using static Novus.Utilities.ReflectionOperatorHelpers;
global using RH = Novus.Utilities.ReflectionHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using Novus.Memory;
using Kantan.Utilities;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Threading;
using JetBrains.Annotations;
using Novus.Runtime;
using Novus.Runtime.Meta;
using Kantan.Diagnostics;

// ReSharper disable InconsistentNaming

// ReSharper disable UnusedTypeParameter
// ReSharper disable UnusedMember.Global

namespace Novus.Utilities;

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

	public static IEnumerable<FI> GetAllFields(this Type t) => t.GetFields(ALL_FLAGS);

	public static IEnumerable<MI> GetAllMethods(this Type t) => t.GetMethods(ALL_FLAGS);

	public static IEnumerable<MMI> GetAllMembers(this Type t) => t.GetMembers(ALL_FLAGS);

	public static IEnumerable<MMI> GetAnyMember(this Type t, string name) => t.GetMember(name, ALL_FLAGS);

	public static FI GetAnyField(this Type t, string name) => t.GetField(name, ALL_FLAGS);

	public static MI GetAnyMethod(this Type t, string name) => t.GetMethod(name, ALL_FLAGS);

	public static MI GetAnyMethod(this Type t, string name, Type[] a) => t.GetMethod(name, ALL_FLAGS, a);

	public static PI GetAnyProperty(this Type t, string name) => t.GetProperty(name, ALL_FLAGS);

	/// <summary>
	/// Resolves the internal field from the member with name <paramref name="fname"/>.
	/// </summary>
	/// <remarks>Returns the backing field if <paramref name="fname"/> is a property; otherwise returns the normal field</remarks>
	public static FI GetAnyResolvedField(this Type t, string fname)
	{
		var member = t.GetAnyMember(fname).FirstOrDefault();

		if (member is PropertyInfo { MemberType: MemberTypes.Property } prop) {
			return prop.GetBackingField();
		}
		else {
			return member as FI;
		}
		
	}

	public static FI GetResolvedField(this MMI member)
	{
		var field = member.MemberType == MemberTypes.Property
			            ? member.DeclaringType.GetBackingField(member.Name)
			            : member as FI;

		return field;
	}

	public static (TAttribute Attribute, MMI Member)[] GetAnnotated<TAttribute>(this Type t)
		where TAttribute : Attribute
	{
		return (from member in t.GetAllMembers()
		        where Attribute.IsDefined(member, typeof(TAttribute))
		        select (member.GetCustomAttribute<TAttribute>(), member)).ToArray();
	}

	[return: MaybeNull]
	public static ConstructorInfo GetConstructor(this Type type, params object[] args)
	{
		if (args.Length == 0) {
			var ctor = type.GetConstructor(Type.EmptyTypes);

			return ctor;
		}

		var ctors    = type.GetConstructors();
		var argTypes = args.Select(x => x.GetType()).ToArray();

		foreach (var ctor in ctors) {
			var paramz = ctor.GetParameters();

			if (paramz.Length == args.Length) {
				if (paramz.Select(x => x.ParameterType).SequenceEqual(argTypes)) {
					return ctor;
				}
			}
		}

		return null;
	}

	/*public static T GetStaticValue<T>(this PI p)
	{
		return (T) p.GetValue(null);
	}

	public static T GetStaticValue<T>(this FI p)
	{
		return (T) p.GetValue(null);
	}*/

	#endregion

	#region Special

	public static IEnumerable<FI> GetAllBackingFields(this Type t)
	{
		var rg = t.GetRuntimeFields()
		          .Where(f => f.Name.Contains(SN_BACKING_FIELD))
		          .ToArray();
		return rg;
	}

	[CBN]
	public static FI GetBackingField(this PI pi)
	{
		/*
		 * https://stackoverflow.com/questions/8817070/is-it-possible-to-access-backing-fields-behind-auto-implemented-properties
		 */

		if (!pi.CanRead || !pi.GetGetMethod(nonPublic: true)
		                      .IsDefined(typeof(CompilerGeneratedAttribute), inherit: true)) {
			return null;
		}

		var backingField = pi.DeclaringType.GetField($"<{pi.Name}>{SN_BACKING_FIELD}", ALL_FLAGS);

		if (backingField == null || !backingField.IsDefined(typeof(CompilerGeneratedAttribute), inherit: true)) {
			return null;
		}

		return backingField;
	}

	public static FI GetBackingField(this Type t, string name)
	{
		var fi = t.GetRuntimeFields()
		          .FirstOrDefault(a => Regex.IsMatch(a.Name, $@"\A<{name}>{SN_BACKING_FIELD}\Z"));

		return fi;
	}

	private const string SN_BACKING_FIELD  = "k__BackingField";
	private const string SN_ANONYMOUS_TYPE = "<>f__AnonymousType";
	private const string SN_CLONE          = "<Clone>$";
	private const string SN_FIXED_BUFFER   = "e__FixedBuffer";

	public static bool IsExtensionMethod(this MI m) => m.IsDefined(typeof(ExtensionAttribute));

	public static bool IsRecord(this Type t) => t.GetMethods().Any(m => m.Name == SN_CLONE);

	public static bool IsFixedBuffer(this FI field)
		=> Regex.IsMatch(field.FieldType.Name, $@"\A<{field.Name}>{SN_FIXED_BUFFER}\Z");

	public static bool IsAnonymous(this Type type)
	{
		/*
		   |       Method |      Mean |    Error |   StdDev |
		   |------------- |----------:|---------:|---------:|
		   | IsAnonymous1 | 640.90 ns | 6.119 ns | 5.724 ns |
		   | IsAnonymous2 |  20.36 ns | 0.125 ns | 0.117 ns |
		 */

		return type.Name.Contains(SN_ANONYMOUS_TYPE);

		// HACK: The only way to detect anonymous types right now.
		/*return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
		       && type.IsGenericType && type.Name.Contains("AnonymousType")
		       && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
		       && type.Attributes.HasFlag(TypeAttributes.NotPublic);*/
	}

	#endregion

	#region Properties

	public static bool ExtendsType(this Type myType, Type superType)
	{
		return myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(superType);
	}

	public static bool ImplementsInterface(this Type type, Type interfaceType)
		=> type.ImplementsInterface(interfaceType.Name);

	public static bool ImplementsInterface(this Type type, string interfaceName)
		=> type.GetInterface(interfaceName) != null;

	public static bool ImplementsGenericInterface(this Type type, Type genericType)
	{
		return type.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == genericType);
	}

	public static bool IsSigned(this Type t)
	{
		return t.IsInteger() && (int) Type.GetTypeCode(t) % 2 == 1;
	}

	public static bool IsUnsigned(this Type t) => !t.IsSigned();

	public static bool IsNumeric(this Type t) => t.IsReal() || t.IsInteger();

	public static bool IsInteger(this Type t)
	{
		var c = Type.GetTypeCode(t);

		var b  = c is <= TypeCode.UInt64 and >= TypeCode.SByte;
		var b2 = t == typeof(BigInteger);
		var b3 = t == typeof(nint) || t == typeof(nuint);

		return b || b2 || b3;
	}

	public static bool IsReal(this Type t)
	{
		var c = Type.GetTypeCode(t);

		var case1 = c is <= TypeCode.Decimal and >= TypeCode.Single;

		// Special case
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

		bool isIntPtr = t == typeof(nint) || t == typeof(nuint);

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
	public static object CallGeneric(this MI method, Type[] args, object value, params object[] fnArgs)
	{
		return method.MakeGenericMethod(args).Invoke(value, fnArgs);
	}

	public static object CallGeneric(this MI method, Type arg, object value, params object[] fnArgs)
	{
		return CallGeneric(method, new[] { arg }, value, fnArgs);
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

		var ct = value.GetType().GetConstructor(args);

		if (ct is { }) {
			ct.Invoke(value, args);
			return true;
		}

		return false;
	}

	#endregion

	#region Assemblies

	public static Type[] GetAllInAssembly(this Type t1, TypeProperties p)
	{
		Func<Type, bool> fn = p switch
		{
			TypeProperties.Subclass  => t => t.ExtendsType(t1),
			TypeProperties.Interface => t => t.ImplementsInterface(t1),

			_ => throw new ArgumentOutOfRangeException(nameof(p), p, null)
		};

		return GetAllInAssembly(t1, fn);
	}

	public static Type[] GetAllInAssembly(this Type t, Func<Type, bool> fn)
	{
		var assembly = Assembly.GetAssembly(t);

		Require.NotNull(assembly);

		var asmTypes = assembly.GetTypes();

		var types = asmTypes.Where(fn);

		return types.ToArray();
	}

	public static IEnumerable<T> CreateAllInAssembly<T>(TypeProperties p)
	{
		return typeof(T).CreateAllInAssembly(p).Cast<T>();
	}

	public static IEnumerable<object> CreateAllInAssembly(this Type type, TypeProperties p)
	{
		return type.GetAllInAssembly(p)
		           .Select(Activator.CreateInstance)
		           .Cast<object>();
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

	public static IEnumerable<AssemblyName> GetUserDependencies(this Assembly asm)
	{
		const string SYSTEM = "System";

		return asm.GetReferencedAssemblies().Where(a => a.Name != null && !a.Name.Contains(SYSTEM));
	}

	#endregion

	#region Field ID

	//todo
	[AttributeUsage(
		AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Class | AttributeTargets.Property)]
	[MeansImplicitUse]
	public class FieldIdAttribute : Attribute { }

	public static FieldInfo[] GetFieldsById(Type t, Assembly[] asmName)
	{
		//TODO

		var f = t.GetRuntimeFields().Where(f =>
		{
			var asm  = f.FieldType.Assembly;
			var name = asm.GetName().Name;

			var contains = name != null && asmName.Any(a => a.GetName().Name.Contains(name));

			var b = f.GetCustomAttribute<FieldIdAttribute>() is { } fa;

			if (!contains) {
				return false;
			}

			return contains && !b;
		});

		return f.ToArray();
	}

	#endregion

	public static T Clone<T>(this T t) where T : class
	{
		var type = t.GetType();

		var f  = type.GetAnyMethod("MemberwiseClone");
		var r  = f.Invoke(t, Array.Empty<object>());
		var t2 = Unsafe.As<T>(r);
		return t2;
	}

	/*public static T Consolidate<T>(T current, IList<object> values)
	{
		var fields = typeof(T).GetRuntimeFields()
		                      .Where(f => !f.IsStatic);

		foreach (var field in fields) {

			foreach (var value in values) {
				object fieldVal        = field.GetValue(value);
				object currentFieldVal = field.GetValue(current);

				/*
				 * (fieldVal != null || (fieldVal is string str && !string.IsNullOrWhiteSpace(str))) &&
				    (currentFieldVal == null || currentFieldVal is string str2 && string.IsNullOrWhiteSpace(str2))
				 #1#

				if (fieldVal != null && currentFieldVal == null) {
					field.SetValue(current, fieldVal);
				}
			}

		}

		return current;
	}

	public static void Assign<T, T2>(Type t, string name, T val, T2 inst = default)
	{
		var m = t.GetAnyResolvedField(name);
		m.SetValue(inst, val);
	}

	public static void Assign<T>(Type t, string name, T val, object inst = null)
	{
		Assign<T, object>(t, name, val, inst);
	}*/

	/*public static void SetValue<T>(object target, T input, Expression<Func<T>> outExpr)
	{
		var x = GetAccessor<T>(outExpr);
		x.Set(input);
	}*/

	/// <summary>
	/// <a href="https://stackoverflow.com/questions/1402803/passing-properties-by-reference-in-c-sharp">See</a>
	/// </summary>
	public static (Action<T> Set, Func<T> Get) GetAccessor<T>(Expression<Func<T>> expr)
	{
		var memberExpression   = (MemberExpression) expr.Body;
		var instanceExpression = memberExpression.Expression;
		var parameter          = Expression.Parameter(typeof(T));

		Action<T> setter = null;
		Func<T>   getter = null;

		switch (memberExpression.Member) {
			case PI pi:
				setter = Expression.Lambda<Action<T>>(
					Expression.Call(instanceExpression, pi.GetSetMethod(), parameter),
					parameter).Compile();

				getter = Expression.Lambda<Func<T>>(Expression.Call(instanceExpression, pi.GetGetMethod()))
				                   .Compile();
				break;
			case FI fi:
				setter = Expression.Lambda<Action<T>>(Expression.Assign(memberExpression, parameter), parameter)
				                   .Compile();

				getter = Expression.Lambda<Func<T>>(
					                   Expression.Field(instanceExpression, fi))
				                   .Compile();
				break;
		}

		return (setter, getter);
	}

	public delegate bool IsNullObject(Type t, object o);

	public static (FieldInfo Field, bool IsNull)[] GetNullMembers(this object value, IsNullObject fn = null,
	                                                              BindingFlags flags = ALL_INSTANCE_FLAGS)
	{
		var fields = value.GetType().GetFields(flags);

		fn ??= (_, o) => RuntimeProperties.IsNull(o);

		var rg = new List<(FI Field, bool IsNull)>();

		foreach (var info in fields) {

			(FI Field, bool IsNull) x = new();

			var v = info.GetValue(value);

			x.IsNull = fn(info.FieldType, v);
			x.Field  = info;

			rg.Add(x);
		}

		return rg.ToArray();
	}
}

[Flags]
public enum TypeProperties
{
	Subclass,
	Interface
}

public static class ReflectionOperatorHelpers
{
	public static MMI memberof<T>(Expression<Func<T>> expression)
	{
		if (expression.Body is ConstantExpression) {
			return null;
		}

		var body = (MemberExpression) expression.Body;
		return body.Member;
	}

	public static ConstantExpression constof<T>(Expression<Func<T>> expression)
	{
		var body = (ConstantExpression) expression.Body;
		return body;
	}

	public static MemberInfo memberof2<T>(Expression<Func<T>> expression)
	{
		var mi = memberof(expression);

		return mi switch
		{
			FieldInfo f    => f,
			PropertyInfo p => p,

			_ => mi
		};
	}

	public static FI fieldof<T>(Expression<Func<T>> expression) => (FI) memberof(expression);

	public static PI propertyof<T>(Expression<Func<T>> expression) => (PI) memberof(expression);

	public static MI methodof<T>(Expression<Func<T>> expression)
	{
		var body = (MethodCallExpression) expression.Body;
		return body.Method;
	}

	public static MI methodof(Expression<Action> expression)
	{
		var body = (MethodCallExpression) expression.Body;
		return body.Method;
	}
}