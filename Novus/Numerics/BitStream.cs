﻿using System.Runtime.InteropServices;
using System.Text;
using Novus.Memory;
using static System.Net.WebRequestMethods;
using static Novus.Numerics.BitCalculator;
using File = System.IO.File;

// ReSharper disable InconsistentNaming

namespace Novus.Numerics;

/// <summary>
/// Stream wrapper to use bit-level operations
/// </summary>
/// <a href="https://github.com/rubendal/BitStream">See</a>
public class BitStream : IDisposable, IAsyncDisposable
{
	private long     m_offset;
	private int      m_bit;
	private bool     m_msb;
	public  Stream   Stream   { get; private set; }
	public  Encoding Encoding { get; set; }

	/// <summary>
	/// Allows the <see cref="BitStream"/> auto increase in size when needed
	/// </summary>
	public bool AutoIncreaseStream { get; set; }

	/// <summary>
	/// Get the stream length
	/// </summary>
	public long Length
	{
		get => Stream.Length;
	}

	/// <summary>
	/// Get the current bit position in the stream
	/// </summary>
	public long BitPosition
	{
		get => m_bit;
	}

	/// <summary>
	/// Check if <see cref="BitStream"/> offset is inside the stream length
	/// </summary>
	private bool ValidPosition
	{
		get { return m_offset < Length; }
	}

	#region Constructors

	/// <summary>
	/// Creates a <see cref="BitStream"/> using a Stream
	/// </summary>
	/// <param name="stream">Stream to use</param>
	/// <param name="MSB">true if Most Significant Bit will be used, if false LSB will be used</param>
	public BitStream(Stream stream, bool MSB = false)
	{
		Stream = new MemoryStream();
		stream.CopyTo(Stream);
		m_msb              = MSB;
		m_offset           = 0;
		m_bit              = 0;
		Encoding           = Encoding.UTF8;
		AutoIncreaseStream = false;
	}

	/// <summary>
	/// Creates a <see cref="BitStream"/> using a Stream
	/// </summary>
	/// <param name="stream">Stream to use</param>
	/// <param name="encoding">Encoding to use with chars</param>
	/// <param name="MSB">true if Most Significant Bit will be used, if false LSB will be used</param>
	public BitStream(Stream stream, Encoding encoding, bool MSB = false)
	{
		Stream = new MemoryStream();
		stream.CopyTo(Stream);
		m_msb              = MSB;
		m_offset           = 0;
		m_bit              = 0;
		Encoding           = encoding;
		AutoIncreaseStream = false;
	}

	/// <summary>
	/// Creates a <see cref="BitStream"/> using a byte[]
	/// </summary>
	/// <param name="buffer">byte[] to use</param>
	/// <param name="MSB">true if Most Significant Bit will be used, if false LSB will be used</param>
	public BitStream(byte[] buffer, bool MSB = false)
	{
		Stream = new MemoryStream();
		var m = new MemoryStream(buffer);
		m.CopyTo(Stream);
		m_msb              = MSB;
		m_offset           = 0;
		m_bit              = 0;
		Encoding           = Encoding.UTF8;
		AutoIncreaseStream = false;
	}

	/// <summary>
	/// Creates a <see cref="BitStream"/> using a byte[]
	/// </summary>
	/// <param name="buffer">byte[] to use</param>
	/// <param name="encoding">Encoding to use with chars</param>
	/// <param name="MSB">true if Most Significant Bit will be used, if false LSB will be used</param>
	public BitStream(byte[] buffer, Encoding encoding, bool MSB = false)
	{
		Stream = new MemoryStream();
		var m = new MemoryStream(buffer);
		m.CopyTo(Stream);
		m_msb              = MSB;
		m_offset           = 0;
		m_bit              = 0;
		Encoding           = encoding;
		AutoIncreaseStream = false;
	}

	/// <summary>
	/// Creates a <see cref="BitStream"/> using a byte[]
	/// </summary>
	/// <param name="buffer">byte[] to use</param>
	/// <param name="msb">true if Most Significant Bit will be used, if false LSB will be used</param>
	public static BitStream Create(byte[] buffer, bool msb = false)
	{
		return new BitStream(buffer, msb);
	}

