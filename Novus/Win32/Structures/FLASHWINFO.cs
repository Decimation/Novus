using System;
using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace Novus.Win32.Structures;

[StructLayout(LayoutKind.Sequential)]
public struct FLASHWINFO
{
	public uint            cbSize;
	public IntPtr          hwnd;
	public FlashWindowType dwFlags;
	public uint            uCount;
	public int             dwTimeout;
}