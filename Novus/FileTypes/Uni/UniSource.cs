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
using Novus.OS;
using Novus.Utilities;
using Novus.FileTypes.Resolvers;

namespace Novus.FileTypes.Uni;

// TODO: UNISOURCE <--> UNIIMAGE

internal abstract class UniSource : IEquatable<UniSource>, IEqualityOperators<UniSource, UniSource, bool>, IDisposable
{

	/*
	public static List<IUniSource.IsTypePredicateCallback> Register { get; } =
		[UniSourceStream.IsType, UniSourceFile.IsType, UniSourceUrl.IsType];
		*/

	static UniSource() { }


	protected UniSource(UniSourceType type, object value)
	{
		SourceType = type;
		Value      = value;
	}

	public UniSourceType SourceType { get; }

	public string Name { get; protected init; }

	public Stream Stream { get; protected set; }

	public bool IsValid => IsUri || IsFile || IsStream;

	public IMediaType MediaType { get; protected set; }

	public object Value { get; protected set; }

	public bool IsUri => SourceType == UniSourceType.Uri;

	public bool IsFile => SourceType == UniSourceType.File;

	public bool IsStream => SourceType == UniSourceType.Stream;

	public virtual void Dispose()
	{
		Stream?.Dispose();
	}

	public override string ToString()
	{
		return $"[{SourceType}] {MediaType}";
	}

	public static async Task<UniSource> GetAsync(object o, IMediaTypeResolver resolver = null, bool autoAlloc = true, CancellationToken ct = default)
	{
		resolver ??= IMediaTypeResolver.Default;
		UniSource buf = null;

		string os;

		switch (o) {
			case null:
				throw new ArgumentNullException(nameof(o));

			case Stream stream:
				buf = new UniSourceStream(stream);
				goto resType;

			default:
				os = o?.ToString()?.CleanString();

				if (Url.IsValid(os)) {
					var osAsUrl = Url.Parse(os);

					if (osAsUrl.Scheme == "file" && File.Exists(os)) {
						buf = new UniSourceFile(new FileInfo(os));
					}
					else {
						buf = new UniSourceUrl(osAsUrl);
					}
				}
				else {
					throw new ArgumentException("Unknown type", nameof(o));
				}

				break;
		}

	resType:

		if (autoAlloc) {
			
			var ok = await buf.AllocStream(ct);

			if (ok) {
				var type = await resolver.ResolveAsync(buf.Stream, ct: ct);

				buf.MediaType = type;
				buf.Stream.TrySeek();

			}
		}

		return buf;
	}


	public static Task<UniSource> TryGetAsync(object value, IMediaTypeResolver resolver = null,
	                                                bool autoAlloc = true,
	                                                CancellationToken ct = default)
	{
		try {
			return GetAsync(value, resolver, autoAlloc, ct: ct);
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

		return Task.FromResult<UniSource>(null);
	}

	[ICBN]
	public virtual async ValueTask<string> TryWriteToFileAsync(string fn = null, string ext = null)
	{
		// var tmp = Path.Combine(Path.GetTempPath(), fn);
		var tmp = FileSystem.GetTempFileName(fn, ext);

		// tmp = FS.SanitizeFilename(tmp);

		var path = await CopyStreamToFileAsync(tmp);

		return path;
	}

	public virtual ValueTask<string> CopyStreamToFileAsync(string fileName)
	{
		var fs = new FileStream(fileName, FileMode.Create)
			{ };

		CopyStream(fs);

		return ValueTask.FromResult(fileName);
	}

	protected void CopyStream(Stream fs)
	{
		lock (Stream) {
			Stream.CopyTo(fs);
			fs.Flush();
			fs.Dispose();
			Stream.TrySeek();
		}
	}

	public abstract ValueTask<bool> AllocStream(CancellationToken ct = default);

	public bool Equals(UniSource other)
	{
		if (ReferenceEquals(null, other))
			return false;

		if (ReferenceEquals(this, other))
			return true;

		return Equals(Stream, other.Stream) && MediaType.Equals(other.MediaType) && Equals(Value, other.Value);
	}

	public override bool Equals(object obj)
	{
		if (obj is null)
			return false;

		if (ReferenceEquals(this, obj))
			return true;

		if (obj.GetType() != GetType())
			return false;

		return Equals((UniSource) obj);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Stream, MediaType, Value);
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
public enum UniSourceType
{

	NA = 0,
	File,
	Uri,
	Stream

}