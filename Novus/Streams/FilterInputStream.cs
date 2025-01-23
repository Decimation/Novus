namespace Novus.Streams;

public class FilterInputStream : InputStream
{
	protected InputStream m_in;

	public FilterInputStream(InputStream s)
	{
		m_in = s;
	}

	public override int Available()
	{
		return m_in.Available();
	}

	public override void Close()
	{
		m_in.Close();
	}

	public override void Mark(int readLimit)
	{
		m_in.Mark(readLimit);
	}

	public override bool MarkSupported()
	{
		return m_in.MarkSupported();
	}

	public override int Read()
	{
		return m_in.Read();
	}

	public override int Read(byte[] buf)
	{
		return m_in.Read(buf);
	}

	public override int Read(byte[] b, int off, int len)
	{
		return m_in.Read(b, off, len);
	}

	public override void Reset()
	{
		m_in.Reset();
	}

	public override long Skip(long cnt)
	{
		return m_in.Skip(cnt);
	}
}