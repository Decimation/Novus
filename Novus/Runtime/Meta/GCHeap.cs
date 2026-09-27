using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Kantan.Diagnostics;
using Novus.Imports.Attributes;
using Novus.Memory;
using Novus.Utilities;

// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace Novus.Runtime.Meta;
#pragma warning disable CA1416

/// <summary>
/// GC heap
/// </summary>
/// <seealso cref="GC"/>
/// <seealso cref="FormatterServices"/>
/// <seealso cref="Activator"/>
/// <seealso cref="RuntimeHelpers"/>
/// <seealso cref="System.Runtime.InteropServices.GCHandle"/>
public static unsafe class GCHeap
{

	static GCHeap()
	{
		Global.Clr.LoadImports(typeof(GCHeap));
	}

	public static bool IsHeapPointer<T>(T t, bool smallHeapOnly = false) where T : class
		=> IsHeapPointer(Mem.AddressOfHeap(t), smallHeapOnly);

	/*public static bool IsHeapPointer(object o, bool smallHeapOnly = false)
		=> IsHeapPointer(Mem.AddressOfHeap(o), smallHeapOnly);*/

	public static bool IsHeapPointer(in Pointer<byte> ptr, bool smallHeapOnly = false)
		=> Func_IsHeapPointer(GlobalHeap.ToPointer(), ptr.ToPointer(), smallHeapOnly);

	public static bool IsLargeObject(object o)
		=> IsLargeObject(Mem.AddressOfHeap(o));

	public static bool IsLargeObject(Pointer<byte> obj)
		=> Func_IsLargeObject(GlobalHeap.ToPointer(), obj.ToPointer());

	/// <summary>
	/// <c>g_pGCHeap</c>
	/// </summary>
	[field: ImportClr("Sym_GCHeap", ImportType.Symbol)]
	public static Pointer<byte> GlobalHeap { get; }

	/// <summary>
	/// <see cref="IsHeapPointer"/>
	/// </summary>
	[field: ImportClr("Sig_IsHeapPointer")]
	private static delegate* unmanaged[Thiscall]<void*, void*, bool, bool> Func_IsHeapPointer { get; }

	/// <summary>
	/// <see cref="IsLargeObject(Pointer{byte})"/>
	/// </summary>
	[field: ImportClr("Sym_IsLargeObj", ImportType.Symbol)]
	private static delegate* unmanaged[Thiscall]<void*, void*, bool> Func_IsLargeObject { get; }

}

[Flags]
public enum GCAllocFlags
{

	GC_ALLOC_NO_FLAGS     = 0,
	GC_ALLOC_FINALIZE     = 1,
	GC_ALLOC_CONTAINS_REF = 2,
	GC_ALLOC_ALIGN8_BIAS  = 4,
	GC_ALLOC_ALIGN8       = 8, // Only implies the initial allocation is 8 byte aligned.

	// Preserving the alignment across relocation depends on
	// RESPECT_LARGE_ALIGNMENT also being defined.
	GC_ALLOC_ZEROING_OPTIONAL   = 16,
	GC_ALLOC_LARGE_OBJECT_HEAP  = 32,
	GC_ALLOC_PINNED_OBJECT_HEAP = 64,
	GC_ALLOC_USER_OLD_HEAP      = GC_ALLOC_LARGE_OBJECT_HEAP | GC_ALLOC_PINNED_OBJECT_HEAP,

};