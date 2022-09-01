using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Novus.Memory;

// ReSharper disable UnusedMember.Global

#nullable enable
namespace Novus.Utilities;

public static class StreamExtensions
{
	public static void ReadFully(this Stream stream, byte[] buffer)
	{
		int offset = 0;
		int readBytes;

		do {
			// If you are using Socket directly instead of a Stream:
			//readBytes = socket.Receive(buffer, offset, buffer.Length - offset,
			//                           SocketFlags.None);

			readBytes =  stream.Read(buffer, offset, buffer.Length - offset);
			
			offset    += readBytes;
		} while (readBytes > 0 && offset < buffer.Length);

		if (offset < buffer.Length) {
			throw new EndOfStreamException();
		}
	}
	public static async Task ReadFullyAsync(this Stream stream, byte[] buffer)
	{
		int offset = 0;
		int readBytes;

		do
		{
			// If you are using Socket directly instead of a Stream:
			//readBytes = socket.Receive(buffer, offset, buffer.Length - offset,
			//                           SocketFlags.None);

			readBytes = await stream.ReadAsync(buffer, offset, buffer.Length - offset);

			offset += readBytes;
		} while (readBytes > 0 && offset < buffer.Length);

		if (offset < buffer.Length)
		{
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

	public static string ReadCString(this BinaryReader br, int count)
	{
		string s = Encoding.ASCII.GetString(br.ReadBytes(count)).TrimEnd('\0');

		return s;
	}
}