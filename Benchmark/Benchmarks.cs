using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Kantan.Utilities;
using Novus;
using Novus.FileTypes;
using Novus.FileTypes.Impl;
using Novus.Memory;
using Novus.Memory.Allocation;
using Novus.Win32.Structures;
using Novus.Runtime.Meta;
using Novus.Utilities;
using Novus.Win32;
using Novus.Win32.Structures.Kernel32;
using Novus.Win32.Wrappers;
using Dia2Lib;
using Flurl.Http;
using Novus.Imports.Attributes;
using Novus.Imports;
using System.Linq;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using JetBrains.Annotations;
using Kantan.Collections;
using Kantan.Text;
using Novus.Runtime;
using Novus.FileTypes.Uni;
using Novus.Properties;

// ReSharper disable InconsistentNaming

#pragma warning disable
namespace TestBenchmark;

public class MyClass
{

	public int   a;
	public float f;

	public override string ToString()
	{
		return $"{nameof(a)}: {a}, {nameof(f)}: {f}";
	}

}

public struct MyStruct
{

	public int   a;
	public float f;

	public override string ToString()
	{
		return $"{nameof(a)}: {a}, {nameof(f)}: {f}";
	}

}

public static class Values
{

	public const string u1 = "https://i.imgur.com/QtCausw.png";
	public const string f1 = @"C:\Users\Deci\Pictures\Epic anime\0c4c80957134d4304538c27499d84dbe.jpeg";

	public const string f2 =
		@"C:\Users\Deci\Pictures\Epic anime\2B_neko_FINALE_1_12.jpg";

	public static object[] vals
	{
		get
		{
			return new object[]
			{
				new MyClass(),
				new MyClass() { a = 123, f = 3.14f },
				new MyStruct(),
				new MyStruct() { a = 123, f = 3.14f }
			};
		}
	}

}

[RyuJitX64Job]
public class Benchmarks29
{

	private string s1;

	[GlobalSetup]
	public void GlobalSetup() { }

	[IterationSetup]
	public void IterationSetup()
	{
		s1 = Strings.CreateRandom(5);
	}

	[Benchmark]
	public Pointer<byte> Test4()
	{
		return Mem.AddressOfHeap(s1, OffsetOptions.StringData);
	}

	[Benchmark]
	public Pointer<byte> Test3()
	{
		return Mem.AddressOfHeap(s1, OffsetOptions.Fields);
	}

	[Benchmark]
	public Pointer<byte> Test2()
	{
		return Mem.AddressOfHeap(s1);
	}

	[Benchmark]
	public Pointer<string> Test1()
	{
		return Mem.AddressOf(ref s1);
	}

}

[RyuJitX64Job]
public class Benchmarks28
{

	[Benchmark]
	public bool IsSigned_()
	{
		return typeof(int).IsSigned();
	}

	[Benchmark]
	public bool IsReal_()
	{
		return typeof(uint).IsReal();
	}

	[Benchmark]
	public bool IsInt_()
	{
		return typeof(int).IsInteger();
	}

	[Benchmark]
	public bool IsUnsigned_()
	{
		return typeof(uint).IsUnsigned();
	}

	[Benchmark]
	public bool IsNumeric_()
	{
		return typeof(uint).IsNumeric();
	}

}

[RyuJitX64Job]
public class Benchmarks27
{

	[Benchmark]
	public async Task<UniSource> Test1()
	{
		return await UniSource.GetAsync(Values.u1);
	}

	[Benchmark]
	public async Task<UniSource> Test2()
	{
		return await UniSource.GetAsync(Values.f1);
	}

}

[RyuJitX64Job]
public class Benchmarks26
{

	private IFileTypeResolver magic, fast, urlmon;
	private FileStream        m_stream1;

	private Consumer m_consumer;

	private Stream m_stream3;

	public static object[] s;

	[ParamsSource(nameof(s))]
	public Stream ss;

	[GlobalSetup]
	public async void GlobalSetup()
	{
		magic  = MagicResolver.Instance;
		fast   = FastResolver.Instance;
		urlmon = UrlmonResolver.Instance;

		m_stream1  = File.OpenRead(Values.f2);
		m_stream3  = await Values.f2.GetStreamAsync();
		m_consumer = new Consumer();

		s = new[] { m_stream1, m_stream3 };

	}

