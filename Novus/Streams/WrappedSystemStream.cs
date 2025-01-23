namespace Novus.Streams;

public class WrappedSystemStream : Stream
{
	private int m_position;
	private int m_markedPosition;

	public WrappedSystemStream(InputStream ist)
	{
		InputStream = ist;
	}

	public WrappedSystemStream(OutputStream ost)
	{
		this.OutputStream = ost;
	}

	public InputStream InputStream { get; }

	public OutputStream OutputStream { get; }

	public override void Close()
	{
		InputStream?.Close();
		OutputStream?.Close();
	}

	public override void Flush()
	{
		OutputStream.Flush();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		int res = InputStream.Read(buffer, offset, count);

		if (res != -1) {
			m_position += res;
			return res;
		}
		else
			return 0;
	}

	public override int ReadByte()
	{
		int res = InputStream.Read();

		if (res != -1)
			m_position++;
		return res;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		Position = origin switch
		{
			SeekOrigin.Begin   => offset,
			SeekOrigin.Current => Position + offset,
			SeekOrigin.End     => Length + offset,
			_                  => Position
		};
		return Position;
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		OutputStream.Write(buffer, offset, count);
		m_position += count;
	}

	public override void WriteByte(byte value)
	{
		OutputStream.Write(value);
		m_position++;
	}

	public override bool CanRead => InputStream != null;

	public override bool CanSeek => true;

	public override bool CanWrite => OutputStream != null;

	public override long Length => throw new NotSupportedException();

	internal void OnMark(int nb)
	{
		m_markedPosition = m_position;
		InputStream.Mark(nb);
	}

	public override long Position
	{
		get
		{
			if (InputStream != null && InputStream.CanSeek())
				return InputStream.Position;
			else
				return m_position;
		}
		set
		{
			if (value == m_position)
				return;
			else if (value == m_markedPosition)
				InputStream.Reset();
			else if (InputStream != null && InputStream.CanSeek()) {
				InputStream.Position = value;
			}
			else
				throw new NotSupportedException();
		}
	}
}