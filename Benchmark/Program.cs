using BenchmarkDotNet.Running;

namespace TestBenchmark
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			BenchmarkRunner.Run<Benchmarks3>();
		}
	}
}
