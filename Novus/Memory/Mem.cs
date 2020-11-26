using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;
using Novus.CoreClr.Meta;
using Novus.CoreClr.VM;
using Novus.Utilities;
using Novus.Win32;
using SimpleCore.Diagnostics;
using SimpleCore.Utilities;

// ReSharper disable SwitchStatementMissingSomeEnumCasesNoDefault

// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault


// ReSharper disable ConvertIfStatementToReturnStatement

// ReSharper disable UnusedVariable

// ReSharper disable UnusedMember.Global


namespace Novus.Memory
{
	/// <summary>
	///     Provides utilities for manipulating pointers, memory, and types.
	///     
	/// </summary>
	/// <remarks>
	/// Also see JitHelpers from <see cref="System.Runtime.CompilerServices" />.
	/// </remarks>
	/// <seealso cref="BitConverter" />
	/// <seealso cref="Convert" />
	/// <seealso cref="MemoryMarshal" />
	/// <seealso cref="Marshal" />
	/// <seealso cref="Span{T}" />
	/// <seealso cref="Memory{T}" />
	/// <seealso cref="Buffer" />
	/// <seealso cref="Mem" />
	/// <seealso cref="Unsafe" />
	/// <seealso cref="System.Runtime.CompilerServices" />
	/// <seealso cref="RuntimeHelpers" />
	/// <seealso cref="RuntimeInfo"/>
	public static unsafe class Mem
	{
		public static bool Is64Bit => Environment.Is64BitProcess;


		public static readonly int Size = IntPtr.Size;

		/// <summary>
		///     Represents a <c>null</c> <see cref="Pointer{T}" />
		/// </summary>
		public static readonly Pointer<byte> Nullptr = null;

		/// <summary>
		///     Root abstraction of <see cref="Native.ReadProcessMemory" />
		/// </summary>
		/// <param name="proc"><see cref="Process" /> whose memory is being read</param>
		/// <param name="baseAddr">Address within the specified process from which to read</param>
		/// <param name="buffer">Buffer that receives the read contents from the address space</param>
		/// <param name="cb">Number of bytes to read</param>
		public static void ReadProcessMemory(Process proc, Pointer<byte> baseAddr, Pointer<byte> buffer, int cb)
		{
			var h = Native.OpenProcess(proc);

			bool ok = Native.ReadProcessMemory(h, baseAddr.Address, buffer.Address, cb, out int numBytesRead);

			Native.CloseHandle(h);
		}

		public static T ReadProcessMemory<T>(Process proc, Pointer<byte> baseAddr)
		{
			T   t    = default!;
			int size = Unsafe.SizeOf<T>();
			var ptr  = AddressOf(ref t);

			ReadProcessMemory(proc, baseAddr.Address, ptr.Address, size);

			return t;
		}

		public static void WriteProcessMemory<T>(Process proc, Pointer<byte> baseAddr, T value)
		{
			int dwSize = Unsafe.SizeOf<T>();
			var ptr    = AddressOf(ref value);

			WriteProcessMemory(proc, baseAddr.Address, ptr.Address, dwSize);
		}

		public static byte[] ReadProcessMemory(Process proc, Pointer<byte> ptrBase, int cb)
		{
			var mem = new byte[cb];

			fixed (byte* p = mem) {
				ReadProcessMemory(proc, ptrBase, p, cb);
			}

			return mem;
		}

		/// <summary>
		///     Root abstraction of <see cref="Native.WriteProcessMemory" />
		/// </summary>
		public static void WriteProcessMemory(Process proc, Pointer<byte> ptrBase, Pointer<byte> ptrBuffer, int dwSize)
		{
			var hProc = Native.OpenProcess(proc);


			bool ok = Native.WriteProcessMemory(hProc, ptrBase.Address, ptrBuffer.Address,
				dwSize, out int numberOfBytesWritten);


			Native.CloseHandle(hProc);
		}

