using System.IO.Pipelines;
using System.Net.Mime;
using System.Linq.Expressions;
using System.Numerics;
using Flurl;
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
using Novus.FileTypes.Media;

namespace Novus.FileTypes.Uni;

// TODO: UNISOURCE <--> UNIIMAGE

public abstract class UniSource : IEquatable<UniSource>, IEqualityOperators<UniSource, UniSource, bool>, IDisposable
{

	static UniSource() { }


	protected UniSource(object value, UniSourceType sourceType)
	{
		SourceType = sourceType;
		Value      = value;
	}

	public UniSourceType SourceType { get; }

	public UniSourceFlags Flags { get; protected set; }

	[MNNW(true, nameof(Buffer))]
	public bool HasBuffer => Buffer != null;

	public string Name { get; protected init; }

	public byte[] Buffer { get; protected set; }

	public object Value { get; }

	[MNNW(true, nameof(MediaType))]
	public bool HasMediaType => MediaType != null;

	public IMediaType MediaType { get; protected set; }

	public static async Task<UniSource> GetAsync(object input, bool autoAlloc = true, IMediaTypeResolver resolver = null, CancellationToken ct = default)
	{
		UniSource ur = null;
		resolver ??= IMediaTypeResolver.Default;

		if (UniSourceUrl.IsUrlType(input, out var url)) {
			ur = new UniSourceUrl(url);
		}
		else if (UniSourceFile.IsFileType(input, out FileInfo file)) {
			ur = new UniSourceFile(file);
		}
		else if (UniSourceStream.IsStreamType(input, out Stream stream)) {
			ur = new UniSourceStream(stream);
		}
		else {
			goto ret;
		}

		if (autoAlloc) {

			var allocOk = await ur.AllocBuffer(ct);
			ur.Flags |= allocOk ? UniSourceFlags.BufferAllocated : UniSourceFlags.None;

			if (allocOk) {
				var getMediaTypeOk = await ur.GetMediaType(resolver, ct);
				ur.Flags |= getMediaTypeOk ? UniSourceFlags.MediaTypeResolved : UniSourceFlags.None;
			}
		}

	ret:
		return ur;
	}

	public virtual ValueTask<bool> GetMediaType(IMediaTypeResolver resolver = null, CancellationToken ct = default)
	{
		if (HasMediaType || !HasBuffer) {
			goto ret;
		}

		resolver ??= IMediaTypeResolver.Default;
		var type = resolver.Resolve(Buffer);

		MediaType = type;
	ret:
		return ValueTask.FromResult(HasMediaType);
	}

	public abstract ValueTask<bool> AllocBuffer(CancellationToken ct = default);

	[ICBN]
	public virtual async ValueTask<string> TryWriteToFileAsync(string fn = null, string ext = null)
	{
		var tmp = FileSystem.GetTempFileName(fn, ext);
		await File.WriteAllBytesAsync(tmp, Buffer);

		return tmp;
	}

	public bool Equals(UniSource other)
	{
		if (ReferenceEquals(null, other))
			return false;

		if (ReferenceEquals(this, other))
			return true;

		return MediaType.Equals(other.MediaType) && Equals(Value, other.Value);
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
		return HashCode.Combine(MediaType, Value);
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
		return $"[{Value}] | ({SourceType}) ({MediaType})";
	}

	public virtual void Dispose() { }

}

[Flags]
public enum UniSourceFlags
{

	None              = 0,
	BufferAllocated   = 1 << 0,
	MediaTypeResolved = 1 << 1,

}

public enum UniSourceType
{

	Unknown = 0,

	/// <see cref="UniSourceFile"/>
	File,

	/// <see cref="UniSourceUrl"/>
	Url,

	/// <see cref="UniSourceStream"/>
	Stream

}