using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Kantan.Model;
using Novus.OS.Win32.Structures;
using Novus.OS.Win32.Structures.DbgHelp;
using Novus.OS.Win32.Structures.Kernel32;
using Novus.OS.Win32.Structures.Other;
using Novus.OS.Win32.Structures.User32;
using Novus.OS.Win32.Wrappers;

#pragma warning disable CA1401, CA2101

// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global


// ReSharper disable InconsistentNaming
#pragma warning disable 649

// ReSharper disable UnusedMember.Local

namespace Novus.OS.Win32;

/// <summary>
///     Native interop; Win32 API
/// </summary>
public static unsafe partial class Native
{
	/*
	 * CsWin32 and Microsoft.Windows.Sdk tanks VS performance; won't use it for now
	 */


	public const int INVALID = -1;

	public static readonly nuint INVALID2 = nuint.MaxValue;

	public const int ERROR_SUCCESS = 0;

	public const int SIZE_1 = 32 << 5;

	#region EXE

	public const string CMD_EXE = "cmd.exe";

	public const string EXPLORER_EXE = "explorer.exe";


	public const string PYTHON_EXE = "python";

	#endregion


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
	public const string UNAME_DLL     = "getuname.dll";

	#endregion

	public static IntPtr GetStdOutputHandle() => GetStdHandle(StandardHandle.STD_OUTPUT_HANDLE);

	public static IntPtr OpenProcess(Process proc) => OpenProcess(ProcessAccess.All, false, proc.Id);

	public static bool Inject(string dllPath, int pid)
	{
		/*
		 * Adapted from https://github.com/TimothyJClark/SharpInjector/blob/master/SharpInjector/Injector.cs
		 */

		//todo: WIP

		IntPtr processHandle = OpenProcess(ProcessAccess.CreateThread |
		                                   ProcessAccess.VmOperation | ProcessAccess.VmWrite,
		                                   false, pid);

		if (processHandle == IntPtr.Zero) {
			return false;
		}


		IntPtr kernel32Base = LoadLibrary(KERNEL32_DLL);

		if (kernel32Base == IntPtr.Zero) {
			return false;
		}

		IntPtr loadLibraryAddr = GetProcAddress(kernel32Base, "LoadLibraryA");

		if (loadLibraryAddr == IntPtr.Zero) {
			return false;
		}

		IntPtr remoteAddress = VirtualAllocEx(processHandle, IntPtr.Zero, (uint) dllPath.Length,
		                                      AllocationType.Commit | AllocationType.Reserve,
		                                      MemoryProtection.ExecuteReadWrite);

		if (remoteAddress == IntPtr.Zero) {
			return false;
		}

		var b = WriteProcessMemory(processHandle, remoteAddress,
		                           Marshal.StringToHGlobalAnsi(dllPath), dllPath.Length, out int _);

		if (!b) {
			return false;
		}

		IntPtr remoteThread = CreateRemoteThread(processHandle, IntPtr.Zero, 0,
		                                         loadLibraryAddr, remoteAddress,
		                                         0, out var rId);

		if (remoteThread == IntPtr.Zero) {
			return false;
		}

		CloseHandle(remoteThread);
		VirtualFreeEx(processHandle, remoteAddress, dllPath.Length, AllocationType.Release);
		CloseHandle(processHandle);

		return true;
	}

	public static int SendInput(InputRecord[] inputs)
	{
		return (int) SendInput((uint) inputs.Length, inputs, Marshal.SizeOf<InputRecord>() * inputs.Length);
	}

	public static string GetWindowText(IntPtr hWnd)
	{

		var sb = new StringBuilder(SIZE_1);

		var sz = GetWindowText(hWnd, sb, SIZE_1);

		sb.Length = sz;

		return sb.ToString();
	}

	public static IntPtr SearchForWindow(string title)
	{
		SearchData sd = new() { Title = title };
		EnumWindows(EnumProc, ref sd);
		return sd.hWnd;
	}

	internal static IntPtr CreateFile(string fileName, FileAccess access, FileShare share,
	                                  FileMode mode, FileAttributes attributes)
	{
		return CreateFile(fileName, access, share, IntPtr.Zero,
		                  mode, attributes, IntPtr.Zero);
	}

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

	private static bool EnumProc(IntPtr hWnd, ref SearchData data)
	{
		// Check classname and title
		// This is different from FindWindow() in that the code below allows partial matches
		var sb = new StringBuilder(SIZE_1);
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

	public static List<ModuleEntry32> EnumProcessModules(uint procId)
	{
		var snapshot = CreateToolhelp32Snapshot(SnapshotFlags.Module | SnapshotFlags.Module32, procId);

		var mod = new ModuleEntry32 { dwSize = (uint) Marshal.SizeOf(typeof(ModuleEntry32)) };

		if (!Module32First(snapshot, ref mod))
			return null;

		List<ModuleEntry32> modules = new();

		do {
			modules.Add(mod);
		} while (Module32Next(snapshot, ref mod));

		return modules;
	}

	public static IntPtr FindWindow(string lpWindowName) => FindWindow(IntPtr.Zero, lpWindowName);

	public static Coord GetConsoleCursorPosition(IntPtr hConsoleOutput)
	{
		ConsoleScreenBufferInfo cbsi = default;

		if (GetConsoleScreenBufferInfo(hConsoleOutput, ref cbsi)) {
			return cbsi.dwCursorPosition;
		}

		// The function failed. Call GetLastError() for details.
		Coord invalid = default;
		return invalid;
	}

	public static void DumpSections(IntPtr hModule)
	{
		var s = GetPESectionInfo(hModule);

		var table = new ConsoleTable("Number", "Name", "Address", "Size", "Characteristics");

		foreach (var info in s) {
			table.AddRow(info.Number, info.Name, $"{info.Address.ToInt64():X}", info.Size, info.Characteristics);
		}

		table.Write();
	}

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

	public static string GetUnicodeName(ushort id)
	{
		/*using var reader = new Win32ResourceReader(UNAME_DLL);
		string    name   = reader.GetString(id);
		return name;*/

		//http://www.pinvoke.net/default.aspx/getuname/GetUName.html
		//https://stackoverflow.com/questions/2087682/finding-out-unicode-character-name-in-net
		var buf = new StringBuilder(SIZE_1);
		_ = GetUName(id, buf);

		var name = buf.ToString();
		return name;
	}

	public static StringBuilder LoadString(IntPtr hInstance, uint id, int buf = SIZE_1)
	{
		var buffer = new StringBuilder(buf);

		_ = LoadString(hInstance, id, buffer, buffer.Capacity);

		if (Marshal.GetLastWin32Error() != 0) {
			FailWin32Error();
		}

		return buffer;
	}

	public static IntPtr LoadLibraryEx(string lpFileName, LoadLibraryFlags dwFlags)
		=> LoadLibraryEx(lpFileName, IntPtr.Zero, dwFlags);

	public static void FailWin32Error()
	{
		var hr = Marshal.GetHRForLastWin32Error();

		var exception = Marshal.GetExceptionForHR(hr);

		// ReSharper disable once PossibleNullReferenceException
		throw exception;
	}
}