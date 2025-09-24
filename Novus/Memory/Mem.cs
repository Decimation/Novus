// ReSharper disable RedundantUsingDirective.Global

#pragma warning disable IDE0005, CS1574
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
///     <para>Also see JitHelpers from <see cref="System.Runtime.CompilerServices" />.</para>
/// </summary>
/// <seealso cref="Pointer"/>
/// <seealso cref="Mem" />
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
/// <seealso cref="RuntimeInformation" />
/// <seealso cref="FormatterServices" />
/// <seealso cref="Activator" />
/// <seealso cref="GCHeap" />
/// <seealso cref="RuntimeProperties" />
/// <seealso cref="Inspector" />
/// <seealso cref="Native" />
/// <seealso cref="PEReader" />
/// <seealso cref="GCHandle" />
/// <seealso cref="NativeMemory"/>
/// <seealso cref="NativeLibrary"/>
/// <seealso cref="System.Runtime.CompilerServices" />
public static unsafe class Mem
{

	/// <summary>
	///     Address size
	/// </summary>
	public static readonly int Size = IntPtr.Size;

	public static readonly nint  Invalid   = Native.ERROR_SV;
	public static readonly nuint Invalid_u = unchecked((nuint) Invalid);

	static Mem()
	{
		Is64Bit = Environment.Is64BitProcess;

		if (!RuntimeFeature.IsDynamicCodeSupported) {
			return;
		}

		PinImpl = CreatePinImpl();

		/*fixed (nint* n = &Invalid) {
			Invalid_u = *(nuint*) n;
		}*/
	}

	/// <summary>
	///     Represents a <c>null</c> <see cref="Pointer{T}" /> or <see cref="ReadOnlyPointer{T}" />
	/// </summary>
	public static readonly Pointer<byte> Nullptr = null;

	public static readonly bool Is64Bit;

	/// <summary>
	///     Returns the offset of the field <paramref name="name" /> within the type <typeparamref name="T" />.
	/// </summary>
	/// <param name="name">Field name</param>
	public static int OffsetOf<T>(string name)
		=> typeof(T).OffsetOf(name);

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

	/// <param name="p">Operand</param>
	/// <param name="lo">Start address (inclusive)</param>
	/// <param name="hi">End address (inclusive)</param>
	public static bool IsAddressInRange(Pointer<byte> p, Pointer<byte> lo, Pointer<byte> hi)
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

	public static bool IsAddressInRange(Pointer<byte> p, Pointer<byte> lo, nint size)
		=> p >= lo && p <= lo.AddBytes(size);


#region Pin

	private static readonly ActionFunctor PinImpl;

