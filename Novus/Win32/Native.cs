using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Novus.Win32.Structures;
using Novus.Win32.Wrappers;
#pragma warning disable CA1401, CA2101

// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global
using MA = System.Runtime.InteropServices.MarshalAsAttribute;
using UT = System.Runtime.InteropServices.UnmanagedType;

// ReSharper disable InconsistentNaming
#pragma warning disable 649

// ReSharper disable UnusedMember.Local

namespace Novus.Win32
{
	/// <summary>
	///     Native interop; Win32 API
	/// </summary>
	public static unsafe partial class Native
	{
		/*
		 * CsWin32 and Microsoft.Windows.Sdk tanks VS performance; won't use it for now
		 */


		public const string CMD_EXE = "cmd.exe";

		public const string EXPLORER_EXE = "explorer.exe";

		public const int INVALID = -1;

		public const int ERROR_SUCCESS = 0;


		#region DLL

		/*
		 * https://github.com/dotnet/runtime/blob/main/src/libraries/Common/src/Interop/Windows/Interop.Libraries.cs
		 */

		public const string KERNEL32_DLL  = "Kernel32.dll";
		public const string USER32_DLL    = "User32.dll";
		public const string SHELL32_DLL   = "Shell32.dll";
		public const string DBGHELP_DLL   = "DbgHelp.dll";
		public const string URLMON_DLL    = "urlmon.dll";
		public const string GDI32_DLL     = "gdi32.dll";
		public const string NTDLL_DLL     = "ntdll.dll";
		public const string OLE32_DLL     = "ole32.dll";
		public const string WEBSOCKET_DLL = "websocket.dll";
		public const string WINHTTP_DLL   = "winhttp.dll";
		public const string UCRTBASE_DLL  = "ucrtbase.dll";

		#endregion

		#region CRT allocation

#if !NET6_0_OR_GREATER

