using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using JetBrains.Annotations;
using Novus.Runtime;
using Novus.Runtime.Meta;
using Novus.Runtime.VM;
using Novus.Utilities;
using Novus.Win32;
using Novus.Win32.Structures;
using SimpleCore.Diagnostics;
using SimpleCore.Utilities;
using System.Linq.Expressions;
using BE = System.Linq.Expressions.BinaryExpression;
using PE = System.Linq.Expressions.ParameterExpression;

// ReSharper disable ConvertIfToOrExpression
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable SwitchStatementMissingSomeEnumCasesNoDefault
// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault
// ReSharper disable ConvertIfStatementToReturnStatement
// ReSharper disable UnusedVariable
// ReSharper disable UnusedMember.Global

#pragma warning disable IDE0059

namespace Novus.Memory
{
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
	/// <seealso cref="Allocator" />
	/// <seealso cref="Unsafe" />
	/// <seealso cref="RuntimeHelpers" />
	/// <seealso cref="RuntimeEnvironment" />
	/// <seealso cref="RuntimeInformation" />
	/// <seealso cref="FormatterServices"/>
	/// <seealso cref="Activator"/>
	/// <seealso cref="GCHeap"/>
	/// <seealso cref="RuntimeInfo" />
	/// <seealso cref="Inspector" />
	/// <seealso cref="Native" />
	/// <seealso cref="PEReader" />
	/// <seealso cref="GCHandle"/>
	/// <seealso cref="System.Runtime.CompilerServices" />
	public static unsafe class Mem
	{
		/// <summary>
		///     Address size
		/// </summary>
		public static readonly int Size = sizeof(nint);

		/// <summary>
		///     Represents a <c>null</c> <see cref="Pointer{T}" />
		/// </summary>
		public static readonly Pointer<byte> Nullptr = null;

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
		public static int OffsetOf(MetaType t, string name)
		{
			var f = t.GetField(name);

			return f.Offset;
		}

		public static int OffsetOf(object t, string name) => OffsetOf(t.GetType(), name);

		public static Pointer<byte> OffsetField<TField>(TField* field, int offset) where TField : unmanaged
		{
			// Alias: PTR_HOST_MEMBER_TADDR

			// m_methodTable.GetValue(PTR_HOST_MEMBER_TADDR(MethodDescChunk, this, m_methodTable));

			//const int MT_FIELD_OFS = 0;
			//return (MethodTable*) (MT_FIELD_OFS + ((long) MethodTableRaw));

			// // Construct a pointer to a member of the given type.
			// #define PTR_HOST_MEMBER_TADDR(type, host, memb) \
			//     (PTR_HOST_TO_TADDR(host) + (TADDR)offsetof(type, memb))

			return (Pointer<byte>) (offset + (long) field);
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

			return modules.Cast<ProcessModule>().Where(module => module != null)
			              .FirstOrDefault(module => module.ModuleName   == moduleName);

		}

		/// <summary>
		///     Forcefully kills a <see cref="Process" /> and ensures the process has exited.
		/// </summary>
		/// <param name="p"><see cref="Process" /> to forcefully kill.</param>
		/// <returns><c>true</c> if <paramref name="p" /> was killed; <c>false</c> otherwise</returns>
		public static bool ForceKill(this Process p)
		{
			p.WaitForExit();
			p.Dispose();

			try {
				if (!p.HasExited) {
					p.Kill();
				}

				return true;
			}
			catch (Exception) {

				return false;
			}
		}

		#region Read/write

		#region Write

		/// <summary>
		///     Writes a value of type <typeparamref name="T" /> with value <paramref name="value" /> to
		///     <paramref name="baseAddr" /> in <paramref name="proc" />
		/// </summary>
		public static void WriteProcessMemory<T>(Process proc, Pointer<byte> baseAddr, T value)
		{
			int dwSize = Unsafe.SizeOf<T>();
			var ptr    = AddressOf(ref value);

			WriteProcessMemory(proc, baseAddr.Address, ptr.Address, dwSize);
		}

		/// <summary>
		///     Root abstraction of <see cref="Native.WriteProcessMemory" />
		/// </summary>
		public static void WriteProcessMemory(Process proc, Pointer<byte> addr, Pointer<byte> ptrBuffer, int dwSize)
		{
			var hProc = Native.OpenProcess(proc);


			Native.WriteProcessMemory(hProc, addr.Address, ptrBuffer.Address, dwSize, out _);


			Native.CloseHandle(hProc);
		}

