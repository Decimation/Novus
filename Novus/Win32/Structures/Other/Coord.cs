using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace Novus.Win32.Structures.Other;

[StructLayout(LayoutKind.Sequential)]
public struct Coord
{
	public short X;
	public short Y;

	public Coord(short x, short y)
	{
		X = x;
		Y = y;
	}
}