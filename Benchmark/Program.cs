using BenchmarkDotNet.Running;

namespace Novus.Benchmark
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			BenchmarkRunner.Run<Benchmarks>();
		}
	}
}
