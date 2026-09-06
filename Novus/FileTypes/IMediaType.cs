// Author: Deci | Project: Novus | Name: IMediaType.cs
// Date: 2026/08/30 @ 03:08:30

using System.Net.Http.Headers;

namespace Novus.FileTypes;

public interface IMediaType
{

	MediaTypeSignature[] Signatures { get; }

	MediaTypeHeaderValue Value { get; }

}