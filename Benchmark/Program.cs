using BenchmarkDotNet.Running;

namespace TestBenchmark
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			//cd .\Benchmark\
			//dotnet run -c Release
			BenchmarkRunner.Run<Benchmarks6>();
		}
	}
}
