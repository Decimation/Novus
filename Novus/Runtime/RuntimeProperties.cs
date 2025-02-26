using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using JetBrains.Annotations;
using Novus.Imports;
using Novus.Memory;
using Novus.Runtime.Meta;
using Novus.Runtime.VM;
using Kantan.Diagnostics;
using Novus.Imports.Attributes;
using Novus.Utilities;
using Novus.Numerics;
using Novus.Runtime.VM.Tokens;

// ReSharper disable UnusedVariable

// ReSharper disable ConvertIfStatementToReturnStatement
// ReSharper disable InconsistentNaming
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable ClassCannotBeInstantiated
// ReSharper disable UnusedMember.Global

// ReSharper disable ArgumentsStyleLiteral

#pragma warning disable CS0618, CS1574, IDE0059
namespace Novus.Runtime;

/// <summary>
///     Runtime properties and utilities.
/// </summary>
/// <seealso cref="Mem" />
/// <seealso cref="RuntimeHelpers" />
/// <seealso cref="System.Runtime.InteropServices.RuntimeEnvironment" />
/// <seealso cref="RuntimeInformation" />
/// <see cref="RuntimeEnvironment"/>
public static unsafe class RuntimeProperties
{

	// https://github.com/dotnet/runtime/blob/master/src/coreclr/src/vm/object.h

	static RuntimeProperties()
	{
		Global.Clr.LoadImports(typeof(RuntimeProperties));

	}

	/// <summary>
	///     <see cref="IsPinnable" />
	/// </summary>
	[field: ImportManaged(typeof(Marshal), "IsPinnable")]
	private static delegate* managed<object, bool> Func_IsPinnable { get; }

	[field: ImportManaged(typeof(RuntimeTypeHandle), "GetCorElementType")]
	private static delegate* managed<Type, CorElementType> Func_GetCorType { get; }

	/// <summary>
	///     <see cref="ResolveType" />
	/// </summary>
	[field: ImportManaged(typeof(Type), "GetTypeFromHandleUnsafe")]
	private static delegate* managed<nint, Type> Func_GetTypeFromHandle { get; }

	/// <summary>
	///     <see cref="GetMethodTable{T}" />
	/// </summary>
	[field: ImportManaged(typeof(RuntimeHelpers), "GetMethodTable")]
	private static delegate* managed<object, MethodTable*> Func_GetMethodTable { get; }

	[field: ImportManaged(typeof(RuntimeHelpers), "GetRawObjectDataSize")]
	private static delegate* managed<object, int> Func_GetRawObjDataSize { get; }

	[field: ImportManaged(typeof(RuntimeHelpers), "GetElementSize")]
	private static delegate* managed<object, int> Func_GetElementSize { get; }


	/// <summary>
	/// Equals <see cref="Mem.SizeOf()"/> with <see cref="SizeOfOptions.Data"/>
	/// </summary>
	/// <see cref="RuntimeHelpers.GetRawObjectDataSize"/>
	public static int GetRawObjDataSize(object o)
		=> Func_GetRawObjDataSize(o);

	/// <summary>
	/// Equals <see cref="MetaType.ComponentSize"/>
	/// </summary>
	/// <see cref="RuntimeHelpers.GetElementSize"/>
	public static int GetElementSize(object o)
		=> Func_GetElementSize(o);


	#region Metadata

	/// <summary>
	///     Reads <see cref="TypeHandle" /> as <see cref="Pointer{T}" /> to <see cref="MethodTable" /> from
	///     <paramref name="value" />
	/// </summary>
	public static Pointer<MethodTable> GetMethodTable<T>(in T value)
	{
		/*var type = value.GetType();
		return ResolveMethodTable(type);*/
		return Func_GetMethodTable(value);
	}

	/// <summary>
	///     Returns a handle to the internal CLR metadata structure of <paramref name="member" />
	/// </summary>
	/// <param name="member">Reflection type</param>
	/// <returns>A pointer to the corresponding structure</returns>
	/// <exception cref="InvalidOperationException">The type of <see cref="MemberInfo" /> doesn't have a handle</exception>
	public static Pointer<byte> ResolveMetadataHandle(MMI member)
	{
		Require.ArgumentNotNull(member, nameof(member));

		return member switch
		{
			Type t            => ResolveMethodTable(t).Cast(),
			FI field          => field.FieldHandle.Value,
			MethodInfo method => method.MethodHandle.Value,
			_                 => throw new InvalidOperationException()
		};
	}

