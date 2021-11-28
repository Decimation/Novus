// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

using System;
using System.Collections;
using System.Collections.Generic;
using Novus.Memory.Allocation;

namespace Novus.Memory;

public readonly struct uarray<T> : IDisposable, IEnumerable<T>, IFormattable
{
	public Pointer<T> Address { get; }

	public int Length { get; }

	public int Size { get; }

	public bool Allocator { get; }
	public Pointer<T> AddressOf(int i)
	{
		ref T t = ref U.Add(ref this[i], i);

		return new(ref t);
	}

	public void Copy(params T[] t)
	{
		unsafe {
			var memory = t.AsMemory();

			using var pin = memory.Pin();

			Buffer.MemoryCopy(pin.Pointer, (void*) Address, Size, Size);

			/*for (int i = 0; i < t.Length; i++) {
				this[i] = t[i];
			}*/
		}

	}


	public static implicit operator Pointer<T>(uarray<T> x)
	{
		return x.Address;
	}

	public static implicit operator Pointer<byte>(uarray<T> x)
	{
		return x.Address;
	}


	public bool Allocated
	{
		get
		{
			return ((!Allocator || AllocManager.IsAllocated(this)) || !Address.IsNull) &&
			       (Allocator || !Address.IsNull);
		}
	}

	public uarray() : this(Mem.Nullptr, 0, false) { }

	public uarray(Pointer<T> p, int i, bool allocator)
	{
		Address = p;
		Length  = i;
		Size    = (int) Mem.GetByteCount(Length, Mem.SizeOf<T>());
		Allocator = allocator;
	}

	public void Free()
	{
		AllocManager.Free(Address);

		unsafe {
			fixed (void* p = &this) {
				U.Write<Pointer<Pointer<T>>>(p, Mem.Nullptr);
			}

		}
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

	public string ToString(string format, IFormatProvider formatProvider)
	{
		return format switch
		{
			"T" => null,
			_   => null,
		};
	}
}