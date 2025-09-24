using System.Reflection;
using JetBrains.Annotations;
using Novus.Win32;

namespace Novus.Memory.Allocation;
#pragma warning disable CA1416

// ReSharper disable UnusedMember.Global
public interface IAllocator
{
	public void Free(Pointer<byte> p);

	[MURV]
	public Pointer<byte> ReAlloc(Pointer<byte> p, nuint n);

	[MURV]
	public Pointer<byte> Alloc(nuint n);

	public bool IsAllocated(Pointer<byte> p)
	{
		return GetSize(p) != Native.ERROR_SV;
	}

	public nint GetSize(Pointer<byte> p);
}