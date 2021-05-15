using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Novus.Win32.Structures;
using Novus.Win32.Wrappers;

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
	}
}