	[Benchmark]
	public FileType Urlmon()
	{
		return urlmon.Resolve(ss);
	}

	[Benchmark]
	public FileType Magic()
	{
		return magic.Resolve(ss);
	}

	[Benchmark]
	public FileType Fast()
	{
		return fast.Resolve(ss);
	}

}

// [InProcess]
[RyuJitX64Job]
public unsafe class Benchmarks25
{

	private Pointer<nint> m_ptr;

	[GlobalSetup]
	public void GlobalSetup()
	{
		m_ptr = NativeMemory.Alloc((nuint) nint.Size);
	}

	[GlobalCleanup]
	public void GlobalCleanup()
	{
		NativeMemory.Free(m_ptr.ToPointer());
	}

	[Benchmark]
	public nint Read()
	{
		return m_ptr.Read();
	}

	[Benchmark]
	public ref nint Ref()
	{
		return ref m_ptr.Reference;
	}

	[Benchmark]
	public void Write()
	{
		m_ptr.Write(123);
	}

	[Benchmark]
	public Pointer<nint> Add()
	{
		return m_ptr.AddBytes();
	}

	[Benchmark]
	public Pointer<nint> Op_Add()
	{
		return m_ptr++;
	}

}

[RyuJitX64Job]
public class Benchmarks24
{

	private readonly Consumer   m_consumer = new Consumer();
	private          string     m_path;
	private          FileStream m_stream;

	[GlobalSetup]
	public void GlobalSetup()
	{
		RuntimeHelpers.RunClassConstructor(typeof(FileType).TypeHandle);

		m_path =
			@"C:\\Users\\Deci\\Pictures\\Art\\yande.re 1034007 ass halloween horns kaos_art nier_automata tail wings yorha_no.2_type_b.png";
		m_stream = File.OpenRead(m_path);

	}

	[GlobalCleanup]
	public void GlobalCleanup() { }

	[Benchmark]
	public FileType Test1()
	{
		return FileType.Resolve(m_stream);
	}

	[Benchmark]
	public FileType Test2()
	{
		return MagicResolver.Instance.Resolve(m_stream);
	}

}

[RyuJitX64Job]
public class Benchmarks23
{

	private readonly Consumer m_consumer = new Consumer();

	[GlobalSetup]
	public void GlobalSetup()
	{
		RuntimeHelpers.RunClassConstructor(typeof(FileType).TypeHandle);
	}

	[GlobalCleanup]
	public void GlobalCleanup() { }

	[Benchmark]
	public void Test1()
	{
		FileType.Find("image").Consume(m_consumer);
	}

}

[RyuJitX64Job]
public class Benchmarks22
{

	private Pointer<int> m_ptr;

	[GlobalSetup]
	public void GlobalSetup()
	{
		m_ptr = AllocManager.Alloc<int>(1);

	}

	[GlobalCleanup]
	public void GlobalCleanup()
	{
		AllocManager.Free(m_ptr);
	}

	[Benchmark]
	public nuint a()
	{
		return m_ptr.ElementSize;
	}

}

[RyuJitX64Job]
public class Benchmarks21
{

	private int  a, b;
	private nint fn;

	[GlobalSetup]
	public void GlobalSetup() { }

	[IterationSetup]
	public void IterationSetup()
	{
		a = 123;
		b = 321;

		fn = AtomicHelper.GetCacheExchangeFunction<int>();
	}

	[IterationCleanup]
	public void IterationCleanup()
	{
		a = 123;
		b = 321;
	}

	[Benchmark]
	public int Test1()
	{
		return Interlocked.Exchange(ref a, b);
	}

	[Benchmark]
	public unsafe int Test2()
	{
		return ((delegate*<ref int, int, int>) fn)(ref a, b);
	}

}

[RyuJitX64Job]
public class Benchmarks20
{

	private int                  i;
	private Pointer<int>         a;
	private ReadonlyPointer<int> b;

