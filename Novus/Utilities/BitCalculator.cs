using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Novus.Utilities
{
	internal class BitCalculator
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadBit(int value, int bitOfs)
		{
			return (value & (1 << bitOfs)) != 0;
		}

		public static int SetBit(int x, int n)
		{
			return x | (1 << n);
		}

		public static int UnsetBit(int x, int n)
		{
			return x & ~(1 << n);
		}

		public static int ToggleBit(int x, int n)
		{
			return x ^ (1 << n);
		}

		public static int GetBitMask(int index, int size)
		{
			return ((1 << size) - 1) << index;
		}

		/// <summary>
		///     Reads <paramref name="bitCount" /> from <paramref name="value" /> at offset <paramref name="bitOfs" />
		/// </summary>
		/// <param name="value"><see cref="int" /> value to read from</param>
		/// <param name="bitOfs">Beginning offset</param>
		/// <param name="bitCount">Number of bits to read</param>
		/// <seealso cref="BitArray" />
		/// <seealso cref="BitVector32" />
		public static int ReadBits(int value, int bitOfs, int bitCount)
		{
			return (value & GetBitMask(bitOfs, bitCount)) >> bitOfs;
		}

		public static int WriteBits(int data, int index, int size, int value)
		{
			return (data & ~GetBitMask(index, size)) | (value << index);
		}
	}
}
