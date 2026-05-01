// ReSharper disable RedundantUsingDirective.Global

#pragma warning disable IDE0005, CS1574
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Versioning;
using JetBrains.Annotations;
using Kantan.Diagnostics;
using Kantan.Text;
using Microsoft.Extensions.Logging;
using Novus.Memory.Allocation;
using Novus.Numerics;
using Novus.OS;
using Novus.Runtime;
using Novus.Runtime.Meta;
using Novus.Runtime.VM;
using Novus.Utilities;
using Novus.Win32;
using Novus.Win32.Structures.Kernel32;
using Novus.Win32.Wrappers;

// ReSharper disable SuggestVarOrType_BuiltInTypes

// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

// ReSharper disable ClassCannotBeInstantiated

// ReSharper disable ConvertIfToOrExpression
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable SwitchStatementMissingSomeEnumCasesNoDefault
// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault
// ReSharper disable ConvertIfStatementToReturnStatement
// ReSharper disable UnusedVariable
// ReSharper disable UnusedMember.Global

#pragma warning disable IDE0059
#pragma warning disable IDE1006

#pragma warning disable CA1416 //todo

namespace Novus.Memory;

using ActionFunctor = Action<object, Action<object>>;

/// <summary>
///     Provides utilities for manipulating pointers, memory, and types.
/// </summary>
/// <seealso cref="Pointer"/>
/// <seealso cref="Mem" />
/// <seealso cref="PinManager"/>
/// <seealso cref="BitConverter" />
/// <seealso cref="BinaryPrimitives" />
/// <seealso cref="Convert" />
/// <seealso cref="CollectionsMarshal" />
/// <seealso cref="MemoryMarshal" />
/// <seealso cref="Marshal" />
/// <seealso cref="Span{T}" />
/// <seealso cref="Memory{T}" />
/// <seealso cref="Buffer" />
/// <seealso cref="AllocManager" />
/// <seealso cref="Unsafe" />
/// <seealso cref="RuntimeHelpers" />
/// <seealso cref="System.Runtime.InteropServices.RuntimeEnvironment" />
/// <seealso cref="System.Runtime.InteropServices.RuntimeInformation" />
/// <seealso cref="FormatterServices" />
/// <seealso cref="Activator" />
/// <seealso cref="GCHeap" />
/// <seealso cref="ObjectUtility" />
/// <seealso cref="Inspector" />
/// <seealso cref="Native" />
/// <seealso cref="PEReader" />
/// <seealso cref="GCHandle" />
/// <seealso cref="NativeMemory"/>
/// <seealso cref="NativeLibrary"/>
/// <seealso cref="System.Runtime.CompilerServices" />
/// <seealso cref="System.Runtime.CompilerServices.JitHelpers"/>
/// <seealso cref="RuntimeHelpers.CoreCLR.cs"/>
public static unsafe class Mem
{

#region

	/// <summary>
	///     Address size
	/// </summary>
	public static readonly int Size = IntPtr.Size;

	/// <summary>
	///     Represents a <c>null</c> <see cref="Pointer{T}" /> or <see cref="ReadOnlyPointer{T}" />
	/// </summary>
	public static readonly Pointer<byte> Nullptr = null;

	public static readonly bool Is64Bit = Environment.Is64BitProcess;

	private static readonly ILogger s_logger = Global.LoggerFactoryInt.CreateLogger(nameof(Mem));

#endregion

	/// <summary>
	///     Returns the offset of the field <paramref name="name" /> within the type <typeparamref name="T" />.
	/// </summary>
	/// <param name="name">Field name</param>
	public static int OffsetOf<T>(string name) => typeof(T).OffsetOf(name);

	/// <summary>
	///     Returns the offset of the field <paramref name="name" /> within the type <paramref name="t" />.
	/// </summary>
	/// <param name="t">Enclosing type</param>
	/// <param name="name">Field name</param>
	public static int OffsetOf(this Type t, string name)
	{
		MetaField f = t.GetAnyResolvedField(name);

		return f.Offset;
	}

#region Cast