	/// <summary>
	/// Creates a <see cref="BitStream"/> using a byte[]
	/// </summary>
	/// <param name="buffer">byte[] to use</param>
	/// <param name="encoding">Encoding to use with chars</param>
	/// <param name="msb">true if Most Significant Bit will be used, if false LSB will be used</param>
	public static BitStream Create(byte[] buffer, Encoding encoding, bool msb = false)
	{
		return new BitStream(buffer, encoding, msb);
	}

	public static BitStream CreateFromFile(string path, Encoding encoding = null)
	{
		if (!File.Exists(path)) {
			throw new IOException("File doesn't exist");
		}

		if (File.GetAttributes(path) == FileAttributes.Directory) {
			throw new IOException("Path is a directory");
		}

		if (encoding == null) {
			encoding = Encoding.UTF8;
		}

		return new BitStream(File.ReadAllBytes(path), encoding);
	}

	#endregion

	#region Methods

	/// <summary>
	/// Seek to the specified offset and check if it is a valid position for reading in the stream
	/// </summary>
	/// <param name="offset">offset on the stream</param>
	/// <param name="bit">bit position</param>
	/// <returns>true if offset is valid to do reading, false otherwise</returns>
	public bool this[long offset, int bit]
	{
		get
		{
			Seek(offset, bit);
			return ValidPosition;
		}
		//set {
		//    Seek(offset, bit);
		//}
		// private set { }
	}

	public void ResetPosition()
		=> Seek(0, 0);

	public void Seek(long offset, int bit)
	{
		if (offset > Length) {
			m_offset = Length;
		}
		else {
			if (offset >= 0) {
				m_offset = offset;
			}
			else {
				offset = 0;
			}
		}

		if (bit >= BITS_PER_BYTE) {
			int n = bit / BITS_PER_BYTE;
			m_offset += n;
			m_bit    =  bit % BITS_PER_BYTE;
		}
		else {
			m_bit = bit;
		}

		Stream.Seek(offset, SeekOrigin.Begin);
	}

	/// <summary>
	/// Advances the stream by one bit
	/// </summary>
	public void AdvanceBit()
	{
		m_bit = (m_bit + 1) % BITS_PER_BYTE;

		if (m_bit == 0) {
			m_offset++;
		}
	}

	/// <summary>
	/// Returns the stream by one bit
	/// </summary>
	public void ReturnBit()
	{
		m_bit = m_bit - 1 == -1 ? 7 : m_bit - 1;

		if (m_bit == 7) {
			m_offset--;
		}

		if (m_offset < 0) {
			m_offset = 0;
		}
	}

	public byte[] ReadToEnd()
	{
		Stream.Seek(0, SeekOrigin.Begin);
		var s = new MemoryStream();
		Stream.CopyTo(s);
		Seek(m_offset, m_bit);
		return s.ToArray();
	}

	/// <summary>
	/// Changes the length of the stream, if new length is less than current length stream data will be truncated
	/// </summary>
	/// <param name="length">InitInline stream length</param>
	/// <returns>return true if stream changed length, false if it wasn't possible</returns>
	public bool ChangeLength(long length)
	{
		if (Stream.CanSeek && Stream.CanWrite) {
			Stream.SetLength(length);
			return true;
		}
		else {
			return false;
		}
	}

	/// <summary>
	/// Cuts the <see cref="BitStream"/> from the specified offset and given length, will throw an exception when length + offset is higher than stream's length, offset and bit will be set to 0
	/// </summary>
	/// <param name="offset">Offset to start</param>
	/// <param name="length">Length of the new <see cref="BitStream"/></param>
	public void CutStream(long offset, long length)
	{
		byte[] data   = ReadToEnd();
		byte[] buffer = new byte[length];
		Array.Copy(data, offset, buffer, 0, length);
		Stream = new MemoryStream();
		var m = new MemoryStream(buffer);
		Stream = new MemoryStream();
		m.CopyTo(Stream);
		m_offset = 0;
		m_bit    = 0;
	}

	/// <summary>
	/// Copies the current <see cref="BitStream"/> buffer to another <see cref="System.IO.Stream"/>
	/// </summary>
	/// <param name="stream"><see cref="System.IO.Stream"/> to copy buffer</param>
	public void CopyStreamTo(Stream stream)
	{
		Seek(0, 0);
		stream.SetLength(Stream.Length);
		Stream.CopyTo(stream);
	}

