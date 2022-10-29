using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;
using Novus.Win32.Structures.User32;
using InputRecord = Novus.Win32.Structures.User32.InputRecord;

// ReSharper disable IdentifierTypo

// ReSharper disable UnusedMember.Global

// ReSharper disable UnusedMember.Local

namespace Novus.Win32;
#pragma warning disable CA1401, CA2101
public static unsafe partial class Native
{
	[DllImport(USER32_DLL)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy,
	                                       WindowFlags uFlags);

	[DllImport(USER32_DLL, SetLastError = false)]
	public static extern IntPtr GetDesktopWindow();

	[DllImport(USER32_DLL)]
	private static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

	[DllImport(USER32_DLL, SetLastError = true)]
	public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className,
	                                         string windowTitle);

	[DllImport(USER32_DLL, SetLastError = true, CharSet = CharSet.Unicode)]
	private static extern IntPtr FindWindow(IntPtr zeroOnly, string lpWindowName);

	[DllImport(USER32_DLL, CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern IntPtr GetForegroundWindow();

	[DllImport(USER32_DLL, CharSet = CharSet.Auto, SetLastError = true)]
	public static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

	[DllImport(USER32_DLL)]
	[return: MA(UT.Bool)]
	private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

	[DllImport(USER32_DLL)]
	public static extern MessageBoxResult MessageBox(IntPtr hWnd, string text, string caption,
	                                                 MessageBoxOptions options);

	[DllImport(USER32_DLL)]
	internal static extern uint SendInput(uint nInputs,
	                                      [MA(UT.LPArray), In] InputRecord[] pInputs,
	                                      int cbSize);

	[DllImport(USER32_DLL, CharSet = CharSet.Auto)]
	public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam,
	                                        [MA(UT.LPWStr)] string lParam);

	[DllImport(USER32_DLL, CharSet = CharSet.Auto)]
	public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam,
	                                        [MA(UT.LPWStr)] string lParam);

	[DllImport(USER32_DLL, CharSet = CharSet.Auto)]
	public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

	[DllImport(USER32_DLL, SetLastError = false)]
	public static extern IntPtr GetMessageExtraInfo();

	#region Key

	[DllImport(USER32_DLL)]
	public static extern short GetKeyState(VirtualKey k);

	[DllImport(USER32_DLL)]
	public static extern short GetAsyncKeyState(VirtualKey k);

	[DllImport(USER32_DLL)]
	[return: MA(UT.Bool)]
	public static extern bool GetKeyboardState([MA(UT.LPArray), In] byte[] r);

	[DllImport(USER32_DLL, SetLastError = false, ExactSpelling = true)]
	public static extern void keybd_event(byte bVk, byte bScan, KEYEVENTF dwFlags, IntPtr dwExtraInfo = default);

	[DllImport(USER32_DLL, SetLastError = true, ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool RegisterHotKey(IntPtr hWnd, int id, HotKeyModifiers fsModifiers, uint vk);

	[DllImport(USER32_DLL, SetLastError = true, ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

	#endregion

	[DllImport(USER32_DLL)]
	public static extern IntPtr GetFocus();

	[DllImport(USER32_DLL)]
	[return: MA(UT.Bool)]
	private static extern bool SetForegroundWindow(IntPtr hWnd);

	[DllImport(USER32_DLL, CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

	[DllImport(USER32_DLL, CharSet = CharSet.Auto)]
	private static extern int GetWindowTextLength(IntPtr hWnd);

	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	private class SearchData
	{
		public string Wndclass;
		public string Title;
		public IntPtr hWnd;
	}

	private delegate bool EnumWindowsProc(IntPtr hWnd, ref SearchData data);

	[DllImport(USER32_DLL)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, ref SearchData data);

	[DllImport(USER32_DLL, SetLastError = true, CharSet = CharSet.Auto)]
	public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

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

	[DllImport(USER32_DLL, SetLastError = true)]
	public static extern IntPtr GetClipboardData(uint uFormat);

	[DllImport(USER32_DLL, SetLastError = true)]
	public static extern IntPtr SetClipboardData(uint uFormat, void* hMem);

	[DllImport(USER32_DLL, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool OpenClipboard(IntPtr hWndNewOwner);

	[DllImport(USER32_DLL, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool CloseClipboard();

	[DllImport(USER32_DLL, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool EmptyClipboard();

	[DllImport(USER32_DLL, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool IsClipboardFormatAvailable(uint uFormat);

	[DllImport(USER32_DLL, SetLastError = true)]
	public static extern uint EnumClipboardFormats(uint uFormat);

	#endregion

	[DllImport(USER32_DLL, SetLastError = true, CharSet = CharSet.Auto)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GetMessage(out MSG lpMsg, [Optional] IntPtr hWnd, [Optional] uint wMsgFilterMin,
	                                     [Optional] uint wMsgFilterMax);

	[DllImport(USER32_DLL, SetLastError = true, CharSet = CharSet.Auto)]
	public static extern uint RegisterWindowMessage(string lpString);

	[DllImport(USER32_DLL)]
	public static extern IntPtr DispatchMessage(in MSG lpMsg);

	[DllImport(USER32_DLL, SetLastError = false, ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool TranslateMessage(in MSG lpMsg);

	[DllImport(USER32_DLL, SetLastError = false, ExactSpelling = true)]
	public static extern void PostQuitMessage([Optional] int nExitCode);

	[DllImport(USER32_DLL, SetLastError = true, CharSet = CharSet.Auto)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool PostThreadMessage(uint idThread, uint Msg, [Optional] IntPtr wParam,
	                                            [Optional] IntPtr lParam);

	[DllImport(USER32_DLL, SetLastError = true, ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool KillTimer([Optional] IntPtr hWnd, IntPtr uIDEvent);

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	public delegate void Timerproc(IntPtr hwnd, uint uMsg, IntPtr idEvent, uint dwTime);

	[DllImport(USER32_DLL, SetLastError = true, ExactSpelling = true)]
	public static extern IntPtr SetTimer([Optional] IntPtr hWnd, [Optional] IntPtr nIDEvent, [Optional] uint uElapse,
	                                     [Optional] Timerproc lpTimerFunc);

	public const int MF_BYCOMMAND = 0x00000000;

	[DllImport(USER32_DLL)]
	public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

	[DllImport(USER32_DLL)]
	public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
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
	CF_DIB = 8,
	CF_DIBV5=17,
	CF_BITMAP=2,

	FileName  = 0xC006,
	FileNameW = 0xC007
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