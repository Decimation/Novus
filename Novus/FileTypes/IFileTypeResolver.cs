using Kantan.Utilities;
using Novus.FileTypes.Impl;

namespace Novus.FileTypes;

public interface IFileTypeResolver : IDisposable
{
	public IEnumerable<FileType> Resolve(byte[] rg);

	public async Task<IEnumerable<FileType>> ResolveAsync(Stream m)
	{
		return Resolve(await m.ReadHeaderAsync());
	}

	public IEnumerable<FileType> Resolve(Stream m)
	{
		return Resolve(m.ReadHeader());
	}

	public static IFileTypeResolver Default { get; set; } = UrlmonResolver.Instance; //todo
}