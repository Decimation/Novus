namespace Novus.FileTypes.Impl;

public sealed class FastResolver : IFileTypeResolver
{
	public void Dispose() { }

	private FastResolver() { }

	public static readonly IFileTypeResolver Instance = new FastResolver();

	public FileType Resolve(byte[] rg, int l = FileType.RSRC_HEADER_LEN)
		=> FileType.Resolve(rg);

	public Task<FileType> ResolveAsync(Stream m, int l = FileType.RSRC_HEADER_LEN, CancellationToken ct = default)
		=> FileType.ResolveAsync(m, l, ct);

	public FileType Resolve(Stream m, int l = FileType.RSRC_HEADER_LEN)
		=> FileType.Resolve(m, l: l);

}