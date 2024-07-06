using System.Runtime.InteropServices;
using System.Text;

namespace Novus.Win32;

public static unsafe partial class Native
{

	[DllImport(SHELL32_DLL, CharSet = CharSet.Unicode)]
	public static extern int DragQueryFile(nint hDrop, uint iFile,
	                                       [Out] StringBuilder lpszFile, int cch);

	[DllImport(SHELL32_DLL, CharSet = CharSet.Unicode)]
	public static extern void DragAcceptFiles(nint hDrop, [MA(UT.Bool)] bool fAccept);

	[DllImport(SHELL32_DLL, CharSet = CharSet.Unicode, SetLastError = true)]
	[return: MA(UT.Bool)]
	public static extern bool SHFileOperation(ref SHFILEOPSTRUCT op);

	[DllImport(SHELL32_DLL, SetLastError = true)]
	public static extern nint CommandLineToArgvW([MA(UT.LPWStr)] string commandLine,
	                                             out int argumentCount);

	public const int FO_DELETE          = 0x0003;
	public const int FOF_ALLOWUNDO      = 0x0040;
	public const int FOF_NOCONFIRMATION = 0x0010;

}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct SHFILEOPSTRUCT
{

	public IntPtr hwnd;
	public uint   wFunc;
	public string pFrom;
	public string pTo;
	public ushort fFlags;
	public bool   fAnyOperationsAborted;
	public IntPtr hNameMappings;
	public string lpszProgressTitle;

}