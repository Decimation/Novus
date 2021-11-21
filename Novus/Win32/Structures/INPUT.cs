using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global

namespace Novus.Win32.Structures;

[StructLayout(LayoutKind.Sequential)]
public struct Input
{
	public uint       type;
	public InputUnion U;
}

/// <summary>
/// Specifies the type of the input event. This member can be one of the following values. 
/// </summary>
public enum InputType : uint
{
	/// <summary>
	/// INPUT_MOUSE = 0x00 (The event is a mouse event. Use the mi structure of the union.)
	/// </summary>
	Mouse = 0,

	/// <summary>
	/// INPUT_KEYBOARD = 0x01 (The event is a keyboard event. Use the ki structure of the union.)
	/// </summary>
	Keyboard = 1,

	/// <summary>
	/// INPUT_HARDWARE = 0x02 (Windows 95/98/Me: The event is from input hardware other than a keyboard or mouse. Use the hi structure of the union.)
	/// </summary>
	Hardware = 2,
}

[StructLayout(LayoutKind.Sequential)]
public struct MouseInput
{
	public int             dx;
	public int             dy;
	public int             mouseData;
	public MouseEventFlags dwFlags;
	public uint            time;
	public UIntPtr         dwExtraInfo;
}

[StructLayout(LayoutKind.Sequential)]
public struct KeyboardInput
{
	public VirtualKey    wVk;
	public ScanCodeShort wScan;
	public KeyEventFlags dwFlags;
	public int           time;
	public UIntPtr       dwExtraInfo;
}

[Flags]
public enum KeyEventFlags : uint
{
	EXTENDEDKEY = 0x0001,
	KEYUP       = 0x0002,
	SCANCODE    = 0x0008,
	UNICODE     = 0x0004
}


[StructLayout(LayoutKind.Sequential)]
public struct HardwareInput
{
	public int   uMsg;
	public short wParamL;
	public short wParamH;
}

[Flags]
public enum MouseEventFlags : uint
{
	ABSOLUTE        = 0x8000,
	HWHEEL          = 0x01000,
	MOVE            = 0x0001,
	MOVE_NOCOALESCE = 0x2000,
	LEFTDOWN        = 0x0002,
	LEFTUP          = 0x0004,
	RIGHTDOWN       = 0x0008,
	RIGHTUP         = 0x0010,
	MIDDLEDOWN      = 0x0020,
	MIDDLEUP        = 0x0040,
	VIRTUALDESK     = 0x4000,
	WHEEL           = 0x0800,
	XDOWN           = 0x0080,
	XUP             = 0x0100
}

[StructLayout(LayoutKind.Explicit)]
public struct InputUnion
{
	[FieldOffset(0)]
	public MouseInput mi;

	[FieldOffset(0)]
	public KeyboardInput ki;

	[FieldOffset(0)]
	public HardwareInput hi;
}