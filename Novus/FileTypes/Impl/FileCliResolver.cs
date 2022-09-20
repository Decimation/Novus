using System.Diagnostics;

namespace Novus.FileTypes.Impl;

public sealed class FileCliResolver : IFileTypeResolver
{
	public void Dispose() { }

	public IEnumerable<FileType> Resolve(byte[] rg)
	{
		return ResolveAsync(new MemoryStream(rg)).Result; //todo
	}

	private FileCliResolver()
	{

	}

	public static readonly IFileTypeResolver Instance = new FileCliResolver();

	public async Task<IEnumerable<FileType>> ResolveAsync(Stream m)
	{

		// IFlurlResponse res = await url.GetAsync();

		try
		{
			var hdr = new byte[0xFF];
			var i2 = await m.ReadAsync(hdr, 0, hdr.Length);

			string s = Path.GetTempFileName();

			await File.WriteAllBytesAsync(s, hdr);

			//@"C:\msys64\usr\bin\file.exe"
			// var name   = SearchInPath("file.exe");

			// var args = $"{s} -i";

			var args = $"{s}";

			var proc1 = new Process()
			{
				StartInfo =
				{
					FileName  = ER.EXE_FILE,
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

			var sz = await proc.StandardOutput.ReadToEndAsync();

			await proc.WaitForExitAsync();
			var output = sz;

			File.Delete(s);

			return new[] { new FileType() { MediaType = output } };
		}
		catch (Exception)
		{
			return null;
		}
	}
}