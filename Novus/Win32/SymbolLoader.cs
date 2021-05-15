using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.PortableExecutable;
using JetBrains.Annotations;
using Novus.Win32.Structures;
using Novus.Win32.Wrappers;
// ReSharper disable UnusedMember.Global

namespace Novus.Win32
{
	public sealed class SymbolLoader : IDisposable
	{
		private bool m_disposed;

		private ulong m_modBase;

		public bool AllLoaded => m_modBase != 0 && SymbolsCache.Any();

		public string Image { get; }

		public IntPtr Process { get; }

		public List<Symbol> SymbolsCache { get; }

		public SymbolLoader(IntPtr process, string image)
		{
			Process      = process;
			Image        = image;
			SymbolsCache = new List<Symbol>();
			m_modBase    = LoadModule();
			m_disposed   = false;

			LoadAll();
		}

		public void Dispose()
		{
			Cleanup();
		}


		[CanBeNull]
		public Symbol GetSymbol(string name)
		{
			//todo

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
				throw new ObjectDisposedException(nameof(SymbolLoader));
			}


			var sym = SymbolsCache.FirstOrDefault(s => s.Name.Contains(name));

			//todo: symfromname...
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

		public void LoadAll()
		{
			if (m_disposed) {
				throw new ObjectDisposedException(nameof(SymbolLoader));
			}

			if (AllLoaded) {
				return;
			}

			const string mask = "*!*";

			Native.SymEnumSymbols(Process, m_modBase, mask, EnumSymCallback, IntPtr.Zero);
		}

		private void Cleanup()
		{
			Native.SymCleanup(Process);
			Native.SymUnloadModule64(Process, m_modBase);
			SymbolsCache.Clear();
			m_modBase  = 0;
			m_disposed = true;
		}

		private unsafe bool EnumSymCallback(IntPtr info, uint symbolSize, IntPtr pUserContext)
		{
			var symbol = (SymbolInfo*) info;

			var item = new Symbol(symbol);

			SymbolsCache.Add(item);

			return true;
		}

		private ulong LoadModule()
		{
			var options = Native.SymGetOptions();

			options |= SymbolOptions.DEBUG;

			Native.SymSetOptions(options);

			// Initialize DbgHelp and load symbols for all modules of the current process 
			Native.SymInitialize(Process, IntPtr.Zero, false);


			const int baseOfDll = 0x400000;

			const int dllSize = 0x20000;


			ulong modBase = Native.SymLoadModuleEx(Process, IntPtr.Zero, Image,
				null, baseOfDll, dllSize, IntPtr.Zero, 0);

			return modBase;
		}

		/// <summary>
		/// Searches for symbol file or downloads it
		/// </summary>
		/// <param name="fname">PE file</param>
		public static string FindOrDownloadSymbolFile(string fname)
		{
			using var peReader = new PEReader(File.OpenRead(fname));

			var codeViewEntry = peReader.ReadDebugDirectory()
				.First(entry => entry.Type == DebugDirectoryEntryType.CodeView);

			var pdbData = peReader.ReadCodeViewDebugDirectoryData(codeViewEntry);

			var cacheDirectoryPath = Global.ProgramData;

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
				Debug.WriteLine($"Using {pdbFilePath}");
				return pdbFilePath;
			}


			var uriString =
				$"https://msdl.microsoft.com/download/symbols/{fileName}/{pdbData.Guid:N}{pdbData.Age}/{fileName}";

			Debug.WriteLine($"Downloading {uriString}");

			//await wc.DownloadFileTaskAsync(new Uri(uriString), pdbFilePath);
			wc.DownloadFile(new Uri(uriString), pdbFilePath);
			Debug.WriteLine($"Downloaded to {pdbFilePath}");

			return pdbFilePath;
		}
	}
}