using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Runtime.Caching;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Web;
using CliWrap;
using CliWrap.Buffered;
using JetBrains.Annotations;
using Kantan.Diagnostics;
using Kantan.Text;
using Novus.Win32.Structures;
using Novus.Properties;
using Novus.Win32.Structures.DbgHelp;
using Novus.Win32.Wrappers;
using Novus.OS;
using Novus.Win32;
using System.Text;
using Novus.Runtime;


// ReSharper disable UnusedParameter.Local

// ReSharper disable InconsistentNaming
#pragma warning disable IDE0060
#pragma warning disable CS0618
#pragma warning disable SYSLIB0014 // Type or member is obsolete

// ReSharper disable UnusedMember.Global

namespace Novus.Utilities;

public enum SymImageType
{

	Unknown = 0,

	/// <summary>
	/// <seealso cref="SymbolReader.EXT_PDB"/>
	/// </summary>
	Pdb,

	/// <summary>
	/// <seealso cref="SymbolReader.EXT_DLL"/>
	/// </summary>
	Dll

}

/// <summary>
/// Windows PDB reader
/// </summary>
[SupportedOSPlatform(RuntimeInformationExtensions.OS_WIN)]
public sealed class SymbolReader : IDisposable
{

	private bool              m_disposed;
	private ulong             m_modBase;
	private ImageHelpModule64 m_imgInfo;

	private static readonly Func<Symbol, bool> AnyPredicate = static _ => true;

	public bool AllLoaded => m_modBase != 0 && !Symbols.IsEmpty;

	public string Image { get; }

	public nint Handle { get; }

	public ConcurrentBag<Symbol> Symbols { get; }

	public static string SymbolPath => Environment.GetEnvironmentVariable(ENV_VAR_NT_SYMBOL_PATH, EnvironmentVariableTarget.Machine);

	public const string ENV_VAR_NT_SYMBOL_PATH = "_NT_SYMBOL_PATH";

	public const string EXT_PDB = ".pdb";
	public const string EXT_DLL = ".dll";

	// private const string MASK_ALL = "*!*";
	public const string MASK_ALL = "*";

	public SymImageType ImageType { get; }

	public SymbolReader(nint handle, string image)
	{
		// Require.FileExists(image);
		Handle = handle;
		Image  = image;

		ImageType = GetImageTypeFromExtension(image);

		m_modBase  = LoadModule();
		m_disposed = false;

		Symbols = [];
		LoadAllSymbols();
	}

	public SymbolReader(string image) : this(Random.Shared.Next(), image) { }

	public IEnumerable<Symbol> GetSymbols(string name, [CBN] Func<Symbol, bool> pred = null)
	{
		pred ??= AnyPredicate;

		/*
		 * https://docs.microsoft.com/en-us/windows/win32/api/dbghelp/ns-dbghelp-symbol_info
		 *
		 * https://github.com/Decimation/NeoCore/blob/master/NeoCore/Win32/Symbols.cs
		 * https://github.com/Decimation/NeoCore/blob/master/NeoCore/Win32/Structures/SymbolStructures.cs
		 * https://stackoverflow.com/questions/18249566/c-sharp-get-the-list-of-unmanaged-c-dll-exports
		 * https://stackoverflow.com/questions/12656737/how-to-obtain-the-dll-list-of-a-specified-process-and-loop-through-it-to-check-i
		 *
		 *
		 * https://github.com/southpolenator/SharpPdb
		 * https://github.com/horsicq/PDBRipper
		 * https://github.com/Broihon/Symbol-Parser
		 * https://github.com/moyix/pdbparse
		 */

		ObjectDisposedException.ThrowIf(m_disposed, this);

		var sym = Symbols.Where(s => s.Name.Contains(name) && pred(s));

		//todo: SymFromName...
		/*var d = new DebugSymbol();
		d.SizeOfStruct = (uint)Marshal.SizeOf<DebugSymbol>();
		d.MaxNameLen = 1000;

		if (!SymFromName(hProc, name, new IntPtr(&d)))
		{
			Debug.WriteLine("ERROR");
		}

		var sym = new Symbol(&d);*/

		return sym;
	}

	[CBN]
	public Symbol GetSymbol(string name, [CBN] Func<Symbol, bool> pred = null)
	{
		pred ??= AnyPredicate;

		return GetSymbols(name).FirstOrDefault(pred);
	}

	public void LoadAllSymbols(string mask = MASK_ALL)
	{
		ObjectDisposedException.ThrowIf(m_disposed, this);

		if (AllLoaded) {
			return;
		}

		Native.SymEnumSymbols(Handle, m_modBase, MASK_ALL, (ptr, u, context) =>
		{
			var b = EnumSymCallback(ptr, u, context, out var symbol);
			Symbols.Add(symbol);
			return b;
		}, IntPtr.Zero);

		Trace.WriteLine($"Loaded {Symbols.Count} symbols", nameof(LoadAllSymbols));
	}

	private static unsafe bool EnumSymCallback(nint info, uint symbolSize, nint pUserContext, out Symbol item)
	{
		var symbol = (SymbolInfo*) info;

		item = new Symbol(symbol);

		// Symbols.Add(item);

		return true;
	}

