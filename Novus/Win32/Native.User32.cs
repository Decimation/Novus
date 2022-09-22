using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;
using Kantan.Cli;
using Novus.Win32.Structures.User32;
using InputRecord = Novus.Win32.Structures.User32.InputRecord;

// ReSharper disable IdentifierTypo

// ReSharper disable UnusedMember.Global

// ReSharper disable UnusedMember.Local

namespace Novus.Win32;
#pragma warning disable CA1401,CA2101
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

	[DllImport(USER32_DLL)]
	public static extern short GetKeyState(VirtualKey k);

	[DllImport(USER32_DLL)]
	public static extern short GetAsyncKeyState(VirtualKey k);

	[DllImport(USER32_DLL)]
	[return: MA(UT.Bool)]
	public static extern bool GetKeyboardState([MA(UT.LPArray), In] byte[] r);

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
	public static extern int GetClipboardFormatName(uint format, char* lpszFormatName, int cchMaxCount);

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
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern uint EnumClipboardFormats(uint uFormat);

	#endregion
}

//todo
public enum ClipboardFormat : uint
{
	CF_TEXT        = 1,
	CF_UNICODETEXT = 13,
	CF_OEMTEXT     = 7
}

public enum HandleWindowPosition
{
	HWND_BOTTOM    = 1,
	HWND_NOTOPMOST = -2,
	HWND_TOP       = 0,
	HWND_TOPMOST   = -1,
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