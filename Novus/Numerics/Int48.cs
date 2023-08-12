// Read S Novus Int48.cs
// 2023-08-11 @ 11:16 PM

using System.Numerics;

namespace Novus.Numerics;
// TODO: IMPLEMENT GENERIC MATH INTERFACES

/// <summary>
/// Represents a 48-bit signed integer
/// </summary>
/// <a href="https://github.com/rubendal/BitStream">See</a>
[Serializable]
public struct Int48
{
	private byte m_b0, m_b1, m_b2, m_b3, m_b4, m_b5;
	private Bit  m_sign;

	private Int48(long value)
	{
		m_b0   = (byte) (value & 0xFF);
		m_b1   = (byte) ((value >> 8) & 0xFF);
		m_b2   = (byte) ((value >> 16) & 0xFF);
		m_b3   = (byte) ((value >> 24) & 0xFF);
		m_b4   = (byte) ((value >> 32) & 0xFF);
		m_b5   = (byte) ((value >> 40) & 0x7F);
		m_sign = (byte) ((value >> 47) & 1);
	}

	public static implicit operator Int48(long value)
	{
		return new Int48(value);
	}

	public static implicit operator long(Int48 i)
	{
		long value = i.m_b0 + (i.m_b1 << 8) + (i.m_b2 << 16) + ((long) i.m_b3 << 24) + ((long) i.m_b4 << 32) +
		             ((long) i.m_b5 << 40);
		return -((long) i.m_sign << 47) + value;
	}

	public Bit GetBit(int index)
	{
		return (byte) (this >> index);
	}
}
// Read S Novus UInt48.cs
// 2023-08-11 @ 11:14 PM

/// <summary>
/// Represents a 48-bit unsigned integer
/// </summary>
/// <a href="https://github.com/rubendal/BitStream">See</a>
[Serializable]
public struct UInt48
{
	private byte m_b0, m_b1, m_b2, m_b3, m_b4, m_b5;

	private UInt48(ulong value)
	{
		m_b0 = (byte) (value & 0xFF);
		m_b1 = (byte) ((value >> 8) & 0xFF);
		m_b2 = (byte) ((value >> 16) & 0xFF);
		m_b3 = (byte) ((value >> 24) & 0xFF);
		m_b4 = (byte) ((value >> 32) & 0xFF);
		m_b5 = (byte) ((value >> 40) & 0xFF);
	}

	public static implicit operator UInt48(ulong value)
	{
		return new UInt48(value);
	}

	public static implicit operator ulong(UInt48 i)
	{
		ulong value = (i.m_b0 + ((ulong) i.m_b1 << 8) + ((ulong) i.m_b2 << 16) + ((ulong) i.m_b3 << 24) +
		               ((ulong) i.m_b4 << 32) + ((ulong) i.m_b5 << 40));
		return value;
	}

	public Bit GetBit(int index)
	{
		return (byte) (this >> index);
	}
}