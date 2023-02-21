using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;
using Novus.Win32.Structures.Other;
using Novus.Win32.Structures.User32;
using InputRecord = Novus.Win32.Structures.User32.InputRecord;

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
	public static extern int CreateWindowEx(int dwExStyle, string lpClassName, string lpWindowName, uint dwStyle,
	                                        int x, int y, int nWidth, int nHeight, int hWndParent, int hMenu,
	                                        int hInstance, int lpParam);

	[DllImport(USER32_DLL)]
	public static extern bool DestroyWindow(int hwnd);

	[DllImport(USER32_DLL, CharSet = CharSet.Auto)]
	public static extern int GetDlgItem(int hDlg, int nIDDlgItem);

	[DllImport(COMDLG32_DLL, CharSet = CharSet.Auto, SetLastError = true)]
	public static extern bool GetOpenFileName(ref OPENFILENAME lpofn);

	[DllImport(USER32_DLL)]
	public static extern int GetParent(int hWnd);

	[DllImport(USER32_DLL)]
	public static extern bool GetWindowRect(int hWnd, ref RECT lpRect);

	[DllImport(USER32_DLL)]
	[return: MarshalAs(UnmanagedType.Bool)]
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
	                                      [MA(UT.LPArray), In] InputRecord[] pInputs,
	                                      int cbSize);

	[LibraryImport(USER32_DLL)]
	[return: MA(UT.Bool)]
	public static partial bool PostMessage(nint hWnd, uint msg, int wParam, int lParam);

	[LibraryImport(USER32_DLL, StringMarshalling = StringMarshalling.Utf16)]
	public static partial nint SendMessage(nint hWnd, int msg, nint wParam,
	                                       [MA(UT.LPWStr)] string lParam);

	[LibraryImport(USER32_DLL, StringMarshalling = StringMarshalling.Utf16)]
	public static partial nint SendMessage(nint hWnd, int msg, int wParam,
	                                       [MA(UT.LPWStr)] string lParam);

	[LibraryImport(USER32_DLL)]
	public static partial nint SendMessage(nint hWnd, int msg, int wParam, nint lParam);

	[DllImport(USER32_DLL, SetLastError = true)]
	// [return: MarshalAs(UnmanagedType.Bool)]
	public static extern int GetMessage(out MSG lpMsg, [Optional] nint hWnd, [Optional] uint wMsgFilterMin,
	                                      [Optional] uint wMsgFilterMax);

	[LibraryImport(USER32_DLL, SetLastError = false)]
	public static partial nint GetMessageExtraInfo();

	[LibraryImport(USER32_DLL, SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
	public static partial uint RegisterWindowMessage(string lpString);

	[LibraryImport(USER32_DLL)]
	public static partial nint DispatchMessage(in MSG lpMsg);

	[LibraryImport(USER32_DLL, SetLastError = false)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool TranslateMessage(in MSG lpMsg);

	[LibraryImport(USER32_DLL, SetLastError = false)]
	public static partial void PostQuitMessage([Optional] int nExitCode);

	[LibraryImport(USER32_DLL, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool PostThreadMessage(uint idThread, uint Msg, [Optional] nint wParam,
	                                             [Optional] nint lParam);

	#region Key

	[DllImport(USER32_DLL)]
	public static extern short GetKeyState(VirtualKey k);

	[DllImport(USER32_DLL)]
	public static extern short GetAsyncKeyState(VirtualKey k);

	[DllImport(USER32_DLL)]
	[return: MA(UT.Bool)]
	public static extern bool GetKeyboardState([MA(UT.LPArray), In] byte[] r);

	[DllImport(USER32_DLL, SetLastError = false, ExactSpelling = true)]
	public static extern void keybd_event(byte bVk, byte bScan, KEYEVENTF dwFlags, nint dwExtraInfo = default);

	[DllImport(USER32_DLL, SetLastError = true, ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool RegisterHotKey(nint hWnd, int id, HotKeyModifiers fsModifiers, uint vk);

	[DllImport(USER32_DLL, SetLastError = true, ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
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
	[return: MarshalAs(UnmanagedType.Bool)]
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
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool OpenClipboard(nint hWndNewOwner);

	[LibraryImport(USER32_DLL, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool CloseClipboard();

	[LibraryImport(USER32_DLL, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool EmptyClipboard();

	[LibraryImport(USER32_DLL, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool IsClipboardFormatAvailable(uint uFormat);

	[LibraryImport(USER32_DLL, SetLastError = true)]
	public static partial uint EnumClipboardFormats(uint uFormat);

	[LibraryImport(USER32_DLL, SetLastError = true)]
	public static partial int CountClipboardFormats();

	[LibraryImport(USER32_DLL, SetLastError = true)]
	public static partial int GetClipboardSequenceNumber();

	[LibraryImport(USER32_DLL, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool AddClipboardFormatListener(IntPtr hwnd);

	[LibraryImport(USER32_DLL, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool RemoveClipboardFormatListener(IntPtr hwnd);

	[LibraryImport(USER32_DLL, SetLastError = true)]
	public static partial IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

	#endregion

	// See http://msdn.microsoft.com/en-us/library/ms633541%28v=vs.85%29.aspx
	// See http://msdn.microsoft.com/en-us/library/ms649033%28VS.85%29.aspx
	[LibraryImport(USER32_DLL)]
	public static partial IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

	[DllImport(USER32_DLL, SetLastError = true, ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool KillTimer([Optional] nint hWnd, nint uIDEvent);

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	public delegate void Timerproc(nint hwnd, uint uMsg, nint idEvent, uint dwTime);

	[DllImport(USER32_DLL, SetLastError = true, ExactSpelling = true)]
	public static extern nint SetTimer([Optional] nint hWnd, [Optional] nint nIDEvent, [Optional] uint uElapse,
	                                   [Optional] Timerproc lpTimerFunc);

	public const int MF_BYCOMMAND = 0x00000000;

	private const string COMDLG32_DLL = "Comdlg32.dll";

	[DllImport(USER32_DLL)]
	public static extern int DeleteMenu(nint hMenu, int nPosition, int wFlags);

	[DllImport(USER32_DLL)]
	public static extern nint GetSystemMenu(nint hWnd, bool bRevert);
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

	PNG = 49299,
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