using System.Collections.Generic;
using System.Collections.Immutable;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.Logging;
using Perfolizer.Horology;

#pragma warning disable IDE0060
namespace TestBenchmark;

public static class Program
{

	public static void Main(string[] args)
	{
		// cd .\Benchmark\ ; dotnet build -c Release ; dotnet run -c Release
		//dotnet build -c Release ; dotnet run -c Release --project .\Benchmark\TestBenchmark.csproj

		var cfg = DefaultConfig.Instance
			// .AddExporter(new HtmlExporter())
			// .AddDiagnoser(new MemoryDiagnoser(new MemoryDiagnoserConfig()) {})
			.AddJob(Job.Default.WithRuntime(CoreRuntime.Core80));

		BenchmarkRunner.Run<Benchmarks26>(cfg);
	}

}