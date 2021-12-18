using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Novus.OS.Win32;

// ReSharper disable UnusedMember.Global

namespace Novus.Memory.Allocation;

/// <summary>
/// Native memory allocator. Uses <see cref="NativeMemory"/>.
/// </summary>
public sealed unsafe class NativeAllocator : IAllocator
{
	public void Free(Pointer p) => NativeMemory.Free(p.ToPointer());

	[MustUseReturnValue]
	public Pointer ReAlloc(Pointer p, nuint n) => NativeMemory.Realloc(p.ToPointer(), n);

	[MustUseReturnValue]
	public Pointer Alloc(nuint n) => NativeMemory.AllocZeroed(n);

	public nuint GetSize(Pointer p) => Native._msize(p.ToPointer());
}