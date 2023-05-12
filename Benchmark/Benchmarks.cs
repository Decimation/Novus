using System;
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
using MimeDetective;
using MimeDetective.Engine;
using Newtonsoft.Json.Linq;
using Novus.Imports.Attributes;
using Novus.Imports;
using System.Linq;

// ReSharper disable InconsistentNaming

#pragma warning disable
namespace TestBenchmark;

public class MyStruct
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
	public const string u1 = "https://us.rule34.xxx//images/4777/eb5d308334c52a2ecd4b0b06846454e4.jpeg?5440124";
	public const string f1 = @"C:\Users\Deci\Pictures\2b_butt.jpg";

	public const string f2 =
		@"C:\Users\Deci\Pictures\Art\yande.re 1034007 ass halloween horns kaos_art nier_automata tail wings yorha_no.2_type_b.png";
}

[RyuJitX64Job]
public class Benchmarks_M2
{
	private ContentInspector     m_i;
	private FileStream           m_s;
	private byte[]               m_m;
	private ImmutableArray<byte> m_r;
	private Consumer             m_c;

	[GlobalSetup]
	public async void GlobalSetup()
	{
		m_i = new MimeDetective.ContentInspectorBuilder()
			{
				Definitions = MimeDetective.Definitions.Default.All()
			}
			.Build();

		m_s = File.OpenRead(Values.f1);
		// var r = ImmutableArray.Create<byte>();
		m_m = new byte[256];
		await m_s.ReadAsync(m_m);
		m_r = m_m.ToImmutableArray();
		m_c = new Consumer();

	}

	/*
	 f1
|              Method |           Mean |        Error |       StdDev |
|-------------------- |---------------:|-------------:|-------------:|
|   Test_MimeDetector | 1,757,476.3 ns | 13,361.30 ns | 11,844.45 ns |
|   Test_FastResolver |       457.0 ns |      8.99 ns |      8.41 ns |
|  Test_MagicResolver |    16,491.3 ns |     35.21 ns |     29.41 ns |
| Test_UrlmonResolver |     1,887.6 ns |      5.43 ns |      4.82 ns |
	 */

	[Benchmark]
	public void Test_MimeDetector()
	{
		m_i.Inspect(m_s).Consume(m_c);

	}

	[Benchmark]
	public void Test_FastResolver()
	{
		FastResolver.Instance.Resolve(m_s).Consume(m_c);
	}

	[Benchmark]
	public void Test_MagicResolver()
	{
		MagicResolver.Instance.Resolve(m_s).Consume(m_c);
	}

	[Benchmark]
	public void Test_UrlmonResolver()
	{
		UrlmonResolver.Instance.Resolve(m_s).Consume(m_c);
	}
}

[RyuJitX64Job]
public class Benchmarks_M
{
	private ContentInspector     m_i;
	private FileStream           m_s;
	private byte[]               m_m;
	private ImmutableArray<byte> m_r;

	[GlobalSetup]
	public async void GlobalSetup()
	{
		m_i = new MimeDetective.ContentInspectorBuilder()
			{
				Definitions = MimeDetective.Definitions.Default.All()
			}
			.Build();

		m_s = File.OpenRead(Values.f1);
		// var r = ImmutableArray.Create<byte>();
		m_m = new byte[256];
		await m_s.ReadAsync(m_m);
		m_r = m_m.ToImmutableArray();

	}

	/*
U, f
| Method |        Mean |       Error |      StdDev |      Median |
|------- |------------:|------------:|------------:|------------:|
|  Test2 |    334.7 us |     6.39 us |     5.66 us |    333.7 us |
| Test1a | 37,035.7 us | 1,886.09 us | 5,319.76 us | 35,040.1 us |
| Test1b | 39,685.8 us |   789.27 us |   616.21 us | 39,635.9 us |
	 */

	[Benchmark]
	public async Task<ImmutableArray<DefinitionMatch>> Test2()
	{

		var mt1 = m_i.Inspect(m_r);

		return mt1;
	}

	[Benchmark]
	public async Task<ImmutableArray<DefinitionMatch>> Test1a()
	{

		var mt1 = m_i.Inspect(m_m);

		return mt1;
	}

	[Benchmark]
	public async Task<ImmutableArray<DefinitionMatch>> Test1b()
	{
		var mt1 = m_i.Inspect(m_r);

		return mt1;
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
	public void Urlmon()
	{
		urlmon.Resolve(ss).Consume(m_consumer);
	}

	[Benchmark]
	public void Magic()
	{
		magic.Resolve(ss).Consume(m_consumer);
	}

	[Benchmark]
	public void Fast()
	{
		fast.Resolve(ss).Consume(m_consumer);
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
	public void Test1()
	{
		FileType.Resolve(m_stream).Consume(m_consumer);
	}

	[Benchmark]
	public void Test2()
	{
		MagicResolver.Instance.Resolve(m_stream).Consume(m_consumer);
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

[RyuJitX64Job]
[InProcess()]
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

	[Benchmark]
	public async void Fast()
	{
		(await FastResolver.Instance.ResolveAsync(m_stream)).Consume(m_consumer);
	}

	[Benchmark]
	public async void Urlmon()
	{
		(await UrlmonResolver.Instance.ResolveAsync(m_stream)).Consume(m_consumer);
	}

	[Benchmark]
	public async void Magic()
	{
		(await MagicResolver.Instance.ResolveAsync(m_stream)).Consume(m_consumer);
	}
}

[RyuJitX64Job]
public class Benchmarks18
{
	public MyStruct ms;

	[GlobalSetup]
	public void GlobalSetup()
	{
		ms = new MyStruct();
	}

	[Benchmark]
	public MyStruct test1()
	{

		return ms.Clone();
	}
}

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

[RyuJitX64Job]
public class Benchmarks12
{
	[Benchmark]
	public UArray<int> alloc()
	{
		return new(10);
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