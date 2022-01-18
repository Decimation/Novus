// ReSharper disable RedundantUsingDirective.Global

#pragma warning disable IDE0005, CS1574
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using Kantan.Diagnostics;
using Kantan.Text;
using Kantan.Utilities;
using Novus.Memory.Allocation;
using Novus.OS.Win32;
using Novus.OS.Win32.Structures;
using Novus.OS.Win32.Structures.Kernel32;
using Novus.Runtime;
using Novus.Runtime.Meta;
using Novus.Runtime.VM;
using Novus.Utilities;

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


namespace Novus.Memory;

/// <summary>
///     Provides utilities for manipulating pointers, memory, and types.
///     <para>Also see JitHelpers from <see cref="System.Runtime.CompilerServices" />.</para>
/// </summary>
/// <seealso cref="Mem" />
/// <seealso cref="BitConverter" />
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
/// <seealso cref="RuntimeEnvironment" />
/// <seealso cref="RuntimeInformation" />
/// <seealso cref="FormatterServices" />
/// <seealso cref="Activator" />
/// <seealso cref="GCHeap" />
/// <seealso cref="RuntimeProperties" />
/// <seealso cref="Inspector" />
/// <seealso cref="Native" />
/// <seealso cref="PEReader" />
/// <seealso cref="GCHandle" />
/// <seealso cref="Hook" />
/// <seealso cref="NativeMemory"/>
/// <seealso cref="NativeLibrary"/>
/// <seealso cref="System.Runtime.CompilerServices" />
public static unsafe class Mem
{
	/// <summary>
	///     Address size
	/// </summary>
	public static readonly int Size = sizeof(nint);

	/// <summary>
	///     Represents a <c>null</c> <see cref="Pointer{T}" /> or <see cref="ReadOnlyPointer{T}" />
	/// </summary>
	public static readonly Pointer Nullptr = null;


	public static bool Is64Bit => Environment.Is64BitProcess;


	/// <summary>
	///     Returns the offset of the field <paramref name="name" /> within the type <typeparamref name="T" />.
	/// </summary>
	/// <param name="name">Field name</param>
	public static int OffsetOf<T>(string name) => OffsetOf(typeof(T), name);

	/// <summary>
	///     Returns the offset of the field <paramref name="name" /> within the type <paramref name="t" />.
	/// </summary>
	/// <param name="t">Enclosing type</param>
	/// <param name="name">Field name</param>
	public static int OffsetOf(Type t, string name)
	{
		MetaField f = t.GetAnyResolvedField(name);

		return f.Offset;
	}

	/// <param name="p">Operand</param>
	/// <param name="lo">Start address (inclusive)</param>
	/// <param name="hi">End address (inclusive)</param>
	public static bool IsAddressInRange(Pointer p, Pointer lo, Pointer hi)
	{
		// [lo, hi]

		// if ((ptrStack < stackBase) && (ptrStack > (stackBase - stackSize)))
		// (p >= regionStart && p < regionStart + regionSize) ;
		// return target >= start && target < end;
		// return m_CacheStackLimit < addr && addr <= m_CacheStackBase;
		// if (!((object < g_gc_highest_address) && (object >= g_gc_lowest_address)))
		// return max.ToInt64() < p.ToInt64() && p.ToInt64() <= min.ToInt64();

		return p <= hi && p >= lo;
	}

	public static bool IsAddressInRange(Pointer p, Pointer lo, long size)
	{
		return p >= lo && p <= lo + size;
	}

	#region CRT

	public static nuint _strlen(Pointer p) => Native.strlen(p.ToPointer());

	/// <summary>
	/// Size of native runtime (e.g., <see cref="NativeMemory"/>) allocations
	/// </summary>
	public static nuint _msize(Pointer p) => (nuint) Native._msize(p.ToPointer());

	#endregion

	#region Pin

	private static readonly Action<object, Action<object>> PinImpl = CreatePinImpl();

	private static Dictionary<object, ManualResetEvent> PinResetEvents { get; } = new();

