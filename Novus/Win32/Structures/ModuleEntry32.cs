using System;
using System.Runtime.InteropServices;

// ReSharper disable IdentifierTypo

namespace Novus.Win32.Structures
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct ModuleEntry32
	{
		internal uint   dwSize;
		internal uint   th32ModuleID;
		internal uint   th32ProcessID;
		internal uint   GlblcntUsage;
		internal uint   ProccntUsage;
		internal IntPtr modBaseAddr;
		internal uint   modBaseSize;
		internal IntPtr hModule;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		internal string szModule;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		internal string szExePath;
	};
}