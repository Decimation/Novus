using System.Reflection;
using Kantan.Diagnostics;
using Kantan.Utilities;

// ReSharper disable InconsistentNaming

namespace Novus.FileTypes;

/// <remarks><a href="https://mimesniff.spec.whatwg.org/#matching-an-image-type-pattern">6.1</a></remarks>
public readonly struct FileType : IEquatable<FileType>
{
	public byte[] Mask { get; init; }

	public byte[] Pattern { get; init; }

	public string MediaType { get; init; }

	public bool IsPartial => Mask is null && Pattern is null && MediaType is not null;

	/*public HttpTypeSignature()
	{
		Mask    = null;
		Pattern = null;
		Type    = null;
	}*/

	// todo: move to Embedded Resources

	#region

	public static readonly FileType gif = new()
	{
		Pattern   = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61, },
		Mask      = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, },
		MediaType = "image/gif"
	};

	public static readonly FileType gif2 = new()
	{
		Pattern   = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61, },
		Mask      = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, },
		MediaType = "image/gif",
	};

	public static readonly FileType webp = new()
	{
		Pattern   = new byte[] { 0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50, 0x56, 0x50, },
		Mask      = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, },
		MediaType = "image/webp"
	};

	public static readonly FileType png = new()
	{
		Pattern   = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, },
		Mask      = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, },
		MediaType = "image/png"
	};

	public static readonly FileType jpg = new()
	{
		Pattern   = new byte[] { 0xFF, 0xD8, 0xFF },
		Mask      = new byte[] { 0xFF, 0xFF, 0xFF },
		MediaType = "image/jpeg"
	};

	public static readonly FileType bmp = new()
	{
		Pattern   = new byte[] { 0x42, 0x4D },
		Mask      = new byte[] { 0xFF, 0xFF },
		MediaType = "image/bmp",

	};

	#endregion

	public bool IsType(string p) => IsType(p, MediaType);

	static FileType()
	{
		All = typeof(FileType)
		      .GetFields(BindingFlags.Static | BindingFlags.Public)
		      .Where(f => f.FieldType == typeof(FileType))
		      .Select(x => (FileType) x.GetValue(null)!)
		      .ToArray();
	}

	public static readonly FileType[] All;

	public const int RSRC_HEADER_LEN = 1445;

	/// <remarks>
	///     <a href="https://mimesniff.spec.whatwg.org/#sniffing-a-mislabeled-binary-resource">7.2</a>
	/// </remarks>
	public static string IsBinaryResource(byte[] input)
	{
		byte[] seq1a = { 0xFE, 0xFF };
		byte[] seq1b = { 0xFF, 0xFE };
		byte[] seq2  = { 0xEF, 0xBB, 0xBF };

		switch (input) {
			case { Length: >= 2 } when input.SequenceEqual(seq1a) || input.SequenceEqual(seq1b):
				return MT_TEXT_PLAIN;
			case { Length: >= 3 } when input.SequenceEqual(seq2):
				return MT_TEXT_PLAIN;
		}

		if (!input.Any(IsBinaryDataByte)) {
			return MT_TEXT_PLAIN;
		}

		return MT_APPLICATION_OCTET_STREAM;
	}

	/// <remarks>
	///     <a href="https://mimesniff.spec.whatwg.org/#terminology">3</a>
	/// </remarks>
	public static bool IsBinaryDataByte(byte b)
	{
		return b is >= 0x00 and <= 0x08 or 0x0B or >= 0x0E and <= 0x1A or >= 0x1C and <= 0x1F;
	}

	public static bool CheckPattern(byte[] input, FileType s, ISet<byte> ignored = null)
		=> CheckPattern(input, s.Pattern, s.Mask, ignored);

	/// <remarks>
	///     <a href="https://mimesniff.spec.whatwg.org/#matching-a-mime-type-pattern">6</a>
	/// </remarks>
	public static bool CheckPattern(byte[] input, byte[] pattern, byte[] mask, ISet<byte> ignored = null)
	{
		Require.Assert(pattern.Length == mask.Length);

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

	public static IEnumerable<FileType> Resolve(Stream s)
	{
		return Resolve(s.ReadHeader());
	}

	public static async Task<IEnumerable<FileType>> ResolveAsync(Stream s)
	{
		return Resolve(await s.ReadHeaderAsync());
	}

	public static IEnumerable<FileType> Resolve(byte[] h)
	{
		return All.Where(t => CheckPattern(h, t))
		          .DistinctBy(x => x.MediaType);
	}

	public static bool IsType(string p, string mt)
	{
		return mt?.Split('/').FirstOrDefault()?.ToLower() == p.ToLower();
	}

	#region Overrides of ValueType

	#region Equality members

	public bool Equals(FileType other)
	{
		return MediaType == other.MediaType;
	}

	public override bool Equals(object obj)
	{
		return obj is FileType other && Equals(other);
	}

	public override int GetHashCode()
	{
		return (MediaType != null ? MediaType.GetHashCode() : 0);
	}

	public static bool operator ==(FileType left, FileType right) => left.Equals(right);

	public static bool operator !=(FileType left, FileType right) => !left.Equals(right);

	#endregion

	#endregion

	public override string ToString()
	{
		/*return $"{nameof(Type)}: {Type} | " +
		       $"{nameof(IsPartial)}: {IsPartial}";*/
		return MediaType;
	}

	#region

	public const string MT_TEXT_PLAIN               = $"{MT_TEXT}/plain";
	public const string MT_APPLICATION_OCTET_STREAM = $"{MT_APPLICATION}/octet-stream";

	public const string MT_IMAGE       = "image";
	public const string MT_TEXT        = "text";
	public const string MT_APPLICATION = "application";
	public const string MT_VIDEO       = "video";
	public const string MT_AUDIO       = "audio";
	public const string MT_MODEL       = "model";

	#endregion
}