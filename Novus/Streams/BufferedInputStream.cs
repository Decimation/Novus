namespace Novus.Streams;

public class BufferedInputStream : InputStream
{
	public BufferedInputStream(InputStream s)
	{
		m_baseStream = s.GetWrappedStream();
		m_wrapped    = new BufferedStream(m_baseStream);
	}

	public BufferedInputStream(InputStream s, int bufferSize)
	{
		m_baseStream = s.GetWrappedStream();
		m_wrapped    = new BufferedStream(m_baseStream, bufferSize);
	}
}