		public static void WriteProcessMemory(Process proc, Pointer<byte> ptrBase, byte[] value)
		{
			fixed (byte* rg = value) {
				WriteProcessMemory(proc, ptrBase, (IntPtr) rg, value.Length);
			}
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

		public static bool IsAddressInRange(Pointer<byte> p, Pointer<byte> lo, long size)
		{
			return p >= lo && p <= lo + size;
		}


		/// <summary>
		///     Calculates the total byte size of <paramref name="elemCnt" /> elements with
		///     the size of <paramref name="elemSize" />.
		/// </summary>
		/// <param name="elemSize">Byte size of one element</param>
		/// <param name="elemCnt">Number of elements</param>
		/// <returns>Total byte size of all elements</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int FlatSize(int elemSize, int elemCnt)
		{
			// (void*) (((long) m_value) + byteOffset)
			// (void*) (((long) m_value) + (elemOffset * ElementSize))
			return elemCnt * elemSize;
		}


		public static int SizeOf<T>()
		{
			return Unsafe.SizeOf<T>();
		}

		public static int SizeOf<T>(SizeOfOptions options)
		{
			MetaType mt = typeof(T);

			// Note: Arrays native size == 0
			// Note: Arrays have no layout

			return options switch
			{
				SizeOfOptions.Native       => mt.NativeSize,
				SizeOfOptions.Managed      => mt.HasLayout ? mt.LayoutInfo.ManagedSize : Native.INVALID,
				SizeOfOptions.Intrinsic    => SizeOf<T>(),
				SizeOfOptions.BaseFields   => mt.InstanceFieldsSize,
				SizeOfOptions.BaseInstance => mt.BaseSize,
				_                          => Native.INVALID
			};

		}

		public static int SizeOf<T>(T value, SizeOfOptions options)
		{
			if (options == SizeOfOptions.Auto) {
				// Break into the next switch branch which will go to resolved case
				options = Inspector.IsStruct(value) ? SizeOfOptions.Intrinsic : SizeOfOptions.Heap;
			}

			Guard.Assert<ArgumentException>(!Inspector.IsNil(value), nameof(value));

			// Value is given

			var mt = new MetaType(value.GetType());

			return options switch
			{
				SizeOfOptions.BaseFields   => mt.InstanceFieldsSize,
				SizeOfOptions.BaseInstance => mt.BaseSize,
				SizeOfOptions.Heap         => HeapSizeOfInternal(value),
				SizeOfOptions.Data         => DataSizeOf<T>(value),
				SizeOfOptions.BaseData     => BaseDataSizeOf(mt.RuntimeType),
				_                          => Native.INVALID
			};

		}

		private static int BaseDataSizeOf(Type type)
		{
			var mtx = (MetaType) type;

			if (mtx.RuntimeType.IsValueType) {
				return (int) ReflectionHelper.CallGeneric(typeof(Mem).GetMethod(nameof(SizeOf)), type, null);
			}

			// Subtract the size of the ObjHeader and MethodTable*
			return mtx.InstanceFieldsSize;
		}

		private static int DataSizeOf<T>(T data)
		{
			if (Inspector.IsStruct(data)) {
				return SizeOf<T>();
			}

			// Subtract the size of the ObjHeader and MethodTable*
			return HeapSizeOfInternal(data) - RuntimeInfo.ObjectBaseSize;
		}

		/*public static int SizeOf<T>()
		{
			// todo

			return Unsafe.SizeOf<T>();
		}

		public static int SizeOf2<T>(SizeOfOptions options = SizeOfOptions.Auto) => SizeOf2<T>(default, options);

		public static int SizeOf2<T>(T value, SizeOfOptions options = SizeOfOptions.Intrinsic)
		{
			MetaType mt = typeof(T);

			if (options == SizeOfOptions.Auto) {
				// Break into the next switch branch which will go to resolved case
				options = Inspector.IsStruct(value) ? SizeOfOptions.Intrinsic : SizeOfOptions.Heap;
			}

			// If a value was supplied
			if (!Inspector.IsNil(value)) {
				mt = new MetaType(value.GetType());

				switch (options) {
					case SizeOfOptions.BaseFields:   return mt.InstanceFieldsSize;
					case SizeOfOptions.BaseInstance: return mt.BaseSize;
					case SizeOfOptions.Heap:         return HeapSizeOfInternal(value);
					case SizeOfOptions.Data:         return SizeOfData(value);
					case SizeOfOptions.BaseData:     return BaseSizeOfData(mt.RuntimeType);
				}
			}

			// Note: Arrays native size == 0
			// Note: Arrays have no layout

			switch (options) {
				case SizeOfOptions.Native:
					return mt.NativeSize;

				case SizeOfOptions.Managed:
					return mt.HasLayout ? mt.LayoutInfo.ManagedSize : Native.INVALID;

				case SizeOfOptions.Intrinsic:
					return SizeOf<T>();
				case SizeOfOptions.BaseFields:
					return mt.InstanceFieldsSize;

				case SizeOfOptions.BaseInstance:
					Guard.Assert(typeof(T) != typeof(string));
					return mt.BaseSize;

				case SizeOfOptions.Heap:
				case SizeOfOptions.Data:
					throw new ArgumentException($"A value must be supplied to use {options}");

				case SizeOfOptions.Auto:
				case SizeOfOptions.BaseData:
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(options), options, null);
			}

			static int SizeOfData(T data)
			{
				if (Inspector.IsStruct(data)) {
					return SizeOf<T>();
				}

				// Subtract the size of the ObjHeader and MethodTable*
				return HeapSizeOfInternal(data) - RuntimeInfo.ObjectBaseSize;
			}

			static int BaseSizeOfData(Type type)
			{
				var mtx = (MetaType) type;

				if (mtx.RuntimeType.IsValueType) {
					return (int) ReflectionHelper.CallGeneric(typeof(Mem).GetMethod(nameof(SizeOf)),
						type, null);
				}

				// Subtract the size of the ObjHeader and MethodTable*
				return mtx.InstanceFieldsSize;
			}


			return Native.INVALID;
		}*/

		/// <summary>
		///     <para>Returns the address of <paramref name="value" />.</para>
		/// </summary>
		/// <param name="value">Value to return the address of.</param>
		/// <returns>The address of the type in memory.</returns>
		public static Pointer<T> AddressOf<T>(ref T value)
		{
			/*var tr = __makeref(t);
			return *(IntPtr*) (&tr);*/

			return Unsafe.AsPointer(ref value);
		}

		/// <summary>
		///     <para>Calculates the complete size of a reference type in heap memory.</para>
		///     <para>This is the most accurate size calculation.</para>
		///     <para>
		///         This follows the size formula of: (<see cref="MethodTable.BaseSize" />) + (length) *
		///         (<see cref="MethodTable.ComponentSize" />)
		///     </para>
		///     <para>where:</para>
		///     <list type="bullet">
		///         <item>
		///             <description>
		///                 <see cref="MethodTable.BaseSize" /> = The base instance size of a type
		///                 (<c>24</c> (x64) or <c>12</c> (x86) by default) (<see cref="RuntimeInfo.MinObjectSize" />)
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
		///         Equals <see cref="SizeOf{T}" /> with <see cref="SizeOfOptions.BaseInstance" /> for objects
		///         that aren't arrays or strings.
		///     </para>
		///     <para>Note: This also includes padding and overhead (<see cref="ObjHeader" /> and <see cref="MethodTable" /> ptr.)</para>
		/// </remarks>
		/// <returns>The size of the type in heap memory, in bytes</returns>
		public static int HeapSizeOf<T>(T value) where T : class
			=> HeapSizeOfInternal(value);


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int HeapSizeOfInternal<T>(T value)
		{
			// Sanity check
			Guard.Assert(!Inspector.IsStruct(value));

			if (Inspector.IsNil(value)) {
				return Native.INVALID;
			}

			// By manually reading the MethodTable*, we can calculate the size correctly if the reference
			// is boxed or cloaked
			var methodTable = RuntimeInfo.ReadTypeHandle(value);

			// Value of GetSizeField()
			int length = 0;

			// Type			x86 size				x64 size
			// 
			// object		12						24
			// object[]		16 + length * 4			32 + length * 8
			// int[]		12 + length * 4			28 + length * 4
			// byte[]		12 + length				24 + length
			// string		14 + length * 2			26 + length * 2

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

			if (Inspector.IsArray(value)) {
				var arr = value as Array;
				Guard.AssertNotNull(arr, nameof(arr));


				// We already know it's not null because the type is an array.
				length = arr.Length;

				// Sanity check
				Guard.Assert(!Inspector.IsString(value));
			}
			else if (Inspector.IsString(value)) {
				string str = value as string;

				// Sanity check
				Guard.Assert(!Inspector.IsArray(value));
				Guard.AssertNotNull(str, nameof(str));

				length = str.Length;
			}

			return methodTable.Reference.BaseSize + length * methodTable.Reference.ComponentSize;
		}


		public static bool TryGetAddressOfHeap<T>(T value, OffsetOptions options, out Pointer<byte> ptr)
		{
			if (Inspector.IsStruct(value)) {
				ptr = null;
				return false;
			}

			ptr = AddressOfHeapInternal(value, options);
			return true;
		}

		public static bool TryGetAddressOfHeap<T>(T value, out Pointer<byte> ptr)
		{
			return TryGetAddressOfHeap(value, OffsetOptions.None, out ptr);
		}

		/// <summary>
		///     Returns the address of reference type <paramref name="value" />'s heap memory, offset by the specified
		///     <see cref="OffsetOptions" />.
		///     <remarks>
		///         <para>
		///             Note: This does not pin the reference in memory if it is a reference type.
		///             This may require pinning to prevent the GC from moving the object.
		///             If the GC compacts the heap, this pointer may become invalid.
		///         </para>
		///     </remarks>
		/// </summary>
		/// <param name="value">Reference type to return the heap address of</param>
		/// <param name="offset">Offset type</param>
		/// <returns>The address of <paramref name="value" /></returns>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="offset"></paramref> is out of range.</exception>
		public static Pointer<byte> AddressOfHeap<T>(T value, OffsetOptions offset = OffsetOptions.None) where T : class
			=> AddressOfHeapInternal(value, offset);

		private static Pointer<byte> AddressOfHeapInternal<T>(T value, OffsetOptions offset)
		{
			// It is already assumed value is a class type

			//var tr = __makeref(value);
			//var heapPtr = **(IntPtr**) (&tr);

			var heapPtr = AddressOf(ref value).ReadPointer();


			// NOTE:
			// Strings have their data offset by Offsets.OffsetToStringData
			// Arrays have their data offset by IntPtr.Size * 2 bytes (may be different for 32 bit)

			int offsetValue = 0;

			switch (offset) {
				case OffsetOptions.StringData:
					Trace.Assert(Inspector.IsString(value));
					offsetValue = RuntimeInfo.OffsetToStringData;
					break;

				case OffsetOptions.ArrayData:
					Trace.Assert(Inspector.IsArray(value));
					offsetValue = RuntimeInfo.OffsetToArrayData;
					break;

				case OffsetOptions.Fields:
					offsetValue = RuntimeInfo.OffsetToData;
					break;

				case OffsetOptions.None:
					break;

				case OffsetOptions.Header:
					offsetValue = -RuntimeInfo.OffsetToData;
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(offset), offset, null);
			}

			return heapPtr + offsetValue;
		}


