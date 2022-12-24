﻿using System.Runtime.InteropServices;

namespace Novus.Win32.Structures.User32;

/// <summary>Contains message information from a thread's message queue.</summary>
// https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-msg typedef struct tagMSG { HWND hwnd; UINT message; WPARAM
// wParam; LPARAM lParam; DWORD time; POINT pt; DWORD lPrivate; } MSG, *PMSG, *NPMSG, *LPMSG;
[StructLayout(LayoutKind.Sequential)]
public struct MSG
{
	/// <summary>
	/// A handle to the window whose window procedure receives the message. This member is NULL when the message is a thread message.
	/// </summary>
	public nint hwnd;

	/// <summary>The message identifier. Applications can only use the low word; the high word is reserved by the system.</summary>
	public uint message;

	/// <summary>Additional information about the message. The exact meaning depends on the value of the message member.</summary>
	public nint wParam;

	/// <summary>Additional information about the message. The exact meaning depends on the value of the message member.</summary>
	public nint lParam;

	/// <summary>The time at which the message was posted.</summary>
	public uint time;

	/// <summary>The horizontal cursor position, in screen coordinates, when the message was posted.</summary>
	public int pt_x;

	/// <summary>The vertical cursor position, in screen coordinates, when the message was posted.</summary>
	public int pt_y;

	/// <summary>Initializes a new instance of the <see cref="MSG"/> struct.</summary>
	/// <param name="hwnd">
	/// A handle to the window whose window procedure receives the message. This member is NULL when the message is a thread message.
	/// </param>
	/// <param name="msg">The message identifier. Applications can only use the low word; the high word is reserved by the system.</param>
	/// <param name="wParam">Additional information about the message. The exact meaning depends on the value of the message member.</param>
	/// <param name="lParam">Additional information about the message. The exact meaning depends on the value of the message member.</param>
	/// <param name="pt_x">The horizontal cursor position, in screen coordinates, when the message was posted.</param>
	/// <param name="pt_y">The vertical cursor position, in screen coordinates, when the message was posted.</param>
	/// <param name="time">The time at which the message was posted.</param>
	public MSG(nint hwnd, uint msg, nint wParam, nint lParam, int pt_x = default, int pt_y = default,
	           uint time = default)
	{
		this.hwnd   = hwnd;
		message     = msg;
		this.wParam = wParam;
		this.lParam = lParam;
		this.time   = time;
		this.pt_x   = pt_x;
		this.pt_y   = pt_y;
	}
}