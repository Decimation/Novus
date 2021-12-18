using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global
#pragma warning disable 414

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace Novus.OS.Win32.Structures;

[StructLayout(LayoutKind.Sequential)]
public struct ShellExecuteInfo
{
	public int    cbSize;
	public uint   fMask;
	public IntPtr hwnd;

	/// <summary>
	/// <see cref="ShellExecuteVerbs"/>; <see cref="ProcessStartInfo.Verb"/>
	/// </summary>
	[MarshalAs(UnmanagedType.LPTStr)]
	public string lpVerb;

	[MarshalAs(UnmanagedType.LPTStr)]
	public string lpFile;

	[MarshalAs(UnmanagedType.LPTStr)]
	public string lpParameters;

	[MarshalAs(UnmanagedType.LPTStr)]
	public string lpDirectory;

	public int    nShow;
	public IntPtr hInstApp;
	public IntPtr lpIDList;

	[MarshalAs(UnmanagedType.LPTStr)]
	public string lpClass;

	public IntPtr hkeyClass;
	public uint   dwHotKey;
	public IntPtr hIcon;
	public IntPtr hProcess;
}

/// <summary>
/// <see cref="ShellExecuteInfo.lpVerb"/>
/// <a href="https://www.pinvoke.net/default.aspx/shell32/ShellExecuteEx.html">See</a>
/// </summary>
public static class ShellExecuteVerbs
{
	/// <summary>
	/// Opens a file or a application
	/// </summary>
	public const string Open = "open";

	/// <summary>
	/// Opens dialog when no program is associated to the extension
	/// </summary>
	public const string OpenAs = "openas";

	public const string OpenNew = "opennew";

	/// <summary>
	/// In Windows 7 and Vista, opens the UAC dialog and in others, open the Run as... Dialog
	/// </summary>
	public const string Runas = "runas";

	/// <summary>
	/// Specifies that the operation is the default for the selected file type.
	/// </summary>
	public const string Null = "null";

	/// <summary>
	/// Opens the default text editor for the file.
	/// </summary>
	public const string Edit = "edit";

	/// <summary>
	/// Opens the Windows Explorer in the folder specified in <seealso cref="ShellExecuteInfo.lpDirectory"/>.
	/// </summary>
	public const string Explore = "explore";

	/// <summary>
	/// Opens the properties window of the file.
	/// </summary>
	public const string Properties = "properties";

	public const string Copy  = "copy";
	public const string Cut   = "cut";
	public const string Paste = "paste";

	/// <summary>
	/// Pastes a shortcut
	/// </summary>
	public const string PasteLink = "pastelink";

	public const string Delete  = "delete";
	public const string Print   = "print";
	public const string PrintTo = "printto";

	/// <summary>
	/// Start a search
	/// </summary>
	public const string Find = "find";
}