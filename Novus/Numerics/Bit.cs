// Author: Deci | Project: Novus | Name: Bit.cs
// Date: 2023/08/11 @ 23:08:26

#region

using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Novus.Memory;

#endregion

namespace Novus.Numerics;

#if NEW_BIT
public enum BitMode
{

	None = 0,
	Bitwise = 1,
	Clamp = 2

}

[StructLayout(LayoutKind.Explicit)]
[Serializable]
public partial struct Bit : IBinaryInteger<Bit>, IBitwiseOperators<Bit, Bit, Bit>
{

	[field: FieldOffset(0)]
	public byte Value { get; private set; }

	public byte ClampValue => Math.Clamp(Value, FALSE_0, TRUE_1);

	public Bit(int value, BitMode mode = BitMode.Clamp) : this((byte) value, mode) { }

	public Bit(bool b) : this(b ? TRUE_1 : FALSE_0) { }

	public Bit(byte value, BitMode mode = BitMode.Clamp)
	{
		Value = mode switch
		{
			BitMode.Clamp     => Math.Clamp(value, FALSE_0, TRUE_1),
			BitMode.Bitwise   => (byte) (value & TRUE_1),
			_ or BitMode.None => value
		};
	}

	public Bit() { }

	public static Bit ParseCast<T>(T t)
	{
		return new Bit(Unsafe.As<T, byte>(ref t));
	}

#region Constants

	public const byte TRUE_1 = 1;
	public const byte FALSE_0 = 0;

	public bool TruthValue => Value == TRUE_1;

	static Bit INumberBase<Bit>.One => TRUE_1;

	public static int Radix => 10;

	static Bit INumberBase<Bit>.Zero => FALSE_0;

#endregion

	/*public static implicit operator string(Bit bit)
	{
		return bit.m_value.ToString();
	}*/

	public string ToString(string format, IFormatProvider formatProvider)
		=> Value.ToString(format, formatProvider);

	public override string ToString()
		=> Value.ToString();

#region Equality comparison

	public int CompareTo(object obj)
		=> Value.CompareTo(obj);

	public int CompareTo(Bit other)
		=> Value.CompareTo(other.Value);

	public bool Equals(Bit other)
		=> Value == other.Value;

	public override bool Equals(object obj)
		=> obj is Bit other && Equals(other);

	public override int GetHashCode()
		=> Value.GetHashCode();

#endregion

#region Try Convert

	public static bool TryConvertFromChecked<TOther>(TOther value, out Bit result) where TOther : INumberBase<TOther>
	{
		throw new NotImplementedException();

	}

	public static bool TryConvertFromSaturating<TOther>(TOther value, out Bit result) where TOther : INumberBase<TOther>
	{
		throw new NotImplementedException();
	}

	public static bool TryConvertFromTruncating<TOther>(TOther value, out Bit result) where TOther : INumberBase<TOther>
	{
		throw new NotImplementedException();
	}

	public static bool TryConvertToChecked<TOther>(Bit value, out TOther result) where TOther : INumberBase<TOther>
	{
		throw new NotImplementedException();
	}

	public static bool TryConvertToSaturating<TOther>(Bit value, out TOther result) where TOther : INumberBase<TOther>
	{
		throw new NotImplementedException();
	}

	public static bool TryConvertToTruncating<TOther>(Bit value, out TOther result) where TOther : INumberBase<TOther>
	{
		throw new NotImplementedException();
	}

#endregion

#region Parse

	static Bit INumberBase<Bit>.Parse(string s, NumberStyles style, IFormatProvider provider)
		=> Byte.Parse(s, style, provider);

