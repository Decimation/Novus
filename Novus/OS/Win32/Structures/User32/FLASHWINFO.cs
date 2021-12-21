using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace Novus.OS.Win32.Structures.User32;

[StructLayout(LayoutKind.Sequential)]
public struct FLASHWINFO
{
	public uint            cbSize;
	public IntPtr          hwnd;
	public FlashWindowType dwFlags;
	public uint            uCount;
	public int             dwTimeout;
}