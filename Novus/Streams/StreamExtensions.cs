using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Novus.Memory;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

// ReSharper disable UnusedMember.Global

#nullable enable
namespace Novus.Streams;

public static class StreamExtensions
{
	/*public static void Write<T>(this Span<T> s, params T[] v)
	{
		for (int i = 0; i < v.Length; i++) {
			s[i] = v[i];
		}
	}*/

	public static string[] ReadAllLines(this StreamReader stream)
	{
		var list = new List<string>();

		while (!stream.EndOfStream) {
			string? line = stream.ReadLine();

			if (line != null) {
				list.Add(line);
			}
		}

		return list.ToArray();
	}

	public static async Task<string[]> ReadAllLinesAsync(this StreamReader stream, CancellationToken ct = default)
	{
		var list = new List<string>();

		while (!stream.EndOfStream) {
			string? line = await stream.ReadLineAsync(ct);

			if (line != null) {
				list.Add(line);
			}
		}

		return list.ToArray();
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
#endif

	public const int BlockSize = 0xFF;

	public static byte[] ReadHeader(this Stream stream, int offset = 0, int l = BlockSize)
	{
		stream.TrySeek(offset);

		var buffer = new byte[l];

		int l2 = stream.Read(buffer);

		if (l2 != l) {
			Array.Resize(ref buffer, l2);
		}

		return buffer;
	}

	public static async Task<byte[]> ReadHeaderAsync(this Stream m, int offset = 0, int l = BlockSize,
	                                                 CancellationToken ct = default)
	{
		if (m.CanSeek) {
			m.Position = offset;
			int d = checked((int) m.Length);
			l = d >= l ? l : d;
		}

		// int l=Math.Clamp(d, d, HttpType.RSRC_HEADER_LEN);
		var data = new byte[l];

		int l2 = await m.ReadAsync(data.AsMemory(0, l), ct);

		if (l2 != l) {
			Array.Resize(ref data, l2);
		}

		return data;
	}

	public static long TrySeek(this Stream s, long pos = 0)
	{
		long oldPos = s.Position;

		if (s.CanSeek) {
			s.Position = pos;
		}

		return oldPos;
	}

	public static Pointer<T> ToPointer<T>(this Span<T> s)
		=> M.AddressOf(ref s.GetPinnableReference());

	[MURV]
	public static MemoryStream Copy(this Stream inputStream, int bufferSize = BlockSize)
	{

		var ret = new MemoryStream();

		var buf = new byte[bufferSize];

		int cb = 0;

		while ((cb = inputStream.Read(buf, 0, bufferSize)) > 0)
			ret.Write(buf, 0, cb);

		ret.Position = 0;

		return ret;
	}

	public static void ReadFully(this Stream stream, byte[] buffer)
	{
		int offset = 0;
		int readBytes;

		do {
			// If you are using Socket directly instead of a Stream:
			//readBytes = socket.Receive(buffer, offset, buffer.Length - offset,
			//                           SocketFlags.None);

			readBytes = stream.Read(buffer, offset, buffer.Length - offset);

			offset += readBytes;
		} while (readBytes > 0 && offset < buffer.Length);

		if (offset < buffer.Length) {
			throw new EndOfStreamException();
		}
	}

	public static async Task ReadFullyAsync(this Stream stream, byte[] buffer, CancellationToken ct = default)
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

	public static T ReadAny<T>(this Stream br)
	{
		var s   = M.SizeOf<T>();
		var rg2 = new byte[s];
		var rg  = br.Read(rg2);

		return M.ReadFromBytes<T>(rg2);
	}

	public static LinkedList<T> ReadUntil<T>(this Stream s, Predicate<T> pred, Func<Stream, T> read,
	                                         int? max = null, CancellationToken token = default)
	{

		var ll = new LinkedList<T>();
		T   t;

		while (!pred(t = read(s))) {
			ll.AddLast(t);

			if (token.IsCancellationRequested || !max.HasValue && ll.Count >= max) {
				goto ret;
			}
		}

		ret:
		return ll;
	}
}

public static class BinaryReaderExtensions
{
	public static T ReadAny<T>(this BinaryReader br)
	{
		var s  = M.SizeOf<T>();
		var rg = br.ReadBytes(s);

		return M.ReadFromBytes<T>(rg);
	}

	public static string ReadCString(this BinaryReader br, int count)
	{
		string s = Encoding.ASCII.GetString(br.ReadBytes(count)).TrimEnd('\0');

		return s;
	}
}