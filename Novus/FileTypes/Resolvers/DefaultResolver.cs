using Novus.Streams;

namespace Novus.FileTypes.Resolvers;

public sealed class DefaultResolver : IMediaTypeResolver
{

	public void Dispose() { }

	public DefaultResolver() { }

	public IMediaType Resolve(byte[] rg, int l = MediaTypeUtilities.RSRC_HEADER_LEN)
	{
		return MediaTypeUtilities.Resolve(rg);
	}

	public IMediaType Resolve(Stream m, int l = MediaTypeUtilities.RSRC_HEADER_LEN)
	{
		return Resolve(m.ReadHeader(l: l), l);
	}

}