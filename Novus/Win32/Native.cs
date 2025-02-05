// global using LI = System.Runtime.InteropServices.LibraryImportAttribute;

using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Novus.OS;
using Novus.Win32.Structures.DbgHelp;
using Novus.Win32.Structures.Kernel32;
using Novus.Win32.Structures.Ntdll;
using Novus.Win32.Structures.Other;
using Novus.Win32.Structures.User32;
using Novus.Win32.Wrappers;

// ReSharper disable UnusedVariable

// #pragma warning disable CA1401, CA2101

// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
#pragma warning disable 649

// ReSharper disable UnusedMember.Local

namespace Novus.Win32;

/// <summary>
///     Native interop; Win32 API
/// </summary>
[SupportedOSPlatform(FileSystem.OS_WIN)]
public static unsafe partial class Native
{

	#region

	/// <summary>
	/// Error sentinel value
	/// </summary>
	public const int ERROR_SV = -1;

	public const int ERROR_SUCCESS = 0;

	public const nuint NUINT_MAXVALUE = UInt32.MaxValue;

	public const int SIZE_1024 = 1 << 10;

	public const uint ZERO_U = 0u;

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
	public const string ADVAPI32_DLL  = "advapi32.dll";

	#endregion

	#region HRESULT

	public const uint E_ABORT        = 0x80004004;
	public const uint E_ACCESSDENIED = 0x80070005;
	public const uint E_FAIL         = 0x80004005;
	public const uint E_HANDLE       = 0x80070006;
	public const uint E_INVALIDARG   = 0x80070057;
	public const uint E_NOINTERFACE  = 0x80004002;
	public const uint E_NOTIMPL      = 0x80004001;
	public const uint E_OUTOFMEMORY  = 0x8007000E;
	public const uint E_POINTER      = 0x80004003;
	public const uint E_UNEXPECTED   = 0x8000FFFF;

	#endregion

	public static nint GetStdOutputHandle()
		=> GetStdHandle(StandardHandle.STD_OUTPUT_HANDLE);

	public static nint OpenProcess(Process proc)
		=> OpenProcess(ProcessAccess.All, false, proc.Id);

