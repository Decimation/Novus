using BenchmarkDotNet.Attributes;
using Novus.Win32;

namespace TestBenchmark
{
	
	public class Benchmarks
	{
		[Benchmark]
		public void Bench1()
		{
			Native.GetSymbol(Native.GetCurrentProcess(), @"C:\Users\Deci\Desktop\coreclr.pdb", "g_pGCHeap");
		}
	}
}
