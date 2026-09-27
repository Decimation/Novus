using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
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
using Novus.Memory.Allocation;
using Novus.Utilities;
using Novus.Numerics;
using Novus.Runtime.VM.EE;
using Novus.Runtime.VM.Tokens;
using Novus.Win32;

// ReSharper disable UnusedVariable
// ReSharper disable ConvertIfStatementToReturnStatement
// ReSharper disable InconsistentNaming
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable ClassCannotBeInstantiated
// ReSharper disable UnusedMember.Global
// ReSharper disable ArgumentsStyleLiteral

#pragma warning disable CS0618, CS1574, IDE0059
#pragma warning disable CA1416

namespace Novus.Runtime;

/// <summary>
///     CLR <see cref="object"/> properties and utilities.
/// </summary>
/// <seealso cref="Mem" />
/// <seealso cref="RuntimeHelpers" />
/// <seealso cref="System.Runtime.InteropServices.RuntimeEnvironment" />
/// <seealso cref="System.Runtime.InteropServices.RuntimeInformation" />
/// <see cref="RuntimeInformationExtensions"/>
public static unsafe class ObjectUtility
{

	// https://github.com/dotnet/runtime/blob/master/src/coreclr/src/vm/object.h

	static ObjectUtility()
	{
		Global.Clr.LoadImports(typeof(ObjectUtility));
	}

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

	/// <summary>
	/// Equals <see cref="LayoutEEClass.OFFSET_LAYOUTINFO"/>
	/// </summary>
	public static readonly int EEClassSize = sizeof(EEClass);

#endregion


#region

	/// <summary>
	/// Equals <see cref="Mem.SizeOf()"/> with <see cref="SizeOfOption.Data"/>
	/// </summary>
	/// <seealso cref="RuntimeHelpers.GetRawObjectDataSize"/>
	public static int GetRawObjDataSize(object o) => Func_GetRawObjDataSize(o);

	/// <summary>
	/// Equals <see cref="MetaType.ComponentSize"/>
	/// </summary>
	/// <see cref="RuntimeHelpers.GetElementSize"/>
	public static int GetElementSize(object o) => Func_GetElementSize(o);

	extension(object o)
	{

		public Pointer<ClrObject> AsClrObject()
			=> Unsafe.As<object, Pointer<ClrObject>>(ref o);

		public ObjectProxy AsObjectProxy()
			=> Unsafe.As<ObjectProxy>(o);

	}

#endregion


#region Metadata

	/// <summary>
	///     Reads <see cref="TypeHandle" /> as <see cref="Pointer{T}" /> to <see cref="MethodTable" /> from
	///     <paramref name="value" />
	/// </summary>
	public static Pointer<MethodTable> GetMethodTable<T>(in T value)
		=> Func_GetMethodTable(value);

	/// <summary>
	///     Returns a handle to the internal CLR metadata structure of <paramref name="member" />
	/// </summary>
	/// <param name="member">Reflection type</param>
	/// <returns>A pointer to the corresponding structure</returns>
	/// <exception cref="InvalidOperationException">The type of <see cref="MemberInfo" /> doesn't have a handle</exception>
	public static Pointer<byte> ResolveMetadataHandle(MMI member)
	{
		ArgumentNullException.ThrowIfNull(member);

		return member switch
		{
			Type t    => t.TypeHandle.Value,
			FI field  => field.FieldHandle.Value,
			MI method => method.MethodHandle.Value,
			_         => throw new InvalidOperationException()
		};
	}

	/// <summary>
	///     Resolves the <see cref="Type" /> from a <see cref="Pointer{T}" /> to the internal <see cref="MethodTable" />.
	/// </summary>
	/// <seealso cref="RuntimeTypeHandle.FromIntPtr"/>
	/// <remarks>Inverse of <see cref="GetMethodTable" /></remarks>
	public static Type GetType(Pointer<MethodTable> handle)
		=> Type.GetTypeFromHandle(RuntimeTypeHandle.FromIntPtr(handle.Address));

	/// <summary>
	///     Resolves the <see cref="Pointer{T}" /> to <see cref="MethodTable" /> from <paramref name="t" />.
	/// </summary>
	/// <remarks>Inverse of <see cref="GetType" /></remarks>
	public static Pointer<MethodTable> GetMethodTable(Type t)
		=> GetTypeHandle(t).AsMethodTable();

	public static Pointer<MethodTable> GetMethodTable<T>() => GetMethodTable(typeof(T));

	/// <summary>
	///     Resolves the <see cref="Pointer{T}" /> to <see cref="MethodTable" /> from <paramref name="t" />.
	/// </summary>
	/// <remarks>Inverse of <see cref="GetType" /></remarks>
	public static TypeHandle GetTypeHandle(Type t)
	{
		/*var handle = t.TypeHandle.Value;
		var value  = *(TypeHandle*) &handle;
		return value;*/
		return new TypeHandle((void*) RuntimeTypeHandle.ToIntPtr(t.TypeHandle));
	}

	public static TypeHandle GetTypeHandle<T>() => GetTypeHandle(typeof(T));

#endregion

#region Comparison & Properties

	/// <see cref="RuntimeHelpers.GetObjectValue"/>
	/// <seealso cref="RuntimeHelpers.Box"/>
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

	public static bool IsUnmanaged<T>([NN] T value) => value.GetType().IsUnmanaged;

	public static bool IsStruct<T>([NN] T value) => value.GetType().IsValueType;

	public static bool IsArray<T>([NN] T value) => value is Array;

	public static bool IsString<T>([NN] T value) => value is string;

	/// <summary>
	///     Determines whether <paramref name="value" /> is boxed.
	/// </summary>
	/// <returns><c>true</c> if boxed; <c>false</c> otherwise</returns>
	/// <remarks>Heuristic; not always correct</remarks>
	public static bool IsBoxed<T>([CBN] in T value)
	{
		// return !typeof(T).IsValueType && (value != null) && value.GetType().IsValueType;
		return (typeof(T).IsInterface || typeof(T) == typeof(object)) && value != null && IsStruct(value);
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
		int s   = Mem.SizeOf(t, SizeOfOption.BaseFields);

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
	public static bool IsEmptyOrDefault<T>([CBN] T value)
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

#region Instances

	/// <summary>
	///     Reads a value of type <paramref name="mt" /> in <paramref name="proc" /> at <paramref name="addr" /> using
	/// <see cref="Mem.ReadProcessMemory(System.Diagnostics.Process,Novus.Memory.Pointer{byte},nint)"/> (<see cref="Native.Kernel32.ReadProcessMemory"/>)
	/// </summary>
	[Obsolete]
	[CBN]
	[SupportedOSPlatform(RuntimeInformationExtensions.OS_WIN)]
	public static object ReadTypeFromProcessMemory(MetaType mt, Process proc, Pointer<byte> addr)
	{
		//todo

		bool valueType = mt.RuntimeType.IsValueType;
		int  size      = valueType ? mt.InstanceFieldsSize : mt.BaseSize;

		Debug.WriteLine($"{size} for {mt.Name}");

		//var i = Activator.CreateInstance(t);

		var    rg  = Mem.ReadProcessMemory(proc, addr, (nint) size);
		object val = null;

		var mh = rg.Pin();

		if (valueType) {
			val = Marshal.PtrToStructure((nint) mh.Pointer, mt.RuntimeType);
		}
		else {
			val = Unsafe.Read<object>(mh.Pointer);

		}

		return val;
	}

	/// <summary>
	/// Clones <paramref name="src"/> by copying its instance data into a new instance of type <typeparamref name="T"/>.
	/// </summary>
	public static T CloneInstanceData<T>(T src) where T : class
	{
		var cpyDest = Activator.CreateInstance<T>();

		Pointer<byte> ptrDataSrc  = Mem.AddressOfData(ref src);
		var           cbDataSrc   = Mem.SizeOf(src, SizeOfOption.Data);
		Pointer<byte> ptrDataDest = Mem.AddressOfData(ref cpyDest);

		// var  elemSize  = Mem.SizeOf<T>(src, SizeOfOption.Auto);
		// var cb = (uint) Mem.GetByteCount((nuint) cbDataSrc, (nuint) elemSize);

		Unsafe.CopyBlock(ptrDataDest, ptrDataSrc, (uint) cbDataSrc);

		return cpyDest;
	}

	/// <summary>
	/// Copies instance data of <paramref name="value"/> into to a <see cref="byte"/> array.
	/// Inverse of <see cref="ReadFromBytes{T}"/>
	/// </summary>
	public static byte[] GetBytes<T>(T value)
	{
		/*if (typeof(T).IsValueType) {
			var ptr = AddressOf(ref value);
			var cb  = SizeOf<T>();
			var rg  = new byte[cb];

			fixed (byte* p = rg) {
				ptr.Copy(p, cb);
			}

			return rg;
		}*/


		if (typeof(T).IsValueType) {
			var ptr  = Mem.AddressOfData(ref value);
			var size = Mem.SizeOf<T>();
			return ptr.ToArray(size);
		}

		Mem.TryGetAddressOfHeap(value, OffsetOptions.Header, out var ptr2);
		var cb2 = Mem.SizeOf(value, SizeOfOption.Heap);

		return ptr2.ToArray(cb2);
	}

	/// <summary>
	/// Reads a value fo type <typeparamref name="T"/> previously returned by <see cref="GetBytes{T}(T)"/>.
	/// </summary>
	/// <seealso cref="Streams.StreamExtensions.ReadAny{T}(Stream)"/>
	public static T ReadFromBytes<T>(byte[] rg)
	{
		/*Memory<byte> asMemory = rg.AsMemory();

		var p2 = asMemory.ToPointer(out var mh);

		if (!typeof(T).IsValueType) {
			p2 += ObjectUtility.ObjHeaderSize;
			return AddressOf(ref p2).Cast<T>().Value;
		}

		return p2.Cast<T>().Value;*/

		Memory<byte> asMemory = rg.AsMemory();
		using var    pin      = asMemory.Pin();

		var p2 = (byte*) pin.Pointer;

		if (!typeof(T).IsValueType) {
			p2 += ObjHeaderSize;
			return Unsafe.Read<T>(&p2);
		}

		return Unsafe.Read<T>(p2);
	}

	/// <summary>
	/// Reads a value of type <typeparamref name="T"/> previously returned by <see cref="GetBytes{T}(T)"/>.
	/// </summary>
	/// <seealso cref="Streams.StreamExtensions.ReadAny{T}(Stream)"/>
	public static object ReadFromBytes(byte[] rg)
		=> ReadFromBytes<object>(rg);

	/// <summary>
	/// Initializes an instance of type <typeparamref name="T"/> in the memory pointed by <paramref name="ptr"/>.
	/// The pre-allocated memory <paramref name="ptr"/> size must be at least &gt;= value returned by
	/// <see cref="SizeOfOption.BaseInstance"/> (<see cref="Mem.SizeOf{T}()"/>)
	/// <seealso cref="PreInitInstanceInitInstance"/>
	/// </summary>
	/// <typeparam name="T">Type to initialize</typeparam>
	/// <param name="ptr">Memory within which to initialize the instance</param>
	/// <param name="ctorArgs"></param>
	/// <returns>An instance of type <typeparamref name="T"/> initialized within <paramref name="ptr"/></returns>
	/// <remarks>This function is analogous to <em>placement <c>new</c></em> in C++</remarks>
	[MURV]
	public static ref T New<T>(Pointer<byte> ptr = default, object[] ctorArgs = null) where T : class
	{
		var     destRefPtr = AllocManager.Alloc((nuint) Mem.SizeOf<T>()).Cast<T>();
		ref var refDestPtr = ref destRefPtr.Reference;
		return ref New<T>(ref refDestPtr, ptr, ctorArgs);
	}

	public static ref T New<T>(ref T destRef, Pointer<byte> destInstPtr = default, object[] ctorArgs = null) where T : class
	{
		ctorArgs ??= [];

		var mt = typeof(T).AsMetaType();

		var     uninitObj = RuntimeHelpers.GetUninitializedObject(mt.RuntimeType, out var ptrObj);
		ref var uninitRef = ref ptrObj.Cast<T>().Reference;

		Pointer<T> uninitRefPtr = Unsafe.AsPointer(ref uninitRef);

		Pointer<Pointer<T>> destRefPtr = Unsafe.AsPointer(ref destRef);

		Trace.Assert(uninitRefPtr == destRefPtr);

		Unsafe.Write(destInstPtr, default(ClrObjHeader)); // Write header
		destInstPtr += ObjHeaderSize;                     // Offset by header
		Unsafe.Write(destInstPtr, GetTypeHandle<T>());    // Write type handle

		// *destRefPtrPtr = (nint*) destInstPtr.Address;
		Unsafe.Write(destRefPtr, destInstPtr.Address);

		ref var destRefPtrVal = ref Unsafe.AsRef<T>(destRefPtr);

		// Call constructor
		if (ctorArgs.Any()) {
			var ctorOk = ReflectionHelper.CallConstructor(destRefPtrVal, ctorArgs);
		}

		return ref destRefPtr.Reference.Reference;
	}

	/// <summary>
	/// Initializes an instance of type <typeparamref name="T"/> in the memory pointed by <paramref name="destInstPtr"/>.
	/// The pre-allocated memory <paramref name="destInstPtr"/> size must be at least &gt;= value returned by
	/// <see cref="SizeOfOption.BaseInstance"/> (<see cref="Mem.SizeOf{T}()"/>)
	/// <seealso cref="PreInitInstanceInitInstance"/>
	/// </summary>
	/// <param name="type">Type to initialize</param>
	/// <param name="destInstPtr">Memory within which to initialize the instance</param>
	/// <param name="ctorArgs"></param>
	/// <returns>An instance of type <typeparamref name="T"/> initialized within <paramref name="destInstPtr"/></returns>
	/// <remarks>This function is analogous to <em>placement <c>new</c></em> in C++</remarks>
	[MURV]
	public static object New(Type type, Pointer<byte> destInstPtr = default, object[] ctorArgs = null)
	{
		var ret = s_newFunc.InvokeGeneric(type, null, [destInstPtr, ctorArgs]);
		return ret;
	}

	extension(RuntimeHelpers)
	{

		public static object GetUninitializedObject(Type type, out Pointer<byte> mem)
		{
			var obj = RuntimeHelpers.GetUninitializedObject(type);

			mem = Mem.AddressOfHeap(obj);

			return obj;
		}

	}

	public static readonly MethodInfo s_newFunc = typeof(ObjectUtility).GetMethod(nameof(New), BindingFlags.Static | BindingFlags.Public,
	                                                                              [typeof(Pointer<byte>), typeof(Pointer<byte>)]);

#endregion

#region

	/// <summary>
	///     <see cref="IsPinnable" />
	/// </summary>
	[field: ImportManaged(typeof(Marshal), "IsPinnable")]
	private static delegate* managed<object, bool> Func_IsPinnable { get; }

	/// <summary>
	///     <see cref="GetType" />
	/// </summary>
	[field: ImportManaged(typeof(Type), "GetTypeFromHandle")]
	private static delegate* managed<nint, Type> Func_GetTypeFromHandle { get; }

	/// <summary>
	///     <see cref="GetMethodTable{T}(in T)" />
	/// </summary>
	[field: ImportManaged(typeof(RuntimeHelpers), "GetMethodTable")]
	private static delegate* managed<object, MethodTable*> Func_GetMethodTable { get; }

	[field: ImportManaged(typeof(RuntimeHelpers), "GetRawObjectDataSize")]
	private static delegate* managed<object, int> Func_GetRawObjDataSize { get; }

	[field: ImportManaged(typeof(RuntimeHelpers), "GetElementSize")]
	private static delegate* managed<object, int> Func_GetElementSize { get; }

#endregion

}