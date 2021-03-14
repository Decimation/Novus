using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

// ReSharper disable UnusedMember.Global

namespace Novus.Memory
{
	/// <summary>
	///     Signature scanner.
	/// </summary>
	/// <remarks>
	/// <a href="https://wiki.alliedmods.net/Signature_scanning">Signature scanning</a>
	/// </remarks>
	public class SigScanner
	{
		/*
		 * Other implementation:
		 * https://github.com/LiveSplit/LiveSplit/blob/master/LiveSplit/LiveSplit.Core/ComponentUtil/SignatureScanner.cs
		 * https://github.com/LiveSplit/LiveSplit/blob/master/LiveSplit/LiveSplit.Core/ComponentUtil/ProcessExtensions.cs
		 */


		public byte[] Buffer { get; }

		public Pointer<byte> Address { get; }


		public ulong Size { get; }
		
		public SigScanner(Process proc, ProcessModule module) 
			: this(module.BaseAddress, (ulong) module.ModuleMemorySize,
			Mem.ReadProcessMemory(proc, module.BaseAddress, module.ModuleMemorySize)) { }


		public SigScanner(ProcessModule module)
			: this(module.BaseAddress, (ulong) module.ModuleMemorySize)
		{ }

		public SigScanner(Pointer<byte> p, ulong c) : this(p,c, p.Copy((int)c)) { }

		public SigScanner(Pointer<byte> ptr, ulong size, byte[] buffer)
		{
			Buffer  = buffer;
			Size = size;
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

		public static byte[] ReadSignature(string szPattern)
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


		public Pointer<byte> FindSignature(string pattern) => FindSignature(ReadSignature(pattern));

		public Pointer<byte> FindSignature(byte[] pattern)
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