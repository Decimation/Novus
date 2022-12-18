using System.Diagnostics;
using System.Net;
using Flurl;
using Flurl.Http;
using JetBrains.Annotations;
using Kantan.Text;

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

	public FileType[] FileTypes { get; protected set; }

	public object Value { get; }

	public bool IsUri    => SourceType == UniSourceType.Uri;
	public bool IsFile   => SourceType == UniSourceType.File;
	public bool IsStream => SourceType == UniSourceType.Stream;

	private UniSource(object value, Stream s, UniSourceType u)
	{
		Value      = value;
		Stream     = s;
		SourceType = u;

	}

	public static async Task<UniSource> GetAsync(object o, IFileTypeResolver resolver, FileType[] whitelist)
	{
		UniSource buf = null;

		resolver ??= IFileTypeResolver.Default;

		if (o is string os) {
			os = os.CleanString();
			o  = os;
		}

		switch (o) {
			case Stream s:
				buf = new UniSource(o, s, UniSourceType.Stream);
				break;
			case string value when Url.IsValid(value):
				// value = value.CleanString();

				var res = await value.AllowAnyHttpStatus()
				                     .WithHeaders(new
				                     {
					                     User_Agent = ER.UserAgent,
				                     })
				                     .GetAsync();

				if (res.ResponseMessage.StatusCode == HttpStatusCode.NotFound) {
					throw new ArgumentException($"{value} returned {HttpStatusCode.NotFound}");
				}

				buf = new UniSource(o, await res.GetStreamAsync(), UniSourceType.Uri)
					{ };
				break;
			case string s when File.Exists(s):
				// s = s.CleanString();

				buf = new UniSource(o, File.OpenRead(s), UniSourceType.File)
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

	public static UniSourceType GetSourceType(object value)
	{
		return HandleType(value, (_, _) => UniSourceType.Stream, (_, _) => UniSourceType.Uri,
		                  (_, _) => UniSourceType.File, (_) => UniSourceType.NA);
	}

	public static T HandleType<T>(Object o, Func<object, Stream, T> fnStream, Func<object, Url, T> fnUri,
	                              Func<object, string, T> fnFile, [CanBeNull] Func<object,T> unknown)
	{
		if (unknown == null) {
			unknown = (o1) => { return default; };

		}
		switch (o) {
			case Stream s:
				return fnStream(o, s);
				break;
			case string value when Url.IsValid(value):
				return fnUri(o, value);
				break;
			case string s when File.Exists(s):
				return fnFile(o, s);
				break;
			default:
				return unknown(o);

		}
	}

	public static async Task<UniSource> TryGetAsync(object value, IFileTypeResolver resolver = null,
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
	NA,
	File,
	Uri,
	Stream
}