		// TODO: Remove when .NET 6 releases
		[DllImport(UCRTBASE_DLL, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
		internal static extern void* calloc(nuint num, nuint size);

		[DllImport(UCRTBASE_DLL, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
		internal static extern void free(void* ptr);

		[DllImport(UCRTBASE_DLL, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
		internal static extern void* malloc(nuint size);

		[DllImport(UCRTBASE_DLL, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
		internal static extern void* realloc(void* ptr, nuint new_size);

		[DllImport(UCRTBASE_DLL, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
		internal static extern int _msize(void* ptr);
#endif

		#endregion

		#region Symbols

		[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
		internal static extern bool SymInitialize(IntPtr hProcess, IntPtr userSearchPath, bool fInvadeProcess);


		[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
		internal static extern bool SymCleanup(IntPtr hProcess);

		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		internal delegate bool EnumSymbolsCallback(IntPtr symInfo, uint symbolSize, IntPtr pUserContext);


		[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
		internal static extern bool SymEnumSymbols(IntPtr hProcess, ulong modBase, string mask,
		                                           EnumSymbolsCallback callback, IntPtr pUserContext);


		[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
		internal static extern SymbolOptions SymGetOptions();


		[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
		internal static extern bool SymGetSearchPath(IntPtr hProcess, sbyte* p, uint sz);


		[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
		internal static extern SymbolOptions SymSetOptions(SymbolOptions options);


		[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
		internal static extern bool SymFromName(IntPtr hProcess, string name, IntPtr pSymbol);


		[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
		internal static extern bool SymUnloadModule64(IntPtr hProc, ulong baseAddr);


		[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
		internal static extern ulong SymLoadModuleEx(IntPtr hProcess, IntPtr hFile, string imageName,
		                                             string moduleName, ulong baseOfDll, uint dllSize, IntPtr data,
		                                             uint flags);

		#endregion


		#region File

		internal static uint GetFileSize(IntPtr hFile) => GetFileSize(hFile, IntPtr.Zero);

		[DllImport(KERNEL32_DLL, SetLastError = true, CharSet = CharSet.Auto)]
		private static extern IntPtr CreateFile(string fileName, FileAccess fileAccess,
		                                        FileShare fileShare,
		                                        IntPtr securityAttributes,
		                                        FileMode creationDisposition,
		                                        FileAttributes flagsAndAttributes,
		                                        IntPtr template);

		[DllImport(KERNEL32_DLL)]
		private static extern uint GetFileSize(IntPtr hFile, IntPtr lpFileSizeHigh);

		#endregion

		#region Memory

		[DllImport(KERNEL32_DLL)]
		public static extern bool ReadProcessMemory(IntPtr proc, IntPtr baseAddr, IntPtr buffer,
		                                            int size, out int numBytesRead);


		[DllImport(KERNEL32_DLL)]
		public static extern bool ReadProcessMemory(IntPtr proc, IntPtr baseAddr, byte[] buffer,
		                                            int size, out int numBytesRead);

		[DllImport(KERNEL32_DLL)]
		public static extern bool ReadProcessMemory(IntPtr proc, IntPtr baseAddr, byte[] buffer,
		                                            IntPtr size, out IntPtr numBytesRead);

		[DllImport(KERNEL32_DLL)]
		public static extern bool WriteProcessMemory(IntPtr proc, IntPtr baseAddr, IntPtr buffer,
		                                             int size, out int numberBytesWritten);

		[DllImport(KERNEL32_DLL, ExactSpelling = true, EntryPoint = "RtlMoveMemory", CharSet = CharSet.Unicode)]
		public static extern void CopyMemoryW(IntPtr pdst, string psrc, int cb);

		[DllImport(KERNEL32_DLL, ExactSpelling = true, EntryPoint = "RtlMoveMemory", CharSet = CharSet.Unicode)]
		public static extern void CopyMemoryW(IntPtr pdst, char[] psrc, int cb);

		[DllImport(KERNEL32_DLL, ExactSpelling = true, EntryPoint = "RtlMoveMemory")]
		public static extern void CopyMemory(IntPtr pdst, byte[] psrc, int cb);

		#endregion

		#region Library

		[DllImport(KERNEL32_DLL)]
		public static extern IntPtr LoadLibrary(string dllToLoad);

		[DllImport(KERNEL32_DLL)]
		public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

		[DllImport(KERNEL32_DLL)]
		public static extern bool FreeLibrary(IntPtr hModule);

		#endregion

		#region Handle

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		public static extern IntPtr GetCurrentProcess();

		[DllImport(KERNEL32_DLL)]
		public static extern IntPtr OpenProcess(ProcessAccess dwDesiredAccess, bool bInheritHandle, int dwProcessId);

		[DllImport(KERNEL32_DLL, SetLastError = true, PreserveSig = true)]
		[return: MarshalAs(UT.Bool)]
		public static extern bool CloseHandle(IntPtr obj);

		[DllImport(USER32_DLL, SetLastError = true)]
		public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className,
		                                         string windowTitle);

		[DllImport(USER32_DLL, SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern IntPtr FindWindow(IntPtr zeroOnly, string lpWindowName);

		[DllImport(USER32_DLL, CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern IntPtr GetForegroundWindow();

		[DllImport(USER32_DLL, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

		#endregion


		#region Virtual

		[DllImport(KERNEL32_DLL)]
		public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress,
		                                           uint dwSize, AllocationType flAllocationType,
		                                           MemoryProtection flProtect);

		[DllImport(KERNEL32_DLL)]
		[return: MA(UT.Bool)]
		public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress,
		                                        int dwSize, AllocationType dwFreeType);

		[DllImport(KERNEL32_DLL)]
		public static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress,
		                                           uint dwSize, MemoryProtection flNewProtect,
		                                           out MemoryProtection lpflOldProtect);

		[DllImport(KERNEL32_DLL)]
		public static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress,
		                                        ref MemoryBasicInformation lpBuffer, uint dwLength);

		[DllImport(KERNEL32_DLL)]
		public static extern int VirtualQuery(IntPtr lpAddress, ref MemoryBasicInformation lpBuffer, int dwLength);

		#endregion

		#region Console

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		public static extern IntPtr GetStdHandle(StandardHandle nStdHandle);

		[DllImport(KERNEL32_DLL)]
		public static extern bool GetConsoleMode(IntPtr hConsoleHandle, out ConsoleModes lpMode);

		[DllImport(KERNEL32_DLL)]
		public static extern bool SetConsoleMode(IntPtr hConsoleHandle, ConsoleModes dwMode);

		[DllImport(KERNEL32_DLL, ExactSpelling = true)]
		public static extern IntPtr GetConsoleWindow();

		#region Code pages

		public const int CP_IBM437 = 437;

		/// <summary>The system default Windows ANSI code page.</summary>
		public const uint CP_ACP = 0;

		/// <summary>The current system Macintosh code page.</summary>
		public const uint CP_MACCP = 2;

		/// <summary>The current system OEM code page.</summary>
		public const uint CP_OEMCP = 1;

		/// <summary>Symbol code page (42).</summary>
		public const uint CP_SYMBOL = 42;

		/// <summary>The Windows ANSI code page for the current thread.</summary>
		public const uint CP_THREAD_ACP = 3;

		/// <summary>UTF-7. Use this value only when forced by a 7-bit transport mechanism. Use of UTF-8 is preferred.</summary>
		public const uint CP_UTF7 = 65000;

		/// <summary>UTF-8.</summary>
		public const uint CP_UTF8 = 65001;

		#endregion


		[DllImport(KERNEL32_DLL, SetLastError = true)]
		public static extern bool SetConsoleOutputCP(uint wCodePageId);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		public static extern bool SetConsoleCP(uint wCodePageId);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		public static extern uint GetConsoleOutputCP();

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		public static extern uint GetConsoleCP();


		[DllImport(KERNEL32_DLL, SetLastError = true)]
		public static extern bool GetConsoleScreenBufferInfo(IntPtr hConsoleOutput,
		                                                     ref ConsoleScreenBufferInfo lpConsoleScreenBufferInfo);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		public static extern bool WriteConsoleOutputCharacter(IntPtr hConsoleOutput, StringBuilder lpCharacter,
		                                                      uint nLength, Coord dwWriteCoord,
		                                                      out uint lpNumberOfCharsWritten);


		[DllImport(KERNEL32_DLL, SetLastError = true)]
		public static extern bool WriteConsoleOutput(
			IntPtr hConsoleOutput,
			CharInfo[] lpBuffer,
			Coord dwBufferSize,
			Coord dwBufferCoord,
			ref SmallRect lpWriteRegion
		);

		/* Writes character and color attribute data to a specified rectangular block of character cells in a console screen buffer.
		The data to be written is taken from a correspondingly sized rectangular block at a specified location in the source buffe */
		[DllImport(KERNEL32_DLL, CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool WriteConsoleOutput(
			IntPtr hConsoleOutput,
			/* This pointer is treated as the origin of a two-dimensional array of CHAR_INFO structures
			whose size is specified by the dwBufferSize parameter.*/
			[MarshalAs(UnmanagedType.LPArray), In] CharInfo[,] lpBuffer,
			Coord dwBufferSize,
			Coord dwBufferCoord,
			ref SmallRect lpWriteRegion);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		public static extern bool WriteConsole(
			IntPtr hConsoleOutput,
			string lpBuffer,
			uint nNumberOfCharsToWrite,
			out uint lpNumberOfCharsWritten,
			IntPtr lpReserved
		);

		[DllImport(KERNEL32_DLL, CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern bool GetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool bMaximumWindow,
		                                                  ref ConsoleFontInfo lpConsoleCurrentFont);

		[DllImport(KERNEL32_DLL, CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern bool SetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool bMaximumWindow,
		                                                  ref ConsoleFontInfo lpConsoleCurrentFont);

		[DllImport(KERNEL32_DLL, ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern int MultiByteToWideChar(int codePage, CharConversionFlags dwFlags, byte[] lpMultiByteStr,
		                                             int cchMultiByte,
		                                             [Out, MA(UT.LPWStr)] StringBuilder lpWideCharStr,
		                                             int cchWideChar);

		[DllImport(KERNEL32_DLL, SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode)]
		public static extern int WideCharToMultiByte(int codePage, CharConversionFlags flags,
		                                             [MA(UT.LPWStr)] string wideStr, int chars,
		                                             [In, Out] byte[] pOutBytes, int bufferBytes, IntPtr defaultChar,
		                                             IntPtr pDefaultUsed);

		#endregion

		#region Image

		[DllImport(DBGHELP_DLL)]
		private static extern ImageNtHeaders* ImageNtHeader(IntPtr hModule);

		#endregion


		#region Toolhelp

		[DllImport(KERNEL32_DLL)]
		internal static extern bool Module32First(IntPtr hSnapshot, ref ModuleEntry32 lpme);

		[DllImport(KERNEL32_DLL)]
		internal static extern bool Module32Next(IntPtr hSnapshot, ref ModuleEntry32 lpme);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		public static extern IntPtr CreateToolhelp32Snapshot(SnapshotFlags dwFlags, uint th32ProcessID);

		#endregion

		#region Input

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

		#endregion

		#region Other

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		public static extern void GetSystemInfo(ref SystemInfo info);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		public static extern void GetNativeSystemInfo(ref SystemInfo info);


		[DllImport(KERNEL32_DLL, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern uint GetShortPathName([MA(UT.LPTStr)] string lpszLongPath,
		                                             [MA(UT.LPTStr)] StringBuilder lpszShortPath,
		                                             uint cchBuffer);

		[DllImport(KERNEL32_DLL, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern uint GetShortPathName(string lpszLongPath, char[] lpszShortPath, int cchBuffer);

		[DllImport(KERNEL32_DLL, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern uint GetShortPathName(string lpszLongPath, char* lpszShortPath, int cchBuffer);

		[DllImport(SHELL32_DLL)]
		internal static extern int SHGetKnownFolderPath([MA(UT.LPStruct)] Guid rfid, uint dwFlags,
		                                                IntPtr hToken, out IntPtr ppszPath);

		[DllImport(KERNEL32_DLL)]
		internal static extern uint LocalSize(IntPtr p);

		[DllImport(URLMON_DLL, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false)]
		internal static extern int FindMimeFromData(IntPtr pBC,
		                                            [MA(UT.LPWStr)] string pwzUrl,
		                                            [MA(UT.LPArray, ArraySubType = UT.I1, SizeParamIndex = 3)]
		                                            byte[] pBuffer,
		                                            int cbSize,
		                                            [MA(UT.LPWStr)] string pwzMimeProposed,
		                                            int dwMimeFlags,
		                                            out IntPtr ppwzMimeOut,
		                                            int dwReserved);

		[DllImport(SHELL32_DLL, CharSet = CharSet.Auto)]
		public static extern bool ShellExecuteEx(ref ShellExecuteInfo lpExecInfo);

		[DllImport(KERNEL32_DLL)]
		public static extern void GetCurrentThreadStackLimits(out IntPtr low, out IntPtr hi);


		[DllImport(USER32_DLL, CharSet = CharSet.Auto)]
		private static extern int GetWindowTextLength(IntPtr hWnd);

		public static IntPtr SearchForWindow(string title)
		{
			SearchData sd = new() { Title = title };
			EnumWindows(EnumProc, ref sd);
			return sd.hWnd;
		}

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


		private static bool EnumProc(IntPtr hWnd, ref SearchData data)
		{
			// Check classname and title
			// This is different from FindWindow() in that the code below allows partial matches
			var sb = new StringBuilder(1024);
			var v  = GetWindowText(hWnd, sb, sb.Capacity);

			if (v != ERROR_SUCCESS) {
				return false;
			}

			if (sb.ToString().Contains(data.Title)) {

				data.hWnd = hWnd;
				return false; // Found the wnd, halt enumeration

			}

			return true;
		}

		#endregion

		[DllImport(USER32_DLL)]
		[return: MA(UT.Bool)]
		private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);


		public static void FlashWindow(IntPtr hWnd)
		{
			var fInfo = new FLASHWINFO
			{
				cbSize    = (uint) Marshal.SizeOf<FLASHWINFO>(),
				hwnd      = hWnd,
				dwFlags   = FlashWindowType.FLASHW_ALL,
				uCount    = 8,
				dwTimeout = 75,

			};


			FlashWindowEx(ref fInfo);
		}

		public static void FlashConsoleWindow() => FlashWindow(GetConsoleWindow());

		public static void BringConsoleToFront() => SetForegroundWindow(GetConsoleWindow());

		[DllImport(USER32_DLL)]
		public static extern MessageBoxResult MessageBox(IntPtr hWnd, string text, string caption,
		                                                 MessageBoxOptions options);
	}
}