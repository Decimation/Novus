// Author: Deci | Project: Novus | Name: MediaType.Internal.cs
// Date: 2024/12/19 @ 00:12:37

using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Runtime.Caching;
using System.Text.Json;
using System.Text.Json.Nodes;
using Kantan.Diagnostics;
using Novus.Memory;
using Novus.Utilities.Converters;

// ReSharper disable UnusedMember.Global

#nullable disable
namespace Novus.FileTypes;

/// <summary>
/// Provides utilities for interacting with media types (MIME types)
/// </summary>
/// <seealso cref="MediaTypeHeaderValue"/>
/// <seealso cref="MediaTypeNames"/>
/// <seealso cref="IMediaType"/>
public static class MediaTypeUtilities
{

#region

	static MediaTypeUtilities()
	{
		s_all = new Lazy<IMediaType[]>(ReadDatabase, LazyThreadSafetyMode.PublicationOnly);
	}

	private static readonly Lazy<IMediaType[]> s_all;

	public static IMediaType[] All => s_all.Value;

	internal static readonly JsonSerializerOptions SerializerOptions = new()
	{
		WriteIndented               = true,
		PropertyNameCaseInsensitive = true,
		Converters =
		{
			new MediaTypeHeaderConverter(),
			new ByteStringConverter()
		}
	};


#region

	/// <summary>
	/// <a href="https://mimesniff.spec.whatwg.org/#sniffing-a-mislabeled-binary-resource">7.2.2</a>
	/// </summary>
	private static readonly byte[] s_utf16BE_BOM = [0xFE, 0xFF];

	/// <summary>
	/// <a href="https://mimesniff.spec.whatwg.org/#sniffing-a-mislabeled-binary-resource">7.2.2</a>
	/// </summary>
	private static readonly byte[] s_utf16LE_BOM = [0xFF, 0xFE];

	/// <summary>
	/// <a href="https://mimesniff.spec.whatwg.org/#sniffing-a-mislabeled-binary-resource">7.2.3</a>
	/// </summary>
	private static readonly byte[] s_utf8_BOM = [0xEF, 0xBB, 0xBF];

	public const char MIME_TYPE_DELIM = '/';
	public const int  RSRC_HEADER_LEN = 1445;

#endregion

	/// <summary>
	///     Reads <see cref="MediaType" /> from <see cref="ER.File_types" />
	/// </summary>
	private static IMediaType[] ReadDatabase()
	{

		return JsonSerializer.Deserialize<MediaType[]>(ER.File_types, SerializerOptions);
	}

#endregion

#region

	/// <remarks>
	///     <a href="https://mimesniff.spec.whatwg.org/#sniffing-a-mislabeled-binary-resource">7.2</a>
	/// </remarks>
	public static string IsBinaryResource(byte[] input)
	{

		switch (input) {
			case { Length: >= 2 } when input.SequenceEqual(s_utf16BE_BOM) || input.SequenceEqual(s_utf16LE_BOM):
			case { Length: >= 3 } when input.SequenceEqual(s_utf8_BOM):
				return MediaTypeNames.Text.Plain;

		}

		if (!input.Any(IsBinaryDataByte)) {
			return MediaTypeNames.Text.Plain;
		}

		return MediaTypeNames.Application.Octet;
	}

	/// <remarks>
	///     <a href="https://mimesniff.spec.whatwg.org/#terminology">3</a>
	/// </remarks>
	public static bool IsBinaryDataByte(byte b)
	{
		return b is >= 0x00 and <= 0x08    // NUL to BS
			       or 0x0B                 // VT
			       or >= 0x0E and <= 0x1A  // SO to SUB
			       or >= 0x1C and <= 0x1F; // FS to US
	}

	/// <remarks>
	///     <a href="https://mimesniff.spec.whatwg.org/#matching-a-mime-type-pattern">6</a>
	/// </remarks>
	public static bool CheckPattern(Span<byte> input, Span<byte> pattern, Span<byte> mask, ISet<byte> ignored = null)
	{
		ArgumentOutOfRangeException.ThrowIfNotEqual(pattern.Length, mask.Length);

		ignored ??= Enumerable.Empty<byte>().ToHashSet();

		// ArgumentOutOfRangeException.ThrowIfLessThan(input.Length, pattern.Length);

		if (input.Length < pattern.Length) {
			return false;
		}

		int s = 0;

		while (s < input.Length) {
			if (!ignored.Contains(input[s])) {
				break;
			}

			s++;
		}

		int p = 0;

		while (p < pattern.Length) {
			int md = input[s] & mask[p];

			if (md != pattern[p]) {
				return false;
			}

			s++;
			p++;
		}

		return true;
	}

	/// <remarks>
	///     <a href="https://mimesniff.spec.whatwg.org/#read-the-resource-header">5.2</a>
	/// </remarks>
	public static async ValueTask<Memory<byte>> ReadResourceHeaderAsync(Stream input, CancellationToken ct = default)
	{
		Memory<byte> buf = new byte[RSRC_HEADER_LEN];
		var          ms  = await input.ReadAsync(buf, ct);
		return buf[0..ms];
	}

#endregion

	public static IEnumerable<IMediaType> Find(string mediaType)
		=> from ft in All
		   let mt = ft.Value.ToString()
		   where mt == mediaType
		   select ft;

	[CBN]
	public static IMediaType Resolve(in Memory<byte> rg)
	{
		foreach (var ft in All) {
			if (ft is MediaType { } rt && rt.CheckPattern(rg.Span)) {
				return ft;
			}
		}

		return null;
	}

	extension(MediaTypeHeaderValue value)
	{

		[MN]
		public string Type => value.Split().Type;

		[MN]
		public string Subtype => value.Split().Subtype;

		public (string Type, string Subtype) Split()
		{
			var split = value.MediaType?.Split(MediaTypeUtilities.MIME_TYPE_DELIM, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

			return split?.Length >= 2 ? (split[0], split[1]) : (null, null);

		}

	}

	private const string CHARSET_ISO_8859_1 = "ISO-8859-1";

	/// <summary>
	/// <a href="https://mimesniff.spec.whatwg.org/#interpreting-the-resource-metadata">5.1 2.2.2</a>
	/// </summary>
	public static readonly MediaTypeHeaderValue[] ApacheBugContentTypes =
	[
		new(MediaTypeNames.Text.Plain),
		new(MediaTypeNames.Text.Plain, CHARSET_ISO_8859_1),
		new(MediaTypeNames.Text.Plain, CHARSET_ISO_8859_1.ToLower()),
		new(MediaTypeNames.Text.Plain, "UTF-8"),
	];

}