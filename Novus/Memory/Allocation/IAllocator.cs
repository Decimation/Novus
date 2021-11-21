namespace Novus.Memory.Allocation;
// ReSharper disable UnusedMember.Global

public interface IAllocator
{
	public void Free(Pointer p);

	public Pointer ReAlloc(Pointer p, nuint n);

	public Pointer Alloc(nuint n);

	public nuint AllocSize(Pointer p);
}