	/*public static ref T ref_cast<T>(in T t)
		=> ref Unsafe.AsRef(ref t);*/

	public static ref T ref_cast<T>(in T t)
		=> ref Unsafe.AsRef(in t);

	/*public static object as_cast(object t)
		=> as_cast<object, object>(t);

	/// <summary>
	/// Shortcut to <see cref="Unsafe.As{T,T}"/>
	/// </summary>
	/// <remarks>Can be used similar to <c>const_cast</c>, <c>static_cast</c>,
	/// <c>dynamic_cast</c> from <em>C++</em></remarks>
	public static TTo as_cast<TFrom, TTo>(TFrom t)
		where TFrom : class
		where TTo : class
	{
		// var ptr  = Mem.AddressOfHeap(t);
		// var ptr2 = ptr.Cast<T2>();
		// var t2   = Unsafe.As<T2>(ptr2.Reference);

		var t2 = Unsafe.As<TTo>(t);
		return t2;
	}*/

	public static Pointer<T> ToPointer<T>(this Memory<T> sp, [MDR] out MemoryHandle mh)
	{
		mh = sp.Pin();
		return mh.Pointer;
	}

	public static Pointer<T> ToPointer<T>(this Span<T> s)
		=> s.ToPointer(ref Unsafe.NullRef<T>());

	public static Pointer<T> ToPointer<T>(this Span<T> s, ref T t)
	{
		t = ref s.GetPinnableReference();

		return AddressOf(ref t);
	}

#endregion

#region Read/write

#region Write

	/// <summary>
	///     Writes a value of type <typeparamref name="T" /> with value <paramref name="value" /> to
	///     <paramref name="baseAddr" /> in <paramref name="proc" />
	/// </summary>
	[SupportedOSPlatform(RuntimeInformationExtensions.OS_WIN)]
	public static bool WriteProcessMemory<T>(Process proc, Pointer<byte> baseAddr, T value)
	{
		int dwSize = Unsafe.SizeOf<T>();
		var ptr    = AddressOf(ref value);

		return WriteProcessMemory(proc, baseAddr.Address, ptr.Address, dwSize);
	}

	/// <summary>
	///     Root abstraction of <see cref="Native.WriteProcessMemory" />
	/// </summary>
	[SupportedOSPlatform(RuntimeInformationExtensions.OS_WIN)]
	public static bool WriteProcessMemory(Process proc, Pointer<byte> addr, Pointer<byte> ptrBuffer, int dwSize)
	{
		nint hProc = Native.OpenProcess(proc);
		bool ok    = false;

		ok = Native.WriteProcessMemory(hProc, addr.Address, ptrBuffer.Address, dwSize, out _);

		ok |= Native.CloseHandle(hProc);
		return ok;
	}

	/// <summary>
	///     Writes <paramref name="value" /> bytes to <paramref name="addr" /> in <paramref name="proc" />
	/// </summary>
	[SupportedOSPlatform(RuntimeInformationExtensions.OS_WIN)]
	public static bool WriteProcessMemory(Process proc, Pointer<byte> addr, [NN] byte[] value)
	{
		bool ok = false;

		fixed (byte* rg = value) {
			ok = WriteProcessMemory(proc, addr, (nint) rg, value.Length);
		}

		return ok;
	}

	/// <summary>
	///     Writes <paramref name="value" /> bytes to <paramref name="addr" /> in <paramref name="proc" />
	/// </summary>
	[SupportedOSPlatform(RuntimeInformationExtensions.OS_WIN)]
	public static bool WriteProcessMemory(Process proc, Pointer<byte> addr, ReadOnlySpan<byte> value)
	{
		bool ok = false;

		fixed (byte* rg = value) {
			ok = WriteProcessMemory(proc, addr, (nint) rg, value.Length);
		}

		return ok;
	}

#endregion

#region Read

