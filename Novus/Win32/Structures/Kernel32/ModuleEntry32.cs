using System.Runtime.InteropServices;

// ReSharper disable IdentifierTypo

namespace Novus.Win32.Structures.Kernel32;

[StructLayout(LayoutKind.Sequential)]
public struct ModuleEntry32
{
	public uint dwSize;
	public uint th32ModuleID;
	public uint th32ProcessID;
	public uint GlblcntUsage;
	public uint ProccntUsage;
	public nint modBaseAddr;
	public uint modBaseSize;
	public nint hModule;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
	public string szModule;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
	public string szExePath;

	public override string ToString()
	{
		return
			$"{nameof(dwSize)}: {dwSize}, {nameof(th32ModuleID)}: {th32ModuleID}, " +
			$"{nameof(th32ProcessID)}: {th32ProcessID}, {nameof(GlblcntUsage)}: {GlblcntUsage}, " +
			$"{nameof(ProccntUsage)}: {ProccntUsage}, {nameof(modBaseAddr)}: {modBaseAddr}, " +
			$"{nameof(modBaseSize)}: {modBaseSize}, {nameof(hModule)}: {hModule}, {nameof(szModule)}: {szModule}, " +
			$"{nameof(szExePath)}: {szExePath}";
	}
};