		/// <summary>
		///     Writes <paramref name="value" /> bytes to <paramref name="addr" /> in <paramref name="proc" />
		/// </summary>
		public static void WriteProcessMemory(Process proc, Pointer<byte> addr, byte[] value)
		{
			fixed (byte* rg = value) {
				WriteProcessMemory(proc, addr, (IntPtr) rg, value.Length);
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
		public static void ReadProcessMemory(Process proc, Pointer<byte> addr, Pointer<byte> buffer, int cb)
		{
			var h = Native.OpenProcess(proc);

			Native.ReadProcessMemory(h, addr.Address, buffer.Address, cb, out _);

			Native.CloseHandle(h);
		}


		/// <summary>
		///     Reads <paramref name="cb" /> bytes at <paramref name="addr" /> in <paramref name="proc" />
		/// </summary>
		public static byte[] ReadProcessMemory(Process proc, Pointer<byte> addr, int cb)
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
		public static T ReadProcessMemory<T>(Process proc, Pointer<byte> addr)
		{
			T value = default!;

			int size = Unsafe.SizeOf<T>();

			var ptr = AddressOf(ref value);

			ReadProcessMemory(proc, addr.Address, ptr.Address, size);

			return value;
		}

		/// <summary>
		///     Reads a value of type <paramref name="mt" /> in <paramref name="proc" /> at <paramref name="addr" />
		/// </summary>
		[CanBeNull]
		public static object ReadProcessMemory(Process proc, Pointer<byte> addr, MetaType mt)
		{
			//todo


			bool valueType = mt.RuntimeType.IsValueType;
			int  size      = valueType ? mt.InstanceFieldsSize : mt.BaseSize;

			Debug.WriteLine($"{size} for {mt.Name}");

			//var i = Activator.CreateInstance(t);

			byte[] rg  = ReadProcessMemory(proc, addr, size);
			object val = null;

			if (valueType) {
				val = ReadStructure(mt.RuntimeType, rg);
			}
			else {
				fixed (byte* ptr = rg) {
					val = Unsafe.Read<object>(ptr);
				}
			}

			return val;
		}

		#endregion

#if EXPERIMENTAL
		public static bool IsBadReadPointer(Pointer<byte> p)
		{
			//todo

			/*
			 * https://stackoverflow.com/questions/496034/most-efficient-replacement-for-isbadreadptr
			 * https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-isbadreadptr
			 */

			MemoryBasicInformation mbi = default;

			if (Native.VirtualQuery(p.Address, ref mbi, Marshal.SizeOf<MemoryBasicInformation>()) != 0) {

				//DWORD mask = (PAGE_READONLY|PAGE_READWRITE|PAGE_WRITECOPY|PAGE_EXECUTE_READ|PAGE_EXECUTE_READWRITE|PAGE_EXECUTE_WRITECOPY);

				const MemoryProtection mask = (MemoryProtection.ReadOnly         | MemoryProtection.ReadWrite   |
				                               MemoryProtection.WriteCopy        | MemoryProtection.ExecuteRead |
				                               MemoryProtection.ExecuteReadWrite | MemoryProtection.ExecuteWriteCopy);


				var b = !Convert.ToBoolean((mbi.Protect & mask));

				// check the page is not a guard page
				if (Convert.ToBoolean(mbi.Protect & (MemoryProtection.GuardModifierFlag | MemoryProtection.NoAccess)))
					b = true;

				return b;
			}

			return true;

		}
#endif


		public static object ReadStructure(MetaType t, byte[] rg, int ofs = 0)
		{
			//todo
			var handle = GCHandle.Alloc(rg, GCHandleType.Pinned);
			//var stackAlloc = stackalloc byte[byteArray.Length];

			var    objAddr = handle.AddrOfPinnedObject() + ofs;
			object value   = Marshal.PtrToStructure(objAddr, t.RuntimeType);

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

			string[] bytes = s.Split(StringConstants.SPACE);

			foreach (string b in bytes) {
				byte n = Byte.Parse(b, NumberStyles.HexNumber);

				rg.Add(n);
			}

			return rg.ToArray();
		}

		public static byte[] Copy(Pointer<byte> p, int startIndex, int cb) => p.Copy(startIndex, cb);

		public static byte[] Copy(Pointer<byte> p, int cb) => p.Copy(cb);


		public static ref T InToRef<T>(in T t) => ref Unsafe.AsRef(in t);

		#endregion

		#region Size

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
			//todo: this is over-engineering
			// (void*) (((long) m_value) + byteOffset)
			// (void*) (((long) m_value) + (elemOffset * ElementSize))
			return elemCnt * elemSize;
		}

		/// <summary>
		///     Calculates the size of <typeparamref name="T" />
		/// </summary>
		public static int SizeOf<T>() => Unsafe.SizeOf<T>();

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
				case SizeOfOptions.Heap:         return HeapSizeOfInternal(value);
				case SizeOfOptions.Data:         return DataSizeOf(value);
				case SizeOfOptions.BaseData:     return BaseDataSizeOf(mt.RuntimeType);
				case SizeOfOptions.Auto:

					if (RuntimeInfo.IsStruct(value)) {
						return SizeOf<T>(SizeOfOptions.Intrinsic);
					}

					else goto case SizeOfOptions.Heap;

				default:
					return Native.INVALID;
			}

		}

