using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Novus.Memory;
using System;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

// ReSharper disable UnusedMember.Global

#nullable enable
namespace Novus.Streams;

public static class StreamExtensions
{

	extension(StreamReader stream)
	{

		public string[] ReadLinesToEnd()
		{
			var list = new List<string>();

			while (!stream.EndOfStream) {
				string? line = stream.ReadLine();

				if (line != null) {
					list.Add(line);
				}
			}

			return [.. list];
		}

		public async Task<string[]> ReadLinesToEndAsync(CancellationToken ct = default)
		{
			var list = new List<string>();

			while (!stream.EndOfStream) {
				string? line = await stream.ReadLineAsync(ct);

				if (line != null) {
					list.Add(line);
				}
			}

			return [.. list];
		}

	}

#if EXTRA
	public static byte[] ToByteArray(this Stream stream)
	{
		// NOTE: throws when stream is not seekable
		stream.TrySeek();
		// using var ms = new MemoryStream();
		// stream.CopyTo(ms);
		// var rg = ms.ToArray();

		int length = checked((int) stream.Length);

		return stream.ReadHeader(length);
	}

	public static MemoryStream Copy(this Stream inputStream, int bufferSize = BLOCK_SIZE)
	{

		var ret = new MemoryStream();

		var buf = new byte[bufferSize];

		int cb = 0;

		while ((cb = inputStream.Read(buf, 0, bufferSize)) > 0)
			ret.Write(buf, 0, cb);

		ret.Position = 0;

		return ret;
	}
#endif

	public const int BLOCK_SIZE = 0xFF;

	/// <param name="stream">Stream to rewind</param>
	extension(Stream stream)
	{

		public ValueTask<Stream> EnsureRewindableHeaderAsync(int peek = BLOCK_SIZE, CancellationToken ct = default)
		{
			if (stream.CanSeek)
				return ValueTask.FromResult(stream);

			return stream.GetHeaderAsync(peek, ct);
		}

		public Stream EnsureRewindableHeader(int peek = BLOCK_SIZE)
		{
			if (stream.CanSeek)
				return stream;

			return stream.GetHeader(peek);
		}

		public async ValueTask<Stream> GetHeaderAsync(int peek, CancellationToken ct = default)
		{
			var head = new byte[peek];
			int n    = await stream.ReadAtLeastAsync(head, peek, throwOnEndOfStream: false, ct);
			return new MemoryStream(head, 0, n, writable: false, publiclyVisible: true);
		}

		public Stream GetHeader(int peek)
		{
			var head = new byte[peek];
			int n    = stream.ReadAtLeast(head, peek, throwOnEndOfStream: false);
			return new MemoryStream(head, 0, n, writable: false, publiclyVisible: true);
		}

		public byte[] ReadHeader(int l = BLOCK_SIZE)
		{
			using var stream2 = stream.EnsureRewindableHeader(l) as MemoryStream;

			var head = new byte[l];
			int n    = stream2.ReadAtLeast(head, l, throwOnEndOfStream: false);
			
			stream2.Rewind();

			return head;
		}

		public async Task<byte[]> ReadHeaderAsync(int l = BLOCK_SIZE, CancellationToken ct = default)
		{
			using var stream2 = await stream.EnsureRewindableHeaderAsync(l, ct) as MemoryStream;
			var       head    = new byte[l];
			int       n       = await stream2.ReadAtLeastAsync(head, l, throwOnEndOfStream: false, cancellationToken: ct);
			
			stream2.Rewind();

			return head;

		}

		public long TrySeek(long pos = 0)
		{
			long oldPos = stream.Position;

			if (stream.CanSeek) {
				stream.Position = pos;
			}

			return oldPos;
		}

		/// <summary>
		/// Rewind stream to first position.
		/// </summary>
		public bool Rewind()
		{
			var canSeek = stream.CanSeek && stream.Position != 0;

			if (canSeek) {
				stream.Seek(0L, SeekOrigin.Begin);
			}

			return canSeek;
		}

		public async Task ReadFullyAsync(byte[] buffer, CancellationToken ct = default)
		{
			int offset = 0;
			int readBytes;

			do {
				// If you are using Socket directly instead of a Stream:
				//readBytes = socket.Receive(buffer, offset, buffer.Length - offset,
				//                           SocketFlags.None);

				readBytes = await stream.ReadAsync(buffer.AsMemory(offset, buffer.Length - offset), ct);

				offset += readBytes;
			} while (readBytes > 0 && offset < buffer.Length);

			if (offset < buffer.Length) {
				throw new EndOfStreamException();
			}
		}

		public T ReadAny<T>()
		{
			var s   = Mem.SizeOf<T>();
			var rg2 = new byte[s];
			var rg  = stream.Read(rg2);

			return Mem.ReadFromBytes<T>(rg2);
		}

		public LinkedList<T> ReadUntil<T>(Predicate<T> pred, Func<Stream, T> read, int? max = null, CancellationToken token = default)
		{

			var ll = new LinkedList<T>();
			T   t;

			while (!pred(t = read(stream))) {
				ll.AddLast(t);

				if (token.IsCancellationRequested || (max is null && ll.Count >= max)) {
					goto ret;
				}
			}

		ret:
			return ll;
		}

	}

}

public static class BinaryReaderExtensions
{

	extension(BinaryReader br)
	{

		public T ReadAny<T>()
		{
			var s  = Mem.SizeOf<T>();
			var rg = br.ReadBytes(s);

			return Mem.ReadFromBytes<T>(rg);
		}

		public string ReadCString(int count)
		{
			string s = Encoding.ASCII.GetString(br.ReadBytes(count)).TrimEnd('\0');

			return s;
		}

	}

}