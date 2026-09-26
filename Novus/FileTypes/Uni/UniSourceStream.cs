// Deci Novus UniSourceStream.cs
// $File.CreatedYear-$File.CreatedMonth-9 @ 2:39

using Novus.OS;
using Novus.Streams;

namespace Novus.FileTypes.Uni;

public class UniSourceStream : UniSource, IUniSource
{

	private readonly Stream m_stream;

	internal UniSourceStream(Stream stream) : base(stream, UniSourceType.Stream)
	{
		m_stream = stream;
		Name     = $"<stream {m_stream.GetHashCode()}>";
	}


	public override async ValueTask<bool> AllocBuffer(CancellationToken ct = default)
	{
		if (HasBuffer) {
			goto ret;
		}

		try {
			Buffer = new byte[m_stream.Length];
			await m_stream.ReadFullyAsync(Buffer, ct);
		}
		finally {
			m_stream?.Rewind();
		}

	ret:
		return HasBuffer;
	}

	public static bool IsStreamType(object input, out Stream stream)
	{
		stream = input switch
		{
			Stream str => str,
			_          => null
		};

		// todo: check for Stream.Null
		return stream != null;
	}

	public override void Dispose()
	{
		base.Dispose();
		m_stream?.Dispose();
	}

}