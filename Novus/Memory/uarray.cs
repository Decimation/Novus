// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

using System;
using System.Collections;
using System.Collections.Generic;
using Novus.Memory.Allocation;

namespace Novus.Memory;

public readonly struct uarray<T> : IDisposable, IEnumerable<T>
{
	public Pointer<T> Address { get; }

	public int Length { get; }


	public static implicit operator Pointer<T>(uarray<T> x)
	{
		return x.Address;
	}

	public static implicit operator Pointer<byte>(uarray<T> x)
	{
		return x.Address;
	}


	public bool Allocated => AllocManager.IsAllocated(this);

	public uarray() : this(Mem.Nullptr, 0) { }

	public uarray(Pointer<T> p, int i)
	{
		Address = p;
		Length  = i;
	}

	public void Free()
	{
		AllocManager.Free(Address);
	}

	public void Dispose()
	{
		Free();
	}

	public ref T this[int i] => ref Address[i];



	public IEnumerator<T> GetEnumerator()
	{
		for (int i = 0; i < Length; i++) {
			yield return this[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public override string ToString()
	{
		return $"{Address} [{Length}]";
	}
}