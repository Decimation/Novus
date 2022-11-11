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

#nullable disable
// ReSharper disable InconsistentNaming

namespace Novus.FileTypes;

/// <remarks><a href="https://mimesniff.spec.whatwg.org/#matching-an-image-type-pattern">6.1</a></remarks>
public readonly struct FileType : IEquatable<FileType>
{
	[MN]
	public byte[] Mask { get; init; }

	[MN]
	public byte[] Pattern { get; init; }

	public string MediaType { get; init; }

	public bool IsPartial => Mask is null && Pattern is null && MediaType is not null;

	public static IEnumerable<FileType> Find(string name)
	{
		var query = Cache.AddOrGetExisting(name, FindInternal(name), new CacheItemPolicy() { })
		            // ReSharper disable once ConstantNullCoalescingCondition
		            ?? Cache[name];
		
		return (IEnumerable<FileType>) query;

		static IEnumerable<FileType> FindInternal(string s)
		{
			return from ft in All
			       let mt = ft.MediaType
			       where mt == s || mt.Split(MIME_TYPE_DELIM).LastOrDefault() == s
			                     || ft.IsType(s)
			       select ft;
		}
	}

	private static readonly ObjectCache Cache = MemoryCache.Default;

	public bool IsType(string p) => IsType(p, MediaType);

	static FileType()
	{
		/*All = typeof(FileType)
		      .GetFields(BindingFlags.Static | BindingFlags.Public)
		      .Where(f => f.FieldType == typeof(FileType))
		      .Select(x => (FileType) x.GetValue(null)!)
		      .ToArray();
		      */

		All = ReadDatabase();
	}

	public static readonly FileType[] All;

	/// <summary>
	/// Reads <see cref="FileType"/> from <see cref="ER.File_types"/>
	/// </summary>
	public static FileType[] ReadDatabase()
	{
		var jNode  = JsonNode.Parse(ER.File_types);
		var jArray = jNode.AsArray();
		var rg     = new List<FileType>();

		foreach (var r in jArray) {
			var o = r.AsObject();

			var mask      = o[ER.K_Mask].ToString();
			var sig       = o[ER.K_Pattern].ToString();
			var mediaType = o[ER.K_Name].ToString();

			var ft = new FileType()
			{
				Mask      = M.ReadByteArrayString(mask),
				Pattern   = M.ReadByteArrayString(sig),
				MediaType = mediaType
			};

			rg.Add(ft);
		}

		return rg.ToArray();
	}

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
		return Resolve(s.ReadBlock());
	}

	public static async Task<IEnumerable<FileType>> ResolveAsync(Stream s)
	{
		return Resolve(await s.ReadBlockAsync());
	}

	public static IEnumerable<FileType> Resolve(byte[] h)
	{
		return All.Where(t => CheckPattern(h, t))
		          .DistinctBy(x => x.MediaType);
	}

	public static bool IsType(string p, string mt)
	{
		return mt.Split(MIME_TYPE_DELIM).FirstOrDefault()?.ToLower() == p.ToLower();
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

	private const char MIME_TYPE_DELIM = '/';

	#endregion

	public static string ResolveMimeType(string file, string mimeProposed = null)
		=> ResolveMimeType(File.ReadAllBytes(file), mimeProposed);

	public static string ResolveMimeType(byte[] dataBytes, string mimeProposed = null)
	{
		//https://stackoverflow.com/questions/2826808/how-to-identify-the-extension-type-of-the-file-using-c/2826884#2826884
		//https://stackoverflow.com/questions/18358548/urlmon-dll-findmimefromdata-works-perfectly-on-64bit-desktop-console-but-gener
		//https://stackoverflow.com/questions/11547654/determine-the-file-type-using-c-sharp
		//https://github.com/GetoXs/MimeDetect/blob/master/src/Winista.MimeDetect/URLMONMimeDetect/urlmonMimeDetect.cs

		Require.ArgumentNotNull(dataBytes, nameof(dataBytes));

		string mimeRet = String.Empty;

		if (!String.IsNullOrEmpty(mimeProposed)) {
			//suggestPtr = Marshal.StringToCoTaskMemUni(mimeProposed); // for your experiments ;-)
			mimeRet = mimeProposed;
		}

		int ret = Native.FindMimeFromData(IntPtr.Zero, null, dataBytes, dataBytes.Length,
		                                  mimeProposed, 0, out var outPtr, 0);

		if (ret == 0 && outPtr != IntPtr.Zero) {

			string str = Marshal.PtrToStringUni(outPtr)!;

			Marshal.FreeHGlobal(outPtr);

			return str;
		}

		return mimeRet;
	}
}