		private static int BaseDataSizeOf(Type type)
		{
			var mtx = (MetaType) type;

			if (mtx.RuntimeType.IsValueType) {

				var m = typeof(Mem).GetRuntimeMethods().First(delegate(MethodInfo n)
				{
					var infos = n.GetParameters();

					return n.Name                    == nameof(SizeOf)
					       && infos.Length           == 2
					       && infos[1].ParameterType == typeof(SizeOfOptions);
				});

				return (int) ReflectionHelper.CallGeneric(m, type, null, null, SizeOfOptions.Intrinsic);
			}

			// Subtract the size of the ObjHeader and MethodTable*
			return mtx.InstanceFieldsSize;
		}

		private static int DataSizeOf<T>(T value)
		{
			if (RuntimeInfo.IsStruct(value)) {
				return SizeOf<T>();
			}

			// Subtract the size of the ObjHeader and MethodTable*
			return HeapSizeOfInternal(value) - RuntimeInfo.ObjectBaseSize;
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
		///                 (<see cref="RuntimeInfo.MinObjectSize" /> by default)
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
			Guard.Assert(!RuntimeInfo.IsStruct(value));

			// By manually reading the MethodTable*, we can calculate the size correctly if the reference
			// is boxed or cloaked
			var metaType = (MetaType) RuntimeInfo.ReadTypeHandle(value);

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

			return Unsafe.AsPointer(ref value);
		}

		public static bool TryGetAddressOfHeap<T>(T value, OffsetOptions options, out Pointer<byte> ptr)
		{
			if (RuntimeInfo.IsStruct(value)) {
				ptr = null;
				return false;
			}

			ptr = AddressOfHeapInternal(value, options);
			return true;
		}

		public static bool TryGetAddressOfHeap<T>(T value, out Pointer<byte> ptr) =>
			TryGetAddressOfHeap(value, OffsetOptions.None, out ptr);

		/// <summary>
		///     Returns the address of reference type <paramref name="value" />'s heap memory, offset by the specified
		///     <see cref="OffsetOptions" />.
		///     <remarks>
		///         <para>
		///             Note: This does not pin the reference in memory if it is a reference type.
		///             This may require pinning to prevent the GC from moving the object.
		///             If the GC compacts the heap, this pointer may become invalid.
		///         </para>
		///         <seealso cref="RuntimeInfo.GetPinningHelper" />
		///     </remarks>
		/// </summary>
		/// <param name="value">Reference type to return the heap address of</param>
		/// <param name="offset">Offset type</param>
		/// <returns>The address of <paramref name="value" /></returns>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="offset"></paramref> is out of range.</exception>
		public static Pointer<byte> AddressOfHeap<T>(T value, OffsetOptions offset = OffsetOptions.None)
			where T : class
		{
			return AddressOfHeapInternal(value, offset);
		}

