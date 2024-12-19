// Read S Novus FileType.cs
// 2022-05-10 @ 2:49 AM

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using Kantan.Utilities;

// ReSharper disable PossibleNullReferenceException

#nullable disable

// ReSharper disable InconsistentNaming

namespace Novus.FileTypes;

/// <remarks>
///     <a href="https://mimesniff.spec.whatwg.org/#matching-an-image-type-pattern">6.1</a>
/// </remarks>
[DAM(DAMT.All)]
public partial class FileType : IEquatable<FileType>
{

	[MN]
	public byte[] Mask { get; }

	[MN]
	public byte[] Pattern { get; }

	/// <summary>
	/// <c><see cref="Subtype"/>/<see cref="Type"/></c>
	/// </summary>
	public string MimeType { get; }

	/// <summary>
	/// First component of <see cref="MimeType"/>
	/// </summary>
	public string Type { get; }

	/// <summary>
	/// Second component of <see cref="MimeType"/>
	/// </summary>
	public string Subtype { get; }

	public int Offset { get; }

	public bool IsPartial => Mask is null && Pattern is null && MimeType is not null;

	public FileType() : this(null, null, null) { }

	public FileType(string mediaType) : this(null, null, mediaType) { }

	public FileType(byte[] mask, byte[] pattern, [MN] string mimeType, int offset = 0)
	{
		Mask     = mask;
		Pattern  = pattern;
		Offset   = offset;
		MimeType = mimeType;

		if (MimeType != null) {
			var split = MimeType.Split(MIME_TYPE_DELIM);
			Type    = split[0];
			Subtype = split[1];

		}
	}


	public bool CheckPattern(Span<byte> input, ISet<byte> ignored = null)
		=> CheckPattern(input, Pattern, Mask, ignored);


	public bool Equals(FileType other)
	{
		return MimeType == other.MimeType;
	}

	public override bool Equals(object obj)
	{
		return obj is FileType other && Equals(other);
	}

	public override int GetHashCode()
	{
		return (MimeType != null ? MimeType.GetHashCode() : 0);
	}

	public static bool operator ==(FileType left, FileType right)
		=> left.Equals(right);

	public static bool operator !=(FileType left, FileType right)
		=> !left.Equals(right);

	public override string ToString()
	{
		/*return $"{nameof(Type)}: {Type} | " +
		       $"{nameof(IsPartial)}: {IsPartial}";*/
		return MimeType;
	}

}