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

	[JsonIgnore]
	public string SuppliedType { get;set; }

	[JsonPropertyName("name")]
	public MediaTypeHeaderValue Value { get; set; }

	public MediaTypeSignature[] Signatures { get; set; }

	public MediaType() : this(null, []) { }

	public MediaType(string suppliedType) : this(suppliedType, []) { }

	public MediaType([MN] string suppliedType, IEnumerable<MediaTypeSignature> signatures)
	{
		SuppliedType = suppliedType;

		if (SuppliedType != null) {
			Value = MediaTypeHeaderValue.Parse(suppliedType);
		}

		Signatures = [.. signatures];
	}


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
		=> HashCode.Combine(SuppliedType, Value, Signatures);

	public static bool operator ==(MediaType left, MediaType right)
		=> left.Equals(right);

	public static bool operator !=(MediaType left, MediaType right)
		=> !left.Equals(right);

	public override string ToString()
	{
		return $"[{SuppliedType}] | {Value}";
	}

}