		private static Pointer<byte> AddressOfHeapInternal<T>(T value, OffsetOptions offset)
		{
			// It is already assumed value is a class type

			//var tr = __makeref(value);
			//var heapPtr = **(IntPtr**) (&tr);

			var heapPtr = AddressOf(ref value).ReadPointer();


			// NOTE:
			// Strings have their data offset by RuntimeInfo.OffsetToStringData
			// Arrays have their data offset by IntPtr.Size * 2 bytes (may be different for 32 bit)

			int offsetValue = 0;

			switch (offset) {
				case OffsetOptions.StringData:
					Guard.Assert(RuntimeInfo.IsString(value));
					offsetValue = RuntimeInfo.OffsetToStringData;
					break;

				case OffsetOptions.ArrayData:
					Guard.Assert(RuntimeInfo.IsArray(value));
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
		///     Returns the address of the data of <paramref name="value" />. If <typeparamref name="T" /> is a value type,
		///     this will return <see cref="AddressOf{T}" />. If <typeparamref name="T" /> is a reference type,
		///     this will return the equivalent of <see cref="AddressOfHeap{T}(T, OffsetOptions)" /> with
		///     <see cref="OffsetOptions.Fields" />.
		/// </summary>
		public static Pointer<byte> AddressOfData<T>(ref T value)
		{
			var addr = AddressOf(ref value);

			if (RuntimeInfo.IsStruct(value)) {
				return addr.Cast();
			}

			return AddressOfHeapInternal(value, OffsetOptions.Fields);
		}

		public static Pointer<byte> AddressOfField(object obj, string name) => AddressOfField<byte>(obj, name);

		public static Pointer<T> AddressOfField<T>(object obj, string name)
		{

			var f = OffsetOf(obj, name);

			var p = AddressOfHeap(obj, OffsetOptions.Fields);

			return p + f;
		}

		public static ref T ReferenceOfField<T>(object obj, string name)
		{
			return ref AddressOfField<T>(obj, name).Reference;
		}

		public static ref TField ReferenceOfField<T, TField>(in T obj, string name)
		{

			var f = OffsetOf(obj, name);

			var p = AddressOfData(ref InToRef(in obj));

			return ref (p.Cast<TField>() + f).Reference;
		}

		#endregion


		#region Virtual

		public static Pointer<byte> VirtualAlloc(Process proc, Pointer<byte> lpAddr, int dwSize,
		                                         AllocationType type, MemoryProtection mp)
		{
			var ptr = Native.VirtualAllocEx(proc.Handle, lpAddr.Address, (uint) dwSize, type, mp);

			return ptr;
		}

		public static bool VirtualFree(Process hProcess, Pointer<byte> lpAddress, int dwSize, AllocationType dwFreeType)
		{
			bool p = Native.VirtualFreeEx(hProcess.Handle, lpAddress.Address, dwSize, dwFreeType);

			return p;
		}

		public static bool VirtualProtect(Process hProcess, Pointer<byte> lpAddress, int dwSize,
		                                  MemoryProtection flNewProtect, out MemoryProtection lpflOldProtect)
		{
			bool p = Native.VirtualProtectEx(hProcess.Handle, lpAddress.Address, (uint) dwSize, flNewProtect,
			                                 out lpflOldProtect);

			return p;
		}

		public static MemoryBasicInformation VirtualQuery(Process proc, Pointer<byte> lpAddr)
		{
			var mbi = new MemoryBasicInformation();

			int v = Native.VirtualQueryEx(proc.Handle, lpAddr.Address, ref mbi,
			                              (uint) Marshal.SizeOf<MemoryBasicInformation>());

			return mbi;
		}

		#endregion

		#region Bits

		/*
		 * https://github.com/pkrumins/bithacks.h/blob/master/bithacks.h
		 * https://catonmat.net/low-level-bit-hacks
		 */


		//public static int ReadBits(int value, int bitOfs, int bitCount) => ((1 << bitCount) - 1) & (value >> bitOfs);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadBit(int value, int bitOfs) => (value & (1 << bitOfs)) != 0;

		public static int SetBit(int x, int n) => x | (1 << n);

		public static int UnsetBit(int x, int n) => x & ~(1 << n);

		public static int ToggleBit(int x, int n) => x ^ (1 << n);

		public static int GetBitMask(int index, int size) => ((1 << size) - 1) << index;

		/// <summary>
		///     Reads <paramref name="bitCount" /> from <paramref name="value" /> at offset <paramref name="bitOfs" />
		/// </summary>
		/// <param name="value"><see cref="int" /> value to read from</param>
		/// <param name="bitOfs">Beginning offset</param>
		/// <param name="bitCount">Number of bits to read</param>
		/// <seealso cref="BitArray"/>
		/// <seealso cref="BitVector32"/>
		public static int ReadBits(int value, int bitOfs, int bitCount) =>
			(value & GetBitMask(bitOfs, bitCount)) >> bitOfs;

		public static int WriteBits(int data, int index, int size, int value) =>
			(data & ~GetBitMask(index, size)) | (value << index);

		#endregion
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
		///     points to the <see cref="TypeHandle" />
		/// </summary>
		None
	}
}