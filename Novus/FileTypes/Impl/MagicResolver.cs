#region

global using ER = Novus.Properties.EmbeddedResources;

using System.Diagnostics;
using System.Runtime.InteropServices;

using Kantan.Utilities;
using Novus.Properties;

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

	public static readonly IFileTypeResolver Instance = new MagicResolver();

	public MagicResolver(string mgc = null)
	{
		mgc ??= GetMagicFile();

		Magic = MagicNative.magic_open(MagicMimeFlags);
		var rd = MagicNative.magic_load(Magic, mgc);
	}

	private static string GetMagicFile()
	{
		var mgc = Path.Combine(Global.DataFolder, EmbeddedResources.F_MAGIC);

		if (!File.Exists(mgc)) {
			File.WriteAllBytes(mgc, EmbeddedResources.magic);
			Debug.WriteLine($"populating {mgc}");
		}

		Debug.WriteLine($"magic file: {mgc}");

		return mgc;
	}

	#region Implementation of IFileTypeResolver

	public IEnumerable<FileType> Resolve(byte[] rg)
	{
		// var buf1 = stream.ReadHeaderAsync(FileType.RSRC_HEADER_LEN);
		// buf1.Wait();
		// var buf  = buf1.Result;

		var sz = MagicNative.magic_buffer(Magic, rg, rg.Length);
		var s  = Marshal.PtrToStringAnsi(sz);
		return new[] { new FileType() { MediaType = s } };
	}

	#endregion

	public IEnumerable<FileType> Resolve(Stream stream)
	{
		var buf = stream.ReadHeader();
		return Resolve(buf);
	}

	public void Dispose()
	{
		MagicNative.magic_close(Magic);
	}
}