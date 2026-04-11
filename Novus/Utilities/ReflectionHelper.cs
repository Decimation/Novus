#pragma warning disable IDE1006

// ReSharper disable RedundantUsingDirective.Global
global using MI = System.Reflection.MethodInfo;
global using PI = System.Reflection.PropertyInfo;
global using MMI = System.Reflection.MemberInfo;
global using FI = System.Reflection.FieldInfo;
global using static Novus.Utilities.ReflectionOperatorHelpers;
using RH = Novus.Utilities.ReflectionHelper;
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
using Novus.Numerics;

// ReSharper disable AnnotateNotNullParameter

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
	public const BindingFlags ALL_INSTANCE_FLAGS = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;

#region Members

	extension(Type t)
	{

		public (TAttribute Attribute, MMI Member)[] GetAnnotated<TAttribute>() where TAttribute : Attribute
		{
			return
			[
				..
				from member in t.GetAllMembers()
				where Attribute.IsDefined(member, typeof(TAttribute))
				select (member.GetCustomAttribute<TAttribute>(), member)
			];
		}

		[return: MN]
		public ConstructorInfo GetConstructor(params object[] args)
		{
			if (args.Length == 0) {
				var ctor = t.GetConstructor(Type.EmptyTypes);

				return ctor;
			}

			var ctors    = t.GetConstructors();
			var argTypes = args.Select(static x => x.GetType()).ToArray();

			foreach (var ctor in ctors) {
				var paramz = ctor.GetParameters();

				if (paramz.Length != args.Length) {
					continue;
				}

				if (paramz.Select(static x => x.ParameterType).SequenceEqual(argTypes)) {
					return ctor;
				}
			}

			return null;
		}

		public IEnumerable<FI> GetAllFields() => t.GetFields(ALL_FLAGS);

		public IEnumerable<MI> GetAllMethods() => t.GetMethods(ALL_FLAGS);

		public IEnumerable<MMI> GetAllMembers() => t.GetMembers(ALL_FLAGS);

		public IEnumerable<MMI> GetAnyMember(string name) => t.GetMember(name, ALL_FLAGS);

		public FI GetAnyField(string name) => t.GetField(name, ALL_FLAGS);

		public MI GetAnyMethod(string name) => t.GetMethod(name, ALL_FLAGS);

		public MI GetAnyMethod(string name, Type[] a) => t.GetMethod(name, ALL_FLAGS, a);

		public PI GetAnyProperty(string name) => t.GetProperty(name, ALL_FLAGS);

		/// <summary>
		/// Resolves the internal field from the member with name <paramref name="fname"/>.
		/// </summary>
		/// <remarks>Returns the backing field if <paramref name="fname"/> is a property; otherwise returns the normal field</remarks>
		public FI GetAnyResolvedField(string fname)
		{
			var infos  = t.GetAnyMember(fname);
			var member = infos.FirstOrDefault();

			switch (member) {
				case PI { MemberType: MemberTypes.Property } prop: return prop.GetBackingField();

				case FI fi: return fi;

				default: return null;
			}

		}

#region

		public bool IsSigned
		{
			get
			{
				var c = Type.GetTypeCode(t);
				var      b        = c.IsInteger;

				var bb = b && (int) c % 2 == 1;
				return /*t.IsInteger() && */bb || ExtraSInt.Contains(t) || t.IsReal;
			}
		}

		public bool IsUnsigned
		{
			get
			{
				var c = Type.GetTypeCode(t);
				var      b        = c.IsInteger;

				var bb = b && (int) c % 2 == 0;
				return /*t.IsInteger() && */bb || ExtraUInt.Contains(t);
			}
		}

		public bool IsNumeric => t.IsInteger || t.IsReal;


		public bool IsInteger
		{
			get
			{
				bool b = Type.GetTypeCode(t).IsInteger;

				return b || ExtraSInt.Contains(t) || ExtraUInt.Contains(t);
			}
		}

		public bool IsReal
		{
			get
			{
				var c     = Type.GetTypeCode(t);
				var case1 = c.IsReal;

				// Special case
				var case2 = t == typeof(Half);

				return case1 || case2;
			}
		}

#endregion

	}

	extension(TypeCode c)
	{

		private bool IsInteger => c is <= TypeCode.UInt64 and >= TypeCode.SByte;

		private bool IsReal => c is <= TypeCode.Decimal and >= TypeCode.Single;

	}


	private static readonly Type[] ExtraUInt = [typeof(UInt128), typeof(nuint)];
	private static readonly Type[] ExtraSInt = [typeof(Int128), typeof(nint), typeof(BigInteger)];

	public static FI GetResolvedField(this MMI member)
	{
		var field = member.MemberType == MemberTypes.Property
			            ? member.DeclaringType.GetBackingField(member.Name)
			            : member as FI;

		return field;
	}

	public static Dictionary<FI, bool> GetNullMembers(object value, Func<Type, object, bool> fn = null, BindingFlags flags = ALL_INSTANCE_FLAGS)
	{
		var fields = value.GetType().GetFields(flags);

		fn ??= static (_, o) => ObjectUtility.IsNull(o);

		var rg = new Dictionary<FI, bool>();

		foreach (var info in fields) {

			var v = info.GetValue(value);

			rg.Add(info, fn(info.FieldType, v));
		}

		return rg;
	}