	public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider provider, out Bit result)
	{
		var ok = Byte.TryParse(s, style, provider, out byte b);
		result = b;
		return ok;
	}

	public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out Bit result)
	{
		var ok = Byte.TryParse(s, style, provider, out byte b);
		result = b;
		return ok;
	}

	static Bit INumberBase<Bit>.Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider provider)
		=> Byte.Parse(s, style, provider);

	static Bit IParsable<Bit>.Parse(string s, IFormatProvider provider)
		=> Byte.Parse(s, provider);

	static Bit ISpanParsable<Bit>.Parse(ReadOnlySpan<char> s, IFormatProvider provider)
		=> Byte.Parse(s, provider);

	public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
		=> Value.TryFormat(destination, out charsWritten, format, provider);

	public static Bit TrailingZeroCount(Bit value)
		=> Byte.TrailingZeroCount(value.Value);

	public static bool TryReadBigEndian(ReadOnlySpan<byte> source, bool isUnsigned, out Bit value)
	{
		var b = source.Length >= 1;
		value = default;

		if (b) {
			value = source[0];
		}

		return b;
	}

	public static bool TryReadLittleEndian(ReadOnlySpan<byte> source, bool isUnsigned, out Bit value)
		=> TryReadBigEndian(source, isUnsigned, out value);

	public bool TryWriteBigEndian(Span<byte> destination, out int bytesWritten)
	{
		destination[0] = Value;
		bytesWritten = GetByteCount();
		return true;
	}

	public bool TryWriteLittleEndian(Span<byte> destination, out int bytesWritten)
	{
		destination[0] = Value;
		bytesWritten = GetByteCount();
		return true;
	}

	public static bool TryParse(string s, IFormatProvider provider, out Bit result)
	{
		var ok = Byte.TryParse(s, provider, out byte b);
		result = b;
		return ok;
	}


	public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out Bit result)
	{
		var ok = Byte.TryParse(s, provider, out byte b);
		result = b;
		return ok;
	}

#endregion

	static Bit IAdditiveIdentity<Bit, Bit>.AdditiveIdentity => TRUE_1;

	static Bit IMultiplicativeIdentity<Bit, Bit>.MultiplicativeIdentity => TRUE_1;

#region Operators

#region Bitwise

	public static Bit operator &(Bit x, Bit y) => x.Value & y.Value;

	public static Bit operator |(Bit x, Bit y) => x.Value | y.Value;

	public static Bit operator ^(Bit x, Bit y) => x.Value ^ y.Value;

	public static Bit operator ~(Bit bit) => (~(bit.Value) & TRUE_1);

	public static Bit operator <<(Bit value, int shiftAmount)
		=> value.Value << shiftAmount;

	public static Bit operator >> (Bit value, int shiftAmount)
		=> value.Value >> shiftAmount;

	public static Bit operator >>> (Bit value, int shiftAmount)
		=> value.Value >>> shiftAmount;

#endregion

#region Conversion

	public static implicit operator Bit(int value) => new Bit(value);

	public static implicit operator Bit(bool value) => new Bit(value);

	public static implicit operator int(Bit bit) => bit.Value;

	public static implicit operator byte(Bit bit) => (byte) bit.Value;

	public static implicit operator bool(Bit bit) => bit.TruthValue;

#endregion

#region Equality

	public static bool operator ==(Bit left, Bit right) => left.Equals(right);

	public static bool operator !=(Bit left, Bit right) => !left.Equals(right);

	public static bool operator >(Bit left, Bit right) => left.Value > right.Value;

	public static bool operator >=(Bit left, Bit right) => left.Value > +right.Value;

	public static bool operator <(Bit left, Bit right) => left.Value < right.Value;

	public static bool operator <=(Bit left, Bit right) => left.Value <= right.Value;

#endregion

	public static Bit operator --(Bit value) => value.Value - 1;

	public static Bit operator /(Bit left, Bit right) => left.Value / right.Value;

	public static Bit operator ++(Bit value) => value.Value + 1;

	public static Bit operator %(Bit left, Bit right) => left.Value % right.Value;

	public static Bit operator *(Bit left, Bit right) => left.Value * right.Value;

	public static Bit operator +(Bit left, Bit right) => left.Value + right.Value;

	public static Bit operator -(Bit left, Bit right) => left.Value + right.Value;

	public static Bit operator -(Bit value) => value;

	public static Bit operator +(Bit value) => value;

#endregion

}

#else

[Serializable]
public struct Bit
{

	private byte m_value;

	private Bit(int value)
	{
		m_value = (byte) (value & 1);
	}

	public static Bit Create<T>(T t) where T : IShiftOperators<T, T, T>, IBitwiseOperators<T, T, T>, INumber<T>
	{
		return new Bit(Unsafe.As<T, int>(ref t));
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
		return m_value;
	}

	public bool AsBool()
	{
		return m_value == 1;
	}

}
#endif