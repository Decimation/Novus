// Author: Deci | Project: Novus | Name: IResourceType.cs
// Date: 2026/08/30 @ 03:08:30

using System.Net.Http.Headers;

namespace Novus.FileTypes;

public interface IResourceType
{

	ResourceTypeSignature[] Signatures { get; }

	MediaTypeHeaderValue MediaType { get; }

}