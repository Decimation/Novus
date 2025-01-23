namespace Novus.Streams;

public class FilterOutputStream : OutputStream
{
	protected OutputStream m_out;

	public FilterOutputStream(OutputStream os)
	{
		m_out = os;
	}

	public override void Close()
	{
		m_out.Close();
	}

	public override void Flush()
	{
		m_out.Flush();
	}

	public override void Write(byte[] b)
	{
		m_out.Write(b);
	}

	public override void Write(int b)
	{
		m_out.Write(b);
	}

	public override void Write(byte[] b, int offset, int len)
	{
		m_out.Write(b, offset, len);
	}
}