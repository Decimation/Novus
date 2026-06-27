using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using Novus.Win32.Structures.Other;
using Novus.Win32.Structures.User32;
using ATOM = ushort;

// ReSharper disable StringLiteralTypo

namespace Novus.Win32;

public static unsafe partial class Native
{

	[DllImport(UNAME_DLL, SetLastError = true)]
	public static extern int GetUName(ATOM wCharCode, [MA(UT.LPWStr)] StringBuilder lpBuf);

#region CRT

	/// <summary>
	/// Size of native runtime (e.g., <see cref="NativeMemory"/>) allocations
	/// </summary>
	[DllImport(UCRTBASE_DLL, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern nuint _msize(void* ptr);

	[DllImport(UCRTBASE_DLL, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern nuint strlen(void* ptr);

#endregion

	[DllImport("urlmon.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false)]
	public static extern int FindMimeFromData(nint                   pBC,
	                                          [MA(UT.LPWStr)] string pwzUrl,
	                                          [MA(UT.LPArray, ArraySubType = UT.I1, SizeParamIndex = 3)]
	                                          byte[] pBuffer,
	                                          int                    cbSize,
	                                          [MA(UT.LPWStr)] string pwzMimeProposed,
	                                          MimeFromDataFlags      dwMimeFlags,
	                                          out nint               ppwzMimeOut,
	                                          int                    dwReserved);

}