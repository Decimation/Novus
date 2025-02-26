using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;
using Novus.Win32.Structures.Other;
using Novus.Win32.Structures.User32;
using InputRecord = Novus.Win32.Structures.User32.InputRecord;

#pragma warning disable SYSLIB1054

// ReSharper disable IdentifierTypo

// ReSharper disable UnusedMember.Global

// ReSharper disable UnusedMember.Local

namespace Novus.Win32;
#pragma warning disable CA1401, CA2101
public static unsafe partial class Native
{

	[DllImport(COMDLG32_DLL)]
	public static extern int CommDlgExtendedError();

	[DllImport(USER32_DLL)]
	public static extern nint CreateWindowEx(int dwExStyle, string lpClassName, string lpWindowName, uint dwStyle,
	                                         int x, int y, int nWidth, int nHeight, nint hWndParent, nint hMenu,
	                                         nint hInstance, nint lpParam);

	[DllImport(USER32_DLL)]
	public static extern bool DestroyWindow(nint hwnd);

	[DllImport(USER32_DLL, CharSet = CharSet.Auto)]
	public static extern int GetDlgItem(nint hDlg, int nIDDlgItem);

	[DllImport(COMDLG32_DLL, CharSet = CharSet.Auto, SetLastError = true)]
	[return: MA(UT.Bool)]
	public static extern bool GetOpenFileName([In] [Out] ref OpenFileName lpofn);

	[DllImport(USER32_DLL)]
	public static extern nint GetParent(nint hWnd);

	[DllImport(USER32_DLL)]
	public static extern bool GetWindowRect(nint hWnd, ref RECT lpRect);

