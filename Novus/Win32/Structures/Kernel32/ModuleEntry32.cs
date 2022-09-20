using System.Runtime.InteropServices;

// ReSharper disable IdentifierTypo

namespace Novus.Win32.Structures.Kernel32;

[StructLayout(LayoutKind.Sequential)]
public struct ModuleEntry32
{
	public uint   dwSize;
	public uint   th32ModuleID;
	public uint   th32ProcessID;
	public uint   GlblcntUsage;
	public uint   ProccntUsage;
	public IntPtr modBaseAddr;
	public uint   modBaseSize;
	public IntPtr hModule;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
	public string szModule;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
	public string szExePath;
};