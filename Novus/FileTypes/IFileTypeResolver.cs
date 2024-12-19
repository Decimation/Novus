using Kantan.Utilities;
using Novus.FileTypes.Impl;
using Novus.Streams;
using Novus.Win32;

namespace Novus.FileTypes;

public interface IFileTypeResolver : IDisposable
{

	public FileType Resolve(byte[] rg, int l = FileType.RSRC_HEADER_LEN);

	public async Task<FileType> ResolveAsync(Stream m, int l = FileType.RSRC_HEADER_LEN, CancellationToken ct = default)
	{
		var bytes = await m.ReadHeaderAsync(ct: ct, l: l);
		return Resolve(bytes);
	}

	public FileType Resolve(Stream m, int l = FileType.RSRC_HEADER_LEN)
	{
		var header = m.ReadHeader(l: l);
		return Resolve(header);
	}

	public static IFileTypeResolver Default { get; set; } = FastResolver.Instance; //todo

	/*
		| Method |        Mean |     Error |    StdDev |
		|------- |------------:|----------:|----------:|
		| Urlmon |  2,123.9 ns |  19.75 ns |  16.49 ns |
		|  Magic | 17,845.1 ns | 142.41 ns | 111.18 ns |
		|   Fast |    460.9 ns |   9.21 ns |  10.97 ns |
	 */

}