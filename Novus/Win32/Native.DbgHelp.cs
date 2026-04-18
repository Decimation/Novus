#nullable enable
using Novus.Win32.Structures.DbgHelp;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

// ReSharper disable ArrangeAttributes

// ReSharper disable IdentifierTypo

// ReSharper disable InconsistentNaming

namespace Novus.Win32;

public static unsafe partial class Native
{

#pragma warning disable SYSLIB1054

	[DllImport(DBGHELP_DLL)]
	private static extern ImageNtHeaders* ImageNtHeader([In] nint hModule);

#region Symbols

	[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
	internal static extern bool SymInitialize(nint hProcess, string userSearchPath, bool fInvadeProcess);

	[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
	internal static extern bool SymCleanup(nint hProcess);

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	internal delegate bool EnumSymbolsCallback(nint symInfo, uint symbolSize, nint pUserContext);

	[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
	internal static extern bool SymEnumSymbols(nint                hProcess, ulong modBase, string mask,
	                                           EnumSymbolsCallback callback, nint  pUserContext);

	[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
	internal static extern SymbolOptions SymGetOptions();

	[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
	internal static extern bool SymGetSearchPath(nint hProcess, sbyte* p, uint sz);

	[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
	internal static extern SymbolOptions SymSetOptions(SymbolOptions options);

	[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
	internal static extern bool SymFromName(nint hProcess, string name, nint pSymbol);

	[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
	internal static extern bool SymUnloadModule64(nint hProc, ulong baseAddr);

	[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
	internal static extern ulong SymLoadModuleEx(nint   hProcess,   nint  hFile,     string imageName,
	                                             string moduleName, ulong baseOfDll, uint   dllSize, nint data,
	                                             uint   flags);

	/*[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
	public static extern ulong SymLoadModuleEx(nint hProcess, [Optional] nint hFile, [Optional, MarshalAs(UnmanagedType.LPTStr)] string? ImageName,
	                                           [Optional, MarshalAs(UnmanagedType.LPTStr)]
	                                           string? ModuleName, ulong BaseOfDll, uint DllSize, in nint Data, [Optional] SymbolFlag Flags);

	[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
	public static extern ulong SymLoadModule64(nint hProcess, [Optional] nint hFile, [Optional, MarshalAs(UnmanagedType.LPStr)] string? ImageName,
	                                           [Optional, MarshalAs(UnmanagedType.LPStr)]
	                                           string? ModuleName, ulong BaseOfDll, uint SizeOfDll);*/

	[DllImport(DBGHELP_DLL, SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true)]
	[return: MA(UnmanagedType.Bool)]
	public static extern bool SymGetModuleInfo64(IntPtr hProcess, ulong qwAddr, ref ImageHelpModule64 ModuleInfo);

	[DllImport(DBGHELP_DLL, SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool SymGetModuleInfoW64(IntPtr hProcess, ulong qwAddr, ref ImageHelpModule64 ModuleInfo);

	[return: MA(UT.Bool)]
	public delegate bool PFINDFILEINPATHCALLBACK([MA(UT.LPTStr)] string filename,
	                                             [In, Opt]       nint   context);


	[return: MA(UT.Bool)]
	public static extern bool SymFindFileInPath(nint                                    hprocess,
	                                            [CBN, Opt, MA(UT.LPTStr)] string        SearchPath,
	                                            [MA(UT.LPTStr)]           string        FileName,
	                                            [In, Opt]                 nint          id,
	                                            uint                                    two,
	                                            uint                                    three,
	                                            SymbolServerOptions                     flags,
	                                            [MA(UT.LPTStr)] StringBuilder           FoundFile,
	                                            [CBN, In, Opt]  PFINDFILEINPATHCALLBACK callback,
	                                            [In, Opt]       nint                    context);

#endregion

}