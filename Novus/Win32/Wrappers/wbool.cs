// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable FieldCanBeMadeReadOnly.Local

#pragma warning disable IDE1006, IDE0044
namespace Novus.Win32.Wrappers;

public struct wbool
{
	private int m_value;

	public wbool(int i)
	{
		m_value = i;
	}

	public bool Value => m_value != 0;


	public static implicit operator wbool(bool i) => new(i ? 1 : 0);

	public static implicit operator wbool(int i) => new(i);

	public static explicit operator bool(wbool i) => i.Value;

	public override string ToString()
	{
		return $"{Value} ({m_value})";
	}
}