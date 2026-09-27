// Author: Deci | Project: Novus | Name: GCHeapAllocator.cs
// Date: 2026/09/26 @ 23:09:16

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Novus.Runtime.Meta;

namespace Novus.Memory.Allocation;

public class GCHeapAllocator : IAllocator
{

	public void Free(Pointer<byte> p)
	{
		
	}

	public Pointer<byte> ReAlloc(Pointer<byte> p, nuint n)
	{
		ThrowUnavailable();
		return Mem.Nullptr;
	}

	
	public Pointer<byte> Alloc(nuint n)
	{
		throw new ArgumentException($"Arbitrary bytes cannot be allocated when using {nameof(GCHeapAllocator)}");
	}

	public nint GetSize(Pointer<byte> p)
	{
		ThrowUnavailable();
		return -1;
	}

	[CA("=> halt")]
	[DNR]
	private static void ThrowUnavailable([CallerMemberName] string caller = null)
	{
		throw new NotSupportedException($"Function {caller} is not available when using {nameof(GCHeapAllocator)}");
	}

}