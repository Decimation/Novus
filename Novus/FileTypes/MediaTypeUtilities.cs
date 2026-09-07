// Author: Deci | Project: Novus | Name: MediaType.Internal.cs
// Date: 2024/12/19 @ 00:12:37

using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
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
	public static bool CheckPattern(ReadOnlySpan<byte> input, ReadOnlySpan<byte> pattern, ReadOnlySpan<byte> mask, ISet<byte> ignored = null)
	{
		ArgumentOutOfRangeException.ThrowIfNotEqual(pattern.Length, mask.Length);

		ignored ??= Enumerable.Empty<byte>().ToHashSet();

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

#endregion

	public static IEnumerable<IMediaType> Find(string mediaType)
		=>
			from ft in All
			let mt = ft.Value.ToString()
			where mt == mediaType
			select ft;

	[CBN]
	public static IMediaType Resolve(ReadOnlySpan<byte> rg)
	{
		foreach (var ft in All) {
			if (ft.CheckPattern(rg)) {
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
			var split = value.MediaType?.Split(MIME_TYPE_DELIM, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

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

	public static async ValueTask<(IMediaType MediaType, Stream Body)> SniffAsync(Stream source, [CBN] string nameHint = null, CancellationToken ct = default)
	{
		var reader = PipeReader.Create(source, new StreamPipeReaderOptions(leaveOpen: true));

		ReadResult             result = await reader.ReadAtLeastAsync(RSRC_HEADER_LEN, ct);
		ReadOnlySequence<byte> buffer = result.Buffer;

		int n = (int) Math.Min(RSRC_HEADER_LEN, buffer.Length);

		IMediaType mediaType = nameHint != null ? Find(nameHint).FirstOrDefault() : null;

		ReadOnlySpan<byte> buf;

		if (buffer.First.Length >= n) {
			buf = buffer.First.Span[..n];

			// mediaType?.SuppliedType = nameHint;
		}
		else {
			// Sequence is segmented; flatten the header.
			byte[] tmp = ArrayPool<byte>.Shared.Rent(n);

			try {
				buffer.Slice(0, n).CopyTo(tmp);
				buf = tmp.AsSpan(0, n);

				// mediaType               = Resolve(buf);
				// mediaType?.SuppliedType = nameHint;
			}
			finally {
				// todo: premature release?
				ArrayPool<byte>.Shared.Return(tmp);
			}
		}

		mediaType = mediaType != null && mediaType.CheckPattern(buf) ? mediaType : Resolve(buf);

		// Examined everything, consumed nothing — bytes remain available to the caller.
		reader.AdvanceTo(buffer.Start, buffer.End);

		return (mediaType, reader.AsStream());
	}

}