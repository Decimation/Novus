using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Kantan.Model;
using Novus.Win32.Structures;
using Novus.Win32.Wrappers;

// ReSharper disable StringLiteralTypo

// ReSharper disable IdentifierTypo

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local

namespace Novus.Win32;

public static unsafe partial class Native
{
	public static IntPtr GetStdOutputHandle() => GetStdHandle(StandardHandle.STD_OUTPUT_HANDLE);

	public static IntPtr OpenProcess(Process proc) => OpenProcess(ProcessAccess.All, false, proc.Id);

	public static bool Inject(string dllPath, int pid)
	{
		/*
		 * Adapted from https://github.com/TimothyJClark/SharpInjector/blob/master/SharpInjector/Injector.cs
		 */


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
		                                         0, out _);

		if (remoteThread == IntPtr.Zero) {
			return false;
		}

		CloseHandle(remoteThread);
		VirtualFreeEx(processHandle, remoteAddress, dllPath.Length, AllocationType.Release);
		CloseHandle(processHandle);

		return true;
	}

	public static int SendInput(Input[] inputs)
	{
		return (int) SendInput((uint) inputs.Length, inputs, Marshal.SizeOf<Input>() * inputs.Length);
	}

	public static string GetWindowText(IntPtr hWnd)
	{
		const int CAPACITY = 1024;

		var sb = new StringBuilder(CAPACITY);

		var sz = GetWindowText(hWnd, sb, CAPACITY);

		sb.Length = sz;

		return sb.ToString();
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

	public static string GetUnicodeName(uint id)
	{
		using var reader = new Win32ResourceReader(UNAME_DLL);

		string name = reader.GetString(id);

		return name;
	}

	public static StringBuilder LoadString(IntPtr hInstance, uint id, int buf = 1024)
	{
		var buffer = new StringBuilder(buf);

		LoadString(hInstance, id, buffer, buffer.Capacity);

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

		throw exception!;
	}
}