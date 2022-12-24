using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Novus.Win32.Structures.User32;

public delegate int OFNHookProcDelegate(int hdlg, int msg, int wParam, int lParam);

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct OPENFILENAME
{
	public int  lStructSize;
	public nint hwndOwner;
	public int  hInstance;

	[MA(UT.LPTStr)]
	public string lpstrFilter;

	[MA(UT.LPTStr)]
	public string lpstrCustomFilter;

	public int nMaxCustFilter;
	public int nFilterIndex;

	[MA(UT.LPTStr)]
	public string lpstrFile;

	public int nMaxFile;

	[MA(UT.LPTStr)]
	public string lpstrFileTitle;

	public int nMaxFileTitle;

	[MA(UT.LPTStr)]
	public string lpstrInitialDir;

	[MA(UT.LPTStr)]
	public string lpstrTitle;

	public int   Flags;
	public short nFileOffset;
	public short nFileExtension;

	[MA(UT.LPTStr)]
	public string lpstrDefExt;

	public int                 lCustData;
	public OFNHookProcDelegate lpfnHook;

	[MA(UT.LPTStr)]
	public string lpTemplateName;

	public int pvReserved;
	public int dwReserved;
	public int FlagsEx;
}