	/// <summary>
	///     Root abstraction of <see cref="Native.ReadProcessMemory(IntPtr,IntPtr,IntPtr,int,out int)" />
	/// </summary>
	/// <param name="proc"><see cref="Process" /> whose memory is being read</param>
	/// <param name="addr">Address within the specified process from which to read</param>
	/// <param name="buffer">Buffer that receives the read contents from the address space</param>
	/// <param name="cb">Number of bytes to read</param>
	/// <remarks>Opens handle to <paramref name="proc"/> and closes it upon return</remarks>
	[SupportedOSPlatform(RuntimeInformationExtensions.OS_WIN)]
	public static bool ReadProcessMemory(Process proc, Pointer<byte> addr, Pointer<byte> buffer, nint cb)
	{
		nint h = Native.OpenProcess(proc);

		var ok = Native.ReadProcessMemory(h, addr.Address, buffer.Address, cb, out _);

		ok |= Native.CloseHandle(h);

		return ok;
	}

	/// <summary>
	///     Reads <paramref name="cb" /> bytes at <paramref name="addr" /> in <paramref name="proc" />
	/// </summary>
	[SupportedOSPlatform(RuntimeInformationExtensions.OS_WIN)]
	public static Memory<byte> ReadProcessMemory(Process proc, Pointer<byte> addr, nint cb)
	{
		Memory<byte> mem = new byte[(int) cb];
		using var    mh  = mem.Pin();

		ReadProcessMemory(proc, addr, mh.Pointer, cb);

		return mem;
	}

	/// <summary>
	///     Reads a value of type <typeparamref name="T" /> in <paramref name="proc" /> at <paramref name="addr" />
	/// </summary>
	[SupportedOSPlatform(RuntimeInformationExtensions.OS_WIN)]
	public static T ReadProcessMemory<T>(Process proc, Pointer<byte> addr)
	{
		T   value = default;
		int size  = Unsafe.SizeOf<T>();

		Pointer<T> ptr = AddressOf(ref value);

		ReadProcessMemory(proc, addr.Address, ptr.Address, size);

		return value;
	}

#endregion

#endregion

#region Size

	/// <summary>
	///     Calculates the size of <typeparamref name="T" />
	/// </summary>
	public static int SizeOf<T>() => Unsafe.SizeOf<T>();

	/// <summary>
	///     Calculates the size of <typeparamref name="T" />
	/// </summary>
	/// <param name="option">Size options</param>
	/// <returns>The size of <typeparamref name="T" />; <see cref="Native.ERROR_SV" /> otherwise</returns>
	public static int SizeOf<T>(SizeOfOption option)
	{
		MetaType mt = typeof(T);

		// Note: Arrays native size == 0
		// Note: Arrays have no layout

		return option switch
		{
			SizeOfOption.Native       => mt.NativeSize,
			SizeOfOption.Managed      => mt.HasLayout ? mt.ManagedSize : Native.ERROR_SV,
			SizeOfOption.Intrinsic    => SizeOf<T>(),
			SizeOfOption.BaseFields   => mt.InstanceFieldsSize,
			SizeOfOption.BaseInstance => mt.BaseSize,
			_                         => Native.ERROR_SV
		};

	}

	/// <summary>
	///     Calculates the size of <paramref name="value" />
	/// </summary>
	/// <param name="value">Value</param>
	/// <param name="option">Size options</param>
	/// <returns>The size of <paramref name="value" />; <see cref="Native.ERROR_SV" /> otherwise</returns>
	public static int SizeOf<T>(T value, SizeOfOption option)
	{
		ArgumentNullException.ThrowIfNull(value, nameof(value));

		//Require.Assert<ArgumentException>(!Inspector.IsNil(value), nameof(value));

		// Value is given

		var mt = value.GetMetaType();

		switch (option) {
			case SizeOfOption.BaseFields: return mt.InstanceFieldsSize;

			case SizeOfOption.BaseInstance: return mt.BaseSize;

			case SizeOfOption.BaseData: return mt.BaseDataSize;

			case SizeOfOption.Heap: return HeapSizeOfInternal(value);

			case SizeOfOption.Data:
				if (ObjectUtility.IsStruct(value)) {
					return SizeOf<T>();
				}

				// Subtract the size of the ObjHeader and MethodTable*
				return HeapSizeOfInternal(value) - ObjectUtility.ObjectBaseSize;

			case SizeOfOption.Auto:

				if (ObjectUtility.IsStruct(value)) {
					return SizeOf<T>(SizeOfOption.Intrinsic);
				}

				else
					goto case SizeOfOption.Heap;

			default:
				// return Native.ERROR_SV;
				return SizeOf<T>(option);
		}

	}