	/*

		| Method |      Mean |     Error |    StdDev | Median |
		|------- |----------:|----------:|----------:|-------:|
		|  Test1 | 0.0082 ns | 0.0130 ns | 0.0122 ns | 0.0 ns |
		|  Test2 | 0.0002 ns | 0.0008 ns | 0.0006 ns | 0.0 ns |

		// * Warnings *
		ZeroMeasurement
		  Benchmarks20.Test1: RyuJitX64 -> The method duration is indistinguishable from the empty method duration
		  Benchmarks20.Test2: RyuJitX64 -> The method duration is indistinguishable from the empty method duration
	 */

	[GlobalSetup]
	public void GlobalSetup()
	{
		i = 123;
		a = Mem.AddressOf(ref i);
		b = a;
	}

	[Benchmark]
	public int Test1()
	{
		return a.Value;
	}

	[Benchmark]
	public int Test2()
	{
		return b.Value;
	}

}

[SimpleJob]
[Config(typeof(AntiVirusFriendlyConfig))]

// [InProcess()]
public class Benchmarks19
{

	private FileStream m_stream;

	private readonly Consumer m_consumer = new Consumer();

	[GlobalSetup]
	public void GlobalSetup()
	{
		m_stream = File.OpenRead(@"C:\Users\Deci\Pictures\Art\0c4c80957134d4304538c27499d84dbe.jpeg");
		RuntimeHelpers.RunClassConstructor(typeof(FileType).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(MagicNative).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(MagicResolver).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(UrlmonResolver).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(FastResolver).TypeHandle);
	}

	/*
	 *
	 *
	 *
	 *| Method | Job        | Toolchain                | Mean       | Error    | StdDev   | Gen0   | Allocated |
	   |------- |----------- |------------------------- |-----------:|---------:|---------:|-------:|----------:|
	   | Fast   | DefaultJob | Default                  |   142.3 ns |  2.65 ns |  2.35 ns | 0.0570 |     896 B |
	   | Urlmon | DefaultJob | Default                  |   936.7 ns | 13.75 ns | 12.86 ns | 0.0458 |     728 B |
	   | Magic  | DefaultJob | Default                  | 7,212.9 ns | 81.40 ns | 76.14 ns | 0.0458 |     728 B |
	   | Fast   | Job-DFDSZU | InProcessNoEmitToolchain |   146.6 ns |  2.73 ns |  2.42 ns | 0.0570 |     896 B |
	   | Urlmon | Job-DFDSZU | InProcessNoEmitToolchain | 1,017.9 ns | 13.93 ns | 13.03 ns | 0.0458 |     728 B |
	   | Magic  | Job-DFDSZU | InProcessNoEmitToolchain | 7,208.3 ns | 54.54 ns | 51.02 ns | 0.0458 |     728 B |
	   | Fast   | Job-PTBGOE | Default                  |   144.7 ns |  2.75 ns |  2.57 ns | 0.0570 |     896 B |
	   | Urlmon | Job-PTBGOE | Default                  |   949.9 ns | 10.08 ns |  9.43 ns | 0.0458 |     728 B |
	   | Magic  | Job-PTBGOE | Default                  | 7,293.7 ns | 60.08 ns | 56.20 ns | 0.0458 |     728 B |

	   // * Hints *
	   Outliers
	     Benchmarks19.Fast: Default                            -> 1 outlier  was  removed (173.32 ns)
	     Benchmarks19.Fast: Toolchain=InProcessNoEmitToolchain -> 1 outlier  was  removed (162.49 ns)

	   // * Legends *
	     Mean      : Arithmetic mean of all measurements
	     Error     : Half of 99.9% confidence interval
	     StdDev    : Standard deviation of all measurements
	     Gen0      : GC Generation 0 collects per 1000 operations
	     Allocated : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)
	     1 ns      : 1 Nanosecond (0.000000001 sec)
	 *
	 */

