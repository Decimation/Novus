using System.Diagnostics;
using System.Runtime.InteropServices;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global
#pragma warning disable 414

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace Novus.Win32.Structures.Kernel32;

[StructLayout(LayoutKind.Sequential)]
public struct ShellExecuteInfo
{
	public int    cbSize;
	public uint   fMask;
	public nint hwnd;

	/// <summary>
	/// <see cref="ShellExecuteVerbs"/>; <see cref="ProcessStartInfo.Verb"/>
	/// </summary>
	[MA(UT.LPTStr)]
	public string lpVerb;

	[MA(UT.LPTStr)]
	public string lpFile;

	[MA(UT.LPTStr)]
	public string lpParameters;

	[MA(UT.LPTStr)]
	public string lpDirectory;

	public int    nShow;
	public nint hInstApp;
	public nint lpIDList;

	[MA(UT.LPTStr)]
	public string lpClass;

	public nint hkeyClass;
	public uint   dwHotKey;
	public nint hIcon;
	public nint hProcess;
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
[Flags]
public enum ShellExecuteMaskFlags : uint
{
	SEE_MASK_DEFAULT           = 0x00000000,
	SEE_MASK_CLASSNAME         = 0x00000001,
	SEE_MASK_CLASSKEY          = 0x00000003,
	SEE_MASK_IDLIST            = 0x00000004,
	SEE_MASK_INVOKEIDLIST      = 0x0000000c, // Note SEE_MASK_INVOKEIDLIST(0xC) implies SEE_MASK_IDLIST(0x04)
	SEE_MASK_HOTKEY            = 0x00000020,
	SEE_MASK_NOCLOSEPROCESS    = 0x00000040,
	SEE_MASK_CONNECTNETDRV     = 0x00000080,
	SEE_MASK_NOASYNC           = 0x00000100,
	SEE_MASK_FLAG_DDEWAIT      = SEE_MASK_NOASYNC,
	SEE_MASK_DOENVSUBST        = 0x00000200,
	SEE_MASK_FLAG_NO_UI        = 0x00000400,
	SEE_MASK_UNICODE           = 0x00004000,
	SEE_MASK_NO_CONSOLE        = 0x00008000,
	SEE_MASK_ASYNCOK           = 0x00100000,
	SEE_MASK_HMONITOR          = 0x00200000,
	SEE_MASK_NOZONECHECKS      = 0x00800000,
	SEE_MASK_NOQUERYCLASSSTORE = 0x01000000,
	SEE_MASK_WAITFORINPUTIDLE  = 0x02000000,
	SEE_MASK_FLAG_LOG_USAGE    = 0x04000000,
}