	public static bool Inject(string dllPath, int pid)
	{
		/*
		 * Adapted from https://github.com/TimothyJClark/SharpInjector/blob/master/SharpInjector/Injector.cs
		 */

		//todo: WIP

		nint processHandle = OpenProcess(ProcessAccess.CreateThread |
		                                 ProcessAccess.VmOperation  | ProcessAccess.VmWrite,
		                                 false, pid);

		if (processHandle == IntPtr.Zero) {
			return false;
		}

		nint kernel32Base = LoadLibrary(KERNEL32_DLL);

		if (kernel32Base == IntPtr.Zero) {
			return false;
		}

		nint loadLibraryAddr = GetProcAddress(kernel32Base, "LoadLibraryA");

		if (loadLibraryAddr == IntPtr.Zero) {
			return false;
		}

		nint remoteAddress = VirtualAllocEx(processHandle, IntPtr.Zero, (uint) dllPath.Length,
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

		nint remoteThread = CreateRemoteThread(processHandle, IntPtr.Zero, 0,
		                                       loadLibraryAddr, remoteAddress,
		                                       0, out nint rId);

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

	public static string GetWindowText(nint hWnd)
	{
		var sb = new StringBuilder(SIZE_1024);

		var sz = GetWindowText(hWnd, sb, SIZE_1024);

		sb.Length = sz;

		return sb.ToString();
	}

	public static nint SearchForWindow(string title)
	{
		SearchData sd = new() { Title = title };
		EnumWindows(EnumProc, ref sd);
		return sd.hWnd;
	}

	public static nint CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode,
	                              FileAttributes attributes)
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

	private static bool EnumProc(nint hWnd, ref SearchData data)
	{
		// Check classname and title
		// This is different from FindWindow() in that the code below allows partial matches
		var sb = new StringBuilder(SIZE_1024);
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

	public static ImageSectionInfo[] GetPESectionInfo(nint hModule)
	{
		//todo

		// get the location of the module's IMAGE_NT_HEADERS structure
		var pNtHdr = ImageNtHeader(hModule);

		// section table immediately follows the IMAGE_NT_HEADERS
		var pSectionHdr = (nint) (pNtHdr + 1);
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

		var mod = new ModuleEntry32
		{
			dwSize = (uint) Marshal.SizeOf(typeof(ModuleEntry32))
		};

		if (!Module32First(snapshot, ref mod))
			return null;

		List<ModuleEntry32> modules = new();

		do {
			modules.Add(mod);
		} while (Module32Next(snapshot, ref mod));

		return modules;
	}

	public static void RemoveWindowOnTop(nint p)
	{
		SetWindowPos(p, new((int) HandleWindowPosition.HWND_NOTOPMOST),
		             0, 0, 0, 0, WindowFlags.TOPMOST_FLAGS);
	}

	public static void KeepWindowOnTop(nint p)
	{
		SetWindowPos(p, new((int) HandleWindowPosition.HWND_TOPMOST),
		             0, 0, 0, 0, WindowFlags.TOPMOST_FLAGS);
	}

	public static nint FindWindow(string lpWindowName)
		=> FindWindow(IntPtr.Zero, lpWindowName);

	public static Coord GetConsoleCursorPosition(nint hConsoleOutput)
	{
		ConsoleScreenBufferInfo cbsi = default;

		if (GetConsoleScreenBufferInfo(hConsoleOutput, ref cbsi)) {
			return cbsi.dwCursorPosition;
		}

		// The function failed. Call GetLastError() for details.
		Coord invalid = default;
		return invalid;
	}

	/*public static void DumpSections(IntPtr hModule)
	{
		var s = GetPESectionInfo(hModule);

		var table = new ConsoleTable("Number", "Name", "Address", "Size", "Characteristics");

		foreach (var info in s) {
			table.AddRow(info.Number, info.Name, $"{info.Address.ToInt64():X}", info.Size, info.Characteristics);
		}

		table.Write();

	}*/

	public static void FlashWindow(nint hWnd)
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

	public static void BringConsoleToFront()
		=> SetForegroundWindow(GetConsoleWindow());

	public static string GetUnicodeName(ushort id)
	{
		/*using var reader = new Win32ResourceReader(UNAME_DLL);
		string    name   = reader.GetString(id);
		return name;*/

		//http://www.pinvoke.net/default.aspx/getuname/GetUName.html
		//https://stackoverflow.com/questions/2087682/finding-out-unicode-character-name-in-net
		var buf = new StringBuilder(SIZE_1024);
		_ = GetUName(id, buf);

		var name = buf.ToString();
		return name;
	}

	public static StringBuilder LoadString(nint hInstance, uint id, int buf = SIZE_1024)
	{
		var buffer = new StringBuilder(buf);

		_ = LoadString(hInstance, id, buffer, buffer.Capacity);

		if (Marshal.GetLastWin32Error() != 0) {
			FailWin32Error();
		}

		return buffer;
	}

	public static nint LoadLibraryEx(string lpFileName, LoadLibraryFlags dwFlags)
		=> LoadLibraryEx(lpFileName, IntPtr.Zero, dwFlags);

	public static nint HRFromWin32(nint x)
	{
		const int FACILITY_WIN32 = 7;

		return (nint) (x <= 0 ? x : (x & 0x0000FFFF) | (FACILITY_WIN32 << 16) | 0x80000000);
	}

	[DoesNotReturn]
	public static void FailWin32Error()
	{
		var hr = Marshal.GetHRForLastWin32Error();

		var exception = Marshal.GetExceptionForHR(hr);

		// ReSharper disable once PossibleNullReferenceException
		throw exception;
	}

	public static bool VirtualFree(Process hProcess, Pointer lpAddress, int dwSize, AllocationType dwFreeType)
	{
		bool p = VirtualFreeEx(hProcess.Handle, lpAddress.Address, dwSize, dwFreeType);

		return p;
	}

	public static MemoryBasicInformation VirtualQuery(Process proc, Pointer lpAddr)
	{
		var mbi = new MemoryBasicInformation();

		int v = VirtualQueryEx(proc.Handle, lpAddr.Address, ref mbi,
		                       (uint) Marshal.SizeOf<MemoryBasicInformation>());

		return mbi;
	}

	public static Pointer VirtualAlloc(Process proc, Pointer lpAddr, int dwSize,
	                                   AllocationType type, MemoryProtection mp)
	{
		nint ptr = VirtualAllocEx(proc.Handle, lpAddr.Address, (uint) dwSize, type, mp);

		return ptr;
	}

	public static bool VirtualProtect(Process hProcess, Pointer lpAddress, int dwSize,
	                                  MemoryProtection flNewProtect, out MemoryProtection lpflOldProtect)
	{
		bool p = VirtualProtectEx(hProcess.Handle, lpAddress.Address, (uint) dwSize, flNewProtect,
		                          out lpflOldProtect);

		return p;
	}

	public static LinkedList<MemoryBasicInformation> EnumeratePages(nint handle)
	{
		SystemInfo sysInfo = default;

		GetSystemInfo(ref sysInfo);

		MemoryBasicInformation mbi = default;

		long lpMem = 0L;

		uint sizeOf = (uint) Marshal.SizeOf(typeof(MemoryBasicInformation));

		long maxAddr = sysInfo.MaximumApplicationAddress.ToInt64();

		var ll = new LinkedList<MemoryBasicInformation>();

		while (lpMem < maxAddr) {
			int result = VirtualQueryEx(handle, (nint) lpMem, ref mbi, sizeOf);

			if (mbi.AllocationBase != IntPtr.Zero && mbi.AllocationBase == mbi.BaseAddress) {
				ll.AddLast(mbi);
			}

			lpMem += mbi.RegionSize;

		}

		return ll;
	}

	public static MemoryBasicInformation QueryMemoryPage(Pointer p)
	{

		/*
		 * https://stackoverflow.com/questions/496034/most-efficient-replacement-for-isbadreadptr
		 * https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-isbadreadptr
		 */

		MemoryBasicInformation mbi = default;

		return VirtualQuery(p.Address, ref mbi, Marshal.SizeOf<MemoryBasicInformation>()) != 0
			       ? mbi
			       : throw new Win32Exception();

	}

	//helper method with "dynamic" buffer allocation
	public static nint NtQueryObject(nint handle, ObjectInformationClass infoClass, uint infoLength = 0)
	{
		if (infoLength == 0)
			infoLength = (uint) Marshal.SizeOf(typeof(uint));

		nint infoPtr = Marshal.AllocHGlobal((int) infoLength);

		int tries = 0;

		NtStatus result;

		while (true) {
			result = NtQueryObject(handle, infoClass, infoPtr, infoLength, out infoLength);

			if (result == NtStatus.INFO_LENGTH_MISMATCH || result == NtStatus.BUFFER_OVERFLOW ||
			    result == NtStatus.BUFFER_TOO_SMALL) {
				Marshal.FreeHGlobal(infoPtr);
				infoPtr = Marshal.AllocHGlobal((int) infoLength);
				tries++;
				continue;
			}

			if (result == NtStatus.SUCCESS || tries > 5)
				break;

			//throw new Exception("Unhandled NtStatus " + result);
			break;
		}

		if (result == NtStatus.SUCCESS)
			return infoPtr; //don't forget to free the pointer with Marshal.FreeHGlobal after you're done with it

		Marshal.FreeHGlobal(infoPtr); //free pointer when not Successful

		return IntPtr.Zero;
	}

	/*public static string GetClipboardFormatName(uint u)
	{
		var c = new StringBuilder(SIZE_1024);

		unsafe {
			var l = GetClipboardFormatName(u, c, c.Capacity);
			return c.ToString();
		}
	}*/

	public static byte[] CopyGlobalObject(nint u)
	{
		var size = Native.GlobalSize(u);
		var rg   = new byte[size];
		Marshal.Copy(u, rg, 0, (int) size);
		return rg;
	}

}