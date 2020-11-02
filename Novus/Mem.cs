using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Novus.Win32;


// ReSharper disable ConvertIfStatementToReturnStatement

// ReSharper disable UnusedVariable

// ReSharper disable UnusedMember.Global

#nullable enable
#pragma warning disable IDE0059

namespace Novus
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
	public static unsafe class Mem
	{
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
			// todo

			return Unsafe.SizeOf<T>();
		}

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
		///     Returns the address of reference type <paramref name="value" /> in heap memory
		///     <remarks>
		///         <para>
		///             Note: This does not pin the reference in memory if it is a reference type.
		///             This may require pinning to prevent the GC from moving the object.
		///             If the GC compacts the heap, this pointer may become invalid.
		///         </para>
		///     </remarks>
		/// </summary>
		/// <param name="value">Reference type to return the heap address of</param>
		/// <returns>The heap address of <paramref name="value" /></returns>
		public static Pointer<byte> AddressOfHeap<T>(T value) where T : class
		{
			TryGetAddressOfHeap(value, out var ptr);

			return ptr;
		}

		public static bool TryGetAddressOfHeap<T>(T value, out Pointer<byte> ptr)
		{
			if (!value.GetType().IsValueType) {
				ptr = AddressOfHeapInternal(value);
				return true;
			}

			ptr = Nullptr;
			return false;
		}

		private static Pointer<byte> AddressOfHeapInternal<T>(T value)
		{
			// It is already assumed value is a class type

			//var tr = __makeref(value);
			//var heapPtr = **(IntPtr**) (&tr);

			var heapPtr = AddressOf(ref value).ReadPointer();

			return heapPtr;
		}


		/// <summary>
		///     Finds a <see cref="ProcessModule" /> in the current process with the <see cref="ProcessModule.ModuleName" /> of
		///     <paramref name="moduleName" />
		/// </summary>
		/// <param name="moduleName">
		///     <see cref="ProcessModule.ModuleName" />
		/// </param>
		/// <returns>The found <see cref="ProcessModule" />; <c>null</c> otherwise</returns>
		public static ProcessModule? FindModule(string moduleName)
		{
			var modules = Process.GetCurrentProcess().Modules;

			foreach (ProcessModule? module in modules) {
				if (module?.ModuleName == moduleName) {
					return module;
				}
			}

			return null;
		}

		public static byte[] Copy(Pointer<byte> p, int o, int cb) => p.Copy(o, cb);

		public static byte[] Copy(Pointer<byte> p, int cb) => p.Copy(cb);

		/// <summary>
		/// SI
		/// </summary>
		public const double MAGNITUDE = 1000D;

		/// <summary>
		/// ISO/IEC 80000
		/// </summary>
		public const double MAGNITUDE2 = 1024D;

		/// <summary>
		/// Convert the given bytes to <see cref="MetricUnit"/>
		/// </summary>
		/// <param name="bytes">Value in bytes to be converted</param>
		/// <param name="type">Unit to convert to</param>
		/// <returns>Converted bytes</returns>
		public static double ConvertToUnit(double bytes, MetricUnit type)
		{
			// var rg  = new[] { "k","M","G","T","P","E","Z","Y"};
			// var pow = rg.ToList().IndexOf(type) +1;

			int pow = (int) type;
			return bytes / Math.Pow(MAGNITUDE, pow);
		}
	}
}