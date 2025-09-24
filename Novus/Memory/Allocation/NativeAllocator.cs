using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Novus.Win32;

// ReSharper disable UnusedMember.Global
#pragma warning disable CA1416

namespace Novus.Memory.Allocation;

/// <summary>
/// Native memory allocator. Uses <see cref="NativeMemory"/>.
/// </summary>
public sealed unsafe class NativeAllocator : IAllocator
{
	public void Free(Pointer<byte> p)
		=> NativeMemory.Free(p.ToPointer());

	public Pointer<byte> ReAlloc(Pointer<byte> p, nuint n)
		=> NativeMemory.Realloc(p.ToPointer(), n);

	public Pointer<byte> Alloc(nuint n)
		=> NativeMemory.AllocZeroed(n);

	public nint GetSize(Pointer<byte> p)
		=> (nint) Native._msize(p.ToPointer());

}