using System.Net.Mime;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Numerics;
using Flurl;
using Flurl.Http;
using JetBrains.Annotations;
using Kantan.Text;
using Novus.Streams;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Novus.Imports;
using Novus.Utilities;

namespace Novus.FileTypes.Uni;

public abstract class UniSource : IDisposable, IEquatable<UniSource>,
								  IEqualityOperators<UniSource, UniSource, bool>
{

	public static List<IUniSource.IsTypePredicate> Register { get; } =
		[UniSourceStream.IsType, UniSourceFile.IsType, UniSourceUrl.IsType];

	static UniSource() { }

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

	public abstract bool IsUri { get; }

	public abstract bool IsFile { get; }

	public abstract bool IsStream { get; }

	public string Name { get; protected init; }

	public virtual void Dispose()
	{
		Stream?.Dispose();
	}

	public override string ToString()
	{
		return $"[{SourceType}] {FileType}";
	}

	public static async Task<UniSource> GetAsync(object o, IFileTypeResolver resolver = null,
												 CancellationToken ct = default)
	{
		UniSource buf = null;

		resolver ??= IFileTypeResolver.Default;

		// whitelist ??= [];

		if (o is string os) {
			os = os.CleanString();
			o  = os;
		}

		// var uh = GetUniType(o, out var o2);
		var    s  = o.ToString();
		object o2 = null;

		if (UniSourceFile.IsType(o, out o2)) {
			buf = new UniSourceFile(File.OpenRead(s), o) { };
		}
		else if (UniSourceUrl.IsType(o, out o2)) {
			buf = await UniSourceUrl.HandleUriAsync(s, ct);
		}
		else if (UniSourceStream.IsType(o, out o2)) {
			buf = new UniSourceStream(o as Stream);
		}
		else {
			throw new ArgumentException(null, nameof(o));
		}

		// Trace.Assert((isFile || isUrl) && !(isFile && isUrl));

		var type = await resolver.ResolveAsync(buf.Stream, ct);

		/*
		if (whitelist.Any()) {

			if (!whitelist.Contains(type)) {
				throw new ArgumentException($"Invalid file type: {type}", nameof(o));
			}

		}
		*/

		buf.FileType = type;
		buf.Stream.TrySeek();

		return buf;

	}

	public static async Task<UniSource> TryGetAsync(object value, IFileTypeResolver resolver = null,
													CancellationToken ct = default)
	{
		try {
			return await GetAsync(value, resolver, ct);
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

	[ICBN]
	public abstract Task<string> TryDownloadAsync();

	public async Task<string> WriteStreamToFileAsync(string tmp)
	{
		var fs = new FileStream(tmp, FileMode.Create)
			{ };
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
		if (obj.GetType() != GetType()) return false;

		return Equals((UniSource) obj);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Stream, FileType, Value);
	}

	public static bool operator ==(UniSource left, UniSource right)
	{
		return Equals(left, right);
	}

	public static bool operator !=(UniSource left, UniSource right)
	{
		return !Equals(left, right);
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