	/*
	 *
	 *
	 *
	 *
		BenchmarkDotNet v0.13.6, Windows 10 (10.0.19043.2364/21H1/May2021Update)
		AMD Ryzen 7 2700X, 1 CPU, 16 logical and 8 physical cores
		.NET SDK 7.0.304
		  [Host]     : .NET 7.0.7 (7.0.723.27404), X64 RyuJIT AVX2
		  DefaultJob : .NET 7.0.7 (7.0.723.27404), X64 RyuJIT AVX2

		| Method |        Mean |    Error |   StdDev |
		|------- |------------:|---------:|---------:|
		|   Fast |    824.5 ns | 16.34 ns | 33.01 ns |
		| Urlmon |  1,905.2 ns |  9.56 ns |  8.47 ns |
		|  Magic | 16,819.5 ns | 60.01 ns | 53.20 ns |
	 *
	 */

	[Benchmark]
	public async Task<FileType> Fast()
	{
		return (await FastResolver.Instance.ResolveAsync(m_stream));
	}

	[Benchmark]
	public async Task<FileType> Urlmon()
	{
		return (await UrlmonResolver.Instance.ResolveAsync(m_stream));
	}

	[Benchmark]
	public async Task<FileType> Magic()
	{
		return (await MagicResolver.Instance.ResolveAsync(m_stream));
	}

}

public class AntiVirusFriendlyConfig : ManualConfig
{

	public AntiVirusFriendlyConfig()
	{
		AddJob(Job.Default.WithToolchain(InProcessNoEmitToolchain.Instance));
	}

}

// [InProcess()]
// [RyuJitX64Job]
public class Benchmarks30
{

	public static object[] vals => Values.vals;

	[ParamsSource(nameof(vals))]
	public object val;

	[GlobalSetup]
	public void GlobalSetup() { }

	[Benchmark]
	public bool Test1()
	{
		return RuntimeProperties.IsNull(val);
	}

	[Benchmark]
	public bool Test2()
	{
		return RuntimeProperties.IsDefault(val);
	}

	[Benchmark]
	public bool Test3()
	{
		return RuntimeProperties.IsEmpty(val);
	}

}

public class Benchmarks31
{

	static Benchmarks31()
	{
		RuntimeHelpers.RunClassConstructor(typeof(Values).TypeHandle);
	}

	public static object[] vals => Values.vals;

	[ParamsSource(nameof(vals))]
	public object val;

	[Benchmark]
	public bool Test1()
	{
		return RuntimeProperties.IsEmpty(val);
	}

}

[Config(typeof(AntiVirusFriendlyConfig))]
[SimpleJob]

// [InProcess()]
public class Benchmarks19b
{

	private FileStream m_stream;

	[GlobalSetup]
	public void GlobalSetup()
	{
		m_stream = File.OpenRead(@"C:\Users\Deci\Pictures\Art\0c4c80957134d4304538c27499d84dbe.jpeg");
		RuntimeHelpers.RunClassConstructor(typeof(FileType).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(MagicNative).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(MagicResolver).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(UrlmonResolver).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(FastResolver).TypeHandle);
		_ = FileType.All;

	}

	/*
BenchmarkDotNet v0.13.6, Windows 10 (10.0.19043.2364/21H1/May2021Update)
AMD Ryzen 7 2700X, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.304
  [Host]     : .NET 7.0.7 (7.0.723.27404), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.7 (7.0.723.27404), X64 RyuJIT AVX2

| Method |        Job |                Toolchain |     Mean |    Error |   StdDev |
|------- |----------- |------------------------- |---------:|---------:|---------:|
|   Fast | DefaultJob |                  Default | 820.5 ns | 13.88 ns | 12.99 ns |
|  Fast2 | DefaultJob |                  Default | 519.4 ns |  3.76 ns |  3.14 ns |
|   Fast | Job-MIJFFU | InProcessNoEmitToolchain | 857.1 ns | 10.94 ns |  9.13 ns |
|  Fast2 | Job-MIJFFU | InProcessNoEmitToolchain | 565.7 ns |  2.60 ns |  2.31 ns |
	 */

	[Benchmark]
	public async Task<FileType> Fast()
	{
		return (await FastResolver.Instance.ResolveAsync(m_stream));
	}

	/*[Benchmark]
	public async Task Fast2()
	{
		(await Fast2Resolver.Instance.ResolveAsync(m_stream)).Consume(m_consumer);
	}*/

}

[Config(typeof(AntiVirusFriendlyConfig))]
[SimpleJob]