	/// <summary>
	/// Copies the current <see cref="BitStream"/> buffer to another <see cref="BitStream"/>
	/// </summary>
	/// <param name="stream"><see cref="BitStream"/> to copy buffer</param>
	public void CopyStreamTo(BitStream stream)
	{
		Seek(0, 0);
		stream.ChangeLength(Stream.Length);
		Stream.CopyTo(stream.Stream);
		stream.Seek(0, 0);
	}

	/// <summary>
	/// Saves current <see cref="BitStream"/> buffer into a file
	/// </summary>
	/// <param name="filename">File to write data, if it exists it will be overwritten</param>
	public void SaveStreamAsFile(string filename)
	{
		File.WriteAllBytes(filename, ReadToEnd());
	}

	/// <summary>
	/// Returns the current content of the stream as a <see cref="MemoryStream"/>
	/// </summary>
	/// <returns><see cref="MemoryStream"/> containing current <see cref="BitStream"/> data</returns>
	public MemoryStream CloneAsMemoryStream()
	{
		return new MemoryStream(ReadToEnd());
	}

	/// <summary>
	/// Returns the current content of the stream as a <see cref="BufferedStream"/>
	/// </summary>
	/// <returns><see cref="BufferedStream"/> containing current <see cref="BitStream"/> data</returns>
	public BufferedStream CloneAsBufferedStream()
	{
		var bs = new BufferedStream(Stream);
		var sw = new StreamWriter(bs);
		sw.Write(ReadToEnd());
		bs.Seek(0, SeekOrigin.Begin);
		return bs;
	}

	/// <summary>
	/// Checks if the <see cref="BitStream"/> will be in a valid position on its last bit read/write
	/// </summary>
	/// <param name="bits">Number of bits it will advance</param>
	/// <returns>true if <see cref="BitStream"/> will be inside the stream length</returns>
	private bool ValidPositionWhen(int bits)
	{
		long o = m_offset;
		int  b = m_bit;
		b = (b + 1) % BITS_PER_BYTE;

		if (b == 0) {
			o++;
		}

		return o < Length;
	}

	#endregion

	#region BitRead/Write

	/// <summary>
	/// Read current position bit and advances the position within the stream by one bit
	/// </summary>
	/// <returns>Returns the current position bit as 0 or 1</returns>
	public Bit ReadBit()
	{
		if (!ValidPosition) {
			throw new IOException("Cannot read in an offset bigger than the length of the stream");
		}

		Stream.Seek(m_offset, SeekOrigin.Begin);
		byte value;

		if (!m_msb) {
			value = (byte) (Stream.ReadByte() >> m_bit & 1);
		}
		else {
			value = (byte) (Stream.ReadByte() >> 7 - m_bit & 1);
		}

		AdvanceBit();
		Stream.Seek(m_offset, SeekOrigin.Begin);
		return value;
	}

	/// <summary>
	/// Read from current position the specified number of bits
	/// </summary>
	/// <param name="length">Bits to read</param>
	/// <returns><see cref="Bit"/>[] containing read bits</returns>
	public Bit[] ReadBits(int length)
	{
		var bits = new Bit[length];

		for (int i = 0; i < length; i++) {
			bits[i] = ReadBit();
		}

		return bits;
	}

	/// <summary>
	/// Writes a bit in the current position
	/// </summary>
	/// <param name="data">Bit to write, it data is not 0 or 1 data = data &amp; 1</param>
	public void WriteBit(Bit data)
	{
		Stream.Seek(m_offset, SeekOrigin.Begin);
		byte value = (byte) Stream.ReadByte();
		Stream.Seek(m_offset, SeekOrigin.Begin);

		if (!m_msb) {
			value &= (byte) ~(1 << m_bit);
			value |= (byte) (data << m_bit);
		}
		else {
			value &= (byte) ~(1 << 7 - m_bit);
			value |= (byte) (data << 7 - m_bit);
		}

		if (ValidPosition) {
			Stream.WriteByte(value);
		}
		else {
			if (AutoIncreaseStream) {
				if (ChangeLength(Length + (m_offset - Length) + 1)) {
					Stream.WriteByte(value);
				}
				else {
					throw new IOException("Cannot write in an offset bigger than the length of the stream");
				}
			}
			else {
				throw new IOException("Cannot write in an offset bigger than the length of the stream");
			}
		}

		AdvanceBit();
		Stream.Seek(m_offset, SeekOrigin.Begin);
	}

