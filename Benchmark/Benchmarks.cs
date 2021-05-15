using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using Novus;
using Novus.Memory;
using Novus.Win32;
using Novus.Win32.Wrappers;

namespace TestBenchmark
{
	public unsafe class Benchmarks
	{
		[Benchmark]
		public Symbol Bench1()
		{
			return sl.GetSymbol("g_pGCHeap");
		}

		private SymbolLoader sl;

		[GlobalSetup]
		public void GlobalSetup()
		{

			Global.Setup();
			sl = new SymbolLoader(Native.GetCurrentProcess(), @"C:\Users\Deci\Desktop\coreclr.pdb");
			sl.LoadAll();
		}

		// [IterationCleanup]
		// public void IterationCleanup()
		// {
		//
		// }
	}
}