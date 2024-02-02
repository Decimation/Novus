using System.Net.Mime;
using System.Diagnostics;
using System.Net;
using Flurl;
using Flurl.Http;
using JetBrains.Annotations;
using Kantan.Net.Utilities;
using Kantan.Text;
using Novus.Streams;
using static System.Net.Mime.MediaTypeNames;

namespace Novus.FileTypes;

// todo: wip

/// <summary>
/// Encapsulates a resource from:
/// <list type="bullet">
/// <item>File</item>
/// <item>HTTP</item>
/// <item><see cref="Stream"/></item>
/// </list>
/// </summary>
public class UniSource : IDisposable, IEquatable<UniSource>
{

	public UniSourceType SourceType { get; }

	public Stream Stream { get; }

	public bool IsValid => IsFile || IsUri || IsStream;

	public FileType FileType { get; protected set; }

	public object Value { get; }

	public bool IsUri => SourceType == UniSourceType.Uri;

	public bool IsFile => SourceType == UniSourceType.File;

	public bool IsStream => SourceType == UniSourceType.Stream;

	private UniSource(object value, Stream s, UniSourceType u)
	{
		Value      = value;
		Stream     = s;
		SourceType = u;
	}

	public string Name
	{
		get
		{
			if (IsFile) {
				return Path.GetFileName(Value.ToString());
			}

			if (IsUri) {
				return ((Url) Value).GetFileName();
			}

			if (IsStream) {
				return $"<stream {Stream.GetHashCode()}>";
			}

			throw new InvalidOperationException();

		}
	}

	public static async Task<UniSource> GetAsync(object o, IFileTypeResolver resolver = null,
	                                             FileType[] whitelist = null,
	                                             CancellationToken ct = default)
	{
		UniSource buf = null;

		resolver  ??= IFileTypeResolver.Default;
		whitelist ??= [];

		if (o is string os) {
			os = os.CleanString();
			o  = os;
		}

		var uh = UniHandler.GetUniType(o, out var o2);
		var s  = o.ToString();

		buf = uh switch
		{
			UniSourceType.Uri =>

				// Debug.Assert(o == o2);
				// Debug.Assert(s == o2);
				await HandleUri(s, ct),
			UniSourceType.Stream => new UniSource(o, o as Stream, UniSourceType.Stream),
			UniSourceType.File =>

				// s = s.CleanString();
				new UniSource(o, File.OpenRead(s), UniSourceType.File) { },
			_ => throw new ArgumentException()
		};

		// Trace.Assert((isFile || isUrl) && !(isFile && isUrl));

		var type = (await resolver.ResolveAsync(buf.Stream, ct));

		if (whitelist.Any()) {

			if (!whitelist.Contains(type)) {
				throw new ArgumentException($"Invalid file type: {type}", nameof(o));
			}

		}

		buf.FileType = type;
		buf.Stream.TrySeek();

		return buf;

		static async Task<UniSource> HandleUri(Url value, CancellationToken ct)
		{
			// value = value.CleanString();

			var res = await value.AllowAnyHttpStatus()
				          .WithHeaders(new
				          {
					          User_Agent = ER.UserAgent,
				          })
				          .GetAsync(cancellationToken: ct);

			if (res.ResponseMessage.StatusCode == HttpStatusCode.NotFound) {
				throw new ArgumentException($"{value} returned {HttpStatusCode.NotFound}");
			}

			var buf = new UniSource(value, await res.GetStreamAsync(), UniSourceType.Uri)
				{ };
			return buf;
		}
	}

	public static async Task<UniSource> TryGetAsync(object value, IFileTypeResolver resolver = null,
	                                                CancellationToken ct = default,
	                                                params FileType[] whitelist)
	{
		try {
			return await GetAsync(value, resolver, whitelist, ct);
		}
		catch (FlurlHttpException e) {
			Debug.WriteLine($"HTTP: {e.Message}", nameof(TryGetAsync));
		}
		catch (ArgumentException e) {
			Debug.WriteLine($"Argument: {e.Message}", nameof(TryGetAsync));
		}
		catch (Exception e) {
			Debug.WriteLine($"{e.Message}", nameof(TryGetAsync));
		}
		finally { }

		return null;
	}

	[ItemCanBeNull]
	public async Task<string> TryDownloadAsync()
	{
		string fn = null, ext = null;

		if (IsUri) {
			var url = (Url) Value;
			fn = url.GetFileName();

			// fn = Path.Combine(Path.GetTempPath(), fn);

		}
		else if (IsFile) {
			Debug.Assert(File.Exists(Value.ToString()));
			return Value.ToString();
		}

		else if (IsStream) {
			ext = FileType.Subtype;
		}

		// var tmp = Path.Combine(Path.GetTempPath(), fn);
		var tmp = FS.GetTempFileName(fn, ext);

		// tmp = FS.SanitizeFilename(tmp);

		var path = await WriteStreamToFileAsync(tmp);
		return path;
	}

	public async Task<string> WriteStreamToFileAsync(string tmp)
	{
		var fs = new FileStream(tmp, FileMode.Create) { };
		await Stream.CopyToAsync(fs);
		await fs.FlushAsync();
		fs.Dispose();
		Stream.TrySeek();
		return tmp;
	}

	public virtual void Dispose()
	{
		if (Value is Stream s) {
			s.Dispose();
		}

		Stream.Dispose();
	}

	public bool Equals(UniSource other)
	{
		if (ReferenceEquals(null, other)) {
			return false;
		}

		if (ReferenceEquals(this, other)) {
			return true;
		}

		return SourceType == other.SourceType
		       && Equals(Stream, other.Stream)
		       && IsValid == other.IsValid
		       && Equals(FileType, other.FileType);
	}

	public override bool Equals(object obj)
	{
		if (ReferenceEquals(null, obj)) {
			return false;
		}

		if (ReferenceEquals(this, obj)) {
			return true;
		}

		if (obj.GetType() != this.GetType()) {
			return false;
		}

		return Equals((UniSource) obj);
	}

	public override int GetHashCode()
	{
		unchecked {
			int hashCode = (int) SourceType;
			hashCode = (hashCode * 397) ^ (Stream != null ? Stream.GetHashCode() : 0);
			hashCode = (hashCode * 397) ^ IsValid.GetHashCode();
			hashCode = (hashCode * 397) ^ (FileType == default ? 0 : FileType.GetHashCode());
			return hashCode;
		}
	}

	public static bool operator ==(UniSource left, UniSource right)
	{
		return Equals(left, right);
	}

	public static bool operator !=(UniSource left, UniSource right)
	{
		return !Equals(left, right);
	}

	public override string ToString()
	{
		return $"[{SourceType}] {FileType}";
	}

}

public enum UniSourceType
{

	NA,
	File,
	Uri,
	Stream

}