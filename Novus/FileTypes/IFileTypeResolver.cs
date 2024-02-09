using Kantan.Utilities;
using Novus.FileTypes.Impl;
using Novus.Streams;

namespace Novus.FileTypes;

public interface IFileTypeResolver : IDisposable
{
	public FileType Resolve(byte[] rg);

	public async Task<FileType> ResolveAsync(Stream m, CancellationToken ct = default)
	{
		return Resolve(await m.ReadHeaderAsync(ct: ct));
	}

	public FileType Resolve(Stream m)
	{
		return Resolve(m.ReadHeader());
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

