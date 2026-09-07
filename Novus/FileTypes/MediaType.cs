// Read S Novus MediaType.cs
// 2022-05-10 @ 2:49 AM

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http.Headers;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using Kantan.Utilities;

// ReSharper disable PossibleNullReferenceException

#nullable disable

// ReSharper disable InconsistentNaming

namespace Novus.FileTypes;

public class MediaType : IEquatable<MediaType>, IEqualityOperators<MediaType, MediaType, bool>, IMediaType
{

	[JsonPropertyName("name")]
	public MediaTypeHeaderValue Value { get; }

	public MediaTypeSignature[] Signatures { get; }

	internal MediaType() : this((string) null, []) { }

	public MediaType(MediaTypeHeaderValue value, IEnumerable<MediaTypeSignature> signatures)
	{
		Value      = value;
		Signatures = [.. signatures];
	}

	public MediaType(string value, IEnumerable<MediaTypeSignature> signatures) : this(MediaTypeHeaderValue.Parse(value), signatures) { }

	public bool CheckPattern(ReadOnlySpan<byte> input, ISet<byte> ignored = null)
	{
		foreach (var signature in Signatures) {
			if (MediaTypeUtilities.CheckPattern(input[signature.Offset..], signature.Pattern, signature.Mask, ignored)) {
				return true;
			}
		}

		return false;
	}


	public bool Equals(MediaType other)
		=> Value.Equals(other.Value);

	public override bool Equals(object obj)
		=> obj is MediaType other && Equals(other);

	public override int GetHashCode()
		=> HashCode.Combine(Value, Signatures);

	public static bool operator ==(MediaType left, MediaType right)
		=> left.Equals(right);

	public static bool operator !=(MediaType left, MediaType right)
		=> !left.Equals(right);

	public override string ToString()
	{
		return $"{Value} | {Signatures?.Length} signatures";
	}

}