	[DllImport(USER32_DLL)]
	[return: MA(UT.Bool)]
	public static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int X, int Y, int cx, int cy,
	                                       WindowFlags uFlags);

	[DllImport(USER32_DLL, SetLastError = false)]
	public static extern nint GetDesktopWindow();

	[DllImport(USER32_DLL, SetLastError = true)]
	public static extern nint FindWindowEx(nint parentHandle, nint childAfter, string className,
	                                       string windowTitle);

	[DllImport(USER32_DLL, SetLastError = true, CharSet = CharSet.Unicode)]
	public static extern nint FindWindow(nint zeroOnly, string lpWindowName);

	[DllImport(USER32_DLL, CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern nint GetForegroundWindow();

	[DllImport(USER32_DLL, CharSet = CharSet.Auto, SetLastError = true)]
	public static extern int GetWindowThreadProcessId(nint handle, out int processId);

	[DllImport(USER32_DLL)]
	[return: MA(UT.Bool)]
	public static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

	[DllImport(USER32_DLL)]
	public static extern MessageBoxResult MessageBox(nint hWnd, string text, string caption,
	                                                 MessageBoxOptions options);

	[DllImport(USER32_DLL)]
	internal static extern uint SendInput(uint nInputs,
	                                      [MA(UT.LPArray)] [In] InputRecord[] pInputs,
	                                      int cbSize);

	[DllImport(USER32_DLL)]
	[return: MA(UT.Bool)]
	public static extern bool PostMessage(nint hWnd, uint msg, int wParam, int lParam);

	[DllImport(USER32_DLL)]
	public static extern nint SendMessage(nint hWnd, int msg, nint wParam,
	                                      [MA(UT.LPWStr)] string lParam);

	[DllImport(USER32_DLL)]
	public static extern nint SendMessage(nint hWnd, int msg, int wParam,
	                                      [MA(UT.LPWStr)] string lParam);

	[DllImport(USER32_DLL)]
	public static extern nint SendMessage(nint hWnd, int msg, nint wParam, nint lParam);

	[DllImport(USER32_DLL, SetLastError = true)]
	[return: MA(UT.Bool)]
	public static extern bool GetMessage(ref MSG lpMsg, [Opt] nint hWnd, [Opt] uint wMsgFilterMin,
	                                     [Opt] uint wMsgFilterMax);

	/*
	[DllImport(USER32_DLL, SetLastError = true)]
	// [return: MarshalAs(UnmanagedType.Bool)]
	public static extern int GetMessage(ref MSG lpMsg, [Optional] nint hWnd, [Optional] uint wMsgFilterMin,
	                                    [Optional] uint wMsgFilterMax);
	                                    */

	[DllImport(USER32_DLL)]
	public static extern nint GetMessageExtraInfo();

	[DllImport(USER32_DLL, SetLastError = true, CharSet = CharSet.Unicode)]
	public static extern uint RegisterWindowMessage(string lpString);

	[DllImport(USER32_DLL)]
	public static extern nint DispatchMessage(ref MSG lpMsg);

	[DllImport(USER32_DLL, SetLastError = false)]
	[return: MA(UT.Bool)]
	public static extern bool TranslateMessage(ref MSG lpMsg);

	[DllImport(USER32_DLL, SetLastError = false)]
	public static extern void PostQuitMessage([Opt] int nExitCode);

	[DllImport(USER32_DLL, SetLastError = true)]
	[return: MA(UT.Bool)]
	public static extern bool PostThreadMessage(uint idThread, uint Msg, [Opt] nint wParam,
	                                            [Opt] nint lParam);

	#region Key

	[DllImport(USER32_DLL)]
	public static extern short GetKeyState(VirtualKey k);

	[DllImport(USER32_DLL)]
	public static extern short GetAsyncKeyState(VirtualKey k);

	[DllImport(USER32_DLL)]
	[return: MA(UT.Bool)]
	public static extern bool GetKeyboardState([MA(UT.LPArray)] [In] byte[] r);

	[DllImport(USER32_DLL, SetLastError = false, ExactSpelling = true)]
	public static extern void keybd_event(byte bVk, byte bScan, KEYEVENTF dwFlags, nint dwExtraInfo = default);

	[DllImport(USER32_DLL, SetLastError = true, ExactSpelling = true)]
	[return: MA(UT.Bool)]
	public static extern bool RegisterHotKey(nint hWnd, int id, HotKeyModifiers fsModifiers, uint vk);

	[DllImport(USER32_DLL, SetLastError = true, ExactSpelling = true)]
	[return: MA(UT.Bool)]
	public static extern bool UnregisterHotKey(nint hWnd, int id);

	#endregion

	[DllImport(USER32_DLL)]
	public static extern nint GetFocus();

	[DllImport(USER32_DLL)]
	[return: MA(UT.Bool)]
	private static extern bool SetForegroundWindow(nint hWnd);

	[DllImport(USER32_DLL, CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int GetWindowText(nint hWnd, StringBuilder lpString, int nMaxCount);

	[DllImport(USER32_DLL, CharSet = CharSet.Auto)]
	private static extern int GetWindowTextLength(nint hWnd);

	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	private class SearchData
	{

		public string Wndclass;
		public string Title;
		public nint   hWnd;

	}

	private delegate bool EnumWindowsProc(nint hWnd, ref SearchData data);

	[DllImport(USER32_DLL)]
	[return: MA(UT.Bool)]
	private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, ref SearchData data);

	[DllImport(USER32_DLL, SetLastError = true, CharSet = CharSet.Auto)]
	public static extern int GetClassName(nint hWnd, StringBuilder lpClassName, int nMaxCount);

	#region Clipboard

	/// <summary>
	/// Retrieves the name of the format from the clipboard.
	/// </summary>
	/// <param name="format">The type of format to be retrieved. This parameter must not specify any of the predefined clipboard formats.</param>
	/// <param name="lpszFormatName">The format name string.</param>
	/// <param name="cchMaxCount">
	/// The length of the <paramref name="lpszFormatName"/> buffer, in characters. The buffer must be large enough to include the
	/// terminating null character; otherwise, the format name string is truncated to <paramref name="cchMaxCount"/>-1 characters.
	/// </param>
	/// <returns>
	/// If the function succeeds, the return value is the number of characters copied to the buffer.
	/// If the function fails, the return value is zero. To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.
	/// </returns>
	[DllImport(USER32_DLL, SetLastError = true, CharSet = CharSet.Unicode)]
	public static extern int GetClipboardFormatName(uint format, [Out] StringBuilder lpszFormatName, int cchMaxCount);

	[LibraryImport(USER32_DLL, SetLastError = true)]
	public static partial nint GetClipboardData(uint uFormat);

	[LibraryImport(USER32_DLL, SetLastError = true)]
	public static partial nint SetClipboardData(uint uFormat, void* hMem);

	[LibraryImport(USER32_DLL, SetLastError = true)]
	[return: MA(UT.Bool)]
	public static partial bool OpenClipboard(nint hWndNewOwner);

	[LibraryImport(USER32_DLL, SetLastError = true)]
	[return: MA(UT.Bool)]
	public static partial bool CloseClipboard();

	[LibraryImport(USER32_DLL, SetLastError = true)]
	[return: MA(UT.Bool)]
	public static partial bool EmptyClipboard();

	[LibraryImport(USER32_DLL, SetLastError = true)]
	[return: MA(UT.Bool)]
	public static partial bool IsClipboardFormatAvailable(uint uFormat);

	[LibraryImport(USER32_DLL, SetLastError = true)]
	public static partial uint EnumClipboardFormats(uint uFormat);

	[LibraryImport(USER32_DLL, SetLastError = true)]
	public static partial int CountClipboardFormats();

	[LibraryImport(USER32_DLL, SetLastError = true)]
	public static partial int GetClipboardSequenceNumber();

	[LibraryImport(USER32_DLL, SetLastError = true)]
	[return: MA(UT.Bool)]
	public static partial bool AddClipboardFormatListener(nint hwnd);

	[LibraryImport(USER32_DLL, SetLastError = true)]
	[return: MA(UT.Bool)]
	public static partial bool RemoveClipboardFormatListener(nint hwnd);

	[LibraryImport(USER32_DLL, SetLastError = true)]
	public static partial nint SetClipboardViewer(nint hWndNewViewer);

	#endregion

	// See http://msdn.microsoft.com/en-us/library/ms633541%28v=vs.85%29.aspx
	// See http://msdn.microsoft.com/en-us/library/ms649033%28VS.85%29.aspx
	[LibraryImport(USER32_DLL)]
	public static partial nint SetParent(nint hWndChild, nint hWndNewParent);

	[DllImport(USER32_DLL, SetLastError = true, ExactSpelling = true)]
	[return: MA(UT.Bool)]
	public static extern bool KillTimer([Opt] nint hWnd, nint uIDEvent);

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	public delegate void TimerProc(nint hwnd, uint uMsg, nint idEvent, uint dwTime);

	[DllImport(USER32_DLL, SetLastError = true, ExactSpelling = true)]
	public static extern nint SetTimer([Opt] nint hWnd, [Opt] nint nIDEvent, [Opt] uint uElapse,
	                                   [Opt] TimerProc lpTimerFunc);

	public const int MF_BYCOMMAND = 0x00000000;

	private const string COMDLG32_DLL = "Comdlg32.dll";

	[DllImport(USER32_DLL)]
	public static extern int DeleteMenu(nint hMenu, int nPosition, int wFlags);

	[DllImport(USER32_DLL)]
	public static extern nint GetSystemMenu(nint hWnd, bool bRevert);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public unsafe delegate nint WndProc(nint hWnd, WindowMessage msg, void* wParam, void* lParam);

	[DllImport(USER32_DLL, SetLastError = true)]
	[return: MA(UT.U2)]
	public static extern short RegisterClassEx(ref WNDCLASSEX lpwcx);

}

public enum SysCommand : uint
{

	SC_CLOSE    = 0xF060,
	SC_MINIMIZE = 0xF020,
	SC_MAXIMIZE = 0xF030,
	SC_SIZE     = 0xF000,

}

[Flags]
public enum KEYEVENTF
{

	/// <summary>If specified, the scan code was preceded by a prefix byte having the value 0xE0 (224).</summary>
	KEYEVENTF_EXTENDEDKEY = 0x0001,

	/// <summary>If specified, the key is being released. If not specified, the key is being depressed.</summary>
	KEYEVENTF_KEYUP = 0x0002,

	/// <summary>
	/// If specified, the system synthesizes a VK_PACKET keystroke. The wVk parameter must be zero. This flag can only be combined
	/// with the KEYEVENTF_KEYUP flag. For more information, see the Remarks section.
	/// </summary>
	KEYEVENTF_UNICODE = 0x0004,

	/// <summary>If specified, wScan identifies the key and wVk is ignored.</summary>
	KEYEVENTF_SCANCODE = 0x0008,

}

[Flags]
public enum HotKeyModifiers
{

	/// <summary>Nothing held down.</summary>
	MOD_NONE = 0,

	/// <summary>Either ALT key must be held down.</summary>
	MOD_ALT = 0x0001,

	/// <summary>Either CTRL key must be held down.</summary>
	MOD_CONTROL = 0x0002,

	/// <summary>Either SHIFT key must be held down.</summary>
	MOD_SHIFT = 0x0004,

	/// <summary>
	/// Either WINDOWS key was held down. These keys are labeled with the Windows logo. Keyboard shortcuts that involve the WINDOWS
	/// key are reserved for use by the operating system.
	/// </summary>
	MOD_WIN = 0x0008,

	/// <summary>
	/// Changes the hotkey behavior so that the keyboard auto-repeat does not yield multiple hotkey notifications.
	/// <para>Windows Vista: This flag is not supported.</para>
	/// </summary>
	MOD_NOREPEAT = 0x4000,

}

//todo
public enum ClipboardFormat : uint
{

	CF_TEXT        = 1,
	CF_UNICODETEXT = 13,
	CF_OEMTEXT     = 7,
	CF_HDROP       = 15,
	CF_DIB         = 8,
	CF_DIBV5       = 17,
	CF_BITMAP      = 2,

	FileName  = 0xC006,
	FileNameW = 0xC007,

	// PNG=49273,

	PNG  = 49299,
	PNG2 = 49496,
	BMP2 = 49443,

}

public enum HandleWindowPosition
{

	HWND_BOTTOM    = 1,
	HWND_NOTOPMOST = -2,
	HWND_TOP       = 0,
	HWND_TOPMOST   = -1,

}

/// <summary>
///     Blittable version of Windows BOOL type. It is convenient in situations where
///     manual marshalling is required, or to avoid overhead of regular bool marshalling.
/// </summary>
/// <remarks>
///     Some Windows APIs return arbitrary integer values although the return type is defined
///     as BOOL. It is best to never compare BOOL to TRUE. Always use bResult != BOOL.FALSE
///     or bResult == BOOL.FALSE .
/// </remarks>
public enum BOOL
{

	FALSE = 0,
	TRUE  = 1

}

public enum WindowFlags
{

	TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE,

	SWP_ASYNCWINDOWPOS = 0x4000,
	SWP_DEFERERASE     = 0x2000,
	SWP_DRAWFRAME      = 0x0020,
	SWP_FRAMECHANGED   = 0x0020,
	SWP_HIDEWINDOW     = 0x0080,
	SWP_NOACTIVATE     = 0x0010,
	SWP_NOCOPYBITS     = 0x0100,
	SWP_NOMOVE         = 0x0002,
	SWP_NOOWNERZORDER  = 0x0200,
	SWP_NOREDRAW       = 0x0008,
	SWP_NOREPOSITION   = 0x0200,
	SWP_NOSENDCHANGING = 0x0400,
	SWP_NOSIZE         = 0x0001,
	SWP_NOZORDER       = 0x0004,
	SWP_SHOWWINDOW     = 0x0040,

}

[Flags]
public enum ClassStyles : uint
{

	/// <summary>
	/// Aligns the window's client area on a byte boundary (in the x direction). This style
	/// affects the width of the window and its horizontal placement on the display.
	/// </summary>
	CS_BYTEALIGNCLIENT = 0x1000,

	/// <summary>
	/// Aligns the window on a byte boundary (in the x direction). This style affects the width
	/// of the window and its horizontal placement on the display.
	/// </summary>
	CS_BYTEALIGNWINDOW = 0x2000,

	/// <summary>
	/// Allocates one device context to be shared by all windows in the class. Because window
	/// classes are process specific, it is possible for multiple threads of an application to
	/// create a window of the same class. It is also possible for the threads to attempt to use
	/// the device context simultaneously. When this happens, the system allows only one thread
	/// to successfully finish its drawing operation.
	/// </summary>
	CS_CLASSDC = 0x0040,

	/// <summary>
	/// Sends a double-click message to the window procedure when the user double-clicks the
	/// mouse while the cursor is within a window belonging to the class.
	/// </summary>
	CS_DBLCLKS = 0x0008,

	/// <summary>
	/// Enables the drop shadow effect on a window. The effect is turned on and off through
	/// SPI_SETDROPSHADOW. Typically, this is enabled for small, short-lived windows such as
	/// menus to emphasize their Z order relationship to other windows.
	/// </summary>
	CS_DROPSHADOW = 0x00020000,

	/// <summary>
	/// Indicates that the window class is an application global class.
	/// </summary>
	CS_GLOBALCLASS = 0x4000,

	/// <summary>
	/// Redraws the entire window if a movement or size adjustment changes the width of the
	/// client area.
	/// </summary>
	CS_HREDRAW = 0x0002,

	/// <summary>
	/// Disables Close on the window menu.
	/// </summary>
	CS_NOCLOSE = 0x0200,

	/// <summary>
	/// Allocates a unique device context for each window in the class.
	/// </summary>
	CS_OWNDC = 0x0020,

	/// <summary>
	/// Sets the clipping rectangle of the child window to that of the parent window so that the
	/// child can draw on the parent. A window with the CS_PARENTDC style bit receives a regular
	/// device context from the system's cache of device contexts. It does not give the child the
	/// parent's device context or device context settings. Specifying CS_PARENTDC enhances an
	/// application's performance.
	/// </summary>
	CS_PARENTDC = 0x0080,

	/// <summary>
	/// Saves, as a bitmap, the portion of the screen image obscured by a window of this class.
	/// When the window is removed, the system uses the saved bitmap to restore the screen image,
	/// including other windows that were obscured. Therefore, the system does not send WM_PAINT
	/// messages to windows that were obscured if the memory used by the bitmap has not been
	/// discarded and if other screen actions have not invalidated the stored image. This style
	/// is useful for small windows (for example, menus or dialog
	/// boxes) that are displayed briefly and then removed before other screen activity takes
	/// place. This style increases the time required to display the window, because the system
	/// must first allocate memory to store the bitmap.
	/// </summary>
	CS_SAVEBITS = 0x0800,

	/// <summary>
	/// Redraws the entire window if a movement or size adjustment changes the height of the
	/// client area.
	/// </summary>
	CS_VREDRAW = 0x0001,

}

public unsafe partial struct WNDCLASSEX
{

	public int cbSize;

	public ClassStyles style;

	[MA(UT.FunctionPtr)]
	public Native.WndProc lpfnWndProc;

	public int cbClsExtra;

	public int cbWndExtra;

	public nint hInstance;

	public nint hIcon;

	public nint hCursor;

	public nint hbrBackground;

	public char* lpszMenuName;

	public char* lpszClassName;

	public nint hIconSm;

	public static WNDCLASSEX Create()
	{
		var nw = default(WNDCLASSEX);
		nw.cbSize = Marshal.SizeOf(typeof(WNDCLASSEX));
		return nw;
	}

}

[Flags]
public enum WindowStylesEx : uint
{

	/// <summary>
	/// Specifies a window that accepts drag-drop files.
	/// </summary>
	WS_EX_ACCEPTFILES = 0x00000010,

	/// <summary>
	/// Forces a top-level window onto the taskbar when the window is visible.
	/// </summary>
	WS_EX_APPWINDOW = 0x00040000,

	/// <summary>
	/// Specifies a window that has a border with a sunken edge.
	/// </summary>
	WS_EX_CLIENTEDGE = 0x00000200,

	/// <summary>
	/// Specifies a window that paints all descendants in bottom-to-top painting order using
	/// double-buffering. This cannot be used if the window has a class style of either CS_OWNDC
	/// or CS_CLASSDC. This style is not supported in Windows 2000.
	/// </summary>
	/// <remarks>
	/// With WS_EX_COMPOSITED set, all descendants of a window get bottom-to-top painting order
	/// using double-buffering. Bottom-to-top painting order allows a descendant window to have
	/// translucency (alpha) and transparency (color-key) effects, but only if the descendant
	/// window also has the WS_EX_TRANSPARENT bit set. Double-buffering allows the window and its
	/// descendents to be painted without flicker.
	/// </remarks>
	WS_EX_COMPOSITED = 0x02000000,

	/// <summary>
	/// Specifies a window that includes a question mark in the title bar. When the user clicks
	/// the question mark, the cursor changes to a question mark with a pointer. If the user then
	/// clicks a child window, the child receives a WM_HELP message. The child window should pass
	/// the message to the parent window procedure, which should call the WinHelp function using
	/// the HELP_WM_HELP command. The Help application displays a pop-up window that typically
	/// contains help for the child window. WS_EX_CONTEXTHELP cannot be used with the
	/// WS_MAXIMIZEBOX or WS_MINIMIZEBOX styles.
	/// </summary>
	WS_EX_CONTEXTHELP = 0x00000400,

	/// <summary>
	/// Specifies a window which contains child windows that should take part in dialog box
	/// navigation. If this style is specified, the dialog manager recurses into children of this
	/// window when performing navigation operations such as handling the TAB key, an arrow key,
	/// or a keyboard mnemonic.
	/// </summary>
	WS_EX_CONTROLPARENT = 0x00010000,

	/// <summary>
	/// Specifies a window that has a double border.
	/// </summary>
	WS_EX_DLGMODALFRAME = 0x00000001,

	/// <summary>
	/// Specifies a window that is a layered window. This cannot be used for child windows or if
	/// the window has a class style of either CS_OWNDC or CS_CLASSDC.
	/// </summary>
	WS_EX_LAYERED = 0x00080000,

	/// <summary>
	/// Specifies a window with the horizontal origin on the right edge. Increasing horizontal
	/// values advance to the left. The shell language must support reading-order alignment for
	/// this to take effect.
	/// </summary>
	WS_EX_LAYOUTRTL = 0x00400000,

	/// <summary>
	/// Specifies a window that has generic left-aligned properties. This is the default.
	/// </summary>
	WS_EX_LEFT = 0x00000000,

	/// <summary>
	/// Specifies a window with the vertical scroll bar (if present) to the left of the client
	/// area. The shell language must support reading-order alignment for this to take effect.
	/// </summary>
	WS_EX_LEFTSCROLLBAR = 0x00004000,

	/// <summary>
	/// Specifies a window that displays text using left-to-right reading-order properties. This
	/// is the default.
	/// </summary>
	WS_EX_LTRREADING = 0x00000000,

	/// <summary>
	/// Specifies a multiple-document interface (MDI) child window.
	/// </summary>
	WS_EX_MDICHILD = 0x00000040,

	/// <summary>
	/// Specifies a top-level window created with this style does not become the foreground
	/// window when the user clicks it. The system does not bring this window to the foreground
	/// when the user minimizes or closes the foreground window. The window does not appear on
	/// the taskbar by default. To force the window to appear on the taskbar, use the
	/// WS_EX_APPWINDOW style. To activate the window, use the SetActiveWindow or
	/// SetForegroundWindow function.
	/// </summary>
	WS_EX_NOACTIVATE = 0x08000000,

	/// <summary>
	/// Specifies a window which does not pass its window layout to its child windows.
	/// </summary>
	WS_EX_NOINHERITLAYOUT = 0x00100000,

	/// <summary>
	/// Specifies that a child window created with this style does not send the WM_PARENTNOTIFY
	/// message to its parent window when it is created or destroyed.
	/// </summary>
	WS_EX_NOPARENTNOTIFY = 0x00000004,

	/// <summary>
	/// Specifies an overlapped window.
	/// </summary>
	WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,

	/// <summary>
	/// Specifies a palette window, which is a modeless dialog box that presents an array of commands.
	/// </summary>
	WS_EX_PALETTEWINDOW = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST,

	/// <summary>
	/// Specifies a window that has generic "right-aligned" properties. This depends on the
	/// window class. The shell language must support reading-order alignment for this to take
	/// effect. Using the WS_EX_RIGHT style has the same effect as using the SS_RIGHT (static),
	/// ES_RIGHT (edit), and BS_RIGHT/BS_RIGHTBUTTON (button) control styles.
	/// </summary>
	WS_EX_RIGHT = 0x00001000,

	/// <summary>
	/// Specifies a window with the vertical scroll bar (if present) to the right of the client
	/// area. This is the default.
	/// </summary>
	WS_EX_RIGHTSCROLLBAR = 0x00000000,

	/// <summary>
	/// Specifies a window that displays text using right-to-left reading-order properties. The
	/// shell language must support reading-order alignment for this to take effect.
	/// </summary>
	WS_EX_RTLREADING = 0x00002000,

	/// <summary>
	/// Specifies a window with a three-dimensional border style intended to be used for items
	/// that do not accept user input.
	/// </summary>
	WS_EX_STATICEDGE = 0x00020000,

	/// <summary>
	/// Specifies a window that is intended to be used as a floating toolbar. A tool window has a
	/// title bar that is shorter than a normal title bar, and the window title is drawn using a
	/// smaller font. A tool window does not appear in the taskbar or in the dialog that appears
	/// when the user presses ALT+TAB. If a tool window has a system menu, its icon is not
	/// displayed on the title bar. However, you can display the system menu by right-clicking or
	/// by typing ALT+SPACE.
	/// </summary>
	WS_EX_TOOLWINDOW = 0x00000080,

	/// <summary>
	/// Specifies a window that should be placed above all non-topmost windows and should stay
	/// above them, even when the window is deactivated. To add or remove this style, use the
	/// SetWindowPos function.
	/// </summary>
	WS_EX_TOPMOST = 0x00000008,

	/// <summary>
	/// Specifies a window that should not be painted until siblings beneath the window (that
	/// were created by the same thread) have been painted. The window appears transparent
	/// because the bits of underlying sibling windows have already been painted. To achieve
	/// transparency without these restrictions, use the SetWindowRgn function.
	/// </summary>
	WS_EX_TRANSPARENT = 0x00000020,

	/// <summary>
	/// Specifies a window that has a border with a raised edge.
	/// </summary>
	WS_EX_WINDOWEDGE = 0x00000100,

	/// <summary>
	/// The window does not render to a redirection surface. This is for windows that do not
	/// have visible content or that use mechanisms other than surfaces to provide their visual.
	/// </summary>
	WS_EX_NOREDIRECTIONBITMAP = 0x00200000,

}