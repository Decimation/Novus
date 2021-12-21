using System;

// ReSharper disable UnusedMember.Global
#pragma warning disable	CA1069

namespace Novus.OS.Win32.Structures.Kernel32;

[Flags]
public enum ConsoleModes : uint
{
	#region Input

	ENABLE_PROCESSED_INPUT = 0x0001,
	ENABLE_LINE_INPUT      = 0x0002,
	ENABLE_ECHO_INPUT      = 0x0004,
	ENABLE_WINDOW_INPUT    = 0x0008,
	ENABLE_MOUSE_INPUT     = 0x0010,
	ENABLE_INSERT_MODE     = 0x0020,
	ENABLE_QUICK_EDIT_MODE = 0x0040,
	ENABLE_EXTENDED_FLAGS  = 0x0080,
	ENABLE_AUTO_POSITION   = 0x0100,

	#endregion


	#region Output

	ENABLE_PROCESSED_OUTPUT            = 0x0001,
	ENABLE_WRAP_AT_EOL_OUTPUT          = 0x0002,
	ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004,
	DISABLE_NEWLINE_AUTO_RETURN        = 0x0008,
	ENABLE_LVB_GRID_WORLDWIDE          = 0x0010

	#endregion
}