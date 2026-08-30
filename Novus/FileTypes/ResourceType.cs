// Read S Novus ResourceType.cs
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

public class ResourceType : IEquatable<ResourceType>, IEqualityOperators<ResourceType, ResourceType, bool>, IResourceType
{

	[JsonIgnore]
	public string SuppliedType { get; }

	[JsonPropertyName("name")]
	public MediaTypeHeaderValue MediaType { get; set;}

	public ResourceTypeSignature[] Signatures { get; set; }

	public ResourceType() : this([], null) { }

	public ResourceType(string suppliedType) : this([], suppliedType) { }

	public ResourceType(IEnumerable<ResourceTypeSignature> signatures, [MN] string suppliedType)
	{
		SuppliedType = suppliedType;

		if (SuppliedType != null) {
			MediaType = MediaTypeHeaderValue.Parse(suppliedType);
		}

		Signatures = [.. signatures];
	}


	public bool CheckPattern(Span<byte> input, ISet<byte> ignored = null)
	{
		foreach (var signature in Signatures) {
			if (ResourceTypeUtilities.CheckPattern(input[signature.Offset..], signature.Pattern, signature.Mask, ignored)) {
				return true;
			}
		}

		return false;
	}


	public bool Equals(ResourceType other)
		=> MediaType.Equals(other.MediaType);

	public override bool Equals(object obj)
		=> obj is ResourceType other && Equals(other);

	public override int GetHashCode()
		=> (MediaType != null ? MediaType.GetHashCode() : 0);

	public static bool operator ==(ResourceType left, ResourceType right)
		=> left.Equals(right);

	public static bool operator !=(ResourceType left, ResourceType right)
		=> !left.Equals(right);

	public override string ToString()
	{
		return MediaType.ToString();
	}

}