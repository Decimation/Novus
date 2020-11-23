using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

// ReSharper disable UnusedMember.Global

namespace Novus.Memory
{
	/// <summary>
	///     A basic signature scanner.
	/// </summary>
	public class SigScanner
	{
		public byte[] Buffer { get; }

		public Pointer<byte> Address { get; }

		public SigScanner(ProcessModule module) : this(module.BaseAddress, module.ModuleMemorySize) { }

		public SigScanner(Pointer<byte> p, int c) : this(p, p.Copy(c)) { }

		public SigScanner(Pointer<byte> ptr, byte[] buffer)
		{
			Buffer  = buffer;
			Address = ptr;
		}


		private bool PatternCheck(int nOffset, IReadOnlyList<byte> arrPattern)
		{
			// ReSharper disable once LoopCanBeConvertedToQuery
			for (int i = 0; i < arrPattern.Count; i++) {
				if (arrPattern[i] == UNKNOWN_BYTE)
					continue;

				if (arrPattern[i] != Buffer[nOffset + i])
					return false;
			}

			return true;
		}

		private const string UNKNOWN_STR = "?";

		private const byte UNKNOWN_BYTE = 0x0;

		public static byte[] ReadByteArraySignature(string szPattern)
		{
			//			List<byte> patternbytes = new List<byte>();
			//			foreach (string szByte in szPattern.Split(' '))
			//				patternbytes.Add(szByte == "?" ? (byte) 0x0 : Convert.ToByte(szByte, 16));
			//			return patternbytes.ToArray();


			string[] strByteArr   = szPattern.Split(' ');
			byte[]   patternBytes = new byte[strByteArr.Length];

			for (int i = 0; i < strByteArr.Length; i++) {
				patternBytes[i] = strByteArr[i] == UNKNOWN_STR
					? UNKNOWN_BYTE
					: Byte.Parse(strByteArr[i], NumberStyles.HexNumber);
			}


			return patternBytes;
		}

		public Pointer<byte> FindPattern(string pattern) => FindPattern(ReadByteArraySignature(pattern));

		public Pointer<byte> FindPattern(byte[] pattern)
		{
			for (int i = 0; i < Buffer.Length; i++) {
				if (Buffer[i] != pattern[0])
					continue;


				if (PatternCheck(i, pattern)) {
					var p = Address + i;
					
					return p;
				}
			}

			return Mem.Nullptr;
		}
	}
}