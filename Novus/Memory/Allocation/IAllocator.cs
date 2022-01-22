using System.Reflection;
using JetBrains.Annotations;

namespace Novus.Memory.Allocation;

// ReSharper disable UnusedMember.Global
public interface IAllocator
{
	public void Free(Pointer<byte> p);

	[MustUseReturnValue]
	public Pointer ReAlloc(Pointer<byte> p, nuint n);

	[MustUseReturnValue]
	public Pointer Alloc(nuint n);

	public bool IsAllocated(Pointer p)
	{
		return GetSize(p) != Native.INVALID;
	}

	public nint GetSize(Pointer p);
}