// [InProcess()]
public class Benchmarks19c
{

	[GlobalSetup]
	public void GlobalSetup()
	{
		RuntimeHelpers.RunClassConstructor(typeof(FileType).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(MagicNative).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(MagicResolver).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(UrlmonResolver).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(FastResolver).TypeHandle);
		_ = FileType.All;

	}

	[Benchmark]
	[NotNull]
	public Task<UniSource> Url1()
	{
		return UniSource.GetAsync("https://i.imgur.com/QtCausw.png");
	}

	[Benchmark]
	[NotNull]
	public Task<UniSource> File1()
	{
		return UniSource.GetAsync((@"C:\Users\Deci\Pictures\Art\0c4c80957134d4304538c27499d84dbe.jpeg"));
	}

}

[RyuJitX64Job]
public class Benchmarks18
{

	public MyClass ms;

	[GlobalSetup]
	public void GlobalSetup()
	{
		ms = new MyClass();
	}

	[Benchmark]
	public MyClass test1()
	{

		return ms.Clone();
	}

}
#if EXPERIMENTAL
[RyuJitX64Job]
public class Benchmarks17
{
	private UArray<int> u1;

	private int[] u2;

	[GlobalSetup]
	public void GlobalSetup()
	{
		u1 = new(5);
		u2 = new int[5];

		for (int i = 0; i < 5; i++) {
			u1[i] = i;
			u2[i] = i;

		}
	}

	[Benchmark]
	public int Index()
	{
		return u1[0];
	}

	[Benchmark]
	public int Index2()
	{
		return u2[0];
	}
}
[RyuJitX64Job]
public class Benchmarks12
{
	[Benchmark]
	public UArray<int> alloc()
	{
		return new(10);
	}
}
#endif

[RyuJitX64Job]
public class Benchmarks16
{

	[GlobalSetup]
	public void setup()
	{
		Global.Setup();
		RuntimeHelpers.RunClassConstructor(typeof(GCHeap).TypeHandle);
	}

	[Benchmark]
	public bool test1()
	{
		return GCHeap.IsHeapPointer("foo");
	}

}

[RyuJitX64Job]
public unsafe class Benchmarks15
{

	private int[]        dest1;
	private Pointer<int> src1;
	private int          len;

	[GlobalSetup]
	public void setup()
	{
		len   = 4;
		src1  = NativeMemory.AllocZeroed((nuint) len);
		dest1 = new int[len];
	}

	[Benchmark]
	public void Copy3()
	{
		src1.Copy(dest1, 0, (int) len);
	}

}

[RyuJitX64Job]
public unsafe class Benchmarks14
{

	private void*         src, dest;
	private nuint         len;
	private Pointer<byte> src1, dest1;

	[GlobalSetup]
	public void setup()
	{
		len   = 256;
		src   = NativeMemory.AllocZeroed(len);
		dest  = NativeMemory.AllocZeroed(len);
		src1  = src;
		dest1 = dest;

	}

	[GlobalCleanup]
	public void cleanup()
	{
		NativeMemory.Free(src);
		NativeMemory.Free(dest);

	}

	[Benchmark]
	public void Copy()
	{
		Buffer.MemoryCopy(src, dest, len, len);
	}

	[Benchmark]
	public void Copy3()
	{
		src1.Copy(dest1, (int) len);
	}

	[Benchmark]
	public void Copy2()
	{
		Unsafe.CopyBlock(dest, src, (uint) len);
	}

}

[RyuJitX64Job]
public class Benchmarks13
{

	private Pointer<int> ptr;

	[GlobalSetup]
	public void setup()
	{
		ptr = AllocManager.Alloc<int>(3);
		ptr.WriteAll(new[] { 1, 2, 3 });
	}

	[Benchmark]
	public Pointer<int> AddressOfIndex()
	{
		return ptr.AddressOfIndex(3);
	}

}

public class Benchmarks9
{

	private nint h;

	[GlobalSetup]
	public void Setup()
	{
		h = Process.GetCurrentProcess().Handle;
	}

	[Benchmark]
	public LinkedList<MemoryBasicInformation> Test2()
	{
		return Native.EnumeratePages(h);
	}

}

