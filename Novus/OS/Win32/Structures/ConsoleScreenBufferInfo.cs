using System.Runtime.InteropServices;

namespace Novus.OS.Win32.Structures;

[StructLayout(LayoutKind.Sequential)]
public struct ConsoleScreenBufferInfo
{
	public Coord dwSize;

	public Coord dwCursorPosition;

	//public short wAttributes;
	public SmallRect srWindow;
	public Coord     dwMaximumWindowSize;
}