// Author: Deci | Project: Novus | Name: SHFILEOPSTRUCT.cs
// Date: 2025/01/23 @ 12:01:18

using System.Runtime.InteropServices;

namespace Novus.Win32.Structures.User32;

/// <remarks>Name: <c>SHFILEOPSTRUCT</c></remarks>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct SHFILEOPSTRUCT
{

	public nint hwnd;
	public uint wFunc;
	public string pFrom;
	public string pTo;
	public ushort fFlags;
	public bool fAnyOperationsAborted;
	public nint hNameMappings;
	public string lpszProgressTitle;

}