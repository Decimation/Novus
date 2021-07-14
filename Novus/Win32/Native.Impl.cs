using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Novus.Win32.Structures;
using Novus.Win32.Wrappers;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local

namespace Novus.Win32
{
	public static unsafe partial class Native
	{

		public static IntPtr GetStdOutputHandle() => Native.GetStdHandle(StandardHandle.STD_OUTPUT_HANDLE);

		public static IntPtr OpenProcess(Process proc) => OpenProcess(ProcessAccess.All, false, proc.Id);

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

			var mod = new ModuleEntry32 {dwSize = (uint) Marshal.SizeOf(typeof(ModuleEntry32))};

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
			else {
				// The function failed. Call GetLastError() for details.
				Coord invalid = default;
				return invalid;
			}
		}
	}
}