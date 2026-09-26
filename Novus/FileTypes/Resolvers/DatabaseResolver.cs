using Novus.FileTypes.Media;
using Novus.Streams;
using Novus.Utilities.Converters;
using System.Buffers;
using System.IO.Pipelines;
using System.Text.Json;

namespace Novus.FileTypes.Resolvers;

public sealed class DatabaseResolver : IMediaTypeResolver
{

	private readonly Lazy<IMediaType[]> m_db;

	public DatabaseResolver(Lazy<IMediaType[]> db)
	{
		m_db = db;
	}

	public DatabaseResolver() { }

	public IMediaType[] All => m_db.Value;
	

	public static readonly IMediaTypeResolver Instance = new DatabaseResolver(new Lazy<IMediaType[]>(ReadDatabase, LazyThreadSafetyMode.PublicationOnly));

	private static IMediaType[] ReadDatabase()
	{
		// ReSharper disable once CoVariantArrayConversion
		return JsonSerializer.Deserialize<MediaType[]>(ER.File_types, MediaTypeUtilities.SerializerOptions);
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

	public IEnumerable<IMediaType> Find(string mediaType) =>
		from ft in All
		let mt = ft.Value.ToString()
		where mt == mediaType
		select ft;

	public IMediaType Resolve(byte[] rg, int l = MediaTypeUtilities.RSRC_HEADER_LEN)
		=> Resolve(rg.AsSpan());

	[CBN]
	public IMediaType Resolve(ReadOnlySpan<byte> rg)
	{
		foreach (var ft in All) {
			if (ft.CheckPattern(rg)) {
				return ft;
			}
		}

		return null;
	}

	public void Dispose() { }

	public async ValueTask<(IMediaType MediaType, Stream Body)> SniffAsync(Stream source, [CBN] string nameHint = null, CancellationToken ct = default)
	{
		// TODO
		var reader = PipeReader.Create(source, new StreamPipeReaderOptions(leaveOpen: true));

		ReadResult             result = await reader.ReadAtLeastAsync(MediaTypeUtilities.RSRC_HEADER_LEN, ct);
		ReadOnlySequence<byte> buffer = result.Buffer;

		int n = (int) Math.Min(MediaTypeUtilities.RSRC_HEADER_LEN, buffer.Length);

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