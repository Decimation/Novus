/*
 * https://github.com/mono/ngit/tree/master/Sharpen/Sharpen
 */

namespace Novus.Streams;

using System;
using System.IO;

public class InputStream : IDisposable
{
	private long m_mark;

	protected Stream m_wrapped;
	protected Stream m_baseStream;

	public static implicit operator InputStream(Stream s)
	{
		return Wrap(s);
	}

	public static implicit operator Stream(InputStream s)
	{
		return s.GetWrappedStream();
	}

	public virtual int Available()
	{
		if (m_wrapped is WrappedSystemStream stream)
			return stream.InputStream.Available();
		else
			return 0;
	}

	public virtual void Close()
	{
		if (m_wrapped != null) {
			m_wrapped.Close();
		}
	}

	public void Dispose()
	{
		Close();
	}

	internal Stream GetWrappedStream()
	{
		// Always create a wrapper stream (not directly Wrapped) since the subclass
		// may be overriding methods that need to be called when used through the Stream class
		return new WrappedSystemStream(this);
	}

	public virtual void Mark(int readLimit)
	{
		if (m_wrapped is WrappedSystemStream stream)
			stream.InputStream.Mark(readLimit);
		else {
			if (m_baseStream is WrappedSystemStream systemStream)
				systemStream.OnMark(readLimit);

			if (m_wrapped != null)
				m_mark = m_wrapped.Position;
		}
	}

	public virtual bool MarkSupported()
	{
		if (m_wrapped is WrappedSystemStream stream)
			return stream.InputStream.MarkSupported();
		else
			return m_wrapped is { CanSeek: true };
	}

	public virtual int Read()
	{
		if (m_wrapped == null) {
			throw new NotImplementedException();
		}

		return m_wrapped.ReadByte();
	}

	public virtual int Read(byte[] buf)
	{
		return Read(buf, 0, buf.Length);
	}

	public virtual int Read(byte[] b, int off, int len)
	{
		if (m_wrapped is WrappedSystemStream stream)
			return stream.InputStream.Read(b, off, len);

		if (m_wrapped != null) {
			int num = m_wrapped.Read(b, off, len);
			return num <= 0 ? -1 : num;
		}

		int totalRead = 0;

		while (totalRead < len) {
			int nr = Read();

			if (nr == -1)
				return -1;
			b[off + totalRead] = (byte) nr;
			totalRead++;
		}

		return totalRead;
	}

	public virtual void Reset()
	{
		if (m_wrapped is WrappedSystemStream stream)
			stream.InputStream.Reset();
		else {
			if (m_wrapped == null)
				throw new IOException();
			m_wrapped.Position = m_mark;
		}
	}

	public virtual long Skip(long cnt)
	{
		if (m_wrapped is WrappedSystemStream stream)
			return stream.InputStream.Skip(cnt);

		long n = cnt;

		while (n > 0) {
			if (Read() == -1)
				return cnt - n;
			n--;
		}

		return cnt - n;
	}

	internal bool CanSeek()
	{
		return m_wrapped is { CanSeek: true };
	}

	internal long Position
	{
		get
		{
			if (m_wrapped != null)
				return m_wrapped.Position;
			else
				throw new NotSupportedException();
		}
		set
		{
			if (m_wrapped != null)
				m_wrapped.Position = value;
			else
				throw new NotSupportedException();
		}
	}

	public static InputStream Wrap(Stream s)
	{
		var stream = new InputStream();
		stream.m_wrapped = s;
		return stream;
	}
}