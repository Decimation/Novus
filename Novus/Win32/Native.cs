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
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
//using Microsoft.Windows.Sdk;
using MA = System.Runtime.InteropServices.MarshalAsAttribute;
using UT = System.Runtime.InteropServices.UnmanagedType;

// ReSharper disable UnusedMember.Local

namespace Novus.Win32
{
	/// <summary>
	///     Native interop; Win32 API
	/// </summary>
	public static unsafe class Native
	{
		public const string CMD_EXE = "cmd.exe";

		public const string EXPLORER_EXE = "explorer.exe";

		public const int INVALID = -1;

		#region DLL

		public const string KERNEL32_DLL = "Kernel32.dll";

		public const string USER32_DLL = "User32.dll";

		public const string SHELL32_DLL = "Shell32.dll";

		public const string DBGHELP_DLL = "DbgHelp.dll";

		public const string URLMON_DLL = "urlmon.dll";

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

		internal static IntPtr CreateFile(string fileName, FileAccess access, FileShare share,
		                                  FileMode mode, FileAttributes attributes)
		{
			return CreateFile(fileName, access, share, IntPtr.Zero,
			                  mode, attributes, IntPtr.Zero);
		}

		#endregion

		#region Memory

		[DllImport(KERNEL32_DLL)]
		internal static extern bool ReadProcessMemory(IntPtr proc, IntPtr baseAddr, IntPtr buffer,
		                                              int size, out int numBytesRead);


		[DllImport(KERNEL32_DLL)]
		internal static extern bool ReadProcessMemory(IntPtr proc, IntPtr baseAddr, byte[] buffer,
		                                              int size, out int numBytesRead);

		[DllImport(KERNEL32_DLL)]
		internal static extern bool ReadProcessMemory(IntPtr proc, IntPtr baseAddr, byte[] buffer,
		                                              IntPtr size, out IntPtr numBytesRead);

		[DllImport(KERNEL32_DLL)]
		internal static extern bool WriteProcessMemory(IntPtr proc, IntPtr baseAddr, IntPtr buffer,
		                                               int size, out int numberBytesWritten);

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

		public static IntPtr OpenProcess(Process proc) => OpenProcess(ProcessAccess.All, false, proc.Id);

		[DllImport(USER32_DLL, SetLastError = true)]
		public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className,
		                                         string windowTitle);

		[DllImport(USER32_DLL, SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern IntPtr FindWindow(IntPtr zeroOnly, string lpWindowName);

		public static IntPtr FindWindow(string lpWindowName) => FindWindow(IntPtr.Zero, lpWindowName);

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


		public const int CP_WIN32_UNITED_STATES = 437;

		public const int CP_WIN32_UNICODE = 65001;

		public static readonly Encoding EncodingWin32Unicode = Encoding.GetEncoding(CP_WIN32_UNICODE);


		[DllImport(KERNEL32_DLL, SetLastError = true)]
		public static extern bool SetConsoleOutputCP(uint wCodePageID);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		public static extern bool SetConsoleCP(uint wCodePageID);


		[DllImport(KERNEL32_DLL, CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern bool GetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool bMaximumWindow,
		                                                  ref ConsoleFontInfo lpConsoleCurrentFont);

		[DllImport(KERNEL32_DLL, CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern bool SetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool bMaximumWindow,
		                                                  ref ConsoleFontInfo lpConsoleCurrentFont);

		public static void SetConsoleFont(string name, short y,
		                                  FontFamily ff = FontFamily.FF_DONTCARE,
		                                  FontWeight fw = FontWeight.FW_NORMAL)
		{
			ConsoleFontInfo ex = default;

			ex.FontFamily   = ff;
			ex.FontWeight   = fw;
			ex.FaceName     = name;
			ex.dwFontSize.X = 0;
			ex.dwFontSize.Y = y;

			SetConsoleFont(ex);
		}

		public static ConsoleFontInfo GetConsoleFont()
		{
			ConsoleFontInfo ex = default;
			ex.cbSize = (uint) Marshal.SizeOf<ConsoleFontInfo>();

			GetCurrentConsoleFontEx(GetStdHandle(StandardHandle.STD_OUTPUT_HANDLE), false, ref ex);
			return ex;
		}

		public static void SetConsoleFont(ConsoleFontInfo ex)
		{
			ex.cbSize = (uint) Marshal.SizeOf<ConsoleFontInfo>();

			SetCurrentConsoleFontEx(GetStdHandle(StandardHandle.STD_OUTPUT_HANDLE), false, ref ex);
		}

		#endregion

		#region Image

		[DllImport(DBGHELP_DLL)]
		private static extern ImageNtHeaders* ImageNtHeader(IntPtr hModule);

		public static ImageSectionInfo[] GetPESectionInfo(IntPtr hModule)
		{
			//todo

			// get the location of the module's IMAGE_NT_HEADERS structure
			var pNtHdr = ImageNtHeader(hModule);

			// section table immediately follows the IMAGE_NT_HEADERS
			var pSectionHdr = (IntPtr) (pNtHdr + 1);
			var arr         = new ImageSectionInfo[pNtHdr->FileHeader.NumberOfSections];

			int size = Marshal.SizeOf<ImageSectionHeader>();

			for (int scn = 0; scn < pNtHdr->FileHeader.NumberOfSections; ++scn) {
				var struc = Marshal.PtrToStructure<ImageSectionHeader>(pSectionHdr);
				arr[scn] = new ImageSectionInfo(struc, scn, hModule + (int) struc.VirtualAddress);

				pSectionHdr += size;
			}

			return arr;
		}

		#endregion


		#region Toolhelp

		[DllImport(KERNEL32_DLL)]
		internal static extern bool Module32First(IntPtr hSnapshot, ref ModuleEntry32 lpme);

		[DllImport(KERNEL32_DLL)]
		internal static extern bool Module32Next(IntPtr hSnapshot, ref ModuleEntry32 lpme);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		public static extern IntPtr CreateToolhelp32Snapshot(SnapshotFlags dwFlags, uint th32ProcessID);

		public static List<ModuleEntry32> EnumProcessModules(uint procId)
		{
			var snapshot = CreateToolhelp32Snapshot(SnapshotFlags.Module | SnapshotFlags.Module32, procId);

			var mod = new ModuleEntry32 {dwSize = (uint) Marshal.SizeOf(typeof(ModuleEntry32))};

			if (!Module32First(snapshot, ref mod))
				return null;

			List<ModuleEntry32> modules = new();

			do {
				modules.Add(mod);
			} while (Module32Next(snapshot, ref mod));

			return modules;
		}

		/*
		 * CsWin32 and Microsoft.Windows.Sdk tanks VS performance; won't use it for now
		 */

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		public static extern void GetSystemInfo(ref SystemInfo Info);

		#endregion

		#region Input

		[DllImport(USER32_DLL)]
		internal static extern uint SendInput(uint nInputs,
		                                      [MA(UT.LPArray), In] INPUT[] pInputs,
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


		public static string GetWindowText(IntPtr hWnd)
		{
			const int CAPACITY = 1024;

			var sb = new StringBuilder(CAPACITY);

			var sz = GetWindowText(hWnd, sb, CAPACITY);

			sb.Length = sz;

			return sb.ToString();
		}

		#endregion

		#region Other

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

		#endregion
	}
}