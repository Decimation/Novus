// Author: Deci | Project: Novus | Name: GCHeapAllocator.cs
// Date: 2026/09/26 @ 23:09:16

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Novus.Runtime.Meta;

// ReSharper disable InconsistentNaming

namespace Novus.Memory.Allocation;

[Experimental("CLR_EXPERIMENTAL")]
public class GCHeapAllocator : IAllocator
{

	public void Free(Pointer<byte> p) { }

	public Pointer<byte> ReAlloc(Pointer<byte> p, nuint n)
	{
		ThrowUnavailable();
		return Mem.Nullptr;
	}


	public Pointer<byte> Alloc(nuint n)
		=> Alloc(n, GCAllocFlags.GC_ALLOC_NO_FLAGS);

	public Pointer<byte> Alloc(nuint n, GCAllocFlags flags)
		=> GCHeap.Alloc(n, flags);

	public nint GetSize(Pointer<byte> p)
	{
		// ThrowUnavailable();
		return -1;
	}

	[CA("=> halt")]
	[DNR]
	private static void ThrowUnavailable([CMN] string caller = null)
	{
		throw new NotSupportedException($"Function {caller} is not available when using {nameof(GCHeapAllocator)}");
	}

}