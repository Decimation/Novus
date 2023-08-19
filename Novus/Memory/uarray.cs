// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Novus.Memory.Allocation;

// ReSharper disable StaticMemberInGenericType

namespace Novus.Memory;

/*
 * Some code adapted from
 * https://github.com/ikorin24/UnmanagedArray
 */
#if EXPERIMENTAL

// TODO: WIP

[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct UArray<T> : IDisposable, IEnumerable<T>, IPinnable
{
	private static readonly nuint s_elementSize;

	public int ElementSize => (int) s_elementSize;

	public Pointer<T> Address { get; }

	public int Length { get; }

	public int Size { get; }

	public bool IsAllocated => !Address.IsNull;

	public ref T this[int i] => ref Address[i];

	public UArray() : this(Mem.Nullptr, 0) { }

	public UArray(nuint s) : this(NativeMemory.AllocZeroed((nuint) s, s_elementSize), s) { }

	private UArray(Pointer<T> p, nuint i)
	{
		Address = p;
		Length  =(int) i;
		Size    = (int) Mem.GetByteCount((nuint) Length,(nuint) Mem.SizeOf<T>());
	}

	static UArray()
	{
		s_elementSize = (nuint) U.SizeOf<T>();
	}

	public static implicit operator Pointer<T>(UArray<T> value) => value.Address;

	public static implicit operator Pointer(UArray<T> value) => value.Address;

	public Pointer<T> AddressOfIndex(int i) => Address.AddressOfIndex(i);

	public MemoryHandle Pin(int elementIndex) => Address.Pin(elementIndex);

	public void Unpin()
	{
		//
	}

	public Span<T> AsSpan() => new(Address.ToPointer(), Length);

	public Span<T> AsMemory() => new(Address.ToPointer(), Length);

	public Stream AsStream() => new UnmanagedMemoryStream(Address.ToPointer<byte>(), Length);

	public T[] ToArray() => Address.ToArray(Length);

	public void CopyFrom(params T[] values)
	{
		var memory = values.AsMemory();

		using var pin = memory.Pin();

		Buffer.MemoryCopy(pin.Pointer, (void*) Address, Size, Size);
	}

	private void Free()
	{
		AllocManager.Free(Address);

		var ptr = M.AddressOfField<UArray<T>, Pointer<T>>(in this, nameof(Address));

		ptr.Write(Mem.Nullptr);

		/*fixed (UArray<T>* p = &this) {
			//hack
			U.Write<Pointer<Pointer<T>>>(p, Mem.Nullptr);
		}*/

	}

	public void Dispose()
	{
		Free();
	}

	/// <summary>Get enumerator instance.</summary>
	/// <returns></returns>
	public UArrayEnumerator GetEnumerator() => new(this);

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		// Avoid boxing by using class enumerator.
		return new UArrayEnumeratorClass(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		// Avoid boxing by using class enumerator.
		return new UArrayEnumeratorClass(this);
	}

	public override string ToString()
	{
		return $"{Address} [{Length}]";
	}

	/// <summary>Enumerator of <see cref="UArray{T}"/></summary>
	public struct UArrayEnumerator : IEnumerator<T>
	{
		private readonly Pointer<T> _ptr;
		private readonly int        _len;
		private          int        _index;

		public T Current { get; private set; }

		internal UArrayEnumerator(UArray<T> array)
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
	public sealed class UArrayEnumeratorClass : IEnumerator<T>
	{
		private readonly Pointer<T> _ptr;
		private readonly int        _len;
		private          int        _index;

		public T Current { get; private set; }

		internal UArrayEnumeratorClass(UArray<T> array)
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
#endif