	private static Action<object, Action<object>> CreatePinImpl()
	{
		var method = new DynamicMethod("InvokeWhilePinnedImpl", typeof(void),
		                               new[] { typeof(object), typeof(Action<object>) },
		                               typeof(RuntimeProperties).Module);

		ILGenerator il = method.GetILGenerator();

		// create a pinned local variable of type object
		// this wouldn't be valid in C#, but the runtime doesn't complain about the IL
		LocalBuilder local = il.DeclareLocal(typeof(object), true);

		// store first argument obj in the pinned local variable
		il.Emit(OpCodes.Ldarg_0);
		il.Emit(OpCodes.Stloc_0);
		// invoke the delegate
		il.Emit(OpCodes.Ldarg_1);
		il.Emit(OpCodes.Ldarg_0);
		il.EmitCall(OpCodes.Callvirt, typeof(Action<object>).GetMethod("Invoke")!, null);

		il.Emit(OpCodes.Ret);

		return (Action<object, Action<object>>) method.CreateDelegate(typeof(Action<object, Action<object>>));
	}


	/// <summary>
	///     <paramref name="obj" /> will be *temporarily* pinned while action is being invoked
	/// </summary>
	public static void InvokeWhilePinned(object obj, Action<object> action) => PinImpl(obj, action);

	/// <summary>
	///     Used for unsafe pinning of arbitrary objects.
	///     This allows for pinning of unblittable objects, with the <c>fixed</c> statement.
	/// </summary>
	public static PinningHelper GetPinningHelper(object value) => U.As<PinningHelper>(value);


	public static void Pin(object obj)
	{
		var value = new ManualResetEvent(false);

		PinResetEvents.Add(obj, value);

		ThreadPool.QueueUserWorkItem(_ =>
		{
			fixed (byte* p = &GetPinningHelper(obj).Data) {
				value.WaitOne();
			}
		});

		Debug.WriteLine($"Pinned obj: {obj.GetHashCode()}");
	}

	public static void Unpin(object obj)
	{
		PinResetEvents[obj].Set();

		Debug.WriteLine($"Unpinned obj: {obj.GetHashCode()}");
	}

	/// <summary>
	///     <para>Helper class to assist with unsafe pinning of arbitrary objects. The typical usage pattern is:</para>
	///     <code>
	///  fixed (byte* pData = &amp;PinHelper.GetPinningHelper(value).Data)
	///  {
	///  }
	///  </code>
	///     <remarks>
	///         <para><c>pData</c> is what <c>Object::GetData()</c> returns in VM.</para>
	///         <para><c>pData</c> is also equal to offsetting the pointer by <see cref="OffsetOptions.Fields" />. </para>
	///         <para>From <see cref="System.Runtime.CompilerServices.JitHelpers" />. </para>
	///     </remarks>
	/// </summary>
	[UsedImplicitly]
	public sealed class PinningHelper
	{
		/// <summary>
		///     Represents the first field in an object.
		/// </summary>
		/// <remarks>Equals <see cref="Mem.AddressOfHeap{T}(T,OffsetOptions)" /> with <see cref="OffsetOptions.Fields" />.</remarks>
		public byte Data;

		private PinningHelper() { }
	}

	#endregion


	#region Read/write

	#region Write

	/// <summary>
	///     Writes a value of type <typeparamref name="T" /> with value <paramref name="value" /> to
	///     <paramref name="baseAddr" /> in <paramref name="proc" />
	/// </summary>
	public static void WriteProcessMemory<T>(Process proc, Pointer baseAddr, T value)
	{
		int dwSize = U.SizeOf<T>();
		var ptr    = AddressOf(ref value);

		WriteProcessMemory(proc, baseAddr.Address, ptr.Address, dwSize);
	}

	/// <summary>
	///     Root abstraction of <see cref="Native.WriteProcessMemory" />
	/// </summary>
	public static void WriteProcessMemory(Process proc, Pointer addr, Pointer ptrBuffer, int dwSize)
	{
		IntPtr hProc = Native.OpenProcess(proc);

		Native.WriteProcessMemory(hProc, addr.Address, ptrBuffer.Address, dwSize, out _);

		Native.CloseHandle(hProc);
	}

