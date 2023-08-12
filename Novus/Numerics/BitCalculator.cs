using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Novus.Numerics;

public static class BitCalculator
{
	public const int BITS_PER_BYTE = 8;

	/*
	 * https://github.com/rubendal/BitStream
	 */
	public static Bit GetBit<T>(this T n, T index)
		where T : INumber<T>, IShiftOperators<T, T, T>, IBitwiseOperators<T, T, T>
	{
		return Bit.Create(n >> index);
	}

	public static T CircularShift<T>(this T n, T bits, bool leftShift)
		where T : INumber<T>, IShiftOperators<T, T, T>, IBitwiseOperators<T, T, T>
	{
		if (leftShift) {
			n = n << bits | n >> T.CreateChecked(M.SizeOf<T>() * BITS_PER_BYTE) - bits;
		}
		else {
			n = n >> bits | n << T.CreateChecked(M.SizeOf<T>() * BITS_PER_BYTE) - bits;
		}

		return n;
	}

	public static byte ReverseBits(this byte b)
	{
		return (byte) (((b & 1) << 7) + ((b >> 1 & 1) << 6) + ((b >> 2 & 1) << 5) + ((b >> 3 & 1) << 4) +
		               ((b >> 4 & 1) << 3) + ((b >> 5 & 1) << 2) + ((b >> 6 & 1) << 1) + (b >> 7 & 1));
	}

	[MImpl(MImplO.AggressiveInlining)]
	public static bool ReadBit<T>(T value, T bitOfs) where T : IBinaryInteger<T>, IShiftOperators<T, T, T>
	{
		return (value & T.One << bitOfs) != T.Zero;
	}

	[MImpl(MImplO.AggressiveInlining)]
	public static T SetBit<T>(T x, T n) where T : IBinaryInteger<T>, IShiftOperators<T, T, T>
	{
		return x | T.One << n;
	}

	[MImpl(MImplO.AggressiveInlining)]
	public static T UnsetBit<T>(T x, T n) where T : IBinaryInteger<T>, IShiftOperators<T, T, T>
	{
		return x & ~(T.One << n);
	}

	[MImpl(MImplO.AggressiveInlining)]
	public static T ToggleBit<T>(T x, T n) where T : IBinaryInteger<T>, IShiftOperators<T, T, T>
	{
		return x ^ T.One << n;
	}

	[MImpl(MImplO.AggressiveInlining)]
	public static T GetBitMask<T>(T index, T size) where T : IBinaryInteger<T>, IShiftOperators<T, T, T>
	{
		return (T.One << size) - T.One << index;
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
	public static T ReadBits<T>(T value, T bitOfs, T bitCount) where T : IBinaryInteger<T>, IShiftOperators<T, T, T>
	{
		return (value & GetBitMask(bitOfs, bitCount)) >> bitOfs;
	}

	[MImpl(MImplO.AggressiveInlining)]
	public static T WriteBits<T>(T data, T index, T size, T value) where T : IBinaryInteger<T>, IShiftOperators<T, T, T>
	{
		return data & ~GetBitMask(index, size) | value << index;
	}
}