		/// <summary>
		///     Finds a <see cref="ProcessModule" /> in the current process with the <see cref="ProcessModule.ModuleName" /> of
		///     <paramref name="moduleName" />
		/// </summary>
		/// <param name="moduleName">
		///     <see cref="ProcessModule.ModuleName" />
		/// </param>
		/// <returns>The found <see cref="ProcessModule" />; <c>null</c> otherwise</returns>
		[CanBeNull]
		public static ProcessModule FindModule(string moduleName)
		{
			var modules = Process.GetCurrentProcess().Modules;

			foreach (ProcessModule module in modules) {
				if (module != null) {
					if (module.ModuleName == moduleName) {
						return module;
					}
				}

			}

			return null;
		}

		public static byte[] Copy(Pointer<byte> p, int o, int cb) => p.Copy(o, cb);

		public static byte[] Copy(Pointer<byte> p, int cb) => p.Copy(cb);

		/// <summary>
		/// Reads a <see cref="byte"/> array as a <see cref="string"/> delimited by spaces in
		/// hex number format
		/// </summary>
		public static byte[] ReadBinaryString(string s)
		{
			var rg = new List<byte>();

			var bytes = s.Split(Formatting.SPACE);

			foreach (string b in bytes) {
				byte n = Byte.Parse(b, NumberStyles.HexNumber);

				rg.Add(n);
			}

			return rg.ToArray();
		}