	/// <summary>
	///     Writes <paramref name="value" /> bytes to <paramref name="addr" /> in <paramref name="proc" />
	/// </summary>
	public static void WriteProcessMemory(Process proc, Pointer addr, byte[] value)
	{
		fixed (byte* rg = value) {
			WriteProcessMemory(proc, addr, (nint) rg, value.Length);
		}
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
	public static void ReadProcessMemory(Process proc, Pointer addr, Pointer buffer, nint cb)
	{
		IntPtr h = Native.OpenProcess(proc);

		Native.ReadProcessMemory(h, addr.Address, buffer.Address, cb, out _);

		Native.CloseHandle(h);
	}


	/// <summary>
	///     Reads <paramref name="cb" /> bytes at <paramref name="addr" /> in <paramref name="proc" />
	/// </summary>
	public static byte[] ReadProcessMemory(Process proc, Pointer addr, nint cb)
	{
		byte[] mem = new byte[cb];

		fixed (byte* p = mem) {
			ReadProcessMemory(proc, addr, p, cb);
		}

		return mem;
	}

	/// <summary>
	///     Reads a value of type <typeparamref name="T" /> in <paramref name="proc" /> at <paramref name="addr" />
	/// </summary>
	public static T ReadProcessMemory<T>(Process proc, Pointer addr)
	{
		T value = default!;

		int size = U.SizeOf<T>();

		Pointer<T> ptr = AddressOf(ref value);

		ReadProcessMemory(proc, addr.Address, ptr.Address, size);

		return value;
	}

	/// <summary>
	///     Reads a value of type <paramref name="mt" /> in <paramref name="proc" /> at <paramref name="addr" />
	/// </summary>
	[CanBeNull]
	public static object ReadProcessMemory(Process proc, Pointer addr, MetaType mt)
	{
		//todo

		bool valueType = mt.RuntimeType.IsValueType;
		int  size      = valueType ? mt.InstanceFieldsSize : mt.BaseSize;

		Debug.WriteLine($"{size} for {mt.Name}");

		//var i = Activator.CreateInstance(t);

		byte[] rg  = ReadProcessMemory(proc, addr, (IntPtr) size);
		object val = null;

		if (valueType) {
			val = ReadStructure(mt.RuntimeType, rg);
		}
		else {
			fixed (byte* ptr = rg) {
				val = U.Read<object>(ptr);
			}
		}

		return val;
	}

	#endregion

	public static object ReadStructure(Type t, byte[] rg, int ofs = 0)
	{
		GCHandle handle = GCHandle.Alloc(rg, GCHandleType.Pinned);
		//var stackAlloc = stackalloc byte[byteArray.Length];

		IntPtr objAddr = handle.AddrOfPinnedObject() + ofs;
		object value   = Marshal.PtrToStructure(objAddr, t);

		handle.Free();

		return value;
	}

	/// <summary>
	///     Reads inherited substructure <typeparamref name="TSub" /> from parent <typeparamref name="TSuper" />.
	/// </summary>
	/// <typeparam name="TSuper">Superstructure (parent) type</typeparam>
	/// <typeparam name="TSub">Substructure (child) type</typeparam>
	/// <param name="super">Superstructure pointer</param>
	/// <returns>Substructure pointer</returns>
	public static Pointer<TSub> ReadSubStructure<TSuper, TSub>(Pointer<TSuper> super)
	{
		int size = SizeOf<TSuper>();
		return super.Add(size).Cast<TSub>();
	}

	/// <summary>
	///     Reads a <see cref="byte" /> array as a <see cref="string" /> delimited by spaces in
	///     hex number format
	/// </summary>
	/// <seealso cref="SigScanner.ReadSignature" />
	public static byte[] ReadBinaryString(string s)
	{
		var rg = new List<byte>();

		string[] bytes = s.Split(Strings.Constants.SPACE);

		foreach (string b in bytes) {
			byte n = Byte.Parse(b, NumberStyles.HexNumber);

			rg.Add(n);
		}

		return rg.ToArray();
	}

	public static byte[] GetStringBytes(string s)
	{
		byte[] rg = new byte[s.Length * sizeof(char)];

		fixed (char* p = s) {
			Pointer p2 = p;

			p2.Copy(rg);
		}

		return rg;
	}

	public static ref T RefCast<T>(in T t) => ref U.AsRef(in t);

	public static object AsCast(object t) => AsCast<object, object>(t);

	/// <summary>
	/// Shortcut to <see cref="Unsafe.As{T,T}"/>
	/// </summary>
	/// <remarks>Can be used similar to <c>const_cast</c>, <c>static_cast</c>,
	/// <c>dynamic_cast</c> from <em>C++</em></remarks>
	public static TTo AsCast<TFrom, TTo>(TFrom t) where TFrom : class
		where TTo : class
	{
		// var ptr  = Mem.AddressOfHeap(t);
		// var ptr2 = ptr.Cast<T2>();
		// var t2   = U.As<T2>(ptr2.Reference);

		var t2 = U.As<TTo>(t);
		return t2;
	}


	public static string ToBinaryString(object obj)
	{
		byte[] bytes = null;

		if (obj is string s) {
			bytes = Encoding.Default.GetBytes(s);
			// bytes = GetStringBytes(s);
		}
		else if (obj.GetType().IsNumeric()) {
			bytes = BitConverter.GetBytes((nint) obj);
		}

		if (bytes != null) {
			return string.Join(String.Empty, bytes.Select(x => Convert.ToString(x, 2)));
		}

		return null;
	}

	public static void Write<T>(this Span<T> s, params T[] v)
	{
		for (int i = 0; i < v.Length; i++) {
			s[i] = v[i];
		}
	}
	public static Pointer<T> ToPointer<T>(this Span<T> s)
	{
		return AddressOf(ref s.GetPinnableReference());
	}

	#endregion

	#region Copy

	public static T CopyInstance<T>(T t) where T : class
	{
		var t2 = Activator.CreateInstance<T>();

		Pointer p  = AddressOfData(ref t);
		int     s  = SizeOf(t, SizeOfOptions.Data);
		Pointer p2 = AddressOfData(ref t2);

		//p2.WriteAll(p.Copy(s));

		Copy(p, s, p2);

		return t2;
	}

	public static void Copy(Pointer src, int cb, Pointer dest) => dest.WriteAll(src.ToArray(cb));

	public static void Copy(Pointer src, int startIndex, int cb, Pointer dest)
		=> dest.WriteAll(src.ToArray(startIndex, cb));

	public static byte[] Copy(Pointer src, int startIndex, int cb) => src.ToArray(startIndex, cb);

	public static byte[] Copy(Pointer src, int cb) => src.ToArray(cb);

	#endregion

	#region Size

	public static nuint GetByteCount(int elemCnt, int elemSize) => GetByteCount((nuint) elemCnt, (nuint) elemSize);

	/// <summary>
	///     Calculates the total byte size of <paramref name="elemCnt" /> elements with
	///     the size of <paramref name="elemSize" />.
	/// </summary>
	/// <param name="elemSize">Byte size of one element</param>
	/// <param name="elemCnt">Number of elements</param>
	/// <returns>Total byte size of all elements</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static nuint GetByteCount(nuint elemCnt, nuint elemSize)
	{
		// This is based on the `mi_count_size_overflow` and `mi_mul_overflow` methods from microsoft/mimalloc.
		// Original source is Copyright (c) 2019 Microsoft Corporation, Daan Leijen. Licensed under the MIT license

		// sqrt(nuint.MaxValue)
		nuint multiplyNoOverflow = (nuint) 1 << (4 * sizeof(nuint));

		return (elemSize >= multiplyNoOverflow || elemCnt >= multiplyNoOverflow)
		       && elemSize > 0 && nuint.MaxValue / elemSize < elemCnt
			       ? nuint.MaxValue
			       : elemCnt * elemSize;
	}

	/// <summary>
	///     Calculates the size of <typeparamref name="T" />
	/// </summary>
	public static int SizeOf<T>()
	{
		return U.SizeOf<T>();
	}

	/// <summary>
	///     Calculates the size of <typeparamref name="T" />
	/// </summary>
	/// <param name="options">Size options</param>
	/// <returns>The size of <typeparamref name="T" />; <see cref="Native.INVALID" /> otherwise</returns>
	public static int SizeOf<T>(SizeOfOptions options)
	{
		MetaType mt = typeof(T);

		// Note: Arrays native size == 0
		// Note: Arrays have no layout

		return options switch
		{
			SizeOfOptions.Native       => mt.NativeSize,
			SizeOfOptions.Managed      => mt.HasLayout ? mt.ManagedSize : Native.INVALID,
			SizeOfOptions.Intrinsic    => SizeOf<T>(),
			SizeOfOptions.BaseFields   => mt.InstanceFieldsSize,
			SizeOfOptions.BaseInstance => mt.BaseSize,
			_                          => Native.INVALID
		};

	}

	/// <summary>
	///     Calculates the size of <paramref name="value" />
	/// </summary>
	/// <param name="value">Value</param>
	/// <param name="options">Size options</param>
	/// <returns>The size of <paramref name="value" />; <see cref="Native.INVALID" /> otherwise</returns>
	public static int SizeOf<T>(T value, SizeOfOptions options)
	{
		Guard.AssertArgumentNotNull(value, nameof(value));

		//Guard.Assert<ArgumentException>(!Inspector.IsNil(value), nameof(value));

		// Value is given

		var mt = new MetaType(value.GetType());

		switch (options) {
			case SizeOfOptions.BaseFields:   return mt.InstanceFieldsSize;
			case SizeOfOptions.BaseInstance: return mt.BaseSize;
			case SizeOfOptions.BaseData:     return mt.BaseDataSize;
			case SizeOfOptions.Heap:         return HeapSizeOfInternal(value);

			case SizeOfOptions.Data:
				if (RuntimeProperties.IsStruct(value)) {
					return SizeOf<T>();
				}

				// Subtract the size of the ObjHeader and MethodTable*
				return HeapSizeOfInternal(value) - RuntimeProperties.ObjectBaseSize;

			case SizeOfOptions.Auto:

				if (RuntimeProperties.IsStruct(value)) {
					return SizeOf<T>(SizeOfOptions.Intrinsic);
				}

				else goto case SizeOfOptions.Heap;

			default:
				return Native.INVALID;
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
	///                 (<see cref="RuntimeProperties.MinObjectSize" /> by default)
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
	///         Equals <see cref="Mem.SizeOf{T}(T,SizeOfOptions)" /> with <see cref="SizeOfOptions.BaseInstance" /> for objects
	///         that aren't arrays or strings.
	///     </para>
	///     <para>Note: This also includes padding and overhead (<see cref="ObjHeader" /> and <see cref="MethodTable" /> ptr.)</para>
	/// </remarks>
	/// <returns>The size of the type in heap memory, in bytes</returns>
	public static int HeapSizeOf<T>(T value) where T : class => HeapSizeOfInternal(value);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int HeapSizeOfInternal<T>(T value)
	{
		// Sanity check
		Guard.Assert(!RuntimeProperties.IsStruct(value));

		// By manually reading the MethodTable*, we can calculate the size correctly if the reference
		// is boxed or cloaked
		var metaType = (MetaType) RuntimeProperties.ReadTypeHandle(value);

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

		int componentSize = metaType.ComponentSize;

		int length = value switch
		{
			Array arr  => arr.Length,
			string str => str.Length,
			_          => 0
		};

		return baseSize + length * componentSize;
	}

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

		return U.AsPointer(ref value);
	}

	public static bool TryGetAddressOfHeap<T>(T value, OffsetOptions options, out Pointer ptr)
	{
		if (RuntimeProperties.IsStruct(value)) {
			ptr = null;
			return false;
		}

		ptr = AddressOfHeapInternal(value, options);
		return true;
	}

	public static bool TryGetAddressOfHeap<T>(T value, out Pointer ptr)
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
	///         <seealso cref="Mem.GetPinningHelper" />
	///     </remarks>
	/// </summary>
	/// <param name="value">Reference type to return the heap address of</param>
	/// <param name="offset">Offset type</param>
	/// <returns>The address of <paramref name="value" /></returns>
	/// <exception cref="ArgumentOutOfRangeException">If <paramref name="offset"></paramref> is out of range.</exception>
	public static Pointer AddressOfHeap<T>(T value, OffsetOptions offset = OffsetOptions.None)
		where T : class
		=> AddressOfHeapInternal(value, offset);

	private static Pointer AddressOfHeapInternal<T>(T value, OffsetOptions offset)
	{
		// It is already assumed value is a class type

		//var tr = __makeref(value);
		//var heapPtr = **(IntPtr**) (&tr);

		Pointer heapPtr = AddressOf(ref value).ReadPointer();

		// NOTE:
		// Strings have their data offset by RuntimeInfo.OffsetToStringData
		// Arrays have their data offset by IntPtr.Size * 2 bytes (may be different for 32 bit)

		int offsetValue = offset switch
		{
			OffsetOptions.ArrayData  => RuntimeProperties.OffsetToArrayData,
			OffsetOptions.StringData => RuntimeProperties.OffsetToStringData,
			OffsetOptions.Fields     => RuntimeProperties.OffsetToData,
			OffsetOptions.Header     => -RuntimeProperties.OffsetToData,

			OffsetOptions.None or _ => 0
		};

		switch (offset) {
			case OffsetOptions.StringData:
				Guard.Assert(RuntimeProperties.IsString(value));
				break;

			case OffsetOptions.ArrayData:
				Guard.Assert(RuntimeProperties.IsArray(value));
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
	public static Pointer AddressOfData<T>(ref T value)
	{
		Pointer<T> addr = AddressOf(ref value);

		if (RuntimeProperties.IsStruct(value)) {
			return addr.Cast();
		}

		return AddressOfHeapInternal(value, OffsetOptions.Fields);
	}

	#region Field

	public static Pointer AddressOfField(object obj, string name) => AddressOfField<object, byte>(obj, name);

	public static Pointer<TField> AddressOfField<TField>(Type t, string name, [NNINN("t")] object o = null)
	{
		MetaField field = t.GetAnyResolvedField(name).AsMetaField();

		Pointer p = field.IsStatic ? field.StaticAddress : AddressOfField(o, name);

		return p.Cast<TField>();
	}

	public static Pointer<TField> AddressOfField<T, TField>(in T obj, string name)
	{
		int offsetOf = OffsetOf(obj.GetType(), name);

		Pointer p = AddressOfData(ref U.AsRef(in obj));

		return p + offsetOf;
	}

	/*public static ref TField ReferenceOfField<TField>(object obj, string name) =>
		ref AddressOfField<object, TField>(obj, name).Reference;

	public static ref TField ReferenceOfField<T, TField>(in T obj, string name) =>
		ref AddressOfField<T, TField>(in obj, name).Reference;

	public static ref TField ReferenceOfField<TField>(Type t, string name, object o = null) =>
		ref AddressOfField<TField>(t, name, o).Reference;*/

	#endregion

	#endregion

	#region Virtual

	public static Pointer VirtualAlloc(Process proc, Pointer lpAddr, int dwSize,
	                                   AllocationType type, MemoryProtection mp)
	{
		IntPtr ptr = Native.VirtualAllocEx(proc.Handle, lpAddr.Address, (uint) dwSize, type, mp);

		return ptr;
	}

	public static bool VirtualFree(Process hProcess, Pointer lpAddress, int dwSize, AllocationType dwFreeType)
	{
		bool p = Native.VirtualFreeEx(hProcess.Handle, lpAddress.Address, dwSize, dwFreeType);

		return p;
	}

	public static bool VirtualProtect(Process hProcess, Pointer lpAddress, int dwSize,
	                                  MemoryProtection flNewProtect, out MemoryProtection lpflOldProtect)
	{
		bool p = Native.VirtualProtectEx(hProcess.Handle, lpAddress.Address, (uint) dwSize, flNewProtect,
		                                 out lpflOldProtect);

		return p;
	}

	public static MemoryBasicInformation VirtualQuery(Process proc, Pointer lpAddr)
	{
		var mbi = new MemoryBasicInformation();

		int v = Native.VirtualQueryEx(proc.Handle, lpAddr.Address, ref mbi,
		                              (uint) Marshal.SizeOf<MemoryBasicInformation>());

		return mbi;
	}


	public static MemoryBasicInformation QueryMemoryPage(Pointer p)
	{

		/*
		 * https://stackoverflow.com/questions/496034/most-efficient-replacement-for-isbadreadptr
		 * https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-isbadreadptr
		 */

		MemoryBasicInformation mbi = default;

		return Native.VirtualQuery(p.Address, ref mbi, Marshal.SizeOf<MemoryBasicInformation>()) != 0
			       ? mbi
			       : throw new Win32Exception();

	}

	public static LinkedList<MemoryBasicInformation> EnumeratePages(IntPtr handle)
	{
		SystemInfo sysInfo = default;

		Native.GetSystemInfo(ref sysInfo);

		MemoryBasicInformation mbi = default;

		long lpMem = 0L;

		uint sizeOf = (uint) Marshal.SizeOf(typeof(MemoryBasicInformation));

		long maxAddr = sysInfo.MaximumApplicationAddress.ToInt64();

		var ll = new LinkedList<MemoryBasicInformation>();

		while (lpMem < maxAddr) {
			int result = Native.VirtualQueryEx(handle, (IntPtr) lpMem, ref mbi, sizeOf);

			/*var b = m.State == AllocationType.Commit &&
			        m.Type is MemType.MEM_MAPPED or MemType.MEM_PRIVATE;*/

			ll.AddLast(mbi);

			long address = (long) mbi.BaseAddress + (long) mbi.RegionSize;

			if (lpMem == address)
				break;

			lpMem = address;

		}

		return ll;
	}

	#endregion

	#region Bits

	/*
	 * https://github.com/pkrumins/bithacks.h/blob/master/bithacks.h
	 * https://catonmat.net/low-level-bit-hacks
	 */

	//public static int ReadBits(int value, int bitOfs, int bitCount) => ((1 << bitCount) - 1) & (value >> bitOfs);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool ReadBit(int value, int bitOfs)
	{
		return (value & (1 << bitOfs)) != 0;
	}

	public static int SetBit(int x, int n)
	{
		return x | (1 << n);
	}

	public static int UnsetBit(int x, int n)
	{
		return x & ~(1 << n);
	}

	public static int ToggleBit(int x, int n)
	{
		return x ^ (1 << n);
	}

	public static int GetBitMask(int index, int size)
	{
		return ((1 << size) - 1) << index;
	}

	/// <summary>
	///     Reads <paramref name="bitCount" /> from <paramref name="value" /> at offset <paramref name="bitOfs" />
	/// </summary>
	/// <param name="value"><see cref="int" /> value to read from</param>
	/// <param name="bitOfs">Beginning offset</param>
	/// <param name="bitCount">Number of bits to read</param>
	/// <seealso cref="BitArray" />
	/// <seealso cref="BitVector32" />
	public static int ReadBits(int value, int bitOfs, int bitCount)
	{
		return (value & GetBitMask(bitOfs, bitCount)) >> bitOfs;
	}

	public static int WriteBits(int data, int index, int size, int value)
	{
		return (data & ~GetBitMask(index, size)) | (value << index);
	}

	#endregion

	[MustUseReturnValue]
	public static UArray<T> AllocUArray<T>(int s)
	{
		Pointer<T> ptr = NativeMemory.AllocZeroed((nuint) s, (nuint) U.SizeOf<T>());

		var u = new UArray<T>(ptr, s);

		return u;
	}
}

/// <summary>
///     Offset options for <see cref="Mem.AddressOfHeap{T}(T,OffsetOptions)" />
/// </summary>
public enum OffsetOptions
{
	/// <summary>
	///     Return the pointer offset by <c>-</c><see cref="Size" />,
	///     so it points to the object's <see cref="ObjHeader" />.
	/// </summary>
	Header,

	/// <summary>
	///     If the type is a <see cref="string" />, return the
	///     pointer offset by <see cref="RuntimeProperties.OffsetToStringData" /> so it
	///     points to the string's characters.
	///     <remarks>
	///         Note: Equal to <see cref="GCHandle.AddrOfPinnedObject" /> and <c>fixed</c>.
	///     </remarks>
	/// </summary>
	StringData,

	/// <summary>
	///     If the type is an array, return
	///     the pointer offset by <see cref="RuntimeProperties.OffsetToArrayData" /> so it points
	///     to the array's elements.
	///     <remarks>
	///         Note: Equal to <see cref="GCHandle.AddrOfPinnedObject" /> and <c>fixed</c>
	///     </remarks>
	/// </summary>
	ArrayData,

	/// <summary>
	///     If the type is a reference type, return
	///     the pointer offset by <see cref="Size" /> so it points
	///     to the object's fields.
	/// </summary>
	Fields,

	/// <summary>
	///     Don't offset the heap pointer at all, so it
	///     points to the <see cref="TypeHandle" />
	/// </summary>
	None
}