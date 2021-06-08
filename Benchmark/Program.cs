using BenchmarkDotNet.Running;
using Novus.Benchmark;

namespace TestBenchmark
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			BenchmarkRunner.Run<Benchmarks>();
		}
	}
}