	/// <summary>
	///     Resolves the <see cref="Type" /> from a <see cref="Pointer{T}" /> to the internal <see cref="MethodTable" />.
	/// </summary>
	/// <remarks>Inverse of <see cref="ResolveMethodTable" /></remarks>
	public static Type ResolveType(Pointer<MethodTable> handle)
		=> Func_GetTypeFromHandle(handle.Address);

	/// <summary>
	///     Resolves the <see cref="Pointer{T}" /> to <see cref="MethodTable" /> from <paramref name="t" />.
	/// </summary>
	/// <remarks>Inverse of <see cref="ResolveType" /></remarks>
	public static Pointer<MethodTable> ResolveMethodTable(Type t)
	{
		/*var handle = t.TypeHandle.Value;
		var value  = *(TypeHandle*) &handle;
		return value.MethodTable;*/

		var typeHandle = ResolveTypeHandle(t);

		return typeHandle.MethodTable;
	}

	/// <summary>
	///     Resolves the <see cref="Pointer{T}" /> to <see cref="MethodTable" /> from <paramref name="t" />.
	/// </summary>
	/// <remarks>Inverse of <see cref="ResolveType" /></remarks>
	public static TypeHandle ResolveTypeHandle(Type t)
	{
		var handle = t.TypeHandle.Value;
		var value  = *(TypeHandle*) &handle;
		return value;
	}

	public static Pointer<ClrObject> AsClrObject(object o)
	{
		return Mem.AddressOfHeap(o).Cast<ClrObject>();
	}

	#endregion

	#region Comparison & Properties

	/// <see cref="RuntimeHelpers.GetObjectValue"/>
	[CBN]
	public static object Box([CBN] object o)
		=> RuntimeHelpers.GetObjectValue(o);

	/// <summary>
	///     Determines whether <paramref name="value" /> is pinnable.
	/// </summary>
	/// <returns><c>true</c> if pinnable; <c>false</c> otherwise</returns>
	public static bool IsPinnable([CBN] object value)
		=> Func_IsPinnable(value);

	/// <summary>
	///     Determines whether <paramref name="obj" /> is blittable; that is, whether it has identical data representation in
	///     both managed and unmanaged memory.
	/// </summary>
	/// <returns><c>true</c> if blittable; <c>false</c> otherwise</returns>
	public static bool IsBlittable<T>(T obj)
		=> obj.GetMetaType().IsBlittable;

	public static bool IsNullable<T>(T obj)
	{
		//https://stackoverflow.com/questions/374651/how-to-check-if-an-object-is-nullable

		/*if (obj == null) {
			return true; // obvious
		}*/

		if (IsDefault(obj)) {
			return true;
		}

		var type = typeof(T);

		if (!type.IsValueType) {
			return true; // ref-type
		}

		if (Nullable.GetUnderlyingType(type) != null) {
			return true; // Nullable<T>
		}

		return false; // value-type
	}

	/// <summary>
	///     Determines whether the value of <paramref name="value" /> is <c>default</c>.
	/// </summary>
	public static bool IsDefault<T>([CBN] in T value)
		=> EqualityComparer<T>.Default.Equals(value, default);

	public static bool IsUnmanaged<T>([NN] T value)
		=> value.GetType().IsUnmanaged();

	public static bool IsStruct<T>([NN] T value)
		=> value.GetType().IsValueType;

	public static bool IsArray<T>([NN] T value)
		=> value is Array;

	public static bool IsString<T>([NN] T value)
		=> value is string;

	/// <summary>
	///     Determines whether <paramref name="value" /> is boxed.
	/// </summary>
	/// <returns><c>true</c> if boxed; <c>false</c> otherwise</returns>
	/// <remarks>Heuristic; not always correct</remarks>
	public static bool IsBoxed<T>([CBN] in T value)
	{
		// return !typeof(T).IsValueType && (value != null) && value.GetType().IsValueType;
		return (typeof(T).IsInterface || typeof(T) == typeof(object))
		       && value != null && IsStruct(value);
	}

	/// <summary>
	///     Determines whether the memory of <paramref name="t" /> is null; that is,
	///     all of its fields are <c>null</c>.
	/// </summary>
	public static bool IsNull<T>(T t)
	{
		if (!typeof(T).IsValueType && t == null) {
			return true;
		}

		var ptr = Mem.AddressOfData(ref t);
		int s   = Mem.SizeOf(t, SizeOfOptions.BaseFields);

		for (int i = 0; i < s; i++) {
			if (ptr[i] != 0) {
				return false;
			}
		}

		return true;
	}

