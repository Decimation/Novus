namespace Novus.Streams;

public class FilterInputStream : InputStream
{
	protected InputStream @in;

	public FilterInputStream(InputStream s)
	{
		@in = s;
	}

	public override int Available()
	{
		return @in.Available();
	}

	public override void Close()
	{
		@in.Close();
	}

	public override void Mark(int readlimit)
	{
		@in.Mark(readlimit);
	}

	public override bool MarkSupported()
	{
		return @in.MarkSupported();
	}

	public override int Read()
	{
		return @in.Read();
	}

	public override int Read(byte[] buf)
	{
		return @in.Read(buf);
	}

	public override int Read(byte[] b, int off, int len)
	{
		return @in.Read(b, off, len);
	}

	public override void Reset()
	{
		@in.Reset();
	}

	public override long Skip(long cnt)
	{
		return @in.Skip(cnt);
	}
}