	[NN]
	private static ActionFunctor CreatePinImpl()
	{
		var method = new DynamicMethod("InvokeWhilePinnedImpl", typeof(void),
		                               [typeof(object), typeof(Action<object>)],
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

		return (ActionFunctor) method.CreateDelegate(typeof(ActionFunctor));
	}

	/// <summary>
	///     <paramref name="obj" /> will be *temporarily* pinned while action is being invoked
	/// </summary>
	public static void InvokeWhilePinned(object obj, Action<object> action)
		=> PinImpl(obj, action);

	/// <summary>
	///     Used for unsafe pinning of arbitrary objects.
	///     This allows for pinning of unblittable objects, with the <c>fixed</c> statement.
	/// </summary>
	public static PinHelperProxy GetPinningHelper(object value)
		=> Unsafe.As<PinHelperProxy>(value);

	private static Dictionary<object, ManualResetEvent> PinResetEvents { get; } = new();

	public static bool IsPinned(object obj)
		=> PinResetEvents.ContainsKey(obj);

	public static bool Pin(object obj, [CBN] object s = null)
	{
		var value = new ManualResetEvent(false);

		if (PinResetEvents.TryAdd(obj, value)) {
			var b = ThreadPool.QueueUserWorkItem(_ =>
			{
				fixed (byte* p = &GetPinningHelper(obj).Data) {
					value.WaitOne();
				}
			}, s);

			if (b) {
				Debug.WriteLine($"Pinned obj: {obj.GetHashCode()}");

			}

			return b;
		}

		return false;
	}

	public static bool Unpin(object obj)
	{

		if (PinResetEvents.TryGetValue(obj, out var p)) {
			var o = p.Set();

			if (o) {
				Debug.WriteLine($"Unpinned obj: {obj.GetHashCode()}");
				o = PinResetEvents.Remove(obj);
			}

			return o;
		}

		return false;
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
	public sealed class PinHelperProxy
	{

		/// <summary>
		///     Represents the first field in an object.
		/// </summary>
		/// <remarks>Equals <see cref="Mem.AddressOfHeap{T}(T,OffsetOptions)" /> with <see cref="OffsetOptions.Fields" />.</remarks>
		public byte Data;

		private PinHelperProxy() { }

	}

#endregion

#region Cast

	/*public static ref T ref_cast<T>(in T t)
		=> ref Unsafe.AsRef(ref t);*/

	public static ref T ref_cast<T>(in T t)
		=> ref Unsafe.AsRef(t);

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

	public static ref TTo reinterpret_cast<TFrom, TTo>(ref TFrom t)
		=> ref reinterpret_cast<TFrom, TTo>(new Pointer<TFrom>(ref t));

	public static ref TTo reinterpret_cast<TFrom, TTo>(Pointer<TFrom> t)
	{
		var p = AddressOf(ref t);
		return ref p.Cast<TTo>().Reference;
	}

#endregion

#region Read/write

#region Write

	/// <summary>
	///     Writes a value of type <typeparamref name="T" /> with value <paramref name="value" /> to
	///     <paramref name="baseAddr" /> in <paramref name="proc" />
	/// </summary>
	[SupportedOSPlatform(FileSystem.OS_WIN)]
	public static void WriteProcessMemory<T>(Process proc, Pointer<byte> baseAddr, T value)
	{
		int dwSize = Unsafe.SizeOf<T>();
		var ptr    = AddressOf(ref value);

		WriteProcessMemory(proc, baseAddr.Address, ptr.Address, dwSize);
	}

	/// <summary>
	///     Root abstraction of <see cref="Native.WriteProcessMemory" />
	/// </summary>
	[SupportedOSPlatform(FileSystem.OS_WIN)]
	public static void WriteProcessMemory(Process proc, Pointer<byte> addr, Pointer<byte> ptrBuffer, int dwSize)
	{
		nint hProc = Native.OpenProcess(proc);

		Native.WriteProcessMemory(hProc, addr.Address, ptrBuffer.Address, dwSize, out _);

		Native.CloseHandle(hProc);
	}

	/// <summary>
	///     Writes <paramref name="value" /> bytes to <paramref name="addr" /> in <paramref name="proc" />
	/// </summary>
	[SupportedOSPlatform(FileSystem.OS_WIN)]
	public static void WriteProcessMemory(Process proc, Pointer<byte> addr, [NN] byte[] value)
	{
		fixed (byte* rg = value) {
			WriteProcessMemory(proc, addr, (nint) rg, value.Length);
		}
	}

	/// <summary>
	///     Writes <paramref name="value" /> bytes to <paramref name="addr" /> in <paramref name="proc" />
	/// </summary>
	[SupportedOSPlatform(FileSystem.OS_WIN)]
	public static void WriteProcessMemory(Process proc, Pointer<byte> addr, ReadOnlySpan<byte> value)
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
	[SupportedOSPlatform(FileSystem.OS_WIN)]
	public static void ReadProcessMemory(Process proc, Pointer<byte> addr, Pointer<byte> buffer, nint cb)
	{
		nint h = Native.OpenProcess(proc);

		Native.ReadProcessMemory(h, addr.Address, buffer.Address, cb, out _);

		Native.CloseHandle(h);
	}

	/// <summary>
	///     Reads <paramref name="cb" /> bytes at <paramref name="addr" /> in <paramref name="proc" />
	/// </summary>
	[SupportedOSPlatform(FileSystem.OS_WIN)]
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
	[SupportedOSPlatform(FileSystem.OS_WIN)]
	public static T ReadProcessMemory<T>(Process proc, Pointer<byte> addr)
	{
		T   value = default;
		int size  = Unsafe.SizeOf<T>();

		Pointer<T> ptr = AddressOf(ref value);

		ReadProcessMemory(proc, addr.Address, ptr.Address, size);

		return value;
	}

#endregion


	/*public static object ReadStructure(Type t, byte[] rg, int ofs = 0)
	{
		var handle = GCHandle.Alloc(rg, GCHandleType.Pinned);

		//var stackAlloc = stackalloc byte[byteArray.Length];

		nint   objAddr = handle.AddrOfPinnedObject() + ofs;
		object value   = Marshal.PtrToStructure(objAddr, t);

		handle.Free();

		return value;
	}*/

	/*public static object ReadStructure(Type t, Memory<byte> rg, int ofs = 0)
	{
		using var    mh    = rg.Pin();
		object value = Marshal.PtrToStructure((nint) mh.Pointer, t);

		return value;
	}*/

	/*public static byte[] GetStringBytes(string s)
	{
		byte[] rg = new byte[s.Length * sizeof(char)];

		fixed (char* p = s) {
			Pointer<byte> p2 = p;

			p2.Copy(rg);
		}

		return rg;
	}*/

	/*public static string ToBinaryString(object obj)
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
	}*/

#endregion

#region Copy

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

	/*public static void Copy(Pointer<byte>src, int cb, Pointer<byte> dest)
		=> dest.WriteAll(src.ToArray(cb));

	public static void Copy(Pointer<byte> src, int startIndex, int cb, Pointer<byte> dest)
		=> dest.WriteAll(src.ToArray(startIndex, cb));

	public static byte[] Copy(Pointer<byte> src, int startIndex, int cb)
		=> src.ToArray(startIndex, cb);

	public static byte[] Copy(Pointer<byte> src, int cb)
		=> src.ToArray(cb);*/

#endregion

#region Size

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

	/// <summary>
	///     Calculates the size of <typeparamref name="T" />
	/// </summary>
	public static int SizeOf<T>()
		=> Unsafe.SizeOf<T>();

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
			case SizeOfOption.BaseFields:
				return mt.InstanceFieldsSize;

			case SizeOfOption.BaseInstance:
				return mt.BaseSize;

			case SizeOfOption.BaseData:
				return mt.BaseDataSize;

			case SizeOfOption.Heap:
				return HeapSizeOfInternal(value);

			case SizeOfOption.Data:
				if (RuntimeProperties.IsStruct(value)) {
					return SizeOf<T>();
				}

				// Subtract the size of the ObjHeader and MethodTable*
				return HeapSizeOfInternal(value) - RuntimeProperties.ObjectBaseSize;

			case SizeOfOption.Auto:

				if (RuntimeProperties.IsStruct(value)) {
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
		// Require.Assert(!RuntimeProperties.IsStruct(value));

		var isStruct = RuntimeProperties.IsStruct(value);
		var isBoxed  = RuntimeProperties.IsBoxed(value);

		Global.Logger.LogTrace("{Value} [{T}] | Struct={Strct} | Boxed={Bxd}", value, typeof(T), isStruct, isBoxed);

		if (throwOnErr && !isStruct) {
			throw new ArgumentException($"Value must be reference type", nameof(value));
		}

		// By manually reading the MethodTable*, we can calculate the size correctly if the reference
		// is boxed or cloaked
		var metaType = (MetaType) RuntimeProperties.GetMethodTable(value);

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


#region

	private static readonly HashSet<SizeOfOption> TypeValue = new()
	{
		SizeOfOption.BaseFields,
		SizeOfOption.BaseInstance,
		SizeOfOption.Heap,
		SizeOfOption.Data,

	};

	private static readonly HashSet<SizeOfOption> TypeParameter = new()
	{
		SizeOfOption.Native,
		SizeOfOption.Managed,
		SizeOfOption.Intrinsic,
		SizeOfOption.BaseFields,
		SizeOfOption.BaseInstance,
		SizeOfOption.BaseData,

	};

	public static bool RequiresTypeValue(this SizeOfOption option)
		=> TypeValue.Contains(option);

	public static bool RequiresTypeParameter(this SizeOfOption option)
		=> TypeParameter.Contains(option);

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
		if (RuntimeProperties.IsStruct(value)) {
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
	///         <seealso cref="Mem.GetPinningHelper" />
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

		Pointer<byte> heapPtr = AddressOf(ref value).ReadPointer();

		// NOTE:
		// Strings have their data offset by RuntimeInfo.OffsetToStringData
		// Arrays have their data offset by IntPtr.Size * 2 bytes (may be different for 32 bit)

		int offsetValue = offset.GetOffsetValue();

		switch (offset) {
			case OffsetOptions.StringData:
				Require.Assert(RuntimeProperties.IsString(value));
				break;

			case OffsetOptions.ArrayData:
				Require.Assert(RuntimeProperties.IsArray(value));
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

		/*if (RuntimeProperties.IsStruct(value)) {
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

	//public static int ReadBits(int value, int bitOfs, int bitCount) => ((1 << bitCount) - 1) & (value >> bitOfs);

#region Object memory operations

	/// <summary>
	///     Reads a value of type <paramref name="mt" /> in <paramref name="proc" /> at <paramref name="addr" />
	/// </summary>
	[CBN]
	[SupportedOSPlatform(FileSystem.OS_WIN)]
	public static object ReadProcessMemory(Process proc, Pointer<byte> addr, MetaType mt)
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
		Memory<byte> asMemory = rg.AsMemory();
		using var    pin      = asMemory.Pin();

		var p2 = (byte*) pin.Pointer;

		if (!typeof(T).IsValueType) {
			p2 += RuntimeProperties.ObjHeaderSize;
			return Unsafe.Read<T>(&p2);
		}

		return Unsafe.Read<T>(p2);
	}


	/// <summary>
	/// <see cref="RuntimeHelpers.GetRawData"/>
	/// </summary>
	[MURV]
	public static ref byte GetRawData(this object obj)
	{

		/*
		 * internal static ref byte GetRawData(this object obj) =>
					ref Unsafe.As<RawData>(obj).Data;
		 */

		return ref Unsafe.As<PinHelperProxy>(obj).Data;
	}

	/// <summary>
	/// Initializes an instance of type <typeparamref name="T"/> in the memory pointed by <paramref name="ptr"/>.
	/// The pre-allocated memory <paramref name="ptr"/> size must be at least &gt;= value returned by
	/// <see cref="SizeOfOption.BaseInstance"/> (<see cref="Mem.SizeOf{T}()"/>)
	/// <seealso cref="AllocManager.InitInline{T}"/>
	/// </summary>
	/// <typeparam name="T">Type to initialize</typeparam>
	/// <param name="ptr">Memory within which to initialize the instance</param>
	/// <param name="ptrOrig">Original base pointer</param>
	/// <returns>An instance of type <typeparamref name="T"/> initialized within <paramref name="ptr"/></returns>
	/// <remarks>This function is analogous to <em>placement <c>new</c></em> in C++</remarks>
	public static T InitInline<T>(Pointer<byte> ptr, out Pointer<byte> ptrOrig) where T : class
	{
		ptrOrig = ptr;
		ptr.Cast<ClrObjHeader>().Write(default);
		ptr += Size;
		ptr.WritePointer<MethodTable>(typeof(T).TypeHandle.Value);

		return AddressOf(ref ptr).Cast<T>().Reference;
	}

#endregion

	/// <summary>
	/// Parses a <see cref="byte"/> array formatted as <c>00 01 02 ...</c>
	/// </summary>
	public static byte[] ParseAOBString(string s)
	{
		return s.Split(Strings.Constants.SPACE)
			.Select(s1 => Byte.Parse(s1, NumberStyles.HexNumber))
			.ToArray();
	}

	/// <summary>
	///     Reads a <see cref="byte" /> array as a <see cref="string" /> delimited by spaces in
	///     hex number format
	/// </summary>
	/// <seealso cref="SigScanner.ReadSignature" />
	public static byte[] ParseBinaryString(string s)
	{
		string[] bytes = s.Split(Strings.Constants.SPACE);
		var      rg    = new List<byte>(bytes.Length);

		foreach (string b in bytes) {
			var n = Byte.Parse(b, NumberStyles.HexNumber);

			rg.Add(n);
		}

		return rg.ToArray();
	}

	public static string ToBinaryString<T>(T value, int totalBits = -1) where T : struct
	{
		// int sizeInBytes = sizeof(T) * BitCalculator.BITS_PER_BYTE;

		int sizeInBytes = SizeOf<T>(SizeOfOption.Intrinsic) * BitCalculator.BITS_PER_BYTE;

		if (totalBits <= -1) {
			// throw new ArgumentOutOfRangeException(nameof(totalBits), "Total bits must be at least 1.");
			totalBits = sizeInBytes;
		}

		if (totalBits > sizeInBytes) {
			throw new ArgumentOutOfRangeException(nameof(totalBits),
			                                      $"Total bits must be less than or equal to {sizeInBytes}.");
		}

		ulong  numericValue = Convert.ToUInt64(value);
		char[] bits         = new char[totalBits];
		int    index        = totalBits - 1;

		while (index >= 0) {
			bits[index]  =   (numericValue & 1) == 1 ? '1' : '0';
			numericValue >>= 1;
			index--;
		}

		return new string(bits);
	}

	/*public static void Write<T>(Pointer<byte> p, T value)
	{
		Unsafe.Write(p.ToPointer(), value);
	}

	public static T Read<T>(Pointer<byte> p)
	{
		return Unsafe.Read<T>(p.ToPointer());
	}*/

	/*
	public static ref T ReadRef<T>(this Span<T> sp)
	{
		return ref sp.GetPinnableReference();
	}
	*/

	[SupportedOSPlatform(FileSystem.OS_WIN)]
	public static (ModuleEntry32, ImageSectionInfo) FindInProcessMemory(Process proc, Pointer<byte> ptr)
	{
		var modules = Native.EnumProcessModules((uint) proc.Id);

		foreach (var m in modules) {
			var b = IsAddressInRange(ptr, m.modBaseAddr, (nint) m.modBaseSize);

			if (!b) {
				continue;
			}

			var pe = Native.GetPESectionInfo(m.hModule);

			// var seg = pe.FirstOrDefault(e => Mem.IsAddressInRange(ptr, e.Address, e.Address + e.Size));

			foreach (var e in pe) {
				var b2 = IsAddressInRange(ptr, e.Address, e.Size);

				if (b2) {
					return (m, e);

				}
			}
		}

		return (default, default);
	}

	public static ref T PinToRef<T>(this Memory<T> sp, [MDR] out MemoryHandle mh)
	{
		mh = sp.Pin();
		Pointer<T> p = mh.Pointer;
		return ref p.Reference;
	}

	/*public static Pointer<T> ToPointer<T>(this Span<T> s)
		=> s.ToPointer(ref s.GetPinnableReference());*/

	public static Pointer<T> ToPointer<T>(this Span<T> s, ref T t)
	{
		t = ref s.GetPinnableReference();

		return AddressOf(ref t);
	}

	public static int GetOffsetValue(this OffsetOptions offset)
	{
		int offsetValue = offset switch
		{
			OffsetOptions.ArrayData  => RuntimeProperties.OffsetToArrayData,
			OffsetOptions.StringData => RuntimeProperties.OffsetToStringData,
			OffsetOptions.Fields     => RuntimeProperties.OffsetToData,
			OffsetOptions.Header     => -RuntimeProperties.OffsetToData,

			OffsetOptions.None or _ => 0
		};
		return offsetValue;
	}

}

/// <summary>
///     Offset options for <see cref="Mem.AddressOfHeap{T}(T,OffsetOptions)" />
/// </summary>
public enum OffsetOptions
{

	/// <summary>
	///     Return the pointer offset by <c>-</c><see cref="Size" />,
	///     so it points to the object's <see cref="ClrObjHeader" />.
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