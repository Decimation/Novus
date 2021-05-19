using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using Novus;
using Novus.Memory;
using Novus.Win32;
using Novus.Win32.Wrappers;

namespace TestBenchmark
{

	public unsafe class Benchmarks2
	{
		private Pointer<int> p;

		private int i = 123;
		[GlobalSetup]
		public void setup()
		{
			p = Mem.AddressOf(ref i);
		}
		[Benchmark]
		public int Bench1()
		{
			return p.Value;
		}
		
	}

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