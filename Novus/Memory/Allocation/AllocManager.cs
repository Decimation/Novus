using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Kantan.Diagnostics;
using Novus.Runtime;
using Novus.Runtime.Meta;
using Novus.Runtime.VM;
using Novus.Utilities;

// ReSharper disable CommentTypo

// ReSharper disable UnusedMember.Global

namespace Novus.Memory.Allocation;

/// <summary>
/// Wraps an <see cref="IAllocator"/>
/// </summary>
public static class AllocManager
{

	/*
	 * https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Runtime/InteropServices/NativeMemory.Windows.cs
	 * https://github.com/dotnet/runtime/blob/main/src/libraries/Common/src/Interop/Windows/Ucrtbase/Interop.MemAlloc.cs
	 * https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Runtime/InteropServices/NativeMemory.cs
	 */

	public static IAllocator Allocator { get; set; } = new NativeAllocator();

	private static List<Pointer<byte>> Allocated { get; } = [];

	public static int AllocCount => Allocated.Count;

	public static bool IsAllocated(Pointer<byte> ptr)
	{
		return Allocated.Contains(ptr);

	}

	public static nuint GetSize(Pointer<byte> ptr)
	{
		/*if (!IsAllocated(ptr)) {
			return Native.INVALID2;
		}*/

		return (nuint) Allocator.GetSize(ptr);
	}

	[MURV]
	public static Pointer<T> ReAlloc<T>(Pointer<T> ptr, nuint elemCnt)
	{
		if (!IsAllocated(ptr)) {
			return null;
		}

		Allocated.Remove(ptr);

		nuint elemSize = (nuint) Mem.SizeOf<T>();
		int   cb       = (int) Mem.GetByteCount(elemSize, elemCnt);

		ptr = (Pointer<T>) Allocator.ReAlloc(ptr.Address, (nuint) cb);

		Allocated.Add(ptr);

		return ptr;
	}

	private static void FreeInternal(Pointer<byte> ptr)
	{
		Allocator.Free(ptr.Address);
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
	[MURV]
	public static Pointer<byte> Alloc(nuint cb)
		=> Alloc<byte>(cb);

	/// <summary>
	/// Allocates memory for <paramref name="elemCnt"></paramref> elements of type <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">Element type</typeparam>
	/// <param name="elemCnt">Number of elements</param>
	[MURV]
	public static Pointer<T> Alloc<T>(nuint elemCnt)
	{
		var elemSize = (nuint) Mem.SizeOf<T>();
		var cb       = (nuint) Mem.GetByteCount(elemSize, elemCnt);

		var h = Allocator.Alloc(cb).Cast<T>();
		h.Clear((int) elemCnt);

		Allocated.Add(h);

		return h;
	}

	public static void Free<T>(T t) where T : class
	{
		var ptr = Mem.AddressOfHeap(t);
		ptr -= ObjectUtility.ObjHeaderSize;
		Free(ptr);
	}


	public static void Close()
	{
		for (int i = Allocated.Count - 1; i >= 0; i--) {
			Pointer<byte> pointer = Allocated[i];
			FreeInternal(pointer);
		}
	}

}