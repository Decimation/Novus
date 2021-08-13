using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using Novus;
using Novus.Memory;
using Novus.Runtime.Meta;
using Novus.Utilities;
using Novus.Win32;
using Novus.Win32.Wrappers;

// ReSharper disable InconsistentNaming

#pragma warning disable
namespace TestBenchmark
{
	public class Benchmarks8
	{
		private object a = new {s = "foo"};

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

	public unsafe class Benchmarks5
	{
		public  int          a;
		private Pointer<int> ptr1;
		private int*         ptr2;

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
		public int Test1()
		{
			return ptr1.Value;
		}

		[Benchmark]
		public int Test2()
		{
			return *ptr2;
		}

		[Benchmark]
		public int Test3()
		{
			return Marshal.ReadInt32((IntPtr) ptr2);
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