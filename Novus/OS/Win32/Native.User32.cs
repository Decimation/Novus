using System;
using System.Runtime.InteropServices;
using System.Text;
using Novus.OS.Win32.Structures;
// ReSharper disable UnusedMember.Local

namespace Novus.OS.Win32;

public static unsafe partial class Native
{
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
	                                      [MA(UT.LPArray), In] Input[] pInputs,
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
}