	/// <summary>
	///     Calculates the complete size of a reference type in heap memory.
	///     This is the most accurate size calculation.
	///     This follows the size formula of: (<see cref="MethodTable.BaseSize" />) + (length) *
	///     (<see cref="MethodTable.ComponentSize" />)
	///     where:
	///     <list type="bullet">
	///         <item>
	///             <description>
	///                 <see cref="MethodTable.BaseSize" /> = The base instance size of a type
	///                 (<see cref="ObjectUtility.MinObjectSize" /> by default)
	///             </description>
	///         </item>
	///         <item>
	///             <description>length	= array or string length; <c>1</c> otherwise</description>
	///         </item>
	///         <item>
	///             <description><see cref="MethodTable.ComponentSize" /> = element size, if available; <c>0</c> otherwise</description>
	///         </item>
	///     </list>
	/// </summary>
	/// <remarks>
	///     <para>Source: /src/vm/object.inl: 45</para>
	///     <para>Equals the Son Of Strike "!do" command.</para>
	///     <para>
	///         Equals <see cref="Mem.SizeOf{T}(T,SizeOfOption)" /> with <see cref="SizeOfOption.BaseInstance" /> for objects
	///         that aren't arrays or strings.
	///     </para>
	///     <para>Note: This also includes padding and overhead (<see cref="ClrObjHeader" /> and <see cref="MethodTable" /> ptr.)</para>
	/// </remarks>
	/// <returns>The size of the type in heap memory, in bytes</returns>
	public static int HeapSizeOf<T>(T value, bool throwOnErr = false) where T : class
		=> HeapSizeOfInternal(value, throwOnErr);

	[MethodImpl(MImplO.AggressiveInlining)]
	private static int HeapSizeOfInternal<T>(T value, bool throwOnErr = false)
	{
		// Sanity check
		// Require.Assert(!ObjectUtility.IsStruct(value));

		var isStruct = ObjectUtility.IsStruct(value);
		var isBoxed  = ObjectUtility.IsBoxed(value);

		s_logger.LogTrace("[{T}] | Struct={Strct} | Boxed={Bxd}", typeof(T), isStruct, isBoxed);

		if (throwOnErr && !isStruct) {
			throw new ArgumentException($"Value must be reference type", nameof(value));
		}

		// By manually reading the MethodTable*, we can calculate the size correctly if the reference
		// is boxed or cloaked
		var metaType = (MetaType) ObjectUtility.GetMethodTable(value);

		// Value of GetSizeField()

		// From object.h line 65:

		// The size of the object in the heap must be able to be computed
		// very, very quickly for GC purposes.   Restrictions on the layout
		// of the object guarantee this is possible.
		// 
		// Any object that inherits from Object must be able to
		// compute its complete size by using the first 4 bytes of
		// the object following the Object part and constants
		// reachable from the MethodTable...
		// 
		// The formula used for this calculation is:
		//     MT->GetBaseSize() + ((OBJECTTYPEREF->GetSizeField() * MT->GetComponentSize())
		// 
		// So for Object, since this is of fixed size, the ComponentSize is 0, which makes the right side
		// of the equation above equal to 0 no matter what the value of GetSizeField(), so the size is just the base size.

		int baseSize = metaType.BaseSize;

		int componentSize = metaType.TypeFlags.HasFlag(TypeFlags.HasComponentSize) ? metaType.ComponentSize : 0;

		int length = value switch
		{
			Array arr  => arr.Length,
			string str => str.Length,
			_          => 0
		};

		return baseSize + length * componentSize;
	}

