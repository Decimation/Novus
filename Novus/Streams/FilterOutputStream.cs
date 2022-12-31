namespace Novus.Streams;

public class FilterOutputStream : OutputStream
{
	protected OutputStream @out;

	public FilterOutputStream(OutputStream os)
	{
		@out = os;
	}

	public override void Close()
	{
		@out.Close();
	}

	public override void Flush()
	{
		@out.Flush();
	}

	public override void Write(byte[] b)
	{
		@out.Write(b);
	}

	public override void Write(int b)
	{
		@out.Write(b);
	}

	public override void Write(byte[] b, int offset, int len)
	{
		@out.Write(b, offset, len);
	}
}