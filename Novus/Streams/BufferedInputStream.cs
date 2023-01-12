namespace Novus.Streams;

public class BufferedInputStream : InputStream
{
	public BufferedInputStream(InputStream s)
	{
		BaseStream = s.GetWrappedStream();
		Wrapped    = new BufferedStream(BaseStream);
	}

	public BufferedInputStream(InputStream s, int bufferSize)
	{
		BaseStream = s.GetWrappedStream();
		Wrapped    = new BufferedStream(BaseStream, bufferSize);
	}
}