	/// <summary>
	/// Write a sequence of bits into the stream
	/// </summary>
	/// <param name="bits"><see cref="Bit"/>[] to write</param>
	public void WriteBits(IList<Bit> bits)
	{
		foreach (Bit b in bits) {
			WriteBit(b);
		}
	}

	/// <summary>
	/// Write a sequence of bits into the stream
	/// </summary>
	/// <param name="bits"><see cref="Bit"/>[] to write</param>
	/// <param name="length">Number of bits to write</param>
	public void WriteBits(IList<Bit> bits, int length)
	{
		var b = new Bit[bits.Count];
		bits.CopyTo(b, 0);

		for (int i = 0; i < length; i++) {
			WriteBit(b[i]);
		}
	}

	/// <summary>
	/// Write a sequence of bits into the stream
	/// </summary>
	/// <param name="bits"><see cref="Bit"/>[] to write</param>
	/// <param name="offset">Offset to begin bit writing</param>
	/// <param name="length">Number of bits to write</param>
	public void WriteBits(Bit[] bits, int offset, int length)
	{
		for (int i = offset; i < length; i++) {
			WriteBit(bits[i]);
		}
	}

	#endregion

	#region Read

	/// <summary>
	/// Read from the current position bit the specified number of bits or bytes and creates a byte[] 
	/// </summary>
	/// <param name="length">Number of bits or bytes</param>
	/// <param name="isBytes">if true will consider length as byte length, if false it will count the specified length of bits</param>
	/// <returns>byte[] containing bytes created from current position</returns>
	public byte[] ReadBytes(long length, bool isBytes = false)
	{
		if (isBytes) {
			length *= BITS_PER_BYTE;
		}

		var data = new List<byte>();

		for (long i = 0; i < length;) {
			byte value = 0;

			for (int p = 0; p < BITS_PER_BYTE && i < length; i++, p++) {
				if (!m_msb) {
					value |= (byte) (ReadBit() << p);
				}
				else {
					value |= (byte) (ReadBit() << 7 - p);
				}
			}

			data.Add(value);
		}

		return data.ToArray();
	}

	public T Read<T>()
	{
		var          size  = Mem.SizeOf<T>();
		var          bsize = size * BITS_PER_BYTE;
		T            val   = default;
		Memory<byte> rg    = ReadBytes(size, true);
		using var    mh    = rg.Pin();
		var          ptr   = Mem.AddressOf(ref val).Cast();

		unsafe {
			NativeMemory.Copy(mh.Pointer, (void*) ptr.ToPointer(), (nuint) size);

		}

		return val;
	}

	public void Write<T>(T t)
	{
		var size  = Mem.SizeOf<T>();
		var bsize = size * BITS_PER_BYTE;
		var rg    = new byte[size];
		var ptr   = Mem.AddressOf(ref t).Cast();
		ptr.CopyTo(rg);
		WriteBytes(rg, rg.Length, true);
	}

	/// <summary>
	/// Read a byte based on the current stream and bit position
	/// </summary>
	public byte ReadByte()
	{
		return ReadBytes(BITS_PER_BYTE)[0];
	}

	/// <summary>
	/// Read a byte made of specified number of bits (1-8)
	/// </summary>
	public byte ReadByte(int bits)
	{
		if (bits < 0) {
			bits = 0;
		}

		if (bits > BITS_PER_BYTE) {
			bits = BITS_PER_BYTE;
		}

		return ReadBytes(bits)[0];
	}

	/// <summary>
	/// Read a signed byte based on the current stream and bit position
	/// </summary>
	public sbyte ReadSByte()
	{
		return (sbyte) ReadBytes(BITS_PER_BYTE)[0];
	}

	/// <summary>
	/// Read a sbyte made of specified number of bits (1-8)
	/// </summary>
	public sbyte ReadSByte(int bits)
	{
		if (bits < 0) {
			bits = 0;
		}

		if (bits > BITS_PER_BYTE) {
			bits = BITS_PER_BYTE;
		}

		return (sbyte) ReadBytes(bits)[0];
	}

