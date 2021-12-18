using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Novus.OS.Win32.Structures;

namespace Novus.OS.Win32;

public static unsafe partial class Native
{

	#region Console

	[DllImport(KERNEL32_DLL, SetLastError = true)]
	public static extern IntPtr GetStdHandle(StandardHandle nStdHandle);

	[DllImport(KERNEL32_DLL)]
	public static extern bool GetConsoleMode(IntPtr hConsoleHandle, out ConsoleModes lpMode);

	[DllImport(KERNEL32_DLL)]
	public static extern bool SetConsoleMode(IntPtr hConsoleHandle, ConsoleModes dwMode);

	[DllImport(KERNEL32_DLL, ExactSpelling = true)]
	public static extern IntPtr GetConsoleWindow();


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
	public static extern bool WriteConsoleOutput(IntPtr hConsoleOutput, CharInfo[] lpBuffer, Coord dwBufferSize,
												 Coord dwBufferCoord, ref SmallRect lpWriteRegion
	);

	/* Writes character and color attribute data to a specified rectangular block of character cells in a console screen buffer.
	The data to be written is taken from a correspondingly sized rectangular block at a specified location in the source buffer */
	[DllImport(KERNEL32_DLL, CharSet = CharSet.Unicode, SetLastError = true)]
	internal static extern bool WriteConsoleOutput(IntPtr hConsoleOutput,
												   /* This pointer is treated as the origin of a two-dimensional array of CHAR_INFO structures
	                                               whose size is specified by the dwBufferSize parameter.*/
												   [MarshalAs(UnmanagedType.LPArray), In] CharInfo[,] lpBuffer,
												   Coord dwBufferSize,
												   Coord dwBufferCoord,
												   ref SmallRect lpWriteRegion);

	[DllImport(KERNEL32_DLL, SetLastError = true)]
	public static extern bool WriteConsole(IntPtr hConsoleOutput, string lpBuffer, uint nNumberOfCharsToWrite,
										   out uint lpNumberOfCharsWritten, IntPtr lpReserved);

	[DllImport(KERNEL32_DLL, CharSet = CharSet.Unicode, SetLastError = true)]
	public static extern bool GetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool bMaximumWindow,
													  ref ConsoleFontInfo lpConsoleCurrentFont);

	[DllImport(KERNEL32_DLL, CharSet = CharSet.Unicode, SetLastError = true)]
	public static extern bool SetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool bMaximumWindow,
													  ref ConsoleFontInfo lpConsoleCurrentFont);

	#endregion


	#region Toolhelp

	[DllImport(KERNEL32_DLL)]
	internal static extern bool Module32First(IntPtr hSnapshot, ref ModuleEntry32 lpme);

	[DllImport(KERNEL32_DLL)]
	internal static extern bool Module32Next(IntPtr hSnapshot, ref ModuleEntry32 lpme);

	[DllImport(KERNEL32_DLL, SetLastError = true)]
	public static extern IntPtr CreateToolhelp32Snapshot(SnapshotFlags dwFlags, uint th32ProcessID);

	#endregion


	#region File

	internal static uint GetFileSize(IntPtr hFile) => GetFileSize(hFile, IntPtr.Zero);

	[DllImport(KERNEL32_DLL, SetLastError = true, CharSet = CharSet.Auto)]
	private static extern IntPtr CreateFile(string fileName, FileAccess fileAccess, FileShare fileShare,
	                                        IntPtr securityAttributes, FileMode creationDisposition,
	                                        FileAttributes flagsAndAttributes, IntPtr template);

	[DllImport(KERNEL32_DLL)]
	private static extern uint GetFileSize(IntPtr hFile, IntPtr lpFileSizeHigh);

	#endregion





	#region Memory

	[DllImport(KERNEL32_DLL)]
	public static extern bool ReadProcessMemory(IntPtr proc, IntPtr baseAddr, IntPtr buffer,
												nint size, out int numBytesRead);


	[DllImport(KERNEL32_DLL)]
	public static extern bool ReadProcessMemory(IntPtr proc, IntPtr baseAddr, byte[] buffer,
												nint size, out int numBytesRead);

	/*[DllImport(KERNEL32_DLL)]
	public static extern bool ReadProcessMemory(IntPtr proc, IntPtr baseAddr, byte[] buffer,
	                                            nint size, out IntPtr numBytesRead);*/

	[DllImport(KERNEL32_DLL)]
	public static extern bool WriteProcessMemory(IntPtr proc, IntPtr baseAddr, IntPtr buffer,
												 int size, out int numberBytesWritten);

	[DllImport(KERNEL32_DLL, ExactSpelling = true, EntryPoint = "RtlMoveMemory", CharSet = CharSet.Unicode)]
	public static extern void CopyMemoryW(IntPtr pdst, string psrc, int cb);

	[DllImport(KERNEL32_DLL, ExactSpelling = true, EntryPoint = "RtlMoveMemory", CharSet = CharSet.Unicode)]
	public static extern void CopyMemoryW(IntPtr pdst, char[] psrc, int cb);

	[DllImport(KERNEL32_DLL, ExactSpelling = true, EntryPoint = "RtlMoveMemory")]
	public static extern void CopyMemory(IntPtr pdst, byte[] psrc, int cb);

	[DllImport(UCRTBASE_DLL, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern nuint _msize(void* ptr);

	[DllImport(UCRTBASE_DLL, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern nuint strlen(void* ptr);

	#endregion

	#region Library

	[DllImport(KERNEL32_DLL)]
	public static extern IntPtr LoadLibrary(string dllToLoad);

	[DllImport(KERNEL32_DLL)]
	public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

	[DllImport(KERNEL32_DLL)]
	public static extern bool FreeLibrary(IntPtr hModule);

	[DllImport(USER32_DLL, CharSet = CharSet.Auto, SetLastError = true)]
	public static extern int LoadString(IntPtr hInstance, uint uID, StringBuilder lpBuffer, int nBufferMax);

	[DllImport(KERNEL32_DLL, CharSet = CharSet.Auto, SetLastError = true)]
	public static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hReservedNull, LoadLibraryFlags dwFlags);

	#endregion

	#region Handle

	[DllImport(KERNEL32_DLL, SetLastError = true)]
	public static extern IntPtr GetCurrentProcess();

	[DllImport(KERNEL32_DLL)]
	public static extern IntPtr OpenProcess(ProcessAccess dwDesiredAccess, bool bInheritHandle, int dwProcessId);

	[DllImport(KERNEL32_DLL, SetLastError = true, PreserveSig = true)]
	[return: MarshalAs(UT.Bool)]
	public static extern bool CloseHandle(IntPtr obj);

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

	#region Code pages

	public enum CodePages
	{
		CP_IBM437 = 437,

		/// <summary>The system default Windows ANSI code page.</summary>
		CP_ACP = 0,

		/// <summary>The current system Macintosh code page.</summary>
		CP_MACCP = 2,

		/// <summary>The current system OEM code page.</summary>
		CP_OEMCP = 1,

		/// <summary>Symbol code page (42).</summary>
		CP_SYMBOL = 42,

		/// <summary>The Windows ANSI code page for the current thread.</summary>
		CP_THREAD_ACP = 3,

		/// <summary>UTF-7. Use this value only when forced by a 7-bit transport mechanism. Use of UTF-8 is preferred.</summary>
		CP_UTF7 = 65000,

		/// <summary>UTF-8.</summary>
		CP_UTF8 = 65001,
	}

	#endregion



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


	[DllImport(KERNEL32_DLL)]
	internal static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize,
													 IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags,
													 out IntPtr lpThreadId);
}