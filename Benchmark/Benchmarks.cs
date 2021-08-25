using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using Novus;
using Novus.Memory;
using Novus.Runtime.Meta;
using Novus.Utilities;
using Novus.Win32;
using Novus.Win32.Structures;
using Novus.Win32.Wrappers;

// ReSharper disable InconsistentNaming

#pragma warning disable
namespace TestBenchmark
{
	public class Benchmarks11
	{
		private int i;

		private Pointer<int> ptr;
		private ReadOnlyPointer<int> ptr2;

		[GlobalSetup]
		public void Setup()
		{
			unsafe {
				fixed (int* p = &i) {
					ptr  = p;
					ptr2 = p;
				}
			}
		}

		[Benchmark]
		public int Normal()
		{
			return ptr.Value;
		}

		[Benchmark]
		public int ReadOnly()
		{
			return ptr2.Value;
		}
	}

	public class Benchmarks10
	{
		private IntPtr h;

		[Benchmark]
		public List<int> Alloc1()
		{
			return GCHeap.AllocObject<List<int>>();
		}
		
	}

	public class Benchmarks9
	{
		private IntPtr h;

		[GlobalSetup]
		public void Setup()
		{
			h = Process.GetCurrentProcess().Handle;
		}


		[Benchmark]
		public LinkedList<MemoryBasicInformation> Test2()
		{
			return Mem.EnumeratePages(h);
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

	public class Benchmarks7
	{
		[Benchmark]
		public List<int> Test1()
		{
			return Activator.CreateInstance<List<int>>();
		}

		[Benchmark]
		public List<int> Test2()
		{
			return (List<int>) GCHeap.AllocObject(typeof(List<int>));
		}

		[Benchmark]
		public List<int> Test3()
		{
			return GCHeap.AllocObject<List<int>>();
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
			return System.Runtime.InteropServices.Marshal.ReadInt32((IntPtr) ptr2);
		}
	}

	public unsafe class Benchmarks4
	{
		private const string DLL = @"C:\Users\Deci\VSProjects\SandboxLibrary\x64\Release\SandboxLibrary.dll";
		private       IntPtr _p;
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

		private SymbolLoader sl;

		[GlobalSetup]
		public void GlobalSetup()
		{

			Global.Setup();
			sl = new SymbolLoader(Native.GetCurrentProcess(), @"C:\Users\Deci\Desktop\coreclr.pdb");
			sl.LoadAll();
		}

		// [IterationCleanup]
		// public void IterationCleanup()
		// {
		//
		// }
	}
}