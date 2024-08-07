﻿#region

global using ER = Novus.Properties.EmbeddedResources;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Kantan.Utilities;
using Microsoft;
using Novus.Properties;
using Novus.Streams;

#endregion

namespace Novus.FileTypes.Impl;

/*
 * Adapted from https://github.com/hey-red/Mime
 */

public sealed class MagicResolver : IFileTypeResolver
{
	public const MagicOpenFlags MagicMimeFlags =
		MagicOpenFlags.MAGIC_ERROR |
		MagicOpenFlags.MAGIC_MIME_TYPE |
		MagicOpenFlags.MAGIC_NO_CHECK_COMPRESS |
		MagicOpenFlags.MAGIC_NO_CHECK_ELF |
		MagicOpenFlags.MAGIC_NO_CHECK_APPTYPE;

	public IntPtr Magic { get; }

	public static IFileTypeResolver Instance { get; set; } = new MagicResolver();

	public MagicResolver(string mgc = null)
	{
		mgc ??= GetMagicFile();

		Magic = MagicNative.magic_open(MagicMimeFlags);
		var rd = MagicNative.magic_load(Magic, mgc);
	}

	private static string GetMagicFile()
	{
		var mgc = Path.Combine(Global.DataFolder, EmbeddedResources.F_Magic);
		Assumes.True(File.Exists(mgc));

		Debug.WriteLine($"magic file: {mgc}");

		return mgc;
	}

	public FileType Resolve(byte[] rg)
	{
		// var buf1 = stream.ReadBlockAsync(FileType.RSRC_HEADER_LEN);
		// buf1.Wait();
		// var buf  = buf1.Result;

		var sz = MagicNative.magic_buffer(Magic, rg, rg.Length);
		var s  = Marshal.PtrToStringAnsi(sz);
		return new FileType(s) { };
	}

	public FileType Resolve(Stream stream)
	{
		var buf = stream.ReadHeader();
		return Resolve(buf);
	}

	public void Dispose()
	{
		MagicNative.magic_close(Magic);
	}
}