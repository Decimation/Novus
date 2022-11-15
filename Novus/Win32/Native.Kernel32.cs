
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Novus.Win32.Structures.Kernel32;
using Novus.Win32.Structures.Other;

// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

namespace Novus.Win32;
#pragma warning disable CA1401,CA2101
public static unsafe partial class Native
{
	// Thread proc, to be used with Create*Thread
	public delegate int ThreadProc(IntPtr param);

	[LibraryImport(KERNEL32_DLL)]
	public static partial uint GetCurrentThreadId();

	// Friendly version, marshals thread-proc as friendly delegate
	[LibraryImport(KERNEL32_DLL)]
	public static partial IntPtr CreateThread(
		IntPtr lpThreadAttributes,
		uint dwStackSize,
		ThreadProc lpStartAddress, // ThreadProc as friendly delegate
		IntPtr lpParameter,
		uint dwCreationFlags,
		out uint dwThreadId);

	// Marshal with ThreadProc's function pointer as a raw IntPtr.
	[LibraryImport(KERNEL32_DLL, EntryPoint = "CreateThread")]
	public static partial IntPtr CreateThreadRaw(
		IntPtr lpThreadAttributes,
		uint dwStackSize,
		IntPtr lpStartAddress, // ThreadProc as raw IntPtr
		IntPtr lpParameter,
		uint dwCreationFlags,
		out uint dwThreadId);

	// CreateRemoteThread, since ThreadProc is in remote process, we must use a raw function-pointer.
	[LibraryImport(KERNEL32_DLL)]
	public static partial IntPtr CreateRemoteThread(
		IntPtr hProcess,
		IntPtr lpThreadAttributes,
		uint dwStackSize,
		IntPtr lpStartAddress, // raw Pointer into remote process
		IntPtr lpParameter,
		uint dwCreationFlags,
		out uint lpThreadId
	);

	// uint output
	[LibraryImport(KERNEL32_DLL, SetLastError = true)]
	[return: MA(UT.Bool)]
	public static partial bool GetExitCodeThread(IntPtr hThread, out uint lpExitCode);

	#region Console

	[LibraryImport(KERNEL32_DLL, SetLastError = true)]
	public static partial IntPtr GetStdHandle(StandardHandle nStdHandle);

	[LibraryImport(KERNEL32_DLL)]
	[return: MA(UT.Bool)]
	public static partial bool GetConsoleMode(IntPtr hConsoleHandle, out ConsoleModes lpMode);

	[LibraryImport(KERNEL32_DLL)]
	[return: MA(UT.Bool)]
	public static partial bool SetConsoleMode(IntPtr hConsoleHandle, ConsoleModes dwMode);

	[LibraryImport(KERNEL32_DLL)]
	public static partial IntPtr GetConsoleWindow();

	[LibraryImport(KERNEL32_DLL, SetLastError = true)]
	[return: MA(UT.Bool)]
	public static partial bool SetConsoleOutputCP(uint wCodePageId);

	[LibraryImport(KERNEL32_DLL, SetLastError = true)]
	[return: MA(UT.Bool)]
	public static partial bool SetConsoleCP(uint wCodePageId);

	[LibraryImport(KERNEL32_DLL, SetLastError = true)]
	public static partial uint GetConsoleOutputCP();

	[LibraryImport(KERNEL32_DLL, SetLastError = true)]
	public static partial uint GetConsoleCP();

	[LibraryImport(KERNEL32_DLL, SetLastError = true)]
	[return: MA(UT.Bool)]
	public static partial bool GetConsoleScreenBufferInfo(IntPtr hConsoleOutput,
	                                                     ref ConsoleScreenBufferInfo lpConsoleScreenBufferInfo);

	[DllImport(KERNEL32_DLL, SetLastError = true)]
	public static extern bool WriteConsoleOutputCharacter(IntPtr hConsoleOutput, StringBuilder lpCharacter,
	                                                      uint nLength, Coord dwWriteCoord,
	                                                      out uint lpNumberOfCharsWritten);

#pragma warning disable SYSLIB1054
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

	[LibraryImport(KERNEL32_DLL, SetLastError = true)]
	public static partial IntPtr CreateToolhelp32Snapshot(SnapshotFlags dwFlags, uint th32ProcessID);

