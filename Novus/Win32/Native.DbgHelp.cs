using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Novus.Win32.Structures.DbgHelp;

// ReSharper disable InconsistentNaming

namespace Novus.Win32;

public static unsafe partial class Native
{
#pragma warning disable SYSLIB1054

	[DllImport(DBGHELP_DLL)]
	private static extern ImageNtHeaders* ImageNtHeader([In] nint hModule);

	#region Symbols

	[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
	internal static extern bool SymInitialize(nint hProcess, nint userSearchPath, bool fInvadeProcess);

	[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
	internal static extern bool SymCleanup(nint hProcess);

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	internal delegate bool EnumSymbolsCallback(nint symInfo, uint symbolSize, nint pUserContext);

	[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
	internal static extern bool SymEnumSymbols(nint hProcess, ulong modBase, string mask,
	                                           EnumSymbolsCallback callback, nint pUserContext);

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
	internal static extern ulong SymLoadModuleEx(nint hProcess, nint hFile, string imageName,
	                                             string moduleName, ulong baseOfDll, uint dllSize, nint data,
	                                             uint flags);

	#endregion
}