using Novus.Streams;

namespace Novus.FileTypes.Resolvers;

public sealed class DefaultResolver : IResourceTypeResolver
{

	public void Dispose() { }

	public DefaultResolver() { }

	public IResourceType Resolve(byte[] rg, int l = ResourceTypeUtilities.RSRC_HEADER_LEN)
	{
		return ResourceTypeUtilities.Resolve(rg);
	}

	public IResourceType Resolve(Stream m, int l = ResourceTypeUtilities.RSRC_HEADER_LEN)
	{
		return Resolve(m.ReadHeader(l: l), l);
	}

}