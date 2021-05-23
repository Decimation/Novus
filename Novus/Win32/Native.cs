using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using Novus.Memory;
using Novus.Win32.Structures;
using Novus.Win32.Wrappers;
using SimpleCore.Diagnostics;
using UnmanagedType = System.Runtime.InteropServices.UnmanagedType;

// ReSharper disable IdentifierTypo

// ReSharper disable InconsistentNaming

// ReSharper disable UnusedMember.Global
//using Microsoft.Windows.Sdk;

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
			return CreateFile(fileName, access, share, IntPtr.Zero, mode, attributes, IntPtr.Zero);
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
		public static extern bool CloseHandle(IntPtr obj);

		public static IntPtr OpenProcess(Process proc) => OpenProcess(ProcessAccess.All, false, proc.Id);

		[DllImport(USER32_DLL, EntryPoint = "FindWindow", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern IntPtr FindWindowByCaption(IntPtr zeroOnly, string lpWindowName);

		public static IntPtr GetWindowByCaption(string lpWindowName) => FindWindowByCaption(IntPtr.Zero, lpWindowName);

		#endregion


		#region Virtual

		[DllImport(KERNEL32_DLL)]
		internal static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress,
		                                             uint dwSize, AllocationType flAllocationType,
		                                             MemoryProtection flProtect);

		[DllImport(KERNEL32_DLL)]
		internal static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress,
		                                          int dwSize, AllocationType dwFreeType);

		[DllImport(KERNEL32_DLL)]
		internal static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress,
		                                             uint dwSize, MemoryProtection flNewProtect,
		                                             out MemoryProtection lpflOldProtect);

		[DllImport(KERNEL32_DLL)]
		internal static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress,
		                                          ref MemoryBasicInformation lpBuffer, uint dwLength);

		[DllImport(KERNEL32_DLL)]
		internal static extern int VirtualQuery(IntPtr lpAddress,
		                                        ref MemoryBasicInformation lpBuffer,
		                                        int dwLength);

		#endregion

		#region Console

		public const uint ENABLE_QUICK_EDIT = 0x0040;

		public const int STD_ERROR_HANDLE = -12;

		public const int STD_INPUT_HANDLE = -10;

		public const int STD_OUTPUT_HANDLE = -11;

		/// <param name="nStdHandle">
		///     <see cref="STD_INPUT_HANDLE" />,
		///     <see cref="STD_OUTPUT_HANDLE" />,
		///     <see cref="STD_ERROR_HANDLE" />
		/// </param>
		[DllImport(KERNEL32_DLL, SetLastError = true)]
		public static extern IntPtr GetStdHandle(int nStdHandle);

		[DllImport(KERNEL32_DLL)]
		public static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

		[DllImport(KERNEL32_DLL)]
		public static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

		[DllImport(KERNEL32_DLL, ExactSpelling = true)]
		public static extern IntPtr GetConsoleWindow();

		/// <summary>
		///     Gets console application's window handle. <see cref="Process.MainWindowHandle" /> does not work in some cases.
		/// </summary>
		public static IntPtr GetConsoleWindowHandle() => GetWindowByCaption(Console.Title);

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

		/*
		 * CsWin32 and Microsoft.Windows.Sdk tanks VS performance; won't use it for now
		 */

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		public static extern void GetSystemInfo(ref SystemInfo Info);

		[DllImport(SHELL32_DLL)]
		internal static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags,
		                                                IntPtr hToken, out IntPtr ppszPath);

		[DllImport(KERNEL32_DLL)]
		internal static extern uint LocalSize(IntPtr p);

		[DllImport(URLMON_DLL, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false)]
		internal static extern int FindMimeFromData(IntPtr pBC, [MarshalAs(UnmanagedType.LPWStr)] string pwzUrl,
		                                            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1,
		                                                       SizeParamIndex                      = 3)]
		                                            byte[] pBuffer,
		                                            int cbSize,
		                                            [MarshalAs(UnmanagedType.LPWStr)] string pwzMimeProposed,
		                                            int dwMimeFlags,
		                                            out IntPtr ppwzMimeOut,
		                                            int dwReserved);

		#region DLL

		public const string KERNEL32_DLL = "Kernel32.dll";

		public const string USER32_DLL = "User32.dll";


		public const string SHELL32_DLL = "Shell32.dll";

		private const string DBGHELP_DLL = "DbgHelp.dll";
		public const  string URLMON_DLL  = "urlmon.dll";

		#endregion
	}

	
}