#endregion

#region Special

	extension(Type t)
	{

		public IEnumerable<FI> GetAllBackingFields()
		{
			var rg = t.GetRuntimeFields()
			          .Where(f => f.Name.Contains(SN_BACKING_FIELD));
			return rg;
		}

		public bool IsAnonymous()
		{
			/*
		   |       Method |      Mean |    Error |   StdDev |
		   |------------- |----------:|---------:|---------:|
		   | IsAnonymous1 | 640.90 ns | 6.119 ns | 5.724 ns |
		   | IsAnonymous2 |  20.36 ns | 0.125 ns | 0.117 ns |
		 */

			return t.Name.Contains(SN_ANONYMOUS_TYPE);

			// HACK: The only way to detect anonymous types right now.
			/*return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
		       && type.IsGenericType && type.Name.Contains("AnonymousType")
		       && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
		       && type.Attributes.HasFlag(TypeAttributes.NotPublic);*/
		}

		public bool IsRecord()
			=> t.GetMethods().Any(m => m.Name == SN_CLONE);

		public FI GetBackingField(string name)
		{
			var fi = t.GetRuntimeFields()
			          .FirstOrDefault(a => Regex.IsMatch(a.Name, $@"\A<{name}>{SN_BACKING_FIELD}\Z"));

			return fi;
		}

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

	public static bool IsIndexed(this PI p) => p.GetIndexParameters().Length > 0;

	private const string SN_BACKING_FIELD  = "k__BackingField";
	private const string SN_ANONYMOUS_TYPE = "<>f__AnonymousType";
	private const string SN_CLONE          = "<Clone>$";
	private const string SN_FIXED_BUFFER   = "e__FixedBuffer";

	public static bool IsExtensionMethod(this MI m)
		=> m.IsDefined(typeof(ExtensionAttribute));

	public static bool IsFixedBuffer(this FI field)
		=> Regex.IsMatch(field.FieldType.Name, $@"\A<{field.Name}>{SN_FIXED_BUFFER}\Z");

	public static bool IsCalculated(this PI property)
	{
		return property.GetBackingField() == null && property.CanRead;
	}

#endregion

#region Type properties

	extension(Type t)
	{

		public bool ExtendsType(Type superType)
		{
			return t.IsClass && !t.IsAbstract && t.IsSubclassOf(superType);
		}

		public bool ImplementsInterface(Type interfaceType)
			=> t.ImplementsInterface(interfaceType.Name);

		public bool ImplementsInterface(string interfaceName)
			=> t.GetInterface(interfaceName) != null;

		public bool ImplementsGenericInterface(Type genericType)
		{
			return t.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == genericType);
		}

		public IEnumerable<Type> GetGenericImplementations()
		{
			var impl = t.Assembly.GetTypes()
			            .Where(static t1 => t1 is { IsAbstract: false, IsInterface: false })
			            .Where(t2 => t2.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == t));

			return impl;
		}

		/// <summary>
		///     Determines whether <paramref name="t"/> type fits the <c>unmanaged</c> type constraint.
		/// </summary>
		[DebuggerHidden]
		public bool IsUnmanaged
		{
			get
			{
				try {
					// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
					typeof(UnmanagedDummyType<>).MakeGenericType(t);
					return true;
				}
				catch {
					return false;
				}
			}
		}

		public bool CanBePointerSurrogate => t.IsValueType && (t.IsAnyPointer || t.AsMetaType().NativeSize == Mem.Size);

		public bool IsAnyPointer
		{
			get
			{
				bool isPointer = t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Pointer<>);

				bool isIntPtr = t == typeof(nint) || t == typeof(nuint);

				return t.IsPointer || isPointer || isIntPtr || t.IsUnmanagedFunctionPointer;
			}
		}

		public bool IsEnumerableType => t.ImplementsInterface(nameof(IEnumerable));

	}


	/// <summary>
	///     Dummy class for use with <see cref="IsUnmanaged" />
	/// </summary>
	private sealed class UnmanagedDummyType<T> where T : unmanaged { }

