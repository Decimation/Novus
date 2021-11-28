using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Kantan.Diagnostics;
using Novus.Runtime.Meta;
using Novus.Utilities;
using Novus.Win32;

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

	private static readonly List<Pointer> Allocated = new();

	public static int AllocCount => Allocated.Count;

	public static void Close()
	{
		foreach (var pointer in Allocated) {
			FreeInternal(pointer);
		}
	}

	public static bool IsAllocated(Pointer ptr) => Allocated.Contains(ptr);

	public static nuint AllocSize(Pointer ptr)
	{
		if (!IsAllocated(ptr)) {
			return Native.INVALID2;
		}

		return (nuint) Allocator.AllocSize(ptr);
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
		int cb       = (int) Mem.GetByteCount(elemSize, elemCnt);

		ptr = Allocator.ReAlloc(ptr.Address, (nuint) cb);

		Allocated.Add(ptr);

		return ptr;
	}

	private static void FreeInternal(Pointer ptr)
	{
		Allocator.Free(ptr.Address);
		Allocated.Remove(ptr);
	}


	public static void Free(Pointer ptr)
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
	public static Pointer Alloc(int cb) => Alloc<byte>(cb);

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
		var cb       = (nuint) Mem.GetByteCount(elemSize, elemCnt);

		Pointer<T> h = Allocator.Alloc(cb);
		h.Clear(elemCnt);

		Allocated.Add(h);

		return h;
	}

	/*[MustUseReturnValue]
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

		var val2 = FormatterServices.PopulateObjectMembers(val, flds, vals);#1#

		//FormatterServices?

		//return (T) val2;

		return val;
	}*/
}