		/// <summary>
		///     Reads <paramref name="bitCount" /> from <paramref name="value" /> at offset <paramref name="bitOfs" />
		/// </summary>
		/// <param name="value"><see cref="int" /> value to read from</param>
		/// <param name="bitOfs">Beginning offset</param>
		/// <param name="bitCount">Number of bits to read</param>
		public static int ReadBits(int value, int bitOfs, int bitCount) => ((1 << bitCount) - 1) & (value >> bitOfs);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadBit(int value, int bitOfs) => (value & (1 << bitOfs)) != 0;

		public static int GetBitMask(int index, int size) => ((1 << size) - 1) << index;


		public static int ReadBitsFrom(int data, int index, int size) => (data & GetBitMask(index, size)) >> index;

		public static int WriteBitsTo(int data, int index, int size, int value)
			=> (data & ~GetBitMask(index, size)) | (value << index);

		public static string ReadCString(this BinaryReader br, int count)
		{
			var s = Encoding.ASCII.GetString(br.ReadBytes(count)).TrimEnd('\0');


			return s;
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
		///     pointer offset by <see cref="RuntimeInfo.OffsetToStringData" /> so it
		///     points to the string's characters.
		///     <remarks>
		///         Note: Equal to <see cref="GCHandle.AddrOfPinnedObject" /> and <c>fixed</c>.
		///     </remarks>
		/// </summary>
		StringData,

		/// <summary>
		///     If the type is an array, return
		///     the pointer offset by <see cref="RuntimeInfo.OffsetToArrayData" /> so it points
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
		///     points to the <see cref="TypeHandle"/>
		/// </summary>
		None
	}
}