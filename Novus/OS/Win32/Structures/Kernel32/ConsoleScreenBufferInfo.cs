using System.Runtime.InteropServices;
using Novus.OS.Win32.Structures.Other;

namespace Novus.OS.Win32.Structures.Kernel32;

[StructLayout(LayoutKind.Sequential)]
public struct ConsoleScreenBufferInfo
{
	public Coord dwSize;

	public Coord dwCursorPosition;

	//public short wAttributes;
	public SmallRect srWindow;
	public Coord     dwMaximumWindowSize;
}