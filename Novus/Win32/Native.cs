using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

		//todo


		#region Sym

		[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
		internal static extern bool SymInitialize(IntPtr hProcess, IntPtr userSearchPath, bool fInvadeProcess);

		[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
		internal static extern bool SymCleanup(IntPtr hProcess);


		[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
		internal static extern bool SymEnumSymbols(IntPtr hProcess, ulong modBase,
			string mask, EnumSymbolsCallback callback,
			IntPtr pUserContext);


		[DllImport(DBGHELP_DLL)]
		internal static extern SymbolOptions SymGetOptions();

		[DllImport(DBGHELP_DLL)]
		internal static extern SymbolOptions SymSetOptions(SymbolOptions options);


		[DllImport(DBGHELP_DLL)]
		private static extern bool SymFromName(IntPtr hProcess, string name, IntPtr pSymbol);


		[DllImport(DBGHELP_DLL)]
		internal static extern bool SymUnloadModule64(IntPtr hProc, ulong baseAddr);


		[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
		internal static extern ulong SymLoadModuleEx(IntPtr hProcess, IntPtr hFile, string imageName,
			string moduleName, ulong baseOfDll, uint dllSize,
			IntPtr data, uint flags);

		internal delegate bool EnumSymbolsCallback(IntPtr symInfo, uint symbolSize, IntPtr pUserContext);


		internal static void SymInitialize(IntPtr hProcess) =>
			SymInitialize(hProcess, IntPtr.Zero, false);


		internal static bool SymEnumSymbols(IntPtr hProcess, ulong modBase, EnumSymbolsCallback callback) =>
			SymEnumSymbols(hProcess, modBase, null, callback, IntPtr.Zero);

		internal static ulong SymLoadModuleEx(IntPtr hProc, string img, ulong dllBase, uint fileSize)
		{
			return SymLoadModuleEx(hProc, IntPtr.Zero, img, null, dllBase,
				fileSize, IntPtr.Zero, default);
		}

		private static List<Symbol> m_rgList = new List<Symbol>();

		internal static bool AddSymCallback(IntPtr sym, uint symSize, IntPtr userCtx)
		{
			var symName = (((DebugSymbol*) sym));

			//size_t maxcmplen = strlen((PCHAR)UserContext);
			//if (maxcmplen == pSymInfo->NameLen)
			//{
			//	if ((strncmp(pSymInfo->Name, (PCHAR)UserContext, pSymInfo->NameLen)) == 0)
			//	{
			//		TI_FINDCHILDREN_PARAMS childs = { 0 };
			//		SymGetTypeInfo(hProcess, pSymInfo->ModBase, pSymInfo->TypeIndex,
			//			TI_GET_CHILDRENCOUNT, &childs.Count);
			//		printf("%8s%10s%10s%16s %s", "Size", "TypeIndex", "Childs", "Address", "Name\n");
			//		printf("%8x %8x %8x %16I64x %10s\n", pSymInfo->Size, pSymInfo->TypeIndex,
			//			childs.Count, pSymInfo->Address, pSymInfo->Name);
			//	}
			//}


			m_rgList.Add(new Symbol(symName));
			//Console.WriteLine($">> {symName->ReadSymbolName()}");


			return true;
		}

		internal static unsafe void GetSymbol(IntPtr hProc, string img, string name)
		{
			//var options = GetOptions();

			// SYMOPT_DEBUG option asks DbgHelp to print additional troubleshooting
			// messages to debug output - use the debugger's Debug Output window
			// to view the messages

			//options |= SymbolOptions.DEBUG;

			//SetOptions(options);

			// Initialize DbgHelp and load symbols for all modules of the current process 
			SymInitialize(hProc);

			//....

			//hProcess = GetCurrentProcess();
			//SymInitialize(hProcess, NULL, FALSE);
			//DWORD64 BaseOfDll = SymLoadModuleEx(hProcess, NULL, pdb, NULL,
			//	0x400000, 0x20000, NULL, 0);
			//SymEnumSymbols(hProcess, BaseOfDll, "*!*", EnumSymProc, ctcx);
			//SymEnumTypes(hProcess, BaseOfDll, EnumSymProc, ctcx);
			//SymCleanup(hProcess);

			//var hFile = CreateFile(img, FileAccess.Read, FileShare.Read,
			//	FileMode.Open, default);

			//var fileSize = GetFileSize(hFile);

			//Console.WriteLine(fileSize);

			var m_modBase = SymLoadModuleEx(hProc, IntPtr.Zero, img,
				null, 0x400000, 0x20000, IntPtr.Zero, 0);

			var methoddescReset = "MethodDesc::Reset";
			SymEnumSymbols(hProc, m_modBase, "*!*", AddSymCallback, Marshal.StringToHGlobalUni(methoddescReset));

			Console.WriteLine(m_modBase);


			// byte* byteBuffer = stackalloc byte[DebugSymbol.FullSize];
			// var   buffer     = (DebugSymbol*) byteBuffer;
			//
			// buffer->SizeOfStruct = (uint) DebugSymbol.SizeOf;
			// buffer->MaxNameLen   = DebugSymbol.MaxNameLength;
			//
			// Guard.Assert(FromName(hProc, name, (IntPtr) buffer),
			// 	"Symbol \"{0}\" not found", name);

			Console.WriteLine(m_rgList.First(s => s.Name.Contains(methoddescReset)));
		}

		#endregion


		[DllImport(KERNEL32_DLL, SetLastError = true)]
		public static extern void GetSystemInfo(ref SystemInfo Info);

		public static uint GetFileSize(IntPtr hFile) => GetFileSize(hFile, IntPtr.Zero);

		[DllImport(KERNEL32_DLL, SetLastError = true, CharSet = CharSet.Auto)]
		private static extern IntPtr CreateFile(string fileName, FileAccess fileAccess,
			FileShare fileShare,
			IntPtr securityAttributes,
			FileMode creationDisposition,
			FileAttributes flagsAndAttributes,
			IntPtr template);

		[DllImport(KERNEL32_DLL)]
		private static extern uint GetFileSize(IntPtr hFile, IntPtr lpFileSizeHigh);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		internal static extern IntPtr GetCurrentProcess();

		//TODO
		public static IntPtr CreateFile(string fileName,
			FileAccess access,
			FileShare share,
			FileMode mode,
			FileAttributes attributes)
		{
			return CreateFile(fileName, access, share, IntPtr.Zero,
				mode, attributes, IntPtr.Zero);
		}

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

		[DllImport(KERNEL32_DLL)]
		internal static extern IntPtr OpenProcess(ProcessAccess dwDesiredAccess, bool bInheritHandle, int dwProcessId);

		[DllImport(KERNEL32_DLL, SetLastError = true, PreserveSig = true)]
		internal static extern bool CloseHandle(IntPtr obj);

		internal static IntPtr OpenProcess(Process proc) => OpenProcess(ProcessAccess.All, false, proc.Id);

		[DllImport(USER32_DLL, EntryPoint = "FindWindow", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern IntPtr FindWindowByCaption(IntPtr zeroOnly, string lpWindowName);

		public static IntPtr GetWindowByCaption(string lpWindowName) => FindWindowByCaption(IntPtr.Zero, lpWindowName);

		#endregion


		#region Virtual

		[DllImport(KERNEL32_DLL)]
		internal static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress,
			uint dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

		[DllImport(KERNEL32_DLL)]
		internal static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress,
			int dwSize, AllocationType dwFreeType);

		[DllImport(KERNEL32_DLL)]
		internal static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress,
			uint dwSize, MemoryProtection flNewProtect, out MemoryProtection lpflOldProtect);

		[DllImport(KERNEL32_DLL)]
		internal static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress,
			ref MemoryBasicInformation lpBuffer, uint dwLength);

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

		[DllImport(SHELL32_DLL)]
		internal static extern int SHGetKnownFolderPath(
			[MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken,
			out IntPtr ppszPath);

		[DllImport(KERNEL32_DLL)]
		internal static extern uint LocalSize(IntPtr p);

		[DllImport(URLMON_DLL, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false)]
		internal static extern int FindMimeFromData(IntPtr pBC,
			[MarshalAs(UnmanagedType.LPWStr)] string pwzUrl,
			[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1, SizeParamIndex = 3)]
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

	[Flags]
	public enum SymbolFlag
	{
		/// <summary>
		/// The symbol is a CLR token.
		/// </summary>
		CLR_TOKEN = 0x00040000,


		/// <summary>
		/// The symbol is a constant.
		/// </summary>
		CONSTANT = 0x00000100,


		/// <summary>
		/// The symbol is from the export table.
		/// </summary>
		EXPORT = 0x00000200,


		/// <summary>
		/// The symbol is a forwarder.
		/// </summary>
		FORWARDER = 0x00000400,


		/// <summary>
		/// Offsets are frame relative.
		/// </summary>
		FRAMEREL = 0x00000020,


		/// <summary>
		/// The symbol is a known function.
		/// </summary>
		FUNCTION = 0x00000800,


		/// <summary>
		/// The symbol address is an offset relative to the beginning of the intermediate language block. This applies to managed code only.
		/// </summary>
		ILREL = 0x00010000,


		/// <summary>
		/// The symbol is a local variable.
		/// </summary>
		LOCAL = 0x00000080,


		/// <summary>
		/// The symbol is managed metadata.
		/// </summary>
		METADATA = 0x00020000,


		/// <summary>
		/// The symbol is a parameter.
		/// </summary>
		PARAMETER = 0x00000040,


		/// <summary>
		/// The symbol is a register. The Register member is used.
		/// </summary>
		REGISTER = 0x00000008,

		/// <summary>
		/// Offsets are register relative.
		/// </summary>
		REGREL = 0x00000010,

		/// <summary>
		/// The symbol is a managed code slot.
		/// </summary>
		SLOT = 0x00008000,

		/// <summary>
		/// The symbol is a thunk.
		/// </summary>
		THUNK = 0x00002000,

		/// <summary>
		/// The symbol is an offset into the TLS data area.
		/// </summary>
		TLSREL = 0x00004000,

		/// <summary>
		/// The Value member is used.
		/// </summary>
		VALUEPRESENT = 0x00000001,

		/// <summary>
		/// The symbol is a virtual symbol created by the SymAddSymbol function.
		/// </summary>
		VIRTUAL = 0x00001000,
	}

	[Flags]
	public enum SymbolOptions : uint
	{
		ALLOW_ABSOLUTE_SYMBOLS = 0x00000800,

		ALLOW_ZERO_ADDRESS = 0x01000000,

		AUTO_PUBLICS = 0x00010000,

		CASE_INSENSITIVE = 0x00000001,

		DEBUG = 0x80000000,

		DEFERRED_LOADS = 0x00000004,

		DISABLE_SYMSRV_AUTODETECT = 0x02000000,

		EXACT_SYMBOLS = 0x00000400,

		FAIL_CRITICAL_ERRORS = 0x00000200,

		FAVOR_COMPRESSED = 0x00800000,

		FLAT_DIRECTORY = 0x00400000,

		IGNORE_CVREC = 0x00000080,

		IGNORE_IMAGEDIR = 0x00200000,

		IGNORE_NT_SYMPATH = 0x00001000,

		INCLUDE_32_BIT_MODULES = 0x00002000,

		LOAD_ANYTHING = 0x00000040,

		LOAD_LINES = 0x00000010,

		NO_CPP = 0x00000008,

		NO_IMAGE_SEARCH = 0x00020000,

		NO_PROMPTS = 0x00080000,

		NO_PUBLICS = 0x00008000,

		NO_UNQUALIFIED_LOADS = 0x00000100,

		OVERWRITE = 0x00100000,

		PUBLICS_ONLY = 0x00004000,

		SECURE = 0x00040000,

		UNDNAME = 0x00000002,
	}

	public enum SymbolTag
	{
		Null,
		Exe,
		Compiland,
		CompilandDetails,
		CompilandEnv,
		Function,
		Block,
		Data,
		Annotation,
		Label,
		PublicSymbol,
		UDT,
		Enum,
		FunctionType,
		PointerType,
		ArrayType,
		BaseType,
		Typedef,
		BaseClass,
		Friend,
		FunctionArgType,
		FuncDebugStart,
		FuncDebugEnd,
		UsingNamespace,
		VTableShape,
		VTable,
		Custom,
		Thunk,
		CustomType,
		ManagedType,
		Dimension,
		CallSite,
		InlineSite,
		BaseInterface,
		VectorType,
		MatrixType,
		HLSLType,
		Caller,
		Callee,
		Export,
		HeapAllocationSite,
		CoffGroup,
		Max
	}
}