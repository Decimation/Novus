using System.Collections.Generic;
using System.Collections.Immutable;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.Logging;

#pragma warning disable IDE0060
namespace TestBenchmark;

public static class Program
{

	public static void Main(string[] args)
	{
		// cd .\Benchmark\ ; dotnet build -c Release ; dotnet run -c Release
		//dotnet build -c Release ; dotnet run -c Release --project .\Benchmark\TestBenchmark.csproj

		var cfg = ManualConfig.CreateMinimumViable()
			.AddExporter(new HtmlExporter())
			.AddDiagnoser(new MemoryDiagnoser(new MemoryDiagnoserConfig()))
			.AddJob(Job.Default);

		BenchmarkRunner.Run<Benchmarks31>(cfg);
	}

}