using BenchmarkDotNet.Attributes;
using Novus.Memory;
using Novus.Win32;
using Novus.Win32.Wrappers;

namespace Novus.Benchmark
{
	
	


	public unsafe class Benchmarks2
	{
		private Pointer<int> p;
		private int*         p2;
		private int          i = 123;

		[GlobalSetup]
		public void setup()
		{
			p = Mem.AddressOf(ref i);

			fixed (int* x = &i) {
				p2 = x;
			}
		}

		[Benchmark]
		public int Bench2()
		{
			return *p2;
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