	/// <summary>
	/// Read a byte based on the current stream and bit position and check if it is 0
	/// </summary>
	public bool ReadBool()
	{
		return ReadBytes(BITS_PER_BYTE)[0] != 0;
	}

	/// <summary>
	/// Read a char based on the current stream and bit position and the <see cref="BitStream"/> encoding
	/// </summary>
	public char ReadChar()
	{
		return Encoding.GetChars(ReadBytes(Encoding.GetMaxByteCount(1) * BITS_PER_BYTE))[0];
	}

	/// <summary>
	/// Read a string based on the current stream and bit position and the <see cref="BitStream"/> encoding
	/// </summary>
	/// <param name="length">Length of the string to read</param>
	public string ReadString(int length)
	{
		int bitsPerChar = Encoding.GetByteCount(" ") * BITS_PER_BYTE;
		return Encoding.GetString(ReadBytes(bitsPerChar * length));
	}

	/// <summary>
	/// Read a short based on the current stream and bit position
	/// </summary>
	public short ReadInt16()
	{
		short value = BitConverter.ToInt16(ReadBytes(16), 0);
		return value;
	}

	/// <summary>
	/// Read a 24bit value based on the current stream and bit position
	/// </summary>
	public Int24 ReadInt24()
	{
		byte[] bytes = ReadBytes(24);
		Array.Resize(ref bytes, 4);
		Int24 value = BitConverter.ToInt32(bytes, 0);
		return value;
	}

	/// <summary>
	/// Read an int based on the current stream and bit position
	/// </summary>
	public int ReadInt32()
	{
		int value = BitConverter.ToInt32(ReadBytes(32), 0);
		return value;
	}

	/// <summary>
	/// Read a 48bit value based on the current stream and bit position
	/// </summary>
	public Int48 ReadInt48()
	{
		byte[] bytes = ReadBytes(48);
		Array.Resize(ref bytes, BITS_PER_BYTE);
		Int48 value = BitConverter.ToInt64(bytes, 0);
		return value;
	}

	/// <summary>
	/// Read a long based on the current stream and bit position
	/// </summary>
	public long ReadInt64()
	{
		long value = BitConverter.ToInt64(ReadBytes(64), 0);
		return value;
	}

	/// <summary>
	/// Read a ushort based on the current stream and bit position
	/// </summary>
	public ushort ReadUInt16()
	{
		ushort value = BitConverter.ToUInt16(ReadBytes(16), 0);
		return value;
	}

	/// <summary>
	/// Read an unsigned 24bit value based on the current stream and bit position
	/// </summary>
	public UInt24 ReadUInt24()
	{
		byte[] bytes = ReadBytes(24);
		Array.Resize(ref bytes, 4);
		UInt24 value = BitConverter.ToUInt32(bytes, 0);
		return value;
	}

	/// <summary>
	/// Read an uint based on the current stream and bit position
	/// </summary>
	public uint ReadUInt32()
	{
		uint value = BitConverter.ToUInt32(ReadBytes(32), 0);
		return value;
	}

	/// <summary>
	/// Read an unsigned 48bit value based on the current stream and bit position
	/// </summary>
	public UInt48 ReadUInt48()
	{
		byte[] bytes = ReadBytes(48);
		Array.Resize(ref bytes, BITS_PER_BYTE);
		UInt48 value = BitConverter.ToUInt64(bytes, 0);
		return value;
	}

	/// <summary>
	/// Read an ulong based on the current stream and bit position
	/// </summary>
	public ulong ReadUInt64()
	{
		ulong value = BitConverter.ToUInt64(ReadBytes(64), 0);
		return value;
	}

	#endregion

	#region Write

