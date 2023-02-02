using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Novus.Memory;

// ReSharper disable UnusedMember.Global

#nullable enable
namespace Novus.Utilities;

public static class StreamExtensions
{
	public static void Write<T>(this Span<T> s, params T[] v)
	{
		for (int i = 0; i < v.Length; i++) {
			s[i] = v[i];
		}
	}

	public static Pointer<T> ToPointer<T>(this Span<T> s) => Mem.AddressOf(ref s.GetPinnableReference());

	[MURV]
	public static MemoryStream Copy(this Stream inputStream, int bufferSize = 256)
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

	public static async Task ReadFullyAsync(this Stream stream, byte[] buffer)
	{
		int offset = 0;
		int readBytes;

		do {
			// If you are using Socket directly instead of a Stream:
			//readBytes = socket.Receive(buffer, offset, buffer.Length - offset,
			//                           SocketFlags.None);

			readBytes = await stream.ReadAsync(buffer, offset, buffer.Length - offset);

			offset += readBytes;
		} while (readBytes > 0 && offset < buffer.Length);

		if (offset < buffer.Length) {
			throw new EndOfStreamException();
		}
	}

	public static T ReadAny<T>(this Stream br)
	{
		var s   = Mem.SizeOf<T>();
		var rg2 = new byte[s];
		var rg  = br.Read(rg2);

		return Mem.ReadFromBytes<T>(rg2);
	}

	public static T ReadAny<T>(this BinaryReader br)
	{
		var s  = Mem.SizeOf<T>();
		var rg = br.ReadBytes(s);

		return Mem.ReadFromBytes<T>(rg);
	}

	public static LinkedList<T> ReadUntil<T>(this Stream s, Predicate<T> pred, Func<Stream, T> read,
	                                         CancellationToken? token = null, int max = Native.INVALID)
	{
		token ??= CancellationToken.None;

		var ll = new LinkedList<T>();
		T   t;

		while (!pred(t = read(s))) {
			ll.AddLast(t);

			if (token.Value.IsCancellationRequested || (max != Native.INVALID && ll.Count >= max)) {
				goto ret;
			}
		}

		ret:
		return ll;
	}

	public static string ReadCString(this BinaryReader br, int count)
	{
		string s = Encoding.ASCII.GetString(br.ReadBytes(count)).TrimEnd('\0');

		return s;
	}
}