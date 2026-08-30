// Author: Deci | Project: Novus | Name: ByteStringConverter.cs
// Date: 2026/08/30 @ 02:08:18

using System.Text.Json;
using System.Text.Json.Serialization;
using Novus.Memory;

namespace Novus.Utilities.Converters;

public class ByteStringConverter : JsonConverter<byte[]>
{

	public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var str = reader.GetString();
		var aob = Mem.ParseAOBString(str);
		return aob;
	}

	public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}

}