using System.Diagnostics;
using System.Net;
using Flurl;
using Flurl.Http;
using Kantan.Text;

namespace Novus.FileTypes;

public class UniSource : IDisposable, IEquatable<UniSource>
{
	public UniSourceType SourceType { get; }

	public Stream Stream { get; }

	public bool IsValid => IsFile || IsUri || IsStream;

	public FileType[] FileTypes { get; protected set; }

	public object Value { get; }

	public bool IsUri    => SourceType == UniSourceType.Uri;
	public bool IsFile   => SourceType == UniSourceType.File;
	public bool IsStream => SourceType == UniSourceType.Stream;

	protected UniSource(object value, Stream s, UniSourceType u)
	{
		Value      = value;
		Stream     = s;
		SourceType = u;

	}

	public static async Task<UniSource> GetAsync(object o, IFileTypeResolver resolver, FileType[] whitelist)
	{
		UniSource buf = null;

		resolver ??= IFileTypeResolver.Default;

		switch (o) {
			case Stream s:
				buf = new UniSource(o, s, UniSourceType.Stream);
				break;
			case string value when Url.IsValid(value):
				value = value.CleanString();

				var res = await value.AllowAnyHttpStatus()
				                     .WithHeaders(new
				                     {
					                     User_Agent = ER.UserAgent,
				                     })
				                     .GetAsync();

				if (res.ResponseMessage.StatusCode == HttpStatusCode.NotFound) {
					throw new ArgumentException($"{value} returned {HttpStatusCode.NotFound}");
				}

				buf = new HttpUniSource(o, await res.GetStreamAsync())
					{ };
				break;
			case string s when File.Exists(s):
				s = s.CleanString();

				buf = new SourceUniSource(o, File.OpenRead(s))
					{ };
				break;
			default:
				throw new ArgumentException();

		}

		// Trace.Assert((isFile || isUrl) && !(isFile && isUrl));

		var types = (await resolver.ResolveAsync(buf.Stream)).ToArray();

		if (whitelist.Any()) {
			var inter = types.Intersect(whitelist);

			if (!inter.Any()) {
				throw new ArgumentException($"Invalid file types: {types.QuickJoin()}", nameof(o));
			}

		}

		buf.FileTypes = types;

		if (buf.Stream.CanSeek) {
			buf.Stream.Position = 0;
		}

		return buf;
	}

	public static readonly UniSource Null = new(null, Stream.Null, default);

	public static (bool IsFile, bool IsUri) IsUriOrFile(string value) => (File.Exists(value), Url.IsValid(value));

	public static async Task<UniSource> TryGetAsync(string value, IFileTypeResolver resolver = null,
	                                                params FileType[] whitelist)
	{
		try {
			return await GetAsync(value, resolver, whitelist);
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

		return SourceType == other.SourceType && Equals(Stream, other.Stream) && IsValid == other.IsValid &&
		       Equals(FileTypes, other.FileTypes);
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
			hashCode = (hashCode * 397) ^ (FileTypes != null ? FileTypes.GetHashCode() : 0);
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
		return $"[{SourceType}] {FileTypes?.QuickJoin()}";
	}
}

public enum UniSourceType
{
	File,
	Uri,
	Stream
}

file sealed class HttpUniSource : UniSource
{
	internal HttpUniSource(object value, Stream s) : base(value, s, UniSourceType.Uri) { }
}

file class SourceUniSource : UniSource
{
	internal SourceUniSource(object value, Stream s) : base(value, s, UniSourceType.File) { }
}