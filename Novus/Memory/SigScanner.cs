using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft;
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
	public Memory<byte> Buffer { get; }

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

	public SigScanner(ProcessModule module)
		: this(module.BaseAddress, (ulong) module.ModuleMemorySize) { }

	public SigScanner(Span<byte> m)
		: this((Pointer) m, (ulong) m.Length) { }

	public SigScanner(Pointer<byte> p, ulong c)
		: this(p, c, p.ToArray((int) c)) { }

	public SigScanner(Pointer<byte> ptr, ulong size, byte[] buffer)
	{
		Buffer  = buffer;
		Size    = size;
		Address = ptr;
	}

	public static Pointer<byte>[] ScanProcess(string sig)
		=> ScanProcess(Process.GetCurrentProcess(), sig);

	/*public static Pointer[] ScanProcess(Process p, string sig)
	{
		var page = p.GetModules();

		var scanners = page.Select(s =>
		{
			return new Lazy<SigScanner>(() => new SigScanner(p, s));
		}).ToArray();

		/*var pointers = scanners.Select(s => s.Value.FindSignature(sig));
		return pointers.FirstOrDefault(p => !p.IsNull);#1#

		return scanners.Select(t => t.Value.FindSignature(sig)).Where(ptr => !ptr.IsNull).ToArray();
	}*/

	public static Pointer[] ScanProcess(Process p, string sig)
		=> ScanProcess(p, ReadSignature(sig));

	public static Pointer[] ScanProcess(Process p, byte[] s)
	{
		var buf = new ConcurrentBag<Pointer>();

		var modules = p.GetModules();

		Parallel.ForEach(modules, (module, token) =>
		{
			var ss = new SigScanner(p, module);

			var px = ss.FindSignatures(s, pointer =>
			{
				if (!pointer.IsNull) {
					buf.Add(pointer);
				}

				return true;
			});

		});
		/*var pointers = scanners.Select(s => s.Value.FindSignature(sig));
		return pointers.FirstOrDefault(p => !p.IsNull);*/

		return [.. buf];
	}

	#endregion

	private bool PatternCheck(int nOffset, IReadOnlyList<byte> arrPattern)
	{
		// ReSharper disable once LoopCanBeConvertedToQuery
		var span = Buffer.Span;

		int l = arrPattern.Count;

		for (int i = 0; i < l; i++) {
			if (arrPattern[i] == UNKNOWN_BYTE)
				continue;

			if (arrPattern[i] != span[nOffset + i])
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

		var patternBytes = new byte[strByteArr.Length];

		for (int i = 0; i < strByteArr.Length; i++) {
			patternBytes[i] = strByteArr[i] == UNKNOWN_STR
				                  ? UNKNOWN_BYTE
				                  : Byte.Parse(strByteArr[i], NumberStyles.HexNumber);
		}

		return patternBytes;
	}

	public Pointer<byte> FindSignature(string pattern)
		=> FindSignature(ReadSignature(pattern));

	/// <summary>
	/// Searches for the location of a signature within the module
	/// </summary>
	/// <param name="pattern">Signature</param>
	/// <returns>Address of the located signature; <see cref="Mem.Nullptr"/> if the signature was not found</returns>
	public Pointer<byte> FindSignature(byte[] pattern)
	{
		var span = Buffer.Span;
		int l    = Buffer.Length;
		var b    = pattern[0];

		for (int i = 0; i < l; i++) {
			if (span[i] != b)
				continue;

			if (PatternCheck(i, pattern)) {
				Pointer<byte> p = Address + i;

				return p;
			}
		}

		return Mem.Nullptr;
	}

	public Pointer<byte> FindSignature2(byte[] pattern)
	{
		Pointer p = Mem.Nullptr;

		FindSignatures(pattern, p2 =>
		{
			p = p2;
			return false;
		});
		return p;
	}

	public bool FindSignatures(byte[] pattern, Func<Pointer<byte>, bool> callback)
	{
		var span = Buffer.Span;
		int l    = Buffer.Length;
		var b    = pattern[0];

		// Requires.Range(ofs < l, nameof(ofs));

		for (int i = 0; i < l; i++) {
			if (span[i] != b)
				continue;

			if (PatternCheck(i, pattern)) {
				Pointer<byte> p = Address + i;

				if (callback(p)) {
					continue;
				}
				else {
					break;
				}
			}
		}

		return false;
	}

	/*public IEnumerable<Pointer> FindSignatures(byte[] pattern)
	{
		var span = Buffer.Span;
		int l    = Buffer.Length;
		var b    = pattern[0];
		var buf  = new ConcurrentBag<Pointer>();

		// Requires.Range(ofs < l, nameof(ofs));

		for (int i = 0; i < l; i++) {
			if (span[i] != b)
				continue;

			if (PatternCheck(i, pattern)) {
				Pointer<byte> p = Address + i;

				buf.Add(p);
			}
		}

		return buf;
	}*/

}