using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using Novus;
using Novus.Memory;
using Novus.Win32;

namespace TestBenchmark
{
	public unsafe class Benchmarks
	{
		//[Benchmark]
		//public void Bench1()
		//{
		//	Native.GetSymbol(Native.GetCurrentProcess(), @"C:\Users\Deci\Desktop\coreclr.pdb", "g_pGCHeap");
		//}

		private Pointer<int> p2;
		private int*         p1;

		[GlobalSetup]
		public void GlobalSetup()
		{
			p1  = (int*) Marshal.AllocHGlobal(sizeof(int));
			*p1 = 123;
			p2  = p1;
			//Global.Setup();
		}

		[Benchmark]
		public int Read1()
		{
			return *p1;
		}

		[Benchmark]
		public int Read2()
		{
			return p2.Reference;
		}
	}
}