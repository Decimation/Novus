using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Web;
using JetBrains.Annotations;
using Novus.OS.Win32;
using Novus.OS.Win32.Structures;
using Novus.OS.Win32.Wrappers;
using Novus.Properties;
// ReSharper disable UnusedParameter.Local

// ReSharper disable InconsistentNaming
#pragma warning disable IDE0060

#pragma warning disable CS0618
#pragma warning disable SYSLIB0014 // Type or member is obsolete

// ReSharper disable UnusedMember.Global

namespace Novus.OS;

public sealed class SymbolReader : IDisposable
{
	private bool m_disposed;

	private ulong m_modBase;

	public bool AllLoaded => m_modBase != 0 && Symbols.Any();

	public string Image { get; }

	public IntPtr Process { get; }

	public List<Symbol> Symbols { get; }


	private const string MASK_ALL = "*!*";

	public SymbolReader(IntPtr process, string image)
	{
		Process    = process;
		Image      = image;
		m_modBase  = LoadModule();
		m_disposed = false;

		Symbols = new List<Symbol>();
		LoadAll();
	}

	public SymbolReader(string image) : this(Native.GetCurrentProcess(), image) { }


	[CanBeNull]
	public Symbol GetSymbol(string name)
	{
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

		if (m_disposed) {
			throw new ObjectDisposedException(nameof(SymbolReader));
		}


		var sym = Symbols.FirstOrDefault(s => s.Name.Contains(name));

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


	public void LoadAll(string mask = MASK_ALL)
	{
		if (m_disposed) {
			throw new ObjectDisposedException(nameof(SymbolReader));
		}

		if (AllLoaded) {
			return;
		}


		Native.SymEnumSymbols(Process, m_modBase, MASK_ALL, (ptr, u, context) =>
		{
			var b = EnumSymCallback(ptr, u, context, out var symbol);
			Symbols.Add(symbol);
			return b;
		}, IntPtr.Zero);


	}

	private void Cleanup()
	{
		Native.SymCleanup(Process);
		Native.SymUnloadModule64(Process, m_modBase);
		Symbols.Clear();
		m_modBase  = 0;
		m_disposed = true;
	}

	private static unsafe bool EnumSymCallback(IntPtr info, uint symbolSize, IntPtr pUserContext, out Symbol item)
	{
		var symbol = (SymbolInfo*) info;

		item = new Symbol(symbol);

		// Symbols.Add(item);

		return true;
	}

	private ulong LoadModule()
	{
		var options = Native.SymGetOptions();

		options |= SymbolOptions.DEBUG;

		Native.SymSetOptions(options);

		// Initialize DbgHelp and load symbols for all modules of the current process 
		Native.SymInitialize(Process, IntPtr.Zero, false);

		const int BASE_OF_DLL = 0x400000;
		const int DLL_SIZE    = 0x20000;

		ulong modBase = Native.SymLoadModuleEx(Process, IntPtr.Zero, Image,
		                                       null, BASE_OF_DLL, DLL_SIZE,
		                                       IntPtr.Zero, 0);

		return modBase;
	}

	/// <summary>
	///     Searches for symbol file locally; otherwise; downloads it
	/// </summary>
	/// <param name="fname">PE file</param>
	/// <param name="cacheDirectoryPath">Output folder</param>
	/// <returns>
	///     Path to the symbol file if it was found
	///     <em>or</em> path to the downloaded symbol file
	/// </returns>
	public static string ResolveSymbolFile(string fname, [CanBeNull] string cacheDirectoryPath = null)
	{
		// fname=FileSystem.SearchInPath(fname);
		fname = Path.GetFullPath(fname);

		using var peReader = new PEReader(File.OpenRead(fname));

		var codeViewEntry = peReader.ReadDebugDirectory()
		                            .First(entry => entry.Type == DebugDirectoryEntryType.CodeView);

		var pdbData = peReader.ReadCodeViewDebugDirectoryData(codeViewEntry);

		// var cacheDirectoryPath = Global.ProgramData;

		cacheDirectoryPath ??= FileSystem.GetPath(KnownFolder.Downloads);
		using var wc = new WebClient();

		// Check if the correct version of the PDB is already cached
		var path       = Path.ChangeExtension(fname, "pdb");
		var fileName   = Path.GetFileName(path);
		var pdbDirPath = Path.Combine(cacheDirectoryPath, fileName);

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
			Debug.WriteLine($"Using {pdbFilePath}", nameof(ResolveSymbolFile));
			return pdbFilePath;
		}


		var uriString = EmbeddedResources.MicrosoftSymbolServer +
		                $"{fileName}/" +
		                $"{pdbData.Guid:N}{pdbData.Age}/{fileName}";

		Debug.WriteLine($"Downloading {uriString}", nameof(ResolveSymbolFile));


		//await wc.DownloadFileTaskAsync(new Uri(uriString), pdbFilePath);
		wc.DownloadFile(new Uri(uriString), pdbFilePath);
		Debug.WriteLine($"Downloaded to {pdbFilePath} ({pdbPlusGuidDirPath})", nameof(ResolveSymbolFile));

		return pdbFilePath;
	}

	public void Dispose()
	{
		Cleanup();
	}
}