	private ulong LoadModule()
	{
		bool ok = Initialize(Handle);

		const int BASE_OF_DLL = 0x400000;
		const int DLL_SIZE    = 0x20000;

		const ulong VIRTUAL_BASE = 0x10000000;

		/*ulong modBase = ImageType switch
		{
			SymImageType.Pdb => Native.SymLoadModuleEx(Handle, IntPtr.Zero, Image, null, BASE_OF_DLL, 0, IntPtr.Zero, 0),
			SymImageType.Dll => Native.SymLoadModuleEx(Handle, 0, Image, null, 0, 0, 0, default),
			_                => 0
		};*/

		ulong modBase = Native.SymLoadModuleEx(Handle, IntPtr.Zero, Image, null, BASE_OF_DLL, 0, IntPtr.Zero, 0);

		m_imgInfo = new ImageHelpModule64() { };

		var effectiveBase = modBase != 0 ? modBase : VIRTUAL_BASE;

		Native.SymGetModuleInfoW64(Handle, modBase, ref m_imgInfo);

		return effectiveBase;
	}

	public static bool Initialize(nint handle)
	{
		var options = Native.SymGetOptions();

		options |= SymbolOptions.DEBUG | SymbolOptions.UNDNAME | SymbolOptions.DEFERRED_LOADS;

		Native.SymSetOptions(options);

		// Initialize DbgHelp and load symbols for all modules of the current process 
		return Native.SymInitialize(handle, SymbolPath, false);
	}

	public static async Task<string> SymchkSymbolFileAsync(string fname, [CBN] string o = null)
	{
		o ??= FileSystem.GetPath(KnownFolder.Downloads);

		// symchk.exe .\urlmon.dll /s SRV*"C:\Symbols\"*http://msdl.microsoft.com/download/symbols /osdbc \.
		//symchk /os <input> /su "SRV**http://msdl.microsoft.com/download/symbols" /oc <output>
		//symchk <input> /su "SRV**http://msdl.microsoft.com/download/symbols" /osc <output>

		if (!File.Exists(fname)) {
			throw new FileNotFoundException(null, fname);
		}

		var cmd = Cli.Wrap(ER.E_Symchk)
		             .WithArguments(["/if", fname, "/su", $"SRV**{ER.MicrosoftSymbolServer}", "/oscdb", o], true)
		             .WithValidation(CommandResultValidation.None);

		var bcr = await cmd.ExecuteBufferedAsync();

		var error  = bcr.StandardError;
		var stdOut = bcr.StandardOutput;

		/*
		if (!string.IsNullOrWhiteSpace(error)) {
			// process.Dispose();
			return null;
		}
		*/

		var outFile = Path.Combine(o, Path.GetFileNameWithoutExtension(fname) + EXT_PDB);

		if (!File.Exists(outFile)) {
			throw new FileNotFoundException(null, outFile);
		}

		return outFile;
	}


	public static async Task<string> DownloadSymbolFileAsync(string fname, string o)
	{
		// fname=FileSystem.SearchInPath(fname);
		fname = Path.GetFullPath(fname);

		using var peReader = new PEReader(File.OpenRead(fname));

		var codeViewEntry = peReader.ReadDebugDirectory()
		                            .First(entry => entry.Type == DebugDirectoryEntryType.CodeView);

		var pdbData = peReader.ReadCodeViewDebugDirectoryData(codeViewEntry);

		// var cacheDirectoryPath = Global.ProgramData;

		o ??= FileSystem.GetPath(KnownFolder.Downloads);
		using var wc = new WebClient();

		// Check if the correct version of the PDB is already cached
		var path       = Path.ChangeExtension(fname, EXT_PDB);
		var fileName   = Path.GetFileName(path);
		var pdbDirPath = Path.Combine(o, fileName);

		if (!Directory.Exists(pdbDirPath)) {
			Directory.CreateDirectory(pdbDirPath);
		}

		var pdbPlusGuidDirPath = Path.Combine(pdbDirPath, pdbData.Guid.ToString());

		if (!Directory.Exists(pdbPlusGuidDirPath)) {
			Directory.CreateDirectory(pdbPlusGuidDirPath);
		}

		//var pdbFilePath = Path.Combine(pdbPlusGuidDirPath, path);
		var pdbFilePath = Path.Combine(pdbPlusGuidDirPath, fileName);

		if (File.Exists(pdbFilePath)) {
			goto ret;
		}

		var uriString = $"{ER.MicrosoftSymbolServer}{fileName}/{pdbData.Guid:N}{pdbData.Age}/{fileName}";

		await wc.DownloadFileTaskAsync(new Uri(uriString), pdbFilePath);

		// wc.DownloadFile(new Uri(uriString), pdbFilePath);

	ret:

		return pdbFilePath;
	}

	public static IEnumerable<string> EnumerateSymbolPath(string pattern, string symPath = ENV_VAR_NT_SYMBOL_PATH)
	{
		// var nt = Environment.ExpandEnvironmentVariables(symPath);
		var nt = Environment.GetEnvironmentVariable(symPath, EnvironmentVariableTarget.Machine);

		/*
		var nt = Environment.GetEnvironmentVariable(symPath, EnvironmentVariableTarget.Machine)?
			.Split(FileSystem.PathDelimiter, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			*/

		if (!Path.Exists(nt) || nt == null) {
			return [];
		}

		return Directory.EnumerateFiles(nt, pattern, new EnumerationOptions()
		{
			MatchType             = MatchType.Simple,
			RecurseSubdirectories = true,
			MaxRecursionDepth     = 3
		});
	}

	public static SymImageType GetImageTypeFromExtension(string image)
	{
		return Path.GetExtension(image) switch
		{
			EXT_PDB => SymImageType.Pdb,
			EXT_DLL => SymImageType.Dll,
			_       => SymImageType.Unknown
		};
	}

	public void Dispose()
	{
		Native.SymCleanup(Handle);
		Native.SymUnloadModule64(Handle, m_modBase);
		Symbols.Clear();
		m_modBase  = 0;
		m_disposed = true;
	}

}