	#endregion

	#region File

	internal static uint GetFileSize(IntPtr hFile) => GetFileSize(hFile, IntPtr.Zero);

	[LibraryImport(KERNEL32_DLL, SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
	public static partial IntPtr CreateFile(string fileName, FileAccess fileAccess, FileShare fileShare,
	                                       IntPtr securityAttributes, FileMode creationDisposition,
	                                       FileAttributes flagsAndAttributes, IntPtr template);

	[LibraryImport(KERNEL32_DLL)]
	public static partial uint GetFileSize(IntPtr hFile, IntPtr lpFileSizeHigh);

	#endregion

	#region Memory

	[LibraryImport(KERNEL32_DLL)]
	[return: MA(UT.Bool)]
	public static partial bool ReadProcessMemory(IntPtr proc, IntPtr baseAddr, IntPtr buffer,
	                                            nint size, out int numBytesRead);

	[LibraryImport(KERNEL32_DLL)]
	[return: MA(UT.Bool)]
	public static partial bool ReadProcessMemory(IntPtr proc, IntPtr baseAddr, byte[] buffer,
	                                            nint size, out int numBytesRead);

	/*[DllImport(KERNEL32_DLL)]
	public static extern bool ReadProcessMemory(IntPtr proc, IntPtr baseAddr, byte[] buffer,
												nint size, out IntPtr numBytesRead);*/
	[LibraryImport(KERNEL32_DLL, EntryPoint = "RtlMoveMemory", SetLastError = false)]
	private static partial void MoveMemory(void* dst, void* src, int size);

	[LibraryImport(KERNEL32_DLL)]
	[return: MA(UT.Bool)]
	public static partial bool WriteProcessMemory(IntPtr proc, IntPtr baseAddr, IntPtr buffer,
	                                             int size, out int numberBytesWritten);

	[LibraryImport(KERNEL32_DLL, EntryPoint = "RtlMoveMemory", StringMarshalling = StringMarshalling.Utf16)]
	public static partial void CopyMemoryW(IntPtr pdst, string psrc, int cb);

	[LibraryImport(KERNEL32_DLL, EntryPoint = "RtlMoveMemory", StringMarshalling = StringMarshalling.Utf16)]
	public static partial void CopyMemoryW(IntPtr pdst,char[] psrc, int cb);

	[LibraryImport(KERNEL32_DLL, EntryPoint = "RtlMoveMemory")]
	public static partial void CopyMemory(IntPtr pdst, byte[] psrc, int cb);

	[LibraryImport(KERNEL32_DLL)]
	public static partial nint HeapSize(IntPtr p, uint f, IntPtr m);

	[LibraryImport(KERNEL32_DLL)]
	public static partial IntPtr GetProcessHeap();

	#endregion

	#region Library

	[LibraryImport(KERNEL32_DLL, StringMarshalling = StringMarshalling.Utf16)]
	public static partial IntPtr LoadLibrary(string dllToLoad);

	[LibraryImport(KERNEL32_DLL, StringMarshalling = StringMarshalling.Utf16)]
	public static partial IntPtr GetProcAddress(IntPtr hModule, string procedureName);

	[LibraryImport(KERNEL32_DLL)]
	[return: MA(UT.Bool)]
	public static partial bool FreeLibrary(IntPtr hModule);

	[DllImport(USER32_DLL, CharSet = CharSet.Auto, SetLastError = true)]
	public static extern int LoadString(IntPtr hInstance, uint uID, StringBuilder lpBuffer, int nBufferMax);

	[LibraryImport(KERNEL32_DLL, SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
	public static partial IntPtr LoadLibraryEx(string lpFileName, IntPtr hReservedNull, LoadLibraryFlags dwFlags);

	#endregion

	#region Handle

	[LibraryImport(KERNEL32_DLL, SetLastError = true)]
	public static partial IntPtr GetCurrentProcess();

	[LibraryImport(KERNEL32_DLL)]
	public static partial IntPtr OpenProcess(ProcessAccess dwDesiredAccess, [MA(UT.Bool)] bool bInheritHandle, int dwProcessId);

	[LibraryImport(KERNEL32_DLL, SetLastError = true)]
	[return: MarshalAs(UT.Bool)]
	public static partial bool CloseHandle(IntPtr obj);

	#endregion

	#region Virtual

	[LibraryImport(KERNEL32_DLL)]
	public static partial IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress,
	                                           uint dwSize, AllocationType flAllocationType,
	                                           MemoryProtection flProtect);

	[LibraryImport(KERNEL32_DLL)]
	[return: MA(UT.Bool)]
	public static partial bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress,
	                                        int dwSize, AllocationType dwFreeType);

	[LibraryImport(KERNEL32_DLL)]
	[return: MA(UT.Bool)]
	public static partial bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress,
	                                           uint dwSize, MemoryProtection flNewProtect,
	                                           out MemoryProtection lpflOldProtect);

	[LibraryImport(KERNEL32_DLL)]
	public static partial int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress,
	                                        ref MemoryBasicInformation lpBuffer, uint dwLength);