	/// <summary>
	///     Calculates the total byte size of <paramref name="elemCnt" /> elements with
	///     the size of <paramref name="elemSize" />.
	/// </summary>
	/// <param name="elemSize">Byte size of one element</param>
	/// <param name="elemCnt">Number of elements</param>
	/// <returns>Total byte size of all elements</returns>
	[MethodImpl(MImplO.AggressiveInlining)]
	public static nuint GetByteCount(nuint elemCnt, nuint elemSize)
	{
		// This is based on the `mi_count_size_overflow` and `mi_mul_overflow` methods from microsoft/mimalloc.
		// Original source is Copyright (c) 2019 Microsoft Corporation, Daan Leijen. Licensed under the MIT license

		// sqrt(nuint.MaxValue)
		nuint multiplyNoOverflow = (nuint) 1 << (4 * sizeof(nuint));

		return (elemSize >= multiplyNoOverflow || elemCnt >= multiplyNoOverflow)
		       && elemSize > 0 && UIntPtr.MaxValue / elemSize < elemCnt
			       ? UIntPtr.MaxValue
			       : elemCnt * elemSize;
	}


#region

	private static readonly HashSet<SizeOfOption> TypeValue =
	[
		SizeOfOption.BaseFields,
		SizeOfOption.BaseInstance,
		SizeOfOption.Heap,
		SizeOfOption.Data

	];

	private static readonly HashSet<SizeOfOption> TypeParameter =
	[
		SizeOfOption.Native,
		SizeOfOption.Managed,
		SizeOfOption.Intrinsic,
		SizeOfOption.BaseFields,
		SizeOfOption.BaseInstance,
		SizeOfOption.BaseData

	];

	extension(SizeOfOption option)
	{

		public bool RequiresTypeValue()
			=> TypeValue.Contains(option);

		public bool RequiresTypeParameter()
			=> TypeParameter.Contains(option);

	}

	public static int GetOffsetValue(this OffsetOptions offset)
	{
		int offsetValue = offset switch
		{
			OffsetOptions.ArrayData  => ObjectUtility.OffsetToArrayData,
			OffsetOptions.StringData => ObjectUtility.OffsetToStringData,
			OffsetOptions.Fields     => ObjectUtility.OffsetToData,
			OffsetOptions.Header     => -ObjectUtility.OffsetToData,

			OffsetOptions.None or _ => 0
		};
		return offsetValue;
	}

#endregion

#endregion

#region Address

	/// <summary>
	///     Returns the address of <paramref name="value" />.
	/// </summary>
	/// <param name="value">Value to return the address of.</param>
	/// <returns>The address of the type in memory.</returns>
	public static Pointer<T> AddressOf<T>(ref T value)
	{
		/*var tr = __makeref(t);
		return *(IntPtr*) (&tr);*/

		return Unsafe.AsPointer(ref value);
	}

	public static bool TryGetAddressOfHeap<T>(T value, OffsetOptions options, out Pointer<byte> ptr)
	{
		if (ObjectUtility.IsStruct(value)) {
			ptr = null;
			return false;
		}

		ptr = AddressOfHeapInternal(value, options);
		return true;
	}

	public static bool TryGetAddressOfHeap<T>(T value, out Pointer<byte> ptr)
		=> TryGetAddressOfHeap(value, OffsetOptions.None, out ptr);

