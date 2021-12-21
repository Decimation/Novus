// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global
namespace Novus.OS.Win32.Structures.User32;

public enum FlashWindowType : uint
{
	/// <summary>
	/// Stop flashing. The system restores the window to its original state.
	/// </summary>    
	FLASHW_STOP = 0,

	/// <summary>
	/// Flash the window caption
	/// </summary>
	FLASHW_CAPTION = 1,

	/// <summary>
	/// Flash the taskbar button.
	/// </summary>
	FLASHW_TRAY = 2,

	/// <summary>
	/// Flash both the window caption and taskbar button.
	/// This is equivalent to setting the <see cref="FLASHW_CAPTION"/> | <see cref="FLASHW_TRAY"/> flags.
	/// </summary>
	FLASHW_ALL = 3,

	/// <summary>
	/// Flash continuously, until the <seealso cref="FLASHW_STOP"/> flag is set.
	/// </summary>
	FLASHW_TIMER = 4,

	/// <summary>
	/// Flash continuously until the window comes to the foreground.
	/// </summary>
	FLASHW_TIMERNOFG = 12
}