public class Benchmarks8
{

	private object a = new { s = "foo" };

	[Benchmark]
	public bool IsAnonymous1()
	{
		return a.GetType().IsAnonymous();
	}

}

public class Benchmarks6
{

	private int a = 1, b = 1;

	[Benchmark]
	public int In()
	{
		return In_(a, b);

	}

	[Benchmark]
	public int Normal()
	{
		return N_(a, b);
	}

	[Benchmark]
	public int Ref()
	{
		return Ref_(ref a, ref b);
	}

	static int N_(int a, int b)
	{
		return a + b;
	}

	static int In_(in int a, in int b)
	{
		return a + b;
	}

	static int Ref_(ref int a, ref int b)
	{
		return a + b;
	}

}

public unsafe class BenchmarksPointer2
{

	public  int          a;
	private Pointer<int> ptr1;

	[GlobalSetup]
	public void Setup()
	{
		a = 123;

		fixed (int* p = &a) {
			ptr1 = p;
		}
	}

	[Benchmark]
	public int Pointer_Value()
	{
		return ptr1.Value;
	}

	[Benchmark]
	public int Pointer_Index()
	{
		return ptr1[0];
	}

	[Benchmark]
	public int Pointer_Ref()
	{
		return ptr1.Reference;
	}

}

public unsafe class BenchmarksPointer
{

	public  int          a;
	private Pointer<int> ptr1;
	private int*         ptr2;

	/*
		|             Method |      Mean |     Error |    StdDev |    Median |
		|------------------- |----------:|----------:|----------:|----------:|
		|      Pointer_Value | 0.0194 ns | 0.0116 ns | 0.0109 ns | 0.0156 ns |
		|        Pointer_Ref | 0.0288 ns | 0.0170 ns | 0.0159 ns | 0.0187 ns |
		| Native_Dereference | 0.3118 ns | 0.0264 ns | 0.0247 ns | 0.3061 ns |
		|       Native_Index | 0.0147 ns | 0.0132 ns | 0.0110 ns | 0.0178 ns |
		|       Marshal_Read | 1.5086 ns | 0.0359 ns | 0.0336 ns | 1.5076 ns |

		// * Warnings *
		ZeroMeasurement
		  BenchmarksPointer.Pointer_Value: Default -> The method duration is indistinguishable from the empty method duration
		  BenchmarksPointer.Native_Index: Default  -> The method duration is indistinguishable from the empty method duration
	 */

	/*
	|             Method |      Mean |     Error |    StdDev |    Median |
	|------------------- |----------:|----------:|----------:|----------:|
	|      Pointer_Value | 0.0009 ns | 0.0018 ns | 0.0015 ns | 0.0000 ns |
	|        Pointer_Ref | 0.0000 ns | 0.0001 ns | 0.0001 ns | 0.0000 ns |
	| Native_Dereference | 0.2863 ns | 0.0092 ns | 0.0082 ns | 0.2833 ns |
	|       Native_Index | 0.0013 ns | 0.0018 ns | 0.0014 ns | 0.0009 ns |
	|       Marshal_Read | 1.4553 ns | 0.0023 ns | 0.0020 ns | 1.4554 ns |

	// * Warnings *
	ZeroMeasurement
	  BenchmarksPointer.Pointer_Value: Default -> The method duration is indistinguishable from the empty method duration
	  BenchmarksPointer.Pointer_Ref: Default   -> The method duration is indistinguishable from the empty method duration
	  BenchmarksPointer.Native_Index: Default  -> The method duration is indistinguishable from the empty method duration

	// * Hints *
	Outliers
	  BenchmarksPointer.Pointer_Value: Default      -> 2 outliers were removed (1.58 ns, 1.59 ns)
	  BenchmarksPointer.Pointer_Ref: Default        -> 2 outliers were removed (1.55 ns, 1.58 ns)
	  BenchmarksPointer.Native_Dereference: Default -> 1 outlier  was  removed (1.87 ns)
	  BenchmarksPointer.Native_Index: Default       -> 3 outliers were removed (1.54 ns..1.55 ns)
	  BenchmarksPointer.Marshal_Read: Default       -> 1 outlier  was  removed (3.00 ns)
	 */

