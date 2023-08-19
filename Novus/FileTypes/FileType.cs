// Read S Novus FileType.cs
// 2022-05-10 @ 2:49 AM

global using DAM = System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembersAttribute;
global using DAMT = System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes;
global using MN = System.Diagnostics.CodeAnalysis.MaybeNullAttribute;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.Caching;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using Kantan.Diagnostics;
using Kantan.Utilities;
using Novus.Streams;

// ReSharper disable PossibleNullReferenceException

#nullable disable
// ReSharper disable InconsistentNaming

namespace Novus.FileTypes;

/// <remarks>
///     <a href="https://mimesniff.spec.whatwg.org/#matching-an-image-type-pattern">6.1</a>
/// </remarks>
[DAM(DAMT.All)]
public readonly struct FileType : IEquatable<FileType>
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
		Mask      = mask;
		Pattern   = pattern;
		Offset    = offset;
		MimeType = mimeType;

		if (MimeType != null) {
			var split = MimeType.Split(MIME_TYPE_DELIM);
			Type    = split[0];
			Subtype = split[1];

		}
	}

	#region

	static FileType()
	{
		/*All = typeof(FileType)
		      .GetFields(BindingFlags.Static | BindingFlags.Public)
		      .Where(f => f.FieldType == typeof(FileType))
		      .Select(x => (FileType) x.GetValue(null)!)
		      .ToArray();
		      */

		All   = ReadDatabase();
		Image = All.Where(a => MT_IMAGE == a.Type).ToArray();
		Video = All.Where(a => MT_VIDEO == a.Type).ToArray();

		Trace.WriteLine($"Read {All.Length} file signatures | {Image.Length} image | {Video.Length} video");
	}

	private static readonly ObjectCache Cache = MemoryCache.Default;

	public static readonly FileType[] All;
	public static readonly FileType[] Image;
	public static readonly FileType[] Video;

	/// <summary>
	///     Reads <see cref="FileType" /> from <see cref="ER.File_types" />
	/// </summary>
	public static FileType[] ReadDatabase()
	{
		var jNode  = JsonNode.Parse(ER.File_types);
		var jArray = jNode.AsArray();
		var rg     = new FileType[jArray.Count];

		for (int i = 0; i < jArray.Count; i++) {
			var r = jArray[i];
			var o = r.AsObject();

			var mask      = o[ER.K_Mask].ToString();
			var sig       = o[ER.K_Pattern].ToString();
			var mediaType = o[ER.K_Name].ToString();

			var jOffset = o[ER.K_Offset];
			var offset  = jOffset == null ? 0 : int.Parse(jOffset.ToString());

			var ft = new FileType(M.ReadAOBString(mask), M.ReadAOBString(sig), mediaType, offset)
				{ };
			rg[i] = ft;
		}

		return rg;
	}

	public static IEnumerable<FileType> Find(string name)
	{
		var query = Cache.AddOrGetExisting(name, FindInternal(name), new CacheItemPolicy() { })
		            // ReSharper disable once ConstantNullCoalescingCondition
		            ?? Cache[name];

		return (IEnumerable<FileType>) query;

		static IEnumerable<FileType> FindInternal(string s)
		{
			return from ft in All
			       let mt = ft.MimeType
			       where mt == s || ft.Subtype == s || s == ft.Type
			       select ft;
		}
	}

	#endregion

	#region

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

	public bool CheckPattern(Span<byte> input, ISet<byte> ignored = null)
		=> CheckPattern(input, Pattern, Mask, ignored);

	public static bool CheckPattern(Span<byte> input, FileType s, ISet<byte> ignored = null)
		=> s.CheckPattern(input, ignored);

	/// <remarks>
	///     <a href="https://mimesniff.spec.whatwg.org/#matching-a-mime-type-pattern">6</a>
	/// </remarks>
	public static bool CheckPattern(Span<byte> input, Span<byte> pattern, Span<byte> mask,
	                                ISet<byte> ignored = null)
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

	public static FileType Resolve(Stream s)
	{
		// var          j  = All.Max(x => x.Pattern.Length);
		Memory<byte> rg = s.ReadHeader();

		return ResolveInternal(rg)

		/* 2-21-23

		| Method |      Mean |     Error |    StdDev |
		|------- |----------:|----------:|----------:|
		| Urlmon |  2.118 us | 0.0135 us | 0.0119 us |
		|  Magic | 17.918 us | 0.2329 us | 0.2179 us |
		|   Fast | 18.167 us | 0.0823 us | 0.0729 us |
		 */
		/*return All.Where(t =>
			{

				unsafe {
					Span<byte> sp = stackalloc byte[256];
					int        i  = s.Read(sp);
					return CheckPattern(sp, t);
				}
			})
			.DistinctBy(x => x.MediaType)*/
		;
	}

	public static async Task<FileType> ResolveAsync(Stream s, CancellationToken ct = default)
	{
		// return Resolve(await s.ReadHeaderAsync(ct: ct));

		// var          j  = All.Max(x => x.Pattern.Length);

		/*
		 * BenchmarkDotNet v0.13.6, Windows 10 (10.0.19043.2364/21H1/May2021Update)
AMD Ryzen 7 2700X, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.304
  [Host]     : .NET 7.0.7 (7.0.723.27404), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.7 (7.0.723.27404), X64 RyuJIT AVX2

| Method |        Job |                Toolchain |     Mean |   Error |  StdDev |
|------- |----------- |------------------------- |---------:|--------:|--------:|
|   Fast | DefaultJob |                  Default | 396.5 ns | 7.88 ns | 9.07 ns |
|   Fast | Job-CSEWXA | InProcessNoEmitToolchain | 414.6 ns | 6.38 ns | 5.97 ns |
		 */

		Memory<byte> rg = await s.ReadHeaderAsync(ct: ct);

		return ResolveInternal(rg);
	}

	private static FileType ResolveInternal(in Memory<byte> rg)
	{
		foreach (FileType ft in All) {
			if (ft.CheckPattern(rg[ft.Offset..].Span)) {
				return ft;
			}
		}

		return default;
	}

	public static FileType Resolve(byte[] h)
	{
		return All.FirstOrDefault(t => t.CheckPattern(h));
	}

	#endregion

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

	#region

	public const string MT_TEXT_PLAIN               = $"{MT_TEXT}/plain";
	public const string MT_APPLICATION_OCTET_STREAM = $"{MT_APPLICATION}/octet-stream";

	public const string MT_IMAGE       = "image";
	public const string MT_TEXT        = "text";
	public const string MT_APPLICATION = "application";
	public const string MT_VIDEO       = "video";
	public const string MT_AUDIO       = "audio";
	public const string MT_MODEL       = "model";

	private const char MIME_TYPE_DELIM = '/';

	#endregion
}