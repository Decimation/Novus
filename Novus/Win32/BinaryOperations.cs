using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using SimpleCore.Utilities;

// ReSharper disable UnusedMember.Global

#nullable enable
#pragma warning disable HAA0501 //
#pragma warning disable HAA0502 //
#pragma warning disable HAA0301 //
#pragma warning disable HAA0302 //

namespace Novus.Win32
{
	public static class BinaryOperations
	{
		/// <summary>
		/// Reads a <see cref="byte"/> array as a <see cref="string"/> delimited by spaces in
		/// hex number format
		/// </summary>
		public static byte[] ReadBinaryString(string s)
		{
			var rg = new List<byte>();

			var bytes = s.Split(Formatting.SPACE);

			foreach (string b in bytes) {
				byte n = Byte.Parse(b, NumberStyles.HexNumber);

				rg.Add(n);
			}

			return rg.ToArray();
		}

		/// <summary>
		///     Reads <paramref name="bitCount" /> from <paramref name="value" /> at offset <paramref name="bitOfs" />
		/// </summary>
		/// <param name="value"><see cref="int" /> value to read from</param>
		/// <param name="bitOfs">Beginning offset</param>
		/// <param name="bitCount">Number of bits to read</param>
		public static int ReadBits(int value, int bitOfs, int bitCount) => ((1 << bitCount) - 1) & (value >> bitOfs);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadBit(int value, int bitOfs) => (value & (1 << bitOfs)) != 0;


		public static int GetMask(int index, int size) => ((1 << size) - 1) << index;

		public static int ReadFrom(int data, int index, int size) => (data & GetMask(index, size)) >> index;

		public static int WriteTo(int data, int index, int size, int value)
			=> (data & ~GetMask(index, size)) | (value << index);
	}
}