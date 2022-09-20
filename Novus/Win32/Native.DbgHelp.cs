using System.Runtime.InteropServices;
using Novus.Win32.Structures.DbgHelp;

// ReSharper disable InconsistentNaming

namespace Novus.Win32;

public static unsafe partial class Native
{

	[DllImport(DBGHELP_DLL)]
	private static extern ImageNtHeaders* ImageNtHeader(IntPtr hModule);
	#region Symbols

	[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
	internal static extern bool SymInitialize(IntPtr hProcess, IntPtr userSearchPath, bool fInvadeProcess);

	[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
	internal static extern bool SymCleanup(IntPtr hProcess);

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	internal delegate bool EnumSymbolsCallback(IntPtr symInfo, uint symbolSize, IntPtr pUserContext);

	[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
	internal static extern bool SymEnumSymbols(IntPtr hProcess, ulong modBase, string mask,
	                                           EnumSymbolsCallback callback, IntPtr pUserContext);

	[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
	internal static extern SymbolOptions SymGetOptions();

	[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
	internal static extern bool SymGetSearchPath(IntPtr hProcess, sbyte* p, uint sz);

	[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
	internal static extern SymbolOptions SymSetOptions(SymbolOptions options);

	[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
	internal static extern bool SymFromName(IntPtr hProcess, string name, IntPtr pSymbol);

	[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
	internal static extern bool SymUnloadModule64(IntPtr hProc, ulong baseAddr);

	[DllImport(DBGHELP_DLL, CharSet = CharSet.Unicode)]
	internal static extern ulong SymLoadModuleEx(IntPtr hProcess, IntPtr hFile, string imageName,
	                                             string moduleName, ulong baseOfDll, uint dllSize, IntPtr data,
	                                             uint flags);

	#endregion

}