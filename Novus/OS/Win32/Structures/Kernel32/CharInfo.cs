using System.Runtime.InteropServices;

namespace Novus.OS.Win32.Structures.Kernel32;

[StructLayout(LayoutKind.Explicit)]
public struct CharInfo
{
	[FieldOffset(0)]
	public char UnicodeChar;

	[FieldOffset(0)]
	public char AsciiChar;

	[FieldOffset(2)] //2 bytes seems to work properly
	public CharAttributes Attributes;
}