	/// <summary>
	///     Returns the address of reference type <paramref name="value" />'s heap memory, offset by the specified
	///     <see cref="OffsetOptions" />.
	///     <remarks>
	///         <para>
	///             Note: This does not pin the reference in memory if it is a reference type.
	///             This may require pinning to prevent the GC from moving the object.
	///             If the GC compacts the heap, this pointer may become invalid.
	///         </para>
	///         <seealso cref="PinManager.GetPinProxy" />
	///     </remarks>
	/// </summary>
	/// <param name="value">Reference type to return the heap address of</param>
	/// <param name="offset">Offset type</param>
	/// <returns>The address of <paramref name="value" /></returns>
	/// <exception cref="ArgumentOutOfRangeException">If <paramref name="offset"></paramref> is out of range.</exception>
	public static Pointer<byte> AddressOfHeap<T>(T value, OffsetOptions offset = OffsetOptions.None)
		where T : class
		=> AddressOfHeapInternal(value, offset);

	private static Pointer<byte> AddressOfHeapInternal<T>(T value, OffsetOptions offset)
	{
		// It is already assumed value is a class type

		//var tr = __makeref(value);
		//var heapPtr = **(IntPtr**) (&tr);

		var           pointer = AddressOf(ref value);
		Pointer<byte> heapPtr = pointer.ReadPointer();

		// NOTE:
		// Strings have their data offset by RuntimeInfo.OffsetToStringData
		// Arrays have their data offset by IntPtr.Size * 2 bytes (may be different for 32 bit)

		int offsetValue = offset.GetOffsetValue();

		switch (offset) {
			case OffsetOptions.StringData:
				Require.Assert(ObjectUtility.IsString(value));
				break;

			case OffsetOptions.ArrayData:
				Require.Assert(ObjectUtility.IsArray(value));
				break;
		}

		return heapPtr + offsetValue;
	}

	/// <summary>
	///     Returns the address of the data of <paramref name="value" />. If <typeparamref name="T" /> is a value type,
	///     this will return <see cref="AddressOf{T}" />. If <typeparamref name="T" /> is a reference type,
	///     this will return the equivalent of <see cref="AddressOfHeap{T}(T, OffsetOptions)" /> with
	///     <see cref="OffsetOptions.Fields" />.
	/// </summary>
	public static Pointer<byte> AddressOfData<T>(ref T value)
	{
		Pointer<T> addr = AddressOf(ref value);

		/*if (ObjectUtility.IsStruct(value)) {
			return addr.Cast();
		}*/

		if (typeof(T).IsValueType) {
			return addr.Cast();
		}

		return AddressOfHeapInternal(value, OffsetOptions.Fields);
	}

	/*public static Pointer<byte> AddressOfData2<T>(in T value)
		=> AddressOfData(ref ref_cast(in value));*/

#region Field

	public static Pointer<byte> AddressOfField(object obj, string name)
		=> AddressOfField<object, byte>(ref obj, name);

	public static Pointer<TField> AddressOfField<T, TField>(ref T obj, string name)
	{
		int offsetOf = obj.GetType().OffsetOf(name);

		Pointer<byte> p = AddressOfData(ref obj);

		return (Pointer<TField>) (p + offsetOf);
	}

	public static Pointer<TField> AddressOfField<TField>(Type t, string name, [NNINN(nameof(t))] object o = null)
	{
		MetaField field = t.GetAnyResolvedField(name).AsMetaField();

		Pointer<byte> p = field.IsStatic ? field.StaticAddress : AddressOfField(o, name);

		return p.Cast<TField>();
	}

	public static Pointer<TField> AddressOfField<T, TField>(ref T obj, Expression<Func<TField>> mem)
	{
		int offsetOf = obj.GetType().OffsetOf(member_of2(mem).Name);

		Pointer<byte> p = AddressOfData(ref obj);

		return (Pointer<TField>) (p + offsetOf);
	}

	/*public static ref TField ReferenceOfField<TField>(object obj, string name) =>
		ref AddressOfField<object, TField>(obj, name).Reference;

	public static ref TField ReferenceOfField<T, TField>(in T obj, string name) =>
		ref AddressOfField<T, TField>(in obj, name).Reference;

	public static ref TField ReferenceOfField<TField>(Type t, string name, object o = null) =>
		ref AddressOfField<TField>(t, name, o).Reference;*/

#endregion

#endregion

