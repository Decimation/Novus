namespace Novus.Streams;

public class BufferedOutputStream : OutputStream
{
	public BufferedOutputStream(OutputStream outs)
	{
		m_wrapped = new BufferedStream(outs == null ? new MemoryStream() : outs.GetWrappedStream());
	}

	public BufferedOutputStream(OutputStream outs, int bufferSize)
	{
		m_wrapped = new BufferedStream(outs.GetWrappedStream(), bufferSize);
	}
}