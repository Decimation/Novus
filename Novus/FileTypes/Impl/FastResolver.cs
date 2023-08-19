namespace Novus.FileTypes.Impl;

public sealed class FastResolver : IFileTypeResolver
{
	public void Dispose() { }

	private FastResolver() { }

	public static readonly IFileTypeResolver Instance = new FastResolver();

	public FileType Resolve(byte[] rg)
		=> FileType.Resolve(rg);

	public Task<FileType> ResolveAsync(Stream m, CancellationToken ct = default)
		=> FileType.ResolveAsync(m, ct);

	public FileType Resolve(Stream m)
		=> FileType.Resolve(m);
}