	[GlobalSetup]
	public void Setup()
	{
		a = 123;

		fixed (int* p = &a) {
			ptr2 = p;
			ptr1 = p;
		}
	}

	[Benchmark]
	public int Pointer_Value()
	{
		return ptr1.Value;
	}

	[Benchmark]
	public int Pointer_Ref()
	{
		return ptr1.Reference;
	}

	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
	[Benchmark]
	public int Native_Dereference()
	{
		return *ptr2;
	}

	[Benchmark]
	public int Native_Index()
	{
		return ptr2[0];
	}

	[Benchmark]
	public int Marshal_Read()
	{
		return System.Runtime.InteropServices.Marshal.ReadInt32((nint) ptr2);
	}

}

public unsafe class Benchmarks4
{

	private const string DLL = @"C:\Users\Deci\VSProjects\SandboxLibrary\x64\Release\SandboxLibrary.dll";
	private       nint _p;
	private       delegate* unmanaged<int, int, int> _x;

	[DllImport(DLL)]
	private static extern int add(int a, int b);

	[GlobalSetup]
	public void Setup()
	{
		_p = NativeLibrary.Load(DLL);
		_x = (delegate* unmanaged<int, int, int>) NativeLibrary.GetExport(_p, @"add");
	}

	[Benchmark]
	public int Bench()
	{
		return _x(1, 1);

	}

	[Benchmark]
	public int Bench2()
	{
		return add(1, 1);

	}

}

public class Benchmarks3
{

	[Benchmark]
	public byte[] Bench()
	{
		return SigScanner.ReadSignature("48 8B 01 A8 02 75 ? C3");

	}

}

public class Benchmarks3b
{

	[Benchmark]
	public Pointer<byte> Test1()
	{
		return m_scanner.FindSignature(m_sig);
	}
	[Benchmark]
	public Pointer<byte> Test1b()
	{
		return m_scanner.FindSignature2(m_sig);
	}

	private SigScanner m_scanner;
	private byte[]     m_sig;
	[GlobalSetup]
	public void GlobalSetup()
	{
		var process = Process.GetCurrentProcess();
		m_scanner = new SigScanner(process, process.FindModule(Global.CLR_MODULE));
		m_sig     = SigScanner.ReadSignature(EmbeddedResources.Sig_GetIL);
	}
/*
 *
 *| Method | Mean     | Error   | StdDev  | Allocated |
   |------- |---------:|--------:|--------:|----------:|
   | Test1  | 158.9 us | 0.46 us | 0.38 us |     816 B |
 *
 */

}

public class Benchmarks3a
{

	[Benchmark]
	public Pointer<byte> Test1()
	{
		return m_scanner.FindSignature("77 0 61");
	}

	private SigScanner m_scanner;

	[GlobalSetup]
	public void GlobalSetup()
	{
		var proc = Process.GetProcessesByName("notepad")[0];
		m_scanner = new SigScanner(proc, proc.MainModule);
	}

}

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

public unsafe class Benchmarks_Pointer
{

	private const int CNT = 2048;

	private Pointer<byte> m_ptr, m_ptr2;

	[GlobalSetup]
	public void GlobalSetup()
	{

		m_ptr  = NativeMemory.Alloc(CNT, sizeof(byte));
		m_ptr2 = NativeMemory.Alloc(CNT, sizeof(byte));

		var s = m_ptr.ToSpan(CNT);

		Random.Shared.NextBytes(s);
	}

	[Benchmark]
	public void Copy1()
	{
		m_ptr.Copy(m_ptr2, CNT);
	}

}

public class Benchmarks
{

	[Benchmark]
	public Symbol Bench1()
	{
		return sl.GetSymbol("g_pGCHeap");
	}

	private Win32SymbolReader sl;

	[GlobalSetup]
	public void GlobalSetup()
	{

		Global.Setup();
		sl = new Win32SymbolReader(Native.GetCurrentProcess(), @"C:\Users\Deci\Desktop\coreclr.pdb");
		sl.LoadAll();
	}

	// [IterationCleanup]
	// public void IterationCleanup()
	// {
	//
	// }

}