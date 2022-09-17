namespace Novus.FileTypes.Impl;

public sealed class FastResolver : IFileTypeResolver
{
    #region Implementation of IDisposable

    public void Dispose() { }

    #endregion

    private FastResolver() { }

    public static readonly IFileTypeResolver Instance = new FastResolver();

    #region Implementation of IFileTypeResolver

    public IEnumerable<FileType> Resolve(byte[] rg)
    {
        return FileType.Resolve(rg);
    }

    public async Task<IEnumerable<FileType>> ResolveAsync(Stream m)
    {
        return await FileType.ResolveAsync(m);
    }

    public IEnumerable<FileType> Resolve(Stream m)
    {
        return FileType.Resolve(m);
    }

    #endregion
}