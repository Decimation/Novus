using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Novus.Win32.Structures;
using Novus.Win32.Wrappers;
// ReSharper disable IdentifierTypo

// ReSharper disable InconsistentNaming

// ReSharper disable UnusedMember.Global
//using Microsoft.Windows.Sdk;

namespace Novus.Win32
{
	/// <summary>
	/// Native interop; Win32 API
	/// </summary>
	public static unsafe class Native
	{
		/*
		 * CsWin32 and Microsoft.Windows.Sdk tanks VS performance; won't use it for now
		 */

		#region DLL

		public const string KERNEL32_DLL = "Kernel32.dll";

		public const string USER32_DLL = "User32.dll";

		public const string DBG_HELP_DLL = "DbgHelp.dll";

		public const string SHELL32_DLL = "Shell32.dll";

		#endregion

		public const string CMD_EXE = "cmd.exe";

		public const string EXPLORER_EXE = "explorer.exe";

		public const int INVALID = -1;

		[DllImport(SHELL32_DLL)]
		internal static extern int SHGetKnownFolderPath(
			[MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken,
			out IntPtr ppszPath);


		[DllImport(DBG_HELP_DLL)]
		private static extern ImageNtHeaders* ImageNtHeader(IntPtr hModule);

		[DllImport(KERNEL32_DLL)]
		internal static extern uint LocalSize(IntPtr p);

		[DllImport(KERNEL32_DLL)]
		internal static extern IntPtr OpenProcess(ProcessAccess dwDesiredAccess, bool bInheritHandle, int dwProcessId);

		[DllImport(KERNEL32_DLL, SetLastError = true, PreserveSig = true)]
		internal static extern bool CloseHandle(IntPtr obj);

		[DllImport(KERNEL32_DLL)]
		internal static extern bool ReadProcessMemory(IntPtr proc, IntPtr baseAddr, IntPtr buffer,
			int size, out int numBytesRead);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		internal static extern bool WriteProcessMemory(IntPtr proc, IntPtr baseAddr, IntPtr buffer,
			int size, out int numberBytesWritten);

		internal static IntPtr OpenProcess(Process proc) => OpenProcess(ProcessAccess.All, false, proc.Id);

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

		[DllImport(USER32_DLL, EntryPoint = "FindWindow", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern IntPtr FindWindowByCaption(IntPtr zeroOnly, string lpWindowName);

		[DllImport(KERNEL32_DLL, ExactSpelling = true)]
		public static extern IntPtr GetConsoleWindow();

		public static IntPtr GetWindowByCaption(string lpWindowName) => FindWindowByCaption(IntPtr.Zero, lpWindowName);

		/// <summary>
		/// Gets console application's window handle. <see cref="Process.MainWindowHandle"/> does not work in some cases.
		/// </summary>
		public static IntPtr GetConsoleWindowHandle() => GetWindowByCaption(Console.Title);


		public static ImageSectionInfo[] GetPESectionInfo(IntPtr hModule)
		{
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
	}

	[Flags]
	internal enum KnownFolderFlags : uint
	{
		SimpleIDList              = 0x00000100,
		NotParentRelative         = 0x00000200,
		DefaultPath               = 0x00000400,
		Init                      = 0x00000800,
		NoAlias                   = 0x00001000,
		DontUnexpand              = 0x00002000,
		DontVerify                = 0x00004000,
		Create                    = 0x00008000,
		NoAppcontainerRedirection = 0x00010000,
		AliasOnly                 = 0x80000000
	}

	[Flags]
	public enum ProcessAccess : uint
	{
		All                     = 0x1F0FFF,
		Terminate               = 0x000001,
		CreateThread            = 0x000002,
		VmOperation             = 0x00000008,
		VmRead                  = 0x000010,
		VmWrite                 = 0x000020,
		DupHandle               = 0x00000040,
		CreateProcess           = 0x000080,
		SetInformation          = 0x00000200,
		QueryInformation        = 0x000400,
		QueryLimitedInformation = 0x001000,
		Synchronize             = 0x00100000
	}

	public enum SubSystemType : ushort
	{
		IMAGE_SUBSYSTEM_UNKNOWN                 = 0,
		IMAGE_SUBSYSTEM_NATIVE                  = 1,
		IMAGE_SUBSYSTEM_WINDOWS_GUI             = 2,
		IMAGE_SUBSYSTEM_WINDOWS_CUI             = 3,
		IMAGE_SUBSYSTEM_POSIX_CUI               = 7,
		IMAGE_SUBSYSTEM_WINDOWS_CE_GUI          = 9,
		IMAGE_SUBSYSTEM_EFI_APPLICATION         = 10,
		IMAGE_SUBSYSTEM_EFI_BOOT_SERVICE_DRIVER = 11,
		IMAGE_SUBSYSTEM_EFI_RUNTIME_DRIVER      = 12,
		IMAGE_SUBSYSTEM_EFI_ROM                 = 13,
		IMAGE_SUBSYSTEM_XBOX                    = 14
	}

	public enum MagicType : ushort
	{
		IMAGE_NT_OPTIONAL_HDR32_MAGIC = 0x10b,
		IMAGE_NT_OPTIONAL_HDR64_MAGIC = 0x20b
	}

	public enum DllCharacteristics : ushort
	{
		RES_0                                          = 0x0001,
		RES_1                                          = 0x0002,
		RES_2                                          = 0x0004,
		RES_3                                          = 0x0008,
		IMAGE_DLL_CHARACTERISTICS_DYNAMIC_BASE         = 0x0040,
		IMAGE_DLL_CHARACTERISTICS_FORCE_INTEGRITY      = 0x0080,
		IMAGE_DLL_CHARACTERISTICS_NX_COMPAT            = 0x0100,
		IMAGE_DLLCHARACTERISTICS_NO_ISOLATION          = 0x0200,
		IMAGE_DLLCHARACTERISTICS_NO_SEH                = 0x0400,
		IMAGE_DLLCHARACTERISTICS_NO_BIND               = 0x0800,
		RES_4                                          = 0x1000,
		IMAGE_DLLCHARACTERISTICS_WDM_DRIVER            = 0x2000,
		IMAGE_DLLCHARACTERISTICS_TERMINAL_SERVER_AWARE = 0x8000
	}

	public enum MachineType : ushort
	{
		Native  = 0,
		I386    = 0x014c,
		Itanium = 0x0200,
		x64     = 0x8664
	}

	[Flags]
	public enum ImageSectionCharacteristics : uint
	{
		// Reserved 0x00000000
		// Reserved 0x00000001
		// Reserved 0x00000002
		// Reserved 0x00000004

		/// <summary>
		///     The section should not be padded to the next boundary. This flag is obsolete and is replaced by
		///     <see cref="ALIGN_1BYTES"/>
		/// </summary>
		TYPE_NO_PAD = 0x00000008,

		// Reserved 0x00000010

		/// <summary>
		///     The section contains executable code.
		/// </summary>
		CNT_CODE = 0x00000020,

		/// <summary>
		///     The section contains initialized data.
		/// </summary>
		CNT_INITIALIZED_DATA = 0x00000040,

		/// <summary>
		///     The section contains uninitialized data.
		/// </summary>
		CNT_UNINITIALIZED_DATA = 0x00000080,

		/// <summary>
		///     Reserved.
		/// </summary>
		LNK_OTHER = 0x00000100,

		/// <summary>
		///     The section contains comments or other information. This is valid only for object files.
		/// </summary>
		LNK_INFO = 0x00000200,

		// Reserved 0x00000400

		/// <summary>
		///     The section will not become part of the image. This is valid only for object files.
		/// </summary>
		LNK_REMOVE = 0x00000800,

		/// <summary>
		///     The section contains COMDAT data. This is valid only for object files.
		/// </summary>
		LNK_COMDAT = 0x00001000,

		// Reserved 0x00002000

		/// <summary>
		///     Reset speculative exceptions handling bits in the TLB entries for this section.
		/// </summary>
		NO_DEFER_SPEC_EXC = 0x00004000,

		/// <summary>
		///     The section contains data referenced through the global pointer.
		/// </summary>
		GPREL = 0x00008000,

		/// <summary>
		///     Reserved.
		/// </summary>
		MEM_PURGEABLE = 0x00020000,

		/// <summary>
		///     Reserved.
		/// </summary>
		MEM_LOCKED = 0x00040000,

		/// <summary>
		///     Reserved.
		/// </summary>
		MEM_PRELOAD = 0x00080000,

		/// <summary>
		///     Align data on a 1-byte boundary. This is valid only for object files.
		/// </summary>
		ALIGN_1BYTES = 0x00100000,

		/// <summary>
		///     Align data on a 2-byte boundary. This is valid only for object files.
		/// </summary>
		ALIGN_2BYTES = 0x00200000,

		/// <summary>
		///     Align data on a 4-byte boundary. This is valid only for object files.
		/// </summary>
		ALIGN_4BYTES = 0x00300000,

		/// <summary>
		///     Align data on a 8-byte boundary. This is valid only for object files.
		/// </summary>
		ALIGN_8BYTES = 0x00400000,

		/// <summary>
		///     Align data on a 16-byte boundary. This is valid only for object files.
		/// </summary>
		ALIGN_16BYTES = 0x00500000,

		/// <summary>
		///     Align data on a 32-byte boundary. This is valid only for object files.
		/// </summary>
		ALIGN_32BYTES = 0x00600000,

		/// <summary>
		///     Align data on a 64-byte boundary. This is valid only for object files.
		/// </summary>
		ALIGN_64BYTES = 0x00700000,

		/// <summary>
		///     Align data on a 128-byte boundary. This is valid only for object files.
		/// </summary>
		ALIGN_128BYTES = 0x00800000,

		/// <summary>
		///     Align data on a 256-byte boundary. This is valid only for object files.
		/// </summary>
		ALIGN_256BYTES = 0x00900000,

		/// <summary>
		///     Align data on a 512-byte boundary. This is valid only for object files.
		/// </summary>
		ALIGN_512BYTES = 0x00A00000,

		/// <summary>
		///     Align data on a 1024-byte boundary. This is valid only for object files.
		/// </summary>
		ALIGN_1024BYTES = 0x00B00000,

		/// <summary>
		///     Align data on a 2048-byte boundary. This is valid only for object files.
		/// </summary>
		ALIGN_2048BYTES = 0x00C00000,

		/// <summary>
		///     Align data on a 4096-byte boundary. This is valid only for object files.
		/// </summary>
		ALIGN_4096BYTES = 0x00D00000,

		/// <summary>
		///     Align data on a 8192-byte boundary. This is valid only for object files.
		/// </summary>
		ALIGN_8192BYTES = 0x00E00000,

		/// <summary>
		///     The section contains extended relocations.
		///     The count of relocations for the section exceeds the 16 bits that is reserved for it in the section header.
		///     If the <see cref="ImageSectionHeader.NumberOfRelocations" /> field in the section header is 0xffff,
		///     the actual relocation count is stored in the <see cref="ImageSectionHeader.VirtualAddress" /> field of the first
		///     relocation.
		///     It is an error if <see cref="LNK_NRELOC_OVFL" /> is set and there are fewer than 0xffff relocations in
		///     the section.
		/// </summary>
		LNK_NRELOC_OVFL = 0x01000000,

		/// <summary>
		///     The section can be discarded as needed.
		/// </summary>
		MEM_DISCARDABLE = 0x02000000,

		/// <summary>
		///     The section cannot be cached.
		/// </summary>
		MEM_NOT_CACHED = 0x04000000,

		/// <summary>
		///     The section cannot be paged.
		/// </summary>
		MEM_NOT_PAGED = 0x08000000,

		/// <summary>
		///     The section can be shared in memory.
		/// </summary>
		MEM_SHARED = 0x10000000,

		/// <summary>
		///     The section can be executed as code.
		/// </summary>
		MEM_EXECUTE = 0x20000000,

		/// <summary>
		///     The section can be read.
		/// </summary>
		MEM_READ = 0x40000000,

		/// <summary>
		///     The section can be written to.
		/// </summary>
		MEM_WRITE = 0x80000000

		// Reserved 0x00010000
	}
}