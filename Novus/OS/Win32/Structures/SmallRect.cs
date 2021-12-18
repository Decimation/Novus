using System.Runtime.InteropServices;

namespace Novus.OS.Win32.Structures;

[StructLayout(LayoutKind.Sequential)]
public struct SmallRect
{
	public short Left;
	public short Top;
	public short Right;
	public short Bottom;
}