	/// <summary>
	/// Writes as bits a byte[] by a specified number of bits or bytes
	/// </summary>
	/// <param name="data">byte[] to write</param>
	/// <param name="length">Number of bits or bytes to use from the array</param>
	/// <param name="isBytes">if true will consider length as byte length, if false it will count the specified length of bits</param>
	public void WriteBytes(byte[] data, long length, bool isBytes = false)
	{
		if (isBytes) {
			length *= BITS_PER_BYTE;
		}

		int position = 0;

		for (long i = 0; i < length;) {
			byte value = 0;

			for (int p = 0; p < BITS_PER_BYTE && i < length; i++, p++) {
				if (!m_msb) {
					value = (byte) (data[position] >> p & 1);
				}
				else {
					value = (byte) (data[position] >> 7 - p & 1);
				}

				WriteBit(value);
			}

			position++;
		}
	}

	/// <summary>
	/// Write a byte value based on the current stream and bit position
	/// </summary>
	public void WriteByte(byte value)
	{
		WriteBytes(new byte[] { value }, BITS_PER_BYTE);
	}

	/// <summary>
	/// Write a byte value based on the current stream and bit position
	/// </summary>
	public void WriteByte(byte value, int bits)
	{
		if (bits < 0) {
			bits = 0;
		}

		if (bits > BITS_PER_BYTE) {
			bits = BITS_PER_BYTE;
		}

		WriteBytes(new byte[] { value }, bits);
	}

	/// <summary>
	/// Write a byte value based on the current stream and bit position
	/// </summary>
	public void WriteSByte(sbyte value)
	{
		WriteBytes(new byte[] { (byte) value }, BITS_PER_BYTE);
	}

	/// <summary>
	/// Write a byte value based on the current stream and bit position
	/// </summary>
	public void WriteSByte(sbyte value, int bits)
	{
		if (bits < 0) {
			bits = 0;
		}

		if (bits > BITS_PER_BYTE) {
			bits = BITS_PER_BYTE;
		}

		WriteBytes(new byte[] { (byte) value }, bits);
	}

	/// <summary>
	/// Write a bool value as 0:false, 1:true as byte based on the current stream and bit position
	/// </summary>
	public void WriteBool(bool value)
	{
		WriteBytes([value ? (byte) 1 : (byte) 0], BITS_PER_BYTE);
	}

	/// <summary>
	/// Write a char value based on the <see cref="BitStream"/> encoding
	/// </summary>
	public void WriteChar(char value)
	{
		byte[] bytes = Encoding.GetBytes(new char[] { value }, 0, 1);
		WriteBytes(bytes, bytes.Length * BITS_PER_BYTE);
	}

	/// <summary>
	/// Write a string based on the <see cref="BitStream"/> encoding
	/// </summary>
	public void WriteString(string value)
	{
		byte[] bytes = Encoding.GetBytes(value);
		WriteBytes(bytes, bytes.Length * BITS_PER_BYTE);
	}

	/// <summary>
	/// Write a short value based on the current stream and bit position
	/// </summary>
	public void WriteInt16(short value)
	{
		WriteBytes(BitConverter.GetBytes(value), 16);
	}

	/// <summary>
	/// Write a 24bit value based on the current stream and bit position
	/// </summary>
	public void WriteInt24(Int24 value)
	{
		WriteBytes(BitConverter.GetBytes(value), 24);
	}

	/// <summary>
	/// Write an int value based on the current stream and bit position
	/// </summary>
	public void WriteInt32(int value)
	{
		WriteBytes(BitConverter.GetBytes(value), 32);
	}

	/// <summary>
	/// Write a 48bit value based on the current stream and bit position
	/// </summary>
	public void WriteInt48(Int48 value)
	{
		WriteBytes(BitConverter.GetBytes(value), 48);
	}

	/// <summary>
	/// Write a long value based on the current stream and bit position
	/// </summary>
	public void WriteInt64(long value)
	{
		WriteBytes(BitConverter.GetBytes(value), 64);
	}

	/// <summary>
	/// Write an ushort value based on the current stream and bit position
	/// </summary>
	public void WriteUInt16(ushort value)
	{
		WriteBytes(BitConverter.GetBytes(value), 16);
	}

	/// <summary>
	/// Write an unsigned 24bit value based on the current stream and bit position
	/// </summary>
	public void WriteUInt24(UInt24 value)
	{
		WriteBytes(BitConverter.GetBytes(value), 24);
	}

	/// <summary>
	/// Write an uint value based on the current stream and bit position
	/// </summary>
	public void WriteUInt32(uint value)
	{
		WriteBytes(BitConverter.GetBytes(value), 32);
	}

