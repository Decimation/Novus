using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Novus.Memory;

namespace Novus.Numerics;

public static class BitCalculator
{

	public const int BITS_PER_BYTE = 8;

	/*
	 * https://github.com/rubendal/BitStream
	 */


	extension<T>(T n) where T : INumber<T>, IShiftOperators<T, T, T>, IBitwiseOperators<T, T, T>
	{

		/*public Bit GetBit(T index)
		{
			return Bit.ParseCast(n >> index);
		}*/

	}

	public static byte ReverseBits(this byte b)
	{
		return (byte) (((b & 1) << 7) + ((b >> 1 & 1) << 6) + ((b >> 2 & 1) << 5) + ((b >> 3 & 1) << 4) +
		               ((b                  >> 4 & 1) << 3) + ((b >> 5 & 1) << 2) + ((b >> 6 & 1) << 1) + (b >> 7 & 1));
	}

	extension(BitOperations)
	{



	}

	extension<T>(T value) where T : IBinaryInteger<T>, IShiftOperators<T, T, T>
	{

		[MImpl(MImplO.AggressiveInlining)]
		public bool ReadBit(T bitOfs)
		{
			return (value & T.One << bitOfs) != T.Zero;
		}

		[MImpl(MImplO.AggressiveInlining)]
		public T SetBit(T n)
		{
			return value | T.One << n;
		}

		[MImpl(MImplO.AggressiveInlining)]
		public T UnsetBit(T n)
		{
			return value & ~(T.One << n);
		}

		[MImpl(MImplO.AggressiveInlining)]
		public T ToggleBit(T n)
		{
			return value ^ T.One << n;
		}

		[MImpl(MImplO.AggressiveInlining)]
		public T GetBitMask(T size)
		{
			return (T.One << size) - T.One << value;
		}

		/// <summary>
		///     Reads <paramref name="bitCount" /> from <paramref name="value" /> at offset <paramref name="bitOfs" />
		/// </summary>
		/// <param name="value"><see cref="int" /> value to read from</param>
		/// <param name="bitOfs">Beginning offset</param>
		/// <param name="bitCount">Number of bits to read</param>
		/// <seealso cref="BitArray" />
		/// <seealso cref="BitVector32" />
		[MImpl(MImplO.AggressiveInlining)]
		public T ReadBits(T bitOfs, T bitCount)
		{
			return (value & bitOfs.GetBitMask(bitCount)) >> bitOfs;
		}

		[MImpl(MImplO.AggressiveInlining)]
		public T WriteBits(T index, T size, T data)
		{
			return data & ~index.GetBitMask(size) | value << index;
		}

		public T CircularShift(T bits, bool leftShift)
		{
			if (leftShift) {
				value = value << bits | value >> T.CreateChecked(Mem.SizeOf<T>() * BITS_PER_BYTE) - bits;
			}
			else {
				value = value >> bits | value << T.CreateChecked(Mem.SizeOf<T>() * BITS_PER_BYTE) - bits;
			}

			return value;
		}

	}

}