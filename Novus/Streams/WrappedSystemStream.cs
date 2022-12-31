﻿namespace Novus.Streams;

internal class WrappedSystemStream : Stream
{
	private InputStream  ist;
	private OutputStream ost;
	int                  position;
	int                  markedPosition;

	public WrappedSystemStream(InputStream ist)
	{
		this.ist = ist;
	}

	public WrappedSystemStream(OutputStream ost)
	{
		this.ost = ost;
	}

	public InputStream InputStream
	{
		get { return ist; }
	}

	public OutputStream OutputStream
	{
		get { return ost; }
	}

	public override void Close()
	{
		if (ist != null)
		{
			ist.Close();
		}

		if (ost != null)
		{
			ost.Close();
		}
	}

	public override void Flush()
	{
		ost.Flush();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		int res = ist.Read(buffer, offset, count);

		if (res != -1)
		{
			position += res;
			return res;
		}
		else
			return 0;
	}

	public override int ReadByte()
	{
		int res = ist.Read();

		if (res != -1)
			position++;
		return res;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		if (origin == SeekOrigin.Begin)
			Position = offset;
		else if (origin == SeekOrigin.Current)
			Position = Position + offset;
		else if (origin == SeekOrigin.End)
			Position = Length + offset;
		return Position;
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		ost.Write(buffer, offset, count);
		position += count;
	}

	public override void WriteByte(byte value)
	{
		ost.Write(value);
		position++;
	}

	public override bool CanRead
	{
		get { return ist != null; }
	}

	public override bool CanSeek
	{
		get { return true; }
	}

	public override bool CanWrite
	{
		get { return ost != null; }
	}

	public override long Length
	{
		get { throw new NotSupportedException(); }
	}

	internal void OnMark(int nb)
	{
		markedPosition = position;
		ist.Mark(nb);
	}

	public override long Position
	{
		get
		{
			if (ist != null && ist.CanSeek())
				return ist.Position;
			else
				return position;
		}
		set
		{
			if (value == position)
				return;
			else if (value == markedPosition)
				ist.Reset();
			else if (ist != null && ist.CanSeek())
			{
				ist.Position = value;
			}
			else
				throw new NotSupportedException();
		}
	}
}