using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace Novus.Win32.Structures.User32;

[StructLayout(LayoutKind.Sequential)]
public struct FLASHWINFO
{
	public uint            cbSize;
	public nint          hwnd;
	public FlashWindowType dwFlags;
	public uint            uCount;
	public int             dwTimeout;
}