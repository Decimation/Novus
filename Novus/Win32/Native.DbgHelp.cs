#nullable enable
#pragma warning disable SYSLIB1054
using Novus.Utilities;
using Novus.Win32.Structures.DbgHelp;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

// ReSharper disable ArrangeAttributes
// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

namespace Novus.Win32;
#nullable disable
public static unsafe partial class Native
{

	[DllImport(DBGHELP_DLL)]
	private static extern ImageNtHeaders* ImageNtHeader([In] nint hModule);

#region Symbols

	[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
	internal static extern bool SymInitialize(nint hProcess, [Opt, CBN] string userSearchPath, bool fInvadeProcess);

	[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
	internal static extern bool SymCleanup(nint hProcess);

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	internal delegate bool EnumSymbolsCallback(nint symInfo, uint symbolSize, nint pUserContext);

	[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
	internal static extern bool SymEnumSymbols(nint hProcess, ulong modBase, string mask, EnumSymbolsCallback callback, nint pUserContext);

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
	internal static extern ulong SymLoadModuleEx(nint hProcess, nint hFile, string imageName, string moduleName, ulong baseOfDll, uint dllSize,
	                                             nint data,     uint flags);

	[DllImport(DBGHELP_DLL, SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true)]
	[return: MA(UT.Bool)]
	public static extern bool SymGetModuleInfo64(IntPtr hProcess, ulong qwAddr, ref ImageHelpModule64 ModuleInfo);

	[DllImport(DBGHELP_DLL, SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
	[return: MA(UT.Bool)]
	public static extern bool SymGetModuleInfoW64(IntPtr hProcess, ulong qwAddr, ref ImageHelpModule64 ModuleInfo);

	[return: MA(UT.Bool)]
	public delegate bool PFINDFILEINPATHCALLBACK([MA(UT.LPTStr)] string filename, [In, Opt] nint context);


	[DllImport(DBGHELP_DLL, SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
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

	[DllImport(DBGHELP_DLL, SetLastError = true, CharSet = CharSet.Auto)]
	[return: MA(UT.Bool)]
	public static extern bool SymGetSymbolFile([Opt]           nint   hProcess,    [CBN] [Opt, MA(UT.LPTStr)] string SymPath,
	                                           [MA(UT.LPTStr)] string ImageFile,   IMAGEHLP_SF_TYPE Type, [Out, MA(UT.LPTStr)] StringBuilder SymbolFile,
	                                           nint                   cSymbolFile, [Out, MA(UT.LPTStr)] StringBuilder DbgFile, nint cDbgFile);

#endregion

	public static bool SymInitialize(nint handle, [CBN] string symPath = null)
	{
		symPath ??= SymbolHandler.SymbolPath;

		var options = SymGetOptions();

		options |= SymbolOptions.DEBUG | SymbolOptions.UNDNAME | SymbolOptions.DEFERRED_LOADS;

		SymSetOptions(options);

		// Initialize DbgHelp and load symbols for all modules of the current process
		return SymInitialize(handle, symPath, false);
	}

}

public enum IMAGEHLP_SF_TYPE
{

	/// <summary>A .exe or .dll file.</summary>
	sfImage = 0,

	/// <summary>A .dbg file.</summary>
	sfDbg,

	/// <summary>A .pdb file.</summary>
	sfPdb,

	/// <summary>Reserved.</summary>
	sfMpd,

}