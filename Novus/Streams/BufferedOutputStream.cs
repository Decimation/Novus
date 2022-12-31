namespace Novus.Streams;

public class BufferedOutputStream : OutputStream
{
	public BufferedOutputStream(OutputStream outs)
	{
		Wrapped = new BufferedStream(outs == null ? new MemoryStream() : outs.GetWrappedStream());
	}

	public BufferedOutputStream(OutputStream outs, int bufferSize)
	{
		Wrapped = new BufferedStream(outs.GetWrappedStream(), bufferSize);
	}
}