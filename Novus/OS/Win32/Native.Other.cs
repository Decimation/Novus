using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable StringLiteralTypo

namespace Novus.OS.Win32;

public static unsafe partial class Native
{
	[DllImport(UNAME_DLL, SetLastError = true)]
	private static extern int GetUName(ushort wCharCode, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpBuf);

}