﻿using System.Runtime.InteropServices;
using System.Text;

// ReSharper disable StringLiteralTypo

namespace Novus.Win32;

public static unsafe partial class Native
{
	[DllImport(UNAME_DLL, SetLastError = true)]
	private static extern int GetUName(ushort wCharCode, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpBuf);

	[DllImport(UCRTBASE_DLL, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern nuint _msize(void* ptr);

	[DllImport(UCRTBASE_DLL, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern nuint strlen(void* ptr);

}