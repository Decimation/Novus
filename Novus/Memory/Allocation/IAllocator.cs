using JetBrains.Annotations;

namespace Novus.Memory.Allocation;
// ReSharper disable UnusedMember.Global

public interface IAllocator
{
	public void Free(Pointer p);

	[MustUseReturnValue]
	public Pointer ReAlloc(Pointer p, nuint n);

	[MustUseReturnValue]
	public Pointer Alloc(nuint n);

	public nuint GetSize(Pointer p);
}