	/// <summary>
	///     Heuristically determines whether <paramref name="value" /> is <em>empty</em>.
	///     This always returns <c>true</c> if <paramref name="value" /> is <c>null</c> or <c>default</c>.
	/// Uses predicates in <see cref="EmptyPredicates"/>.
	/// </summary>
	/// <remarks>
	///     <em>Empty</em> is defined as one of the following:
	/// <list type="bullet">
	/// <item><c>null</c></item>
	/// <item><c>default</c> (<see cref="IsDefault{T}"/>),</item>
	/// <item>non-unique,</item>
	/// <item>or unmodified</item>
	/// </list>
	/// </remarks>
	/// <example>
	///     If <paramref name="value" /> is a <see cref="string" />, this function returns <c>true</c> if the
	///     <see cref="string" /> is <c>null</c> or whitespace (<see cref="string.IsNullOrWhiteSpace" />).
	/// </example>
	/// <param name="value">Value to check for</param>
	/// <typeparam name="T">Type of <paramref name="value" /></typeparam>
	/// <returns>
	///     <c>true</c> if <paramref name="value" /> is <c>null</c> or <c>default</c>; or
	///     if <paramref name="value" /> is heuristically determined to be <em>empty</em>.
	/// </returns>
	public static bool IsEmpty<T>([CBN] T value)
	{
		/*if (IsDefault(value)) {
			return true;
		}*/

		//if (IsBoxed(value)) {
		//	return false;
		//}

		// As for strings, IsNullOrWhiteSpace should always be true when
		// IsNullOrEmpty is true

		bool test = value switch
		{
			string str    => String.IsNullOrWhiteSpace(str),
			IList list    => list.Count == 0,
			IEnumerable e => !e.Cast<object>().Any(),
			_             => IsDefault(value)
		};

		return test;

	}

	#endregion

	#region Constants

	/// <summary>
	///     Size of the length field and first character
	///     <list type="bullet">
	///         <item>
	///             <description>+ sizeof <see cref="char" />: First character</description>
	///         </item>
	///         <item>
	///             <description>+ sizeof <see cref="int" />: String length</description>
	///         </item>
	///     </list>
	/// </summary>
	public static readonly int StringOverhead = sizeof(char) + sizeof(int);

	/// <summary>
	///     Size of the length field and padding (x64)
	///     <list type="bullet">
	///         <item>
	///             <description>+ sizeof <see cref="int" />: Length field</description>
	///         </item>
	///         <item>
	///             <description>+ sizeof <see cref="int" />: Padding (x64)</description>
	///         </item>
	///     </list>
	/// </summary>
	public static readonly int ArrayOverhead = Mem.Size;

	/// <summary>
	///     Heap offset to the first field.
	///     <list type="bullet">
	///         <item>
	///             <description>+ <see cref="Mem.Size" /> for <see cref="TypeHandle" /></description>
	///         </item>
	///     </list>
	/// </summary>
	public static readonly int OffsetToData = Mem.Size;

	/// <summary>
	///     Heap offset to the first array element.
	///     <list type="bullet">
	///         <item>
	///             <description>+ <see cref="Mem.Size" /> for <see cref="TypeHandle" /></description>
	///         </item>
	///         <item>
	///             <description>+ sizeof (<see cref="uint" />) for length </description>
	///         </item>
	///         <item>
	///             <description>+ sizeof (<see cref="uint" />) for padding (x64 only)</description>
	///         </item>
	///     </list>
	/// </summary>
	public static readonly int OffsetToArrayData = OffsetToData + ArrayOverhead;

	/// <summary>
	///     Heap offset to the first string character.
	///     On 64 bit platforms, this should be 12 and on 32 bit 8.
	///     (<see cref="Mem.Size" /> + <see cref="int" />)
	/// </summary>
	public static readonly int OffsetToStringData = RuntimeHelpers.OffsetToStringData;

	public static readonly int ObjHeaderSize = sizeof(ClrObjHeader);

	/// <summary>
	///     Size of <see cref="TypeHandle" /> and <see cref="ClrObjHeader" />
	///     <list type="bullet">
	///         <item>
	///             <description>+ <see cref="ObjHeaderSize" />: <see cref="ClrObjHeader" /></description>
	///         </item>
	///         <item>
	///             <description>+ sizeof <see cref="TypeHandle" />: <see cref="TypeHandle" /></description>
	///         </item>
	///     </list>
	/// </summary>
	public static readonly int ObjectBaseSize = ObjHeaderSize + sizeof(TypeHandle);

	/// <summary>
	///     <para>Minimum GC object heap size</para>
	/// </summary>
	public static readonly int MinObjectSize = Mem.Size * 2 + ObjHeaderSize;

	#endregion

	public static CorElementType GetCorElementType(this Type t)
		=> Func_GetCorType(t);

}