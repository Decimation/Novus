using System.Runtime.InteropServices;
using JetBrains.Annotations;

// ReSharper disable UnusedMember.Global

namespace Novus.Memory.Allocation;

/// <summary>
/// Native memory allocator. Uses <see cref="NativeMemory"/>.
/// </summary>
public sealed unsafe class NativeAllocator : IAllocator
{
	public void Free(Pointer p) => NativeMemory.Free(p.ToPointer());

	[MURV]
	public Pointer ReAlloc(Pointer p, nuint n) => NativeMemory.Realloc(p.ToPointer(), n);

	[MURV]
	public Pointer Alloc(nuint n) => NativeMemory.AllocZeroed(n);

	public nint GetSize(Pointer p) => (nint) Native._msize(p.ToPointer());
}