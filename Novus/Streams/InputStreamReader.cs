﻿using System.Text;

namespace Novus.Streams;

public class InputStreamReader : StreamReader
{
	protected InputStreamReader(string file) : base(file) { }

	public InputStreamReader(InputStream s) : base(s.GetWrappedStream()) { }

	public InputStreamReader(InputStream s, string encoding) : base(
		s.GetWrappedStream(), Encoding.GetEncoding(encoding))
	{ }

	public InputStreamReader(InputStream s, Encoding e) : base(s.GetWrappedStream(), e) { }
}