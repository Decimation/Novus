// Author: Deci | Project: Novus | Name: MediaTypeSignature.cs
// Date: 2026/08/30 @ 02:08:49

using Novus.Utilities.Converters;
using System.Text.Json.Serialization;

namespace Novus.FileTypes;

public class MediaTypeSignature
{
	[JsonConverter(typeof(ByteStringConverter))]
	public byte[] Pattern { get; set; }

	[JsonConverter(typeof(ByteStringConverter))]
	public byte[] Mask { get; set; }

	public int Offset { get; set; }

	/*[JsonConstructor]
	public MediaTypeSignature(byte[] mask, byte[] pattern, int offset = 0)
	{
		Mask    = mask;
		Pattern = pattern;
		Offset  = offset;
	}*/

	public override string ToString()
	{
		return $"{Offset} | {Pattern?.Length} | {Mask?.Length}";
	}

}