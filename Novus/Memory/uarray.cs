// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

using System;
using System.Collections;
using System.Collections.Generic;
using Novus.Memory.Allocation;

namespace Novus.Memory;

/*
 * Some code adapted from
 * https://github.com/ikorin24/UnmanagedArray
 */

public readonly struct UArray<T> : IDisposable, IEnumerable<T>
{
	public Pointer<T> Address { get; }

	public int Length { get; }

	public int Size { get; }

	public bool IsAllocated => !Address.IsNull;

	public ref T this[int i] => ref Address[i];

	public UArray() : this(Mem.Nullptr, 0) { }

	public UArray(Pointer<T> p, int i)
	{
		Address = p;
		Length  = i;
		Size    = (int) Mem.GetByteCount(Length, Mem.SizeOf<T>());
	}

	public static implicit operator Pointer<T>(UArray<T> value) => value.Address;

	public static implicit operator Pointer<byte>(UArray<T> value) => value.Address;

	public Pointer<T> AddressOfIndex(int i)
	{
		ref T value = ref U.Add(ref this[i], i);

		return new(ref value);
	}


	public void CopyFrom(params T[] values)
	{
		unsafe {
			var memory = values.AsMemory();

			using var pin = memory.Pin();

			Buffer.MemoryCopy(pin.Pointer, (void*) Address, Size, Size);

			/*for (int i = 0; i < t.Length; i++) {
				this[i] = t[i];
			}*/
		}

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


	/// <summary>Get enumerator instance.</summary>
	/// <returns></returns>
	public Enumerator GetEnumerator()
	{
		return new(this);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		// Avoid boxing by using class enumerator.
		return new EnumeratorClass(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		// Avoid boxing by using class enumerator.
		return new EnumeratorClass(this);
	}

	public override string ToString()
	{
		return $"{Address} [{Length}]";
	}
	

	/// <summary>Enumerator of <see cref="UArray{T}"/></summary>
	public struct Enumerator : IEnumerator<T>
	{
		private readonly Pointer<T> _ptr;
		private readonly int        _len;
		private          int        _index;
		
		public T Current { get; private set; }

		internal Enumerator(UArray<T> array)
		{
			_ptr    = array.Address;
			_len    = array.Length;
			_index  = 0;
			Current = default;
		}
		
		public void Dispose() { }
		
		public bool MoveNext()
		{
			if ((uint) _index < (uint) _len) {
				Current = _ptr[_index];
				_index++;
				return true;
			}

			_index  = _len + 1;
			Current = default;
			return false;
		}

		object IEnumerator.Current => Current;

		void IEnumerator.Reset()
		{
			_index  = 0;
			Current = default;
		}
	}

	/// <summary>Enumerator of <see cref="UArray{T}"/></summary>
	public class EnumeratorClass : IEnumerator<T>
	{
		private readonly Pointer<T> _ptr;
		private readonly int        _len;
		private          int        _index;
		
		public T Current { get; private set; }

		internal EnumeratorClass(UArray<T> array)
		{
			_ptr    = array.Address;
			_len    = array.Length;
			_index  = 0;
			Current = default;
		}
		
		public void Dispose() { }

		public bool MoveNext()
		{
			if ((uint) _index < (uint) _len) {
				Current = _ptr[_index];
				_index++;
				return true;
			}

			_index  = _len + 1;
			Current = default;
			return false;
		}

		object IEnumerator.Current => Current;

		void IEnumerator.Reset()
		{
			_index  = 0;
			Current = default;
		}
	}
}