	[LibraryImport(KERNEL32_DLL)]
	public static partial int VirtualQuery(IntPtr lpAddress, ref MemoryBasicInformation lpBuffer, int dwLength);

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

	[DllImport(KERNEL32_DLL, SetLastError = true, CharSet = CharSet.Unicode)]
	public static extern int WideCharToMultiByte(int codePage, CharConversionFlags flags,
	                                             [MA(UT.LPWStr)] string wideStr, int chars,
	                                             [In, Out] byte[] pOutBytes, int bufferBytes, IntPtr defaultChar,
	                                             IntPtr pDefaultUsed);

	[LibraryImport(KERNEL32_DLL, SetLastError = true)]
	public static partial void GetSystemInfo(ref SystemInfo info);

	[LibraryImport(KERNEL32_DLL, SetLastError = true)]
	public static partial void GetNativeSystemInfo(ref SystemInfo info);

	[DllImport(KERNEL32_DLL, CharSet = CharSet.Auto, SetLastError = true)]
	internal static extern uint GetShortPathName([MA(UT.LPTStr)] string lpszLongPath,
	                                             [MA(UT.LPTStr)] StringBuilder lpszShortPath,
	                                             uint cchBuffer);

	[LibraryImport(KERNEL32_DLL, SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
	internal static partial uint GetShortPathName(string lpszLongPath, char[] lpszShortPath, int cchBuffer);

	[LibraryImport(KERNEL32_DLL, SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
	internal static partial uint GetShortPathName(string lpszLongPath, char* lpszShortPath, int cchBuffer);

	[DllImport(SHELL32_DLL)]
	internal static extern int SHGetKnownFolderPath([MA(UT.LPStruct)] Guid rfid, uint dwFlags,
	                                                IntPtr hToken, out IntPtr ppszPath);

	[LibraryImport(KERNEL32_DLL)]
	internal static partial uint LocalSize(IntPtr p);

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

	[LibraryImport(SHELL32_DLL, StringMarshalling = StringMarshalling.Utf16)]
	public static partial IntPtr ShellExecute(IntPtr hwnd, string lpOperation, string lpFile, string lpParameters,
	                                         string lpDirectory, int nShowCmd);

	[DllImport(SHELL32_DLL)]
	[return: MA(UT.Bool)]
	public static extern bool ShellExecuteEx(ref ShellExecuteInfo lpExecInfo);

	[LibraryImport(KERNEL32_DLL)]
	public static partial void GetCurrentThreadStackLimits(out IntPtr low, out IntPtr hi);

	[LibraryImport(KERNEL32_DLL)]
	internal static partial IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize,
	                                                 IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags,
	                                                 out IntPtr lpThreadId);

	public const uint INFINITE       = 0xFFFFFFFF;
	public const uint WAIT_ABANDONED = 0x00000080;
	public const uint WAIT_OBJECT_0  = 0x00000000;
	public const uint WAIT_TIMEOUT   = 0x00000102;

	[LibraryImport(KERNEL32_DLL, SetLastError = true)]
	public static partial uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

	[LibraryImport(KERNEL32_DLL)]
	public static partial IntPtr GlobalLock(IntPtr hMem);
	[LibraryImport(KERNEL32_DLL)]
	public static partial IntPtr GlobalUnlock(IntPtr hMem);
}