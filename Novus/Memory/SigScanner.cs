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
		 * Signature scanning
		 *
		 * https://wiki.alliedmods.net/Signature_scanning
		 * https://archive.vn/lGmyp
		 * https://archive.vn/M6hUN
		 * https://archive.vn/A1yEs
		 * https://github.com/alliedmodders/sourcemod/blob/master/tools/ida_scripts/makesig.idc
		 *
		 * Other implementations:
		 * https://github.com/LiveSplit/LiveSplit/blob/master/LiveSplit/LiveSplit.Core/ComponentUtil/SignatureScanner.cs
		 * https://github.com/LiveSplit/LiveSplit/blob/master/LiveSplit/LiveSplit.Core/ComponentUtil/ProcessExtensions.cs
		 *
		 * https://github.com/Zer0Mem0ry/SignatureScanner
		 */


		/// <summary>
		/// Memory of the module
		/// </summary>
		public byte[] Buffer { get; }

		/// <summary>
		/// Module pointer
		/// </summary>
		public Pointer<byte> Address { get; }


		/// <summary>
		/// Module size
		/// </summary>
		public ulong Size { get; }

		#region Constructors

		public SigScanner(Process proc, ProcessModule module)
		{
			Address = module.BaseAddress;

			Size = (ulong) module.ModuleMemorySize;

			Buffer = Mem.ReadProcessMemory(proc, Address, (int) Size);
		}

		public SigScanner(ProcessModule module) : this(module.BaseAddress, (ulong) module.ModuleMemorySize) { }

		public SigScanner(Pointer<byte> p, ulong c) : this(p, c, p.Copy((int) c)) { }

		public SigScanner(Pointer<byte> ptr, ulong size, byte[] buffer)
		{
			Buffer  = buffer;
			Size    = size;
			Address = ptr;
		}

		#endregion


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

		public static byte[] ReadSignature(string pattern)
		{
			string[] strByteArr   = pattern.Split(' ');
			byte[]   patternBytes = new byte[strByteArr.Length];

			for (int i = 0; i < strByteArr.Length; i++) {
				patternBytes[i] = strByteArr[i] == UNKNOWN_STR
					? UNKNOWN_BYTE
					: Byte.Parse(strByteArr[i], NumberStyles.HexNumber);
			}


			return patternBytes;
		}


		public Pointer<byte> FindSignature(string pattern) => FindSignature(ReadSignature(pattern));


		/// <summary>
		/// Searches for the location of a signature within the module
		/// </summary>
		/// <param name="pattern">Signature</param>
		/// <returns>Address of the located signature; <see cref="Mem.Nullptr"/> if the signature was not found</returns>
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