	/// <summary>
	/// Write an unsigned 48bit value based on the current stream and bit position
	/// </summary>
	public void WriteUInt48(UInt48 value)
	{
		WriteBytes(BitConverter.GetBytes(value), 48);
	}

	/// <summary>
	/// Write an ulong value based on the current stream and bit position
	/// </summary>
	public void WriteUInt64(ulong value)
	{
		WriteBytes(BitConverter.GetBytes(value), 64);
	}

	#endregion

	#region Shifts

	/// <summary>
	/// Do a bitwise shift on the current position of the stream on bit 0
	/// </summary>
	/// <param name="bits">bits to shift</param>
	/// <param name="leftShift">true to left shift, false to right shift</param>
	public void BitwiseShift(int bits, bool leftShift)
	{
		if (!ValidPositionWhen(BITS_PER_BYTE)) {
			throw new IOException("Cannot read in an offset bigger than the length of the stream");
		}

		Seek(m_offset, 0);

		if (bits != 0 && bits <= 7) {
			byte value = (byte) Stream.ReadByte();

			if (leftShift) {
				value = (byte) (value << bits);
			}
			else {
				value = (byte) (value >> bits);
			}

			Seek(m_offset, 0);
			Stream.WriteByte(value);
		}

		m_bit = 0;
		m_offset++;
	}

	/// <summary>
	/// Do a bitwise shift on the current position of the stream on current bit
	/// </summary>
	/// <param name="bits">bits to shift</param>
	/// <param name="leftShift">true to left shift, false to right shift</param>
	public void BitwiseShiftOnBit(int bits, bool leftShift)
	{
		if (!ValidPositionWhen(BITS_PER_BYTE)) {
			throw new IOException("Cannot read in an offset bigger than the length of the stream");
		}

		Seek(m_offset, m_bit);

		if (bits != 0 && bits <= 7) {
			byte value = ReadByte();

			if (leftShift) {
				value = (byte) (value << bits);
			}
			else {
				value = (byte) (value >> bits);
			}

			m_offset--;
			Seek(m_offset, m_bit);
			WriteByte(value);
		}

		m_offset++;
	}

	/// <summary>
	/// Do a circular shift on the current position of the stream on bit 0
	/// </summary>
	/// <param name="bits">bits to shift</param>
	/// <param name="leftShift">true to left shift, false to right shift</param>
	public void CircularShift(int bits, bool leftShift)
	{
		if (!ValidPositionWhen(BITS_PER_BYTE)) {
			throw new IOException("Cannot read in an offset bigger than the length of the stream");
		}

		Seek(m_offset, 0);

		if (bits != 0 && bits <= 7) {
			byte value = (byte) Stream.ReadByte();

			if (leftShift) {
				value = (byte) (value << bits | value >> BITS_PER_BYTE - bits);
			}
			else {
				value = (byte) (value >> bits | value << BITS_PER_BYTE - bits);
			}

			Seek(m_offset, 0);
			Stream.WriteByte(value);
		}

		m_bit = 0;
		m_offset++;
	}

	/// <summary>
	/// Do a circular shift on the current position of the stream on current bit
	/// </summary>
	/// <param name="bits">bits to shift</param>
	/// <param name="leftShift">true to left shift, false to right shift</param>
	public void CircularShiftOnBit(int bits, bool leftShift)
	{
		if (!ValidPositionWhen(BITS_PER_BYTE)) {
			throw new IOException("Cannot read in an offset bigger than the length of the stream");
		}

		Seek(m_offset, m_bit);

		if (bits != 0 && bits <= 7) {
			byte value = ReadByte();

			if (leftShift) {
				value = (byte) (value << bits | value >> BITS_PER_BYTE - bits);
			}
			else {
				value = (byte) (value >> bits | value << BITS_PER_BYTE - bits);
			}

			m_offset--;
			Seek(m_offset, m_bit);
			WriteByte(value);
		}

		m_offset++;
	}

	#endregion

	#region Bitwise Operators

