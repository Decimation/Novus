using BenchmarkDotNet.Running;
#pragma warning disable IDE0060
namespace TestBenchmark;

public static class Program
{
	public static void Main(string[] args)
	{
		// cd .\Benchmark\ ; dotnet build -c Release ; dotnet run -c Release
			
		BenchmarkRunner.Run<Benchmarks19>();

	}
}