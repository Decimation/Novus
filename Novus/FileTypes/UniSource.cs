using System.Net.Mime;
using System.Diagnostics;
using System.Net;
using System.Numerics;
using Flurl;
using Flurl.Http;
using JetBrains.Annotations;
using Kantan.Net.Utilities;
using Kantan.Text;
using Novus.Streams;
using static System.Net.Mime.MediaTypeNames;

namespace Novus.FileTypes;

// todo: wip

public abstract class UniSource : IDisposable, IEquatable<UniSource>,
									  IEqualityOperators<UniSource, UniSource, bool>
{

	protected UniSource(Stream stream, object value)
	{
		Stream = stream;
		Value  = value;
	}

	public abstract UniSourceType SourceType { get; }

	public Stream Stream { get; protected set; }

	public bool IsValid => IsUri || IsFile || IsStream;

	public FileType FileType { get; protected set; }

	public object Value { get; protected set; }

	public bool IsUri => SourceType == UniSourceType.Uri;

	public bool IsFile => SourceType == UniSourceType.File;

	public bool IsStream => SourceType == UniSourceType.Stream;

	public abstract string Name { get; }

	public virtual void Dispose()
	{
		Stream?.Dispose();
	}

	public override string ToString()
	{
		return $"[{SourceType}] {FileType}";
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
			UniSourceType.Stream => new UniSourceStream(o as Stream),
			UniSourceType.File =>

				// s = s.CleanString();
				new UniSourceFile(File.OpenRead(s), o) { },
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

			var buf = new UniSourceUri(await res.GetStreamAsync(), value)
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
	public abstract Task<string> TryDownloadAsync();

	public async Task<string> WriteStreamToFileAsync(string tmp)
	{
		var fs = new FileStream(tmp, FileMode.Create) { };
		await Stream.CopyToAsync(fs);
		await fs.FlushAsync();
		fs.Dispose();
		Stream.TrySeek();
		return tmp;
	}

	public bool Equals(UniSource other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;

		return Equals(Stream, other.Stream) && FileType.Equals(other.FileType) && Equals(Value, other.Value);
	}

	public override bool Equals(object obj)
	{
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != this.GetType()) return false;

		return Equals((UniSource) obj);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Stream, FileType, Value);
	}

	public static bool operator ==(UniSource left, UniSource right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(UniSource left, UniSource right)
	{
		return !left.Equals(right);
	}

}

public class UniSourceFile : UniSource
{

	public override UniSourceType SourceType => UniSourceType.File;

	internal UniSourceFile(Stream stream, object value) : base(stream, value) { }

	public override string Name => Path.GetFileName(Value.ToString());

	public override async Task<string> TryDownloadAsync()
	{
		Debug.Assert(File.Exists(Value.ToString()));
		return Value.ToString();
	}

}

public class UniSourceStream : UniSource
{

	public override UniSourceType SourceType => UniSourceType.Stream;

	public override string Name => $"<stream {Stream.GetHashCode()}>";

	internal UniSourceStream(Stream stream) : base(stream, stream) { }

	public override async Task<string> TryDownloadAsync()
	{
		string fn = null, ext = null;

		ext = FileType.Subtype;
		var tmp = FS.GetTempFileName(fn, ext);

		// tmp = FS.SanitizeFilename(tmp);

		var path = await WriteStreamToFileAsync(tmp);
		return path;
	}

}

public class UniSourceUri : UniSource
{

	public override UniSourceType SourceType => UniSourceType.Uri;

	public override string Name => ((Url) Value).GetFileName();

	internal UniSourceUri(Stream stream, object value) : base(stream, value) { }

	public override async Task<string> TryDownloadAsync()
	{
		string fn  = null, ext = null;
		var    url = (Url) Value;
		fn = url.GetFileName();

		// var tmp = Path.Combine(Path.GetTempPath(), fn);
		var tmp = FS.GetTempFileName(fn, ext);

		// tmp = FS.SanitizeFilename(tmp);

		var path = await WriteStreamToFileAsync(tmp);
		return path;
	}

}

/// <summary>
/// Encapsulates a resource from:
/// <list type="bullet">
/// <item>File</item>
/// <item>HTTP</item>
/// <item><see cref="UniSource.Stream"/></item>
/// </list>
/// </summary>
/*
public class UniSource : UniSourceBase, IEquatable<UniSource>, IEqualityOperators<UniSource, UniSource, bool>
{

	private UniSource(object value, Stream s, UniSourceType u)
	{
		Value      = value;
		Stream     = s;
		SourceType = u;
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
*/

public enum UniSourceType
{

	NA,
	File,
	Uri,
	Stream

}