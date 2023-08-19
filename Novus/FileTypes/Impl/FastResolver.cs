namespace Novus.FileTypes.Impl;

public sealed class FastResolver : IFileTypeResolver
{
	public void Dispose() { }

	private FastResolver() { }

	public static readonly IFileTypeResolver Instance = new FastResolver();

	#region Implementation of IFileTypeResolver

	public IEnumerable<FileType> Resolve(byte[] rg)
		=> FileType.Resolve(rg);

	public Task<IEnumerable<FileType>> ResolveAsync(Stream m, CancellationToken ct = default)
		=> FileType.ResolveAsync(m, ct);

	public IEnumerable<FileType> Resolve(Stream m)
		=> FileType.Resolve(m);

	#endregion
}