using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Novus.Win32;

// ReSharper disable UnusedMember.Global

namespace Novus.Memory
{
	public static class Allocator
	{
		private static readonly List<Pointer<byte>> AllocatedPointers = new List<Pointer<byte>>();

		public static bool IsAllocated(Pointer<byte> p)
		{
			return AllocatedPointers.Contains(p);
		}

		public static int GetAllocSize(Pointer<byte> ptr)
		{
			if (!IsAllocated(ptr)) {
				return NativeInterop.INVALID;
			}

			return (int) NativeInterop.LocalSize(ptr.Address);
		}

		public static Pointer<T> ReAlloc<T>(Pointer<T> ptr, int elemCnt)
		{
			if (!IsAllocated(ptr)) {
				return null;
			}

			AllocatedPointers.Remove(ptr);

			var elemSize = Mem.SizeOf<T>();
			var cb       = Mem.FlatSize(elemSize, elemCnt);

			ptr = Marshal.ReAllocHGlobal(ptr.Address, (IntPtr) cb);

			AllocatedPointers.Add(ptr);

			return ptr;
		}

		public static void Free(Pointer<byte> ptr)
		{
			if (!IsAllocated(ptr)) {
				return;
			}

			Marshal.FreeHGlobal(ptr.Address);
			AllocatedPointers.Remove(ptr);
		}

		public static Pointer<byte> Alloc(int cb) => Alloc<byte>(cb);

		public static Pointer<T> Alloc<T>(int elemCnt)
		{
			var elemSize = Mem.SizeOf<T>();
			var cb       = Mem.FlatSize(elemSize, elemCnt);

			Pointer<T> h = Marshal.AllocHGlobal(cb);
			h.Clear(elemCnt);

			AllocatedPointers.Add(h);


			return h;
		}
	}
}