	/*
	 * https://github.com/pkrumins/bithacks.h/blob/master/bithacks.h
	 * https://catonmat.net/low-level-bit-hacks
	 */


#region Object memory operations

	public static T CopyInstance<T>(T t) where T : class
	{
		var t2 = Activator.CreateInstance<T>();

		Pointer<byte> p  = AddressOfData(ref t);
		int           s  = SizeOf(t, SizeOfOption.Data);
		Pointer<byte> p2 = AddressOfData(ref t2);

		//p2.WriteAll(p.Copy(s));
		p2.WriteAll(p.ToArray(s));

		// Copy(p, s, p2);

		return t2;
	}

	/// <summary>
	///     Reads a value of type <paramref name="mt" /> in <paramref name="proc" /> at <paramref name="addr" /> using
	/// <see cref="ReadProcessMemory(System.Diagnostics.Process,Novus.Memory.Pointer{byte},nint)"/> (<see cref="Native.Kernel32.ReadProcessMemory"/>)
	/// </summary>
	[CBN]
	[SupportedOSPlatform(RuntimeInformationExtensions.OS_WIN)]
	public static object ReadTypeFromProcessMemory(Process proc, Pointer<byte> addr, MetaType mt)
	{
		//todo

		bool valueType = mt.RuntimeType.IsValueType;
		int  size      = valueType ? mt.InstanceFieldsSize : mt.BaseSize;

		Debug.WriteLine($"{size} for {mt.Name}");

		//var i = Activator.CreateInstance(t);

		var    rg  = ReadProcessMemory(proc, addr, (nint) size);
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
	/// Converts a value of type <typeparamref name="T"/> to a <see cref="byte"/> array.
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
			var x    = AddressOfData(ref value);
			var size = SizeOf<T>();
			return x.ToArray(size);
		}

		TryGetAddressOfHeap(value, OffsetOptions.Header, out var ptr2);
		var cb2 = SizeOf(value, SizeOfOption.Heap);

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
			p2 += ObjectUtility.ObjHeaderSize;
			return Unsafe.Read<T>(&p2);
		}

		return Unsafe.Read<T>(p2);
	}

	/// <summary>
	/// Reads a value of type <typeparamref name="T"/> previously returned by <see cref="GetBytes{T}(T)"/>.
	/// </summary>
	/// <seealso cref="Streams.StreamExtensions.ReadAny{T}(Stream)"/>
	public static object ReadFromBytes(byte[] rg)
	{
		return ReadFromBytes<object>(rg);
	}

	/// <summary>
	/// Initializes an instance of type <typeparamref name="T"/> in the memory pointed by <paramref name="ptr"/>.
	/// The pre-allocated memory <paramref name="ptr"/> size must be at least &gt;= value returned by
	/// <see cref="SizeOfOption.BaseInstance"/> (<see cref="Mem.SizeOf{T}()"/>)
	/// <seealso cref="AllocManager.New{T}"/>
	/// </summary>
	/// <typeparam name="T">Type to initialize</typeparam>
	/// <param name="ptr">Memory within which to initialize the instance</param>
	/// <param name="ptrOrig">Original base pointer</param>
	/// <returns>An instance of type <typeparamref name="T"/> initialized within <paramref name="ptr"/></returns>
	/// <remarks>This function is analogous to <em>placement <c>new</c></em> in C++</remarks>
	[MURV]
	public static ref T New<T>(Pointer<byte> ptr, out Pointer<byte> ptrOrig) where T : class
	{
		ptrOrig = ptr;
		
		Unsafe.Write(ptr, default(ClrObjHeader));
		// ptr.Cast<ClrObjHeader>().Write(default);
		ptr += ObjectUtility.ObjHeaderSize;
		Unsafe.Write(ptr, ObjectUtility.ToTypeHandle<T>());
		// ptr.WritePointer<MethodTable>(typeof(T).TypeHandle.Value);

		// return ref Unsafe.AsRef<T>(ptr);

		// return ref AddressOf(ref ptr).Cast<T>().Reference;
		ref var val = ref Unsafe.AsRef<T>(&ptr);
		return ref val;
	}

