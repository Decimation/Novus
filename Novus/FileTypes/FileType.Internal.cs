// Author: Deci | Project: Novus | Name: FileType.Internal.cs
// Date: 2024/12/19 @ 00:12:37

using System.Diagnostics;
using System.Runtime.Caching;
using System.Text.Json.Nodes;
using Kantan.Diagnostics;
using Novus.Memory;
using Novus.Streams;
#nullable disable
namespace Novus.FileTypes;

public partial class FileType
{

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

	#endregion

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
			var offset  = jOffset == null ? 0 : Int32.Parse(jOffset.ToString());

			var ft = new FileType(Mem.ParseAOBString(mask), Mem.ParseAOBString(sig), mediaType, offset)
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

	}

	public static IEnumerable<FileType> FindInternal(string s)
	{
		return
			from ft in All
			let mt = ft.MimeType
			where mt == s || ft.Subtype == s || s == ft.Type
			select ft;
	}

	#endregion

	#region

	public const int RSRC_HEADER_LEN = 1445;

	/// <remarks>
	///     <a href="https://mimesniff.spec.whatwg.org/#sniffing-a-mislabeled-binary-resource">7.2</a>
	/// </remarks>
	public static string IsBinaryResource(byte[] input)
	{

		switch (input) {
			case { Length: >= 2 } when input.SequenceEqual(s_utf16BE_BOM) || input.SequenceEqual(s_utf16LE_BOM):
				return MT_TEXT_PLAIN;

			case { Length: >= 3 } when input.SequenceEqual(s_utf8_BOM):
				return MT_TEXT_PLAIN;
		}

		if (!input.Any(IsBinaryDataByte)) {
			return MT_TEXT_PLAIN;
		}

		return MT_APPLICATION_OCTET_STREAM;
	}

	/// <remarks>
	///     <a href="https://mimesniff.spec.whatwg.org/#read-the-resource-header">5.2</a>
	/// </remarks>
	public static async Task<Memory<byte>> ReadResourceHeaderAsync(Stream input, CancellationToken ct = default)
	{
		Memory<byte> buf = new byte[RSRC_HEADER_LEN];
		var          ms  = await input.ReadAsync(buf, ct);
		return buf[0..ms];
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

	public static FileType Resolve(Stream s, int l = FileType.RSRC_HEADER_LEN)
	{
		// var          j  = All.Max(x => x.Pattern.Length);
		Memory<byte> rg = s.ReadHeader(l: l);

		return ResolveInternal(rg);

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

	}

	public static async Task<FileType> ResolveAsync(Stream s, int l = FileType.RSRC_HEADER_LEN,
	                                                CancellationToken ct = default)
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

		Memory<byte> rg = await s.ReadHeaderAsync(ct: ct, l: l);

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