#endregion

#region Invocation

	/// <param name="method">Method to execute</param>
	extension(MI method)
	{

		/// <summary>
		///     Executes a generic method
		/// </summary>
		/// <param name="args">Generic type parameters</param>
		/// <param name="value">Instance of type; <c>null</c> if the method is static</param>
		/// <param name="fnArgs">Method arguments</param>
		/// <returns>Return value of the method specified by <paramref name="method"/></returns>
		public object CallGeneric(Type[] args, object value, params object[] fnArgs)
			=> method.MakeGenericMethod(args).Invoke(value, fnArgs);

		public object CallGeneric(Type arg, object value, params object[] fnArgs)
			=> method.CallGeneric([arg], value, fnArgs);

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

		var ct = value.GetType2().GetConstructor(args);

		if (ct is not null) {
			ct.Invoke(value, args);
			return true;
		}

		return false;
	}

#endregion

#region Assemblies

	extension(Type type)
	{

		public Type[] GetImplementations()
		{
			var types = type.Assembly.GetTypes()
			                .Where(p => type.IsAssignableFrom(p) && p.IsClass)
			                .ToArray();
			return types;
		}

		public IEnumerable<Type> GetAllInAssembly(InheritanceProperties p)
		{
			Func<Type, bool> fn = p switch
			{
				InheritanceProperties.Subclass  => t => t.ExtendsType(type),
				InheritanceProperties.Interface => t => t.ImplementsInterface(type),

				_ => throw new ArgumentOutOfRangeException(nameof(p), p, null)
			};

			return type.GetAllInAssembly(fn);
		}

		public IEnumerable<Type> GetAllInAssembly(Func<Type, bool> fn)
		{
			var assembly = Assembly.GetAssembly(type);

			Require.NotNull(assembly);

			var asmTypes = assembly.GetTypes();

			var types = asmTypes.Where(fn);

			return types;
		}

	}

	public static IEnumerable<T> CreateAllInAssembly<T>(InheritanceProperties p)
	{
		return typeof(T).GetAllInAssembly(p)
		                .Select(Activator.CreateInstance).Cast<T>();
	}

	public static HashSet<AssemblyName> DumpDependencies([CBN] this Assembly asm2)
	{
		asm2 ??= Assembly.GetCallingAssembly();

		var asm = new HashSet<AssemblyName>();

		var dependencies = asm2.GetNonSystemReferencedAssemblies();
		asm.UnionWith(dependencies);

		return asm;
	}

	public static IEnumerable<AssemblyName> GetNonSystemReferencedAssemblies(this Assembly asm)
	{
		const string SYSTEM = "System";

		return asm.GetReferencedAssemblies().Where(a => a.Name != null && !a.Name.Contains(SYSTEM));
	}

	public static Assembly FindAssemblyByName(string name)
	{
		return AppDomain.CurrentDomain.GetAssemblies()
		                .SingleOrDefault(assembly => assembly.GetName().FullName.Contains(name));
	}

#endregion

	public static T Clone<T>(this T t) where T : class
	{
		var type = t.GetType();

		var f  = type.GetAnyMethod(nameof(MemberwiseClone));
		var r  = f.Invoke(t, []);
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

	/*public static object GetDefaultFieldValue(this Type t)
	{
		//todo
		object o = default;

		if (t.IsFunctionPointer) {
			o = IntPtr.Zero;

		}

		return o;
	}*/

	public static Type GetType2<T>([CBN] this T t)
		=> t?.GetType() ?? typeof(T);

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

	/*public static void Deconstruct(this object o, Func<MMI, bool> m = null)
	{
		var mem  = o.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Default);
		var dict = new Dictionary<string, object>();

		foreach (var info in mem) {
			//todo
		}
	}*/

}

[Flags]
public enum InheritanceProperties
{

	/// <summary>
	/// <see cref="ReflectionHelper.ExtendsType"/>
	/// </summary>
	Subclass,

	/// <summary>
	/// <see cref="ReflectionHelper.ImplementsInterface(Type,string)"/>
	/// </summary>
	Interface

}