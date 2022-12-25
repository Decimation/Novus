using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Novus.Utilities;

// ReSharper disable UnusedMember.Global

namespace Novus.Memory;

/// <summary>
///     Signature scanner.
/// </summary>
/// <remarks>
/// <a href="https://wiki.alliedmods.net/Signature_scanning">Signature scanning</a>
/// <p/>
/// Uses IDA signature format
/// </remarks>
public sealed class SigScanner
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

		Buffer = Mem.ReadProcessMemory(proc, Address, (nint) Size);
	}

	public SigScanner(ProcessModule module) : this(module.BaseAddress, (ulong) module.ModuleMemorySize) { }

	public SigScanner(Span<byte> m) : this(m, (ulong) m.Length) { }

	public SigScanner(Pointer<byte> p, ulong c) : this(p, c, p.ToArray((int) c)) { }

	public SigScanner(Pointer<byte> ptr, ulong size, byte[] buffer)
	{
		Buffer  = buffer;
		Size    = size;
		Address = ptr;
	}

	public static Pointer<byte>[] ScanProcess(string sig, Process p = null)
	{
		p ??= Process.GetCurrentProcess();
		var page = p.GetModules();

		var scanners = page.Select(s =>
		{
			return new Lazy<SigScanner>(() => new SigScanner(p, s));
		}).ToArray();

		/*var pointers = scanners.Select(s => s.Value.FindSignature(sig));
		return pointers.FirstOrDefault(p => !p.IsNull);*/

		return scanners.Select(t => t.Value.FindSignature(sig)).Where(ptr => !ptr.IsNull).ToArray();
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

	/// <summary>
	/// Parses a <see cref="byte"/> array signature in the following format:<para />
	/// <c>X1 X2 Xn...</c> format where <c>X</c> is an unsigned byte value<br />
	/// <c>?</c> indicates wildcard<br />
	/// Space delimited
	/// </summary>
	public static byte[] ReadSignature(string pattern)
	{
		//todo: Convert.To/FromHexString
		string[] strByteArr = pattern.Split(' ');

		byte[] patternBytes = new byte[strByteArr.Length];

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
				Pointer<byte> p = Address + i;

				return p;
			}
		}

		return Mem.Nullptr;
	}
}