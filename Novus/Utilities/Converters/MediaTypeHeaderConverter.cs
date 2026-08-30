// Author: Deci | Project: Novus | Name: MediaTypeHeaderConverter.cs
// Date: 2026/08/30 @ 02:08:32

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Novus.Utilities.Converters;

[UI]
public class MediaTypeHeaderConverter : JsonConverter<MediaTypeHeaderValue>
{

	public override MediaTypeHeaderValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var str = reader.GetString();

		return MediaTypeHeaderValue.TryParse(str, out var mhv) ? mhv : null;
	}

	public override void Write(Utf8JsonWriter writer, MediaTypeHeaderValue value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}

}