	/// <summary>
	/// Apply an and operator on the current stream and bit position byte and advances one byte position
	/// </summary>
	/// <param name="x">Byte value to apply and</param>
	public void And(byte x)
	{
		if (!ValidPositionWhen(BITS_PER_BYTE)) {
			throw new IOException("Cannot read in an offset bigger than the length of the stream");
		}

		Seek(m_offset, m_bit);
		byte value = ReadByte();
		m_offset--;
		Seek(m_offset, m_bit);
		WriteByte((byte) (value & x));
	}

	/// <summary>
	/// Apply an or operator on the current stream and bit position byte and advances one byte position
	/// </summary>
	/// <param name="x">Byte value to apply or</param>
	public void Or(byte x)
	{
		if (!ValidPositionWhen(BITS_PER_BYTE)) {
			throw new IOException("Cannot read in an offset bigger than the length of the stream");
		}

		Seek(m_offset, m_bit);
		byte value = ReadByte();
		m_offset--;
		Seek(m_offset, m_bit);
		WriteByte((byte) (value | x));
	}

	/// <summary>
	/// Apply a xor operator on the current stream and bit position byte and advances one byte position
	/// </summary>
	/// <param name="x">Byte value to apply xor</param>
	public void Xor(byte x)
	{
		if (!ValidPositionWhen(BITS_PER_BYTE)) {
			throw new IOException("Cannot read in an offset bigger than the length of the stream");
		}

		Seek(m_offset, m_bit);
		byte value = ReadByte();
		m_offset--;
		Seek(m_offset, m_bit);
		WriteByte((byte) (value ^ x));
	}

	/// <summary>
	/// Apply a not operator on the current stream and bit position byte and advances one byte position
	/// </summary>
	public void Not()
	{
		if (!ValidPositionWhen(BITS_PER_BYTE)) {
			throw new IOException("Cannot read in an offset bigger than the length of the stream");
		}

		Seek(m_offset, m_bit);
		byte value = ReadByte();
		m_offset--;
		Seek(m_offset, m_bit);
		WriteByte((byte) ~value);
	}

	/// <summary>
	/// Apply an and operator on the current stream and bit position and advances one bit position
	/// </summary>
	/// <param name="bit">Bit value to apply and</param>
	public void BitAnd(Bit bit)
	{
		if (!ValidPosition) {
			throw new IOException("Cannot read in an offset bigger than the length of the stream");
		}

		Seek(m_offset, m_bit);
		Bit value = ReadBit();
		ReturnBit();
		WriteBit(bit & value);
	}

	/// <summary>
	/// Apply an or operator on the current stream and bit position and advances one bit position
	/// </summary>
	/// <param name="bit">Bit value to apply or</param>
	public void BitOr(Bit bit)
	{
		if (!ValidPosition) {
			throw new IOException("Cannot read in an offset bigger than the length of the stream");
		}

		Seek(m_offset, m_bit);
		Bit value = ReadBit();
		ReturnBit();
		WriteBit(bit | value);
	}

	/// <summary>
	/// Apply a xor operator on the current stream and bit position and advances one bit position
	/// </summary>
	/// <param name="bit">Bit value to apply xor</param>
	public void BitXor(Bit bit)
	{
		if (!ValidPosition) {
			throw new IOException("Cannot read in an offset bigger than the length of the stream");
		}

		Seek(m_offset, m_bit);
		Bit value = ReadBit();
		ReturnBit();
		WriteBit(bit ^ value);
	}

	/// <summary>
	/// Apply a not operator on the current stream and bit position and advances one bit position
	/// </summary>
	public void BitNot()
	{
		if (!ValidPosition) {
			throw new IOException("Cannot read in an offset bigger than the length of the stream");
		}

		Seek(m_offset, m_bit);
		Bit value = ReadBit();
		ReturnBit();
		WriteBit(~value);
	}

	/// <summary>
	/// Reverses the bit order on the byte in the current position of the stream
	/// </summary>
	public void ReverseBits()
	{
		if (!ValidPosition) {
			throw new IOException("Cannot read in an offset bigger than the length of the stream");
		}

		Seek(m_offset, 0);
		byte value = ReadByte();
		m_offset--;
		Seek(m_offset, 0);
		WriteByte(value.ReverseBits());
	}

	#endregion

	public void Dispose()
	{
		Stream?.Dispose();
	}

	public async ValueTask DisposeAsync()
	{
		if (Stream != null) {
			await Stream.DisposeAsync();
		}
	}
}