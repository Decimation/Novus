// Author: Deci | Project: Novus | Name: Bit.Math.cs
// Date: 2026/08/09 @ 00:08:11
#define NEW_BIT

namespace Novus.Numerics;
#if NEW_BIT

public partial struct Bit
{

#region Math properties

	public int GetByteCount()
		=> 1;

	public int GetShortestBitLength()
		=> 1;

	public static Bit PopCount(Bit value)
		=> Byte.PopCount(value.Value);

	public static Bit Abs(Bit value)
		=> value;

	public static bool IsCanonical(Bit value)
	{
		throw new NotImplementedException();
	}

	public static bool IsComplexNumber(Bit value)
	{
		throw new NotImplementedException();
	}

	public static bool IsEvenInteger(Bit value)
	{
		return Byte.IsEvenInteger(value.Value);
	}

	public static bool IsFinite(Bit value)
	{
		throw new NotImplementedException();
	}

	public static bool IsImaginaryNumber(Bit value)
	{
		throw new NotImplementedException();
	}

	public static bool IsInfinity(Bit value)
	{
		throw new NotImplementedException();
	}

	public static bool IsInteger(Bit value)
	{
		return true;
	}

	public static bool IsNaN(Bit value)
	{
		throw new NotImplementedException();
	}

	public static bool IsNegative(Bit value)
	{
		throw new NotImplementedException();
	}

	public static bool IsNegativeInfinity(Bit value)
	{
		throw new NotImplementedException();
	}

	public static bool IsNormal(Bit value)
	{
		throw new NotImplementedException();
	}

	public static bool IsOddInteger(Bit value)
	{
		return Byte.IsOddInteger(value.Value);
	}

	public static bool IsPositive(Bit value)
	{
		throw new NotImplementedException();
	}

	public static bool IsPositiveInfinity(Bit value)
	{
		throw new NotImplementedException();
	}

	public static bool IsRealNumber(Bit value)
	{
		throw new NotImplementedException();
	}

	public static bool IsSubnormal(Bit value)
	{
		throw new NotImplementedException();
	}

	public static bool IsZero(Bit value)
		=> value.TruthValue;

	public static bool IsPow2(Bit value)
		=> Byte.IsPow2(value.Value);

	public static Bit Log2(Bit value)
		=> Byte.Log2(value.Value);

	public static Bit MaxMagnitude(Bit x, Bit y)
	{
		throw new NotImplementedException();
	}

	public static Bit MaxMagnitudeNumber(Bit x, Bit y)
	{
		throw new NotImplementedException();
	}

	public static Bit MinMagnitude(Bit x, Bit y)
	{
		throw new NotImplementedException();
	}

	public static Bit MinMagnitudeNumber(Bit x, Bit y)
	{
		throw new NotImplementedException();
	}

#endregion

}
#endif
