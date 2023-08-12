// Read S Novus Int24.cs
// 2023-08-11 @ 11:15 PM

namespace Novus.Numerics;

// TODO: IMPLEMENT GENERIC MATH INTERFACES

/// <summary>
/// Represents a 24-bit signed integer
/// </summary>
[Serializable]
public struct Int24
{
	private byte m_b0, m_b1, m_b2;
	private Bit  m_sign;

	private Int24(int value)
	{
		m_b0   = (byte) (value & 0xFF);
		m_b1   = (byte) ((value >> 8) & 0xFF);
		m_b2   = (byte) ((value >> 16) & 0x7F);
		m_sign = (byte) ((value >> 23) & 1);
	}

	public static implicit operator Int24(int value)
		=> new(value);

	public static implicit operator int(Int24 i)
	{
		int value = (i.m_b0 | (i.m_b1 << 8) | (i.m_b2 << 16));
		return -(i.m_sign << 23) + value;
	}

	public Bit GetBit(int index)
		=> (this >> index);
}

/// <summary>
/// Represents a 24-bit unsigned integer
/// </summary>
[Serializable]
public struct UInt24
{
	private byte m_b0, m_b1, m_b2;

	private UInt24(uint value)
	{
		m_b0 = (byte) (value & 0xFF);
		m_b1 = (byte) ((value >> 8) & 0xFF);
		m_b2 = (byte) ((value >> 16) & 0xFF);
	}

	public static implicit operator UInt24(uint value)
		=> new(value);

	public static implicit operator uint(UInt24 i)
		=> (uint) (i.m_b0 | (i.m_b1 << 8) | (i.m_b2 << 16));

	public Bit GetBit(int index)
		=> (byte) (this >> index);
}