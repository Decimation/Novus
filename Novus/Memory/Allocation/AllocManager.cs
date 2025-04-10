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

	private static List<Pointer> Allocated { get; } = new();

	public static int AllocCount => Allocated.Count;

	public static bool IsAllocated(Pointer ptr)
	{
		return Allocated.Contains(ptr);

		// return Allocator.IsAllocated(ptr);
	}

	public static nuint GetSize(Pointer ptr)
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
	[MURV]
	public static Pointer Alloc(nuint cb)
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

		Pointer<T> h = (Pointer<T>) Allocator.Alloc(cb);
		h.Clear((int) elemCnt);

		Allocated.Add(h);

		return h;
	}

	public static void Close()
	{
		foreach (var pointer in Allocated) {
			FreeInternal(pointer);
		}
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

	public static void Free<T>(T t) where T : class
	{
		var ptr = Mem.AddressOfHeap(t);
		ptr -= IntPtr.Size;
		Free(ptr);
	}


	public static object New(Type t, object[] ctor)
	{
		var m = typeof(AllocManager).GetRuntimeMethods()
			.First(x => x.Name == nameof(New) && x.ContainsGenericParameters);

		return m.CallGeneric(t, null, [ctor]);

	}

	/// <summary>
	/// <seealso cref="Mem.InitInline{T}"/>
	/// </summary>
	public static T New<T>(object[] ctor) where T : class
	{
		int size = Mem.SizeOf<T>(SizeOfOption.BaseInstance);
		var ptr  = Alloc((nuint) size);

		var value = Mem.InitInline<T>(ptr, out var ptr2);

		if (ctor.Any()) {
			var cc = ReflectionHelper.CallConstructor(value, ctor);
		}

		return value;
	}

}