#endregion

#region

	/// <summary>
	/// Parses a <see cref="byte"/> array formatted as <c>00 01 02 ...</c>
	/// </summary>
	/// <seealso cref="SigScanner.ParseSignature" />
	public static byte[] ParseAOBString(string s)
	{
		return [.. s.Split(Strings.Constants.SPACE).Select(static s1 => Byte.Parse(s1, NumberStyles.HexNumber))];
	}

	public static string ToBinaryString<T>(T value, int? totalBits = null) where T : struct
	{
		// int sizeInBytes = sizeof(T) * BitCalculator.BITS_PER_BYTE;

		int sizeInBytes = SizeOf<T>(SizeOfOption.Intrinsic) * BitCalculator.BITS_PER_BYTE;

		totalBits ??= sizeInBytes;

		if (totalBits > sizeInBytes) {
			throw new ArgumentOutOfRangeException(nameof(totalBits), $"Total bits must be less than or equal to {sizeInBytes}.");
		}

		ulong  numericValue = Convert.ToUInt64(value);
		char[] bits         = new char[totalBits.Value];

		int index = totalBits.Value - 1;

		while (index >= 0) {
			bits[index]  =   (numericValue & 1) == 1 ? '1' : '0';
			numericValue >>= 1;
			index--;
		}

		return new string(bits);
	}

#endregion

	[SupportedOSPlatform(RuntimeInformationExtensions.OS_WIN)]
	public static (ModuleEntry32, ImageSectionInfo) FindInProcessMemory(Process proc, Pointer<byte> ptr)
	{
		var modules = Native.EnumProcessModules((uint) proc.Id);

		foreach (var m in modules) {
			nint size = (nint) m.modBaseSize;
			var  b    = ptr >= m.modBaseAddr && ptr <= (m.modBaseAddr + (size));

			if (!b) {
				continue;
			}

			var pe = Native.GetPESectionInfo(m.hModule);

			// var seg = pe.FirstOrDefault(e => Mem.IsAddressInRange(ptr, e.Address, e.Address + e.Size));

			foreach (var e in pe) {
				var b2 = ptr >= e.Address && ptr <= (e.Address + size);

				if (b2) {
					return (m, e);

				}
			}
		}

		return (default, default);
	}

}

/// <summary>
///     Offset options for <see cref="Mem.AddressOfHeap{T}(T,OffsetOptions)" />
/// </summary>
public enum OffsetOptions
{

	/// <summary>
	///     Return the pointer offset by <c>-</c><see cref="ObjectUtility.OffsetToData" />,
	///     so it points to the object's <see cref="ClrObjHeader" />.
	/// </summary>
	Header,

	/// <summary>
	///     If the type is a <see cref="string" />, return the
	///     pointer offset by <see cref="ObjectUtility.OffsetToStringData" /> so it
	///     points to the string's characters.
	///     <remarks>
	///         Note: Equal to <see cref="GCHandle.AddrOfPinnedObject" /> and <c>fixed</c>.
	///     </remarks>
	/// </summary>
	StringData,

	/// <summary>
	///     If the type is an array, return
	///     the pointer offset by <see cref="ObjectUtility.OffsetToArrayData" /> so it points
	///     to the array's elements.
	///     <remarks>
	///         Note: Equal to <see cref="GCHandle.AddrOfPinnedObject" /> and <c>fixed</c>
	///     </remarks>
	/// </summary>
	ArrayData,

	/// <summary>
	///     If the type is a reference type, return
	///     the pointer offset by <see cref="ObjectUtility.OffsetToData" /> so it points
	///     to the object's fields.
	/// </summary>
	Fields,

	/// <summary>
	///     Don't offset the heap pointer at all, so it
	///     points to the <see cref="TypeHandle" />
	/// </summary>
	None

}