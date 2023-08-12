// Read S Novus Bit.cs
// 2023-08-11 @ 11:16 PM

using System.Numerics;

namespace Novus.Numerics;

[Serializable]
public struct Bit
{
	private byte m_value;

	private Bit(int value)
	{
		this.m_value = (byte) (value & 1);
	}

	public static Bit Create<T>(T t) where T : IShiftOperators<T, T, T>, IBitwiseOperators<T, T, T>, INumber<T>
	{
		return new Bit(U.As<T, int>(ref t));
	}

	public static implicit operator Bit(int value)
	{
		return new Bit(value);
	}

	public static implicit operator Bit(bool value)
	{
		return new Bit(value ? 1 : 0);
	}

	public static implicit operator int(Bit bit)
	{
		return bit.m_value;
	}

	public static implicit operator byte(Bit bit)
	{
		return (byte) bit.m_value;
	}

	public static implicit operator bool(Bit bit)
	{
		return bit.m_value == 1;
	}

	public static Bit operator &(Bit x, Bit y)
	{
		return x.m_value & y.m_value;
	}

	public static Bit operator |(Bit x, Bit y)
	{
		return x.m_value | y.m_value;
	}

	public static Bit operator ^(Bit x, Bit y)
	{
		return x.m_value ^ y.m_value;
	}

	public static Bit operator ~(Bit bit)
	{
		return (~(bit.m_value) & 1);
	}

	public static implicit operator string(Bit bit)
	{
		return bit.m_value.ToString();
	}

	public int AsInt()
	{
		return this.m_value;
	}

	public bool AsBool()
	{
		return this.m_value == 1;
	}
}