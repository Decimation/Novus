﻿using System.Diagnostics;

namespace Novus.FileTypes.Impl;

[Obsolete($"Use other {nameof(IFileTypeResolver)}")]
public sealed class FileCliResolver : IFileTypeResolver
{

	public void Dispose() { }

	public FileType Resolve(byte[] rg, int l = FileType.RSRC_HEADER_LEN)
	{
		return ResolveAsync(new MemoryStream(rg)).Result; //todo
	}

	private FileCliResolver() { }

	public static readonly IFileTypeResolver Instance = new FileCliResolver();

	public async Task<FileType> ResolveAsync(Stream m, int l = FileType.RSRC_HEADER_LEN, CancellationToken ct = default)
	{

		// IFlurlResponse res = await url.GetAsync();

		try {
			var hdr = new byte[0xFF];
			var i2  = await m.ReadAsync(hdr, 0, hdr.Length, ct);

			string s = Path.GetTempFileName();

			await File.WriteAllBytesAsync(s, hdr, ct);

			//@"C:\msys64\usr\bin\file.exe"
			// var name   = SearchInPath("file.exe");

			// var args = $"{s} -i";

			var args = $"{s}";

			var proc1 = new Process()
			{
				StartInfo =
				{
					FileName  = ER.E_File,
					Arguments = args,

					RedirectStandardInput  = false,
					RedirectStandardOutput = true,
					RedirectStandardError  = true,

					UseShellExecute = false,

					// WindowStyle     = ProcessWindowStyle.Hidden,
				},

				EnableRaisingEvents = true,
			};

			using Process proc = proc1;

			proc.Start();

			var sz = await proc.StandardOutput.ReadToEndAsync(ct);

			await proc.WaitForExitAsync(ct);
			var output = sz;

			File.Delete(s);

			return new FileType(output) { };
		}
		catch (Exception) {
			return default;
		}
	}

}