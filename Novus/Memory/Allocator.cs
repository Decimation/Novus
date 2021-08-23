using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Novus.Runtime.Meta;
using Novus.Utilities;
using Novus.Win32;
using Kantan.Diagnostics;
// ReSharper disable CommentTypo

// ReSharper disable UnusedMember.Global

namespace Novus.Memory
{
	/// <summary>
	/// Memory allocation manager.
	/// </summary>
	public static class Allocator
	{
		private static readonly List<Pointer<byte>> Allocated = new();

		public static int AllocCount => Allocated.Count;


		public static void Close()
		{
			foreach (var pointer in Allocated) {
				FreeInternal(pointer);
			}
		}

		public static bool IsAllocated(Pointer<byte> ptr) => Allocated.Contains(ptr);

		public static int GetAllocSize(Pointer<byte> ptr)
		{
			if (!IsAllocated(ptr)) {
				return Native.INVALID;
			}

			return (int) Native.LocalSize(ptr.Address);
		}

		[MustUseReturnValue]
		public static Pointer<T> ReAlloc<T>(Pointer<T> ptr, int elemCnt)
		{
			if (!IsAllocated(ptr)) {
				return null;
			}

			Guard.AssertPositive(elemCnt);

			Allocated.Remove(ptr);

			int elemSize = Mem.SizeOf<T>();
			int cb       = Mem.FlatSize(elemSize, elemCnt);

			ptr = Marshal.ReAllocHGlobal(ptr.Address, (IntPtr) cb);

			Allocated.Add(ptr);

			return ptr;
		}

		private static void FreeInternal(Pointer<byte> ptr)
		{
			Marshal.FreeHGlobal(ptr.Address);
			Allocated.Remove(ptr);
		}


		public static void Free(Pointer<byte> ptr)
		{
			if (!IsAllocated(ptr)) {
				return;
			}

			FreeInternal(ptr);
		}

		/// <summary>
		/// Allocates memory for <paramref name="cb"/> elements of type <see cref="byte"/>.
		/// </summary>
		/// <param name="cb">Number of bytes</param>
		[MustUseReturnValue]
		public static Pointer<byte> Alloc(int cb) => Alloc<byte>(cb);

		/// <summary>
		/// Allocates memory for <paramref name="elemCnt"></paramref> elements of type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Element type</typeparam>
		/// <param name="elemCnt">Number of elements</param>
		[MustUseReturnValue]
		public static Pointer<T> Alloc<T>(int elemCnt)
		{
			Guard.AssertPositive(elemCnt);

			int elemSize = Mem.SizeOf<T>();
			int cb       = Mem.FlatSize(elemSize, elemCnt);

			Pointer<T> h = Marshal.AllocHGlobal(cb);
			h.Clear(elemCnt);

			Allocated.Add(h);

			return h;
		}

		[MustUseReturnValue]
		public static T AllocU<T>(params object[] args)
		{
			// NOTE: WIP

			var mt = typeof(T).AsMetaType();

			var alloc = Alloc(mt.BaseSize);

			alloc += Mem.Size;

			alloc.WritePointer(mt.Value);

			var alloc2 = Alloc<T>(1);

			alloc2.WritePointer(alloc);


			var val = alloc2.Value;

			//RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);
			ReflectionHelper.CallConstructor(val, args);


			/*var def = Activator.CreateInstance<T>();

			var flds = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

			var vals = flds.Select(f => f.GetValue(def)).ToArray();

			var val2 = FormatterServices.PopulateObjectMembers(val, flds, vals);*/

			//FormatterServices?

			//return (T) val2;

			return val;
		}
	}
}