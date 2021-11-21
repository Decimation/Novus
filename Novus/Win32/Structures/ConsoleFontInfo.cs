using System.Runtime.InteropServices;

// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global

// ReSharper disable InconsistentNaming

namespace Novus.Win32.Structures;

/// <summary>
/// <a href="https://docs.microsoft.com/en-us/windows/console/console-font-infoex">Documentation</a>
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct ConsoleFontInfo
{
	public uint cbSize;

	public uint nFont;

	public Coord dwFontSize;

	public FontFamily FontFamily;

	public FontWeight FontWeight;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
	public string FaceName;
}

/// <summary>
/// <a href="https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-wmf/9a632766-1f1c-4e2b-b1a4-f5b1a45f99ad">Documentation</a>
/// </summary>
public enum FontFamily
{
	FF_DONTCARE   = 0x00,
	FF_ROMAN      = 0x01,
	FF_SWISS      = 0x02,
	FF_MODERN     = 0x03,
	FF_SCRIPT     = 0x04,
	FF_DECORATIVE = 0x05
}

/// <summary>
/// <a href="https://docs.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-createfontw">Documentation</a>
/// </summary>
public enum FontWeight
{
	FW_DONTCARE   = 0,
	FW_THIN       = 100,
	FW_EXTRALIGHT = 200,
	FW_ULTRALIGHT = 200,
	FW_LIGHT      = 300,
	FW_NORMAL     = 400,
	FW_REGULAR    = 400,
	FW_MEDIUM     = 500,
	FW_SEMIBOLD   = 600,
	FW_DEMIBOLD   = 600,
	FW_BOLD       = 700,
	FW_EXTRABOLD  = 800,
	FW_ULTRABOLD  = 800,
	FW_HEAVY      = 900,
	FW_BLACK      = 900
}