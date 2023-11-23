using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Kantan.Text;
using Novus;
using Novus.FileTypes;
using Novus.FileTypes.Impl;
using Novus.Imports;
using Novus.Imports.Attributes;
using Novus.Memory;
using Novus.Memory.Allocation;
using Novus.Numerics;
using Novus.OS;
using Novus.Runtime;
using Novus.Runtime.Meta;
using Novus.Runtime.VM.IL;
using Novus.Streams;
using Novus.Utilities;
using Novus.Win32;
using Novus.Win32.Structures.Kernel32;
using NUnit.Framework;
using TestTypes;
using NUnit.Framework.Internal;
using static Novus.Utilities.ReflectionOperatorHelpers;

// ReSharper disable StringLiteralTypo

// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
#pragma warning disable 0649, IDE0044, CA1822, IDE1006, CA2211, IDE0052, CS1998, CS0612
#pragma warning disable SYSLIB0014

namespace UnitTest;

[SetUpFixture]
public class SetupTrace
{

	[OneTimeSetUp]
	public void StartTest()
	{
		Trace.Listeners.Add(new ConsoleTraceListener());
	}

	[OneTimeTearDown]
	public void EndTest()
	{
		Trace.Flush();
	}

}

[TestFixture]
[Parallelizable]
public class Tests_FileTypes1
{

	public static object[] _rg =
	{
		// new[] { @"http://www.zerochan.net/2750747", null },
		new Object[] { @"https://i.imgur.com/QtCausw.png", FileType.Find("jpeg").First() },
		// new[] { @"https://kemono.party/patreon/user/3332300/post/65227512", null },
		// @"https://i.pximg.net/img-master/img/2022/05/01/19/44/39/98022741_p0_master1200.jpg",
		new Object[] { "C:\\Users\\Deci\\Pictures\\Test Images\\Test1.jpg", FileType.Find("jpeg").First() },
		new Object[] { "http://static.zerochan.net/atago.(azur.lane).full.2750747.png", FileType.Find("png").First() }
	};

	[Test]
	[TestCaseSource(nameof(_rg))]
	public async Task Test4(string s, FileType type)
	{
		var t  = await UniSource.TryGetAsync(s, IFileTypeResolver.Default);
		var tt = t.FileType;
		// var tt = await IFileTypeResolver.Default.ResolveAsync(t.Stream);
		Assert.AreEqual(type, tt);
		// Assert.True(tt.Any(x => x.MediaType == type));
		// Assert.Contains(new FileType { MediaType = type }, types.ToArray());
	}

	[Test]
	[TestCaseSource(nameof(_rg))]
	public async Task Test1(string s, FileType type)
	{
		var t = await UniSource.TryGetAsync(s, UrlmonResolver.Instance);
		// var tt = await (IFileTypeResolver.Default.ResolveAsync(t.Stream));
		var tt = t.FileType;
		Assert.AreEqual(type, tt);

	}

	[Test]
	[TestCaseSource(nameof(_rg))]
	public async Task Test2(string s, FileType type)
	{
		var t = await UniSource.TryGetAsync(s, FastResolver.Instance);
		// var tt = await MagicResolver.Instance.ResolveAsync(t.Stream);
		// Assert.Contains(new FileType { MediaType = type }, tt.ToList());
		var tt = t.FileType;
		// Assert.True(tt.Any(x => x.MediaType == type));
		Assert.AreEqual(type, tt);

	}

	[Test]
	[TestCaseSource(nameof(_rg))]
	public async Task Test3(string s, FileType type)
	{
		var t = await UniSource.TryGetAsync(s, MagicResolver.Instance);
		// var tt = await FastResolver.Instance.ResolveAsync(t.Stream);
		var tt = t.FileType;
		// Assert.True(tt.Any(x => x.MediaType == type));
		Assert.AreEqual(type, tt);

		// Assert.Contains(new FileType { MediaType = type }, tt.ToList());
	}

}

[TestFixture]
[Parallelizable]
public class Tests_UniSource
{

	private static object[] _rg = Tests_FileTypes1._rg;

	[Test]
	[TestCaseSource(nameof(_rg))]
	public async Task Test3(string s, FileType type)
	{
		var t = await UniSource.GetAsync(s, IFileTypeResolver.Default, FileType.Image);
		// var tt = await FastResolver.Instance.ResolveAsync(t.Stream);
		var tt = t.FileType;
		// Assert.True(tt.Any(x => x.MediaType == type));
		Assert.AreEqual(type, tt);

		// Assert.Contains(new FileType { MediaType = type }, tt.ToList());
	}

	[Test]
	[TestCaseSource(nameof(_rg))]
	public async Task Test1(string s, FileType type)
	{
		var t = await UniSource.GetAsync(s, IFileTypeResolver.Default, FileType.Image);
		var f = await t.TryDownloadAsync();
		TestContext.WriteLine($"{f}");

	}

	[Test]
	public async Task Test1b()
	{
		const string png = @"C:\Users\Deci\Pictures\1mcvbqv39yta1.png";
		var          str = File.OpenRead(png);
		var          t   = await UniSource.GetAsync(str, IFileTypeResolver.Default, FileType.Image);
		var          f   = await t.TryDownloadAsync();
		TestContext.WriteLine($"{f}");

	}

	[Test]
	public async Task Test2()
	{
		//------------------------------------------------------------
		//-----------       Created with 010 Editor        -----------
		//------         www.sweetscape.com/010editor/          ------
		//
		// File    : C:\Users\Deci\Pictures\Epic anime\__sashou_mihiro_original_drawn_by_infinote__ec1ebb276d934ba6ce9d03a08d02f7d9.png
		// Address : 0 (0x0)
		// Size    : 256 (0x100)
		//------------------------------------------------------------
		byte[] x =
		{
			0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
			0x00, 0x00, 0x0A, 0x72, 0x00, 0x00, 0x0E, 0x97, 0x08, 0x02, 0x00, 0x00, 0x00, 0xF4, 0xA6, 0xBB,
			0xC2, 0x00, 0x00, 0x00, 0x19, 0x74, 0x45, 0x58, 0x74, 0x53, 0x6F, 0x66, 0x74, 0x77, 0x61, 0x72,
			0x65, 0x00, 0x41, 0x64, 0x6F, 0x62, 0x65, 0x20, 0x49, 0x6D, 0x61, 0x67, 0x65, 0x52, 0x65, 0x61,
			0x64, 0x79, 0x71, 0xC9, 0x65, 0x3C, 0x00, 0x00, 0x03, 0x84, 0x69, 0x54, 0x58, 0x74, 0x58, 0x4D,
			0x4C, 0x3A, 0x63, 0x6F, 0x6D, 0x2E, 0x61, 0x64, 0x6F, 0x62, 0x65, 0x2E, 0x78, 0x6D, 0x70, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x3C, 0x3F, 0x78, 0x70, 0x61, 0x63, 0x6B, 0x65, 0x74, 0x20, 0x62, 0x65,
			0x67, 0x69, 0x6E, 0x3D, 0x22, 0xEF, 0xBB, 0xBF, 0x22, 0x20, 0x69, 0x64, 0x3D, 0x22, 0x57, 0x35,
			0x4D, 0x30, 0x4D, 0x70, 0x43, 0x65, 0x68, 0x69, 0x48, 0x7A, 0x72, 0x65, 0x53, 0x7A, 0x4E, 0x54,
			0x63, 0x7A, 0x6B, 0x63, 0x39, 0x64, 0x22, 0x3F, 0x3E, 0x20, 0x3C, 0x78, 0x3A, 0x78, 0x6D, 0x70,
			0x6D, 0x65, 0x74, 0x61, 0x20, 0x78, 0x6D, 0x6C, 0x6E, 0x73, 0x3A, 0x78, 0x3D, 0x22, 0x61, 0x64,
			0x6F, 0x62, 0x65, 0x3A, 0x6E, 0x73, 0x3A, 0x6D, 0x65, 0x74, 0x61, 0x2F, 0x22, 0x20, 0x78, 0x3A,
			0x78, 0x6D, 0x70, 0x74, 0x6B, 0x3D, 0x22, 0x41, 0x64, 0x6F, 0x62, 0x65, 0x20, 0x58, 0x4D, 0x50,
			0x20, 0x43, 0x6F, 0x72, 0x65, 0x20, 0x36, 0x2E, 0x30, 0x2D, 0x63, 0x30, 0x30, 0x32, 0x20, 0x31,
			0x31, 0x36, 0x2E, 0x31, 0x36, 0x34, 0x36, 0x35, 0x35, 0x2C, 0x20, 0x32, 0x30, 0x32, 0x31, 0x2F,
			0x30, 0x31, 0x2F, 0x32, 0x36, 0x2D, 0x31, 0x35, 0x3A, 0x34, 0x31, 0x3A, 0x32, 0x30, 0x20, 0x20
		};

		var t = await UniSource.GetAsync(new MemoryStream(x), IFileTypeResolver.Default, FileType.Image);
		// var tt = await FastResolver.Instance.ResolveAsync(t.Stream);
		var tt = t.FileType;
		// Assert.True(tt.Any(x => x.MediaType == type));
		Assert.AreEqual(FileType.Find("png").First(), tt);

		// Assert.Contains(new FileType { MediaType = type }, tt.ToList());
	}

}

[TestFixture]
[Parallelizable]
public class Tests_FileResolvers
{

	[Test]
	[TestCase(@"C:\Users\Deci\Pictures\NSFW\17EA29A6-8966-4801-A508-AC89FABE714D.png")]
	public async Task Test1(string s)
	{
		var stream = File.OpenRead(s);
		var task   = await IFileTypeResolver.Default.ResolveAsync(stream);
		Assert.True(task.Type == FileType.MT_IMAGE);
	}

	/*[Test]
	[TestCase(@"https://kemono.party/patreon/user/587897/post/64451923","image/png")]
	public async Task Test2(string s,string s2)
	{
	    var result = await HttpResourceSniffer.Default.ScanAsync(s);

	    foreach (HttpResourceHandle httpResource in result) {
	        httpResource.Resolve();
	    }

	    Assert.True(result.Any(x=>x.ResolvedTypes.Select(x=>x.Type).Contains(s2)));
	}*/

}

[TestFixture]
[Parallelizable]
public class Tests_MediaTypes
{

	[Test]
	[TestCase("http://s1.zerochan.net/atago.(azur.lane).600.2750747.jpg")]
	// [TestCase("https://www.zerochan.net/2750747", "http://static.zerochan.net/atago.(azur.lane).full.2750747.png")]
	// [TestCase(@"C:\Users\Deci\Pictures\NSFW\17EA29A6-8966-4801-A508-AC89FABE714D.png", true, false)]
	// [TestCase("http://s1.zerochan.net/atago.(azur.lane).600.2750747.jpg", false, true)]
	public async Task Test1(string s)
	{
		// var binaryUris = MediaSniffer.Scan(u, new HttpMediaResourceFilter());
		// Assert.True(binaryUris.Select(x => x.Value.ToString()).ToList().Contains(s));
		/*var h = new HttpClient();
		var r = await h.GetAsync(s);
		var t = await h.GetStreamAsync(s);*/
		var r = await s.AllowAnyHttpStatus().GetAsync();

		if (!r.ResponseMessage.IsSuccessStatusCode) {
			Assert.Inconclusive();
		}

		// TestContext.WriteLine($"{r.ResponseMessage.IsSuccessStatusCode}");
		var t  = await r.GetStreamAsync();
		var ft = await IFileTypeResolver.Default.ResolveAsync(t);
		Assert.True(ft.Type == FileType.MT_IMAGE);
	}

}

[TestFixture]
[Parallelizable]
public unsafe class Tests_DynamicLibrary
{

	[Test]
	public void Test1()
	{
		var dynamicLibrary = new DynamicLibrary(Native.KERNEL32_DLL);

		var ff = dynamicLibrary.GetFunction<nint>(nameof(Native.GetStdHandle),
		                                          CallingConvention.Winapi, CharSet.Auto, typeof(StandardHandle));

		Console.WriteLine(Native.GetStdHandle((StandardHandle) 1));

		var actual = ff.Call(StandardHandle.STD_OUTPUT_HANDLE);

		var expected = Native.GetStdHandle(StandardHandle.STD_OUTPUT_HANDLE);

		var fn = (delegate* unmanaged<StandardHandle, nint>) ff.Method.MethodHandle.GetFunctionPointer().ToPointer();

		var actual2 = fn(StandardHandle.STD_OUTPUT_HANDLE);

		Assert.AreEqual(expected, actual);
		Assert.AreEqual(expected, actual2);
	}

}

[TestFixture]
public class Tests_GCHeap
{

	[Test]
	public unsafe void GCTest()
	{
		var s   = "bar";
		var ptr = Mem.AddressOfHeap(s).ToPointer();
		Assert.True(GCHeap.IsHeapPointer(ptr));

		var p = new Point();
		Assert.False(GCHeap.IsHeapPointer(&p));

		Assert.True(GCHeap.IsHeapPointer(s));

	}

	[Test]
	public void Alloc()
	{
		var o = GCHeap.AllocObject<List<int>>();

		o.Add(1);

		Assert.True(o.Count == 1);
		Assert.True(GCHeap.IsHeapPointer(o));

	}

}

[TestFixture]
public class Tests_Atomic
{

	[Test]
	public void Test1()
	{
		const int i  = 1;
		const int i1 = 0;

		int a = i, b = i1;

		Assert.AreEqual(Interlocked.Exchange(ref a, b), i);
		Assert.AreEqual(AtomicHelper.Exchange(ref a, b), b);

	}

}

#if EXPERIMENTAL
[TestFixture]
[Parallelizable]
public class Tests_UArray
{
	[Test]
	public void Test1()
	{
		var u = new UArray<int>(6);
		var rg = new[] { 1, 2, 3, 4, 5, 6 };
		u.CopyFrom(rg);
		Assert.True(u.SequenceEqual(rg));
		u.Dispose();
		Assert.True(!u.IsAllocated);

	}
}
#endif

[TestFixture]
[Parallelizable]
public unsafe class Tests_NativeUtilities
{

	[Test]
	public void NativeLibTest()
	{
		const nuint i = 256;

		var a = NativeMemory.Alloc(i);
		Assert.AreEqual(Mem._msize(a), i);
	}

}

[TestFixture]
[Parallelizable]
public unsafe class Tests_Pointer
{

	[Test]
	public void Test5()
	{
		Span<int> s = stackalloc int[4] { 1, 2, 3, 4 };
		var       p = (Pointer<int>) s;
		Assert.AreEqual(s[0], p[0]);
		p++;
		Assert.AreEqual(s[1], p.Value);

		var rg2 = new[] { "foo", "bar" };
		var rg  = new[] { 1, 2, 3, 4 };

		var p2       = Mem.AddressOfHeap(rg, OffsetOptions.ArrayData);
		var asMemory = rg.AsMemory();
		var pin      = asMemory.Pin();

		Assert.True(pin.Pointer == p2.ToPointer());

		var p3 = Mem.AddressOfHeap(rg2, OffsetOptions.ArrayData).Cast<string>();
		Assert.True(p3.Value == rg2[0]);
	}

	[Test]
	public void Test3()
	{
		Pointer<int> p1 = stackalloc int[4];
		Pointer<int> p2 = stackalloc int[4];

		var rg2 = new[] { 1, 2, 3, 4 };
		p1.WriteAll(rg2);
		p1.Copy(p2, rg2.Length);

		for (int i = 0; i < rg2.Length; i++) {
			Assert.True(p1[i] == p2[i]);
		}

		p2.Clear(rg2.Length);
		p1.Copy(p2, 1, rg2.Length - 1);
		var rg1 = new[] { 2, 3, 4 };

		for (int i = 1; i < rg1.Length - 1; i++) {
			Assert.True(p2[i] == p1[i + 1]);
		}

	}

	[Test]
	public void Test4()
	{
		Pointer<int> p1  = stackalloc int[4];
		var          rg2 = new[] { 1, 2, 3, 4 };
		var          rg4 = new int[4];

		p1.WriteAll(rg2);
		p1.Copy(rg4);
		Assert.True(rg2.SequenceEqual(rg4));
		var rg3 = p1.ToArray(1, rg4.Length - 1);
		Assert.True(rg3.SequenceEqual(new[] { 2, 3, 4 }));
	}

	[Test]
	public void Test2()
	{
		Pointer<int> p1  = stackalloc int[4];
		var          rg2 = new[] { 1, 2, 3, 4 };
		p1.WriteAll(rg2);
		var rg1 = p1.ToArray(4);
		Assert.True(rg2.SequenceEqual(rg1));
		var rg3 = p1.ToArray(1, rg2.Length - 1);
		Assert.True(rg3.SequenceEqual(new[] { 2, 3, 4 }));
	}

	[Test]
	[TestCase(new[] { 1, 2, 3 })]
	public void Test6(int[] r)
	{
		Span<int> s = stackalloc int[r.Length];
		r.CopyTo(s);

		var p = s.ToPointer();

		for (int i = 0; i < r.Length; i++) {
			Assert.AreEqual(s[i], p[i]);
		}

		var array = r.Reverse().ToArray();

		for (int i = 0; i < r.Length; i++) {
			p[i] = array[i];
		}

		Assert.True(s.ToArray().SequenceEqual(p.ToArray(r.Length)));
		Assert.True(s.ToArray().SequenceEqual(array));

	}

	[Test]
	public void Test7()
	{
		Pointer<int> a = stackalloc int[1];
		Pointer<int> b = stackalloc int[1];

		var a_cpy = a;
		var b_cpy = b;

		a++;

		Assert.AreEqual(a_cpy, a - 1);

		a--;
		Assert.AreEqual(a_cpy, a);

	}

	[Test]
	public void Test1()
	{
		int i = 256;

		var ptr1 = Mem.AddressOf(ref i);

		Assert.AreEqual(ptr1.Value, i);
		Assert.AreEqual(ptr1.Reference, i);

		var          rg   = new[] { 1, 2, 3 };
		var          mem  = rg.AsMemory();
		var          pin  = mem.Pin();
		Pointer<int> ptr2 = (Pointer<int>) Mem.AddressOfHeap(rg, OffsetOptions.ArrayData);
		int*         pinp = (int*) pin.Pointer;
		Assert.AreEqual(ptr2.Address, (nint) pinp);

		for (int j = 0; j < mem.Length; j++) {
			var p2 = &pinp[j];
			var p3 = Marshal.UnsafeAddrOfPinnedArrayElement(rg, j);
			Assert.True(ptr2.Address == p3);
			Assert.True(*p2 == ptr2.Value);
			Assert.True(p2 == ptr2++);
		}

	}

}

[TestFixture]
[Parallelizable]
public unsafe class Tests_Resources
{

	private const string s =
		@"C:\Users\Deci\VSProjects\SandboxLibrary\x64\Release\SandboxLibrary.dll";

	private const string s2 = "SandboxLibrary.dll";

	[ImportUnmanaged(s2, ImportType.Signature, "89 54 24 10 89 4C 24 08")]
	private static delegate* unmanaged<int, int, int> doSomething;

	[Test]
	public void Test()
	{

		using var r = RuntimeResource.LoadModule(s);
		r.LoadImports(typeof(Tests_Resources));

		int c = doSomething(1, 1);

		Assert.AreEqual(c, 2);

	}

}

[TestFixture]
[Parallelizable]
public unsafe class Tests_Resources2
{

	[Test]
	public void Test()
	{

		using var r = new RuntimeResource(Process.GetCurrentProcess(), "coreclr.dll");
		Assert.Null(r.Symbols.Value);
		Assert.AreEqual(r.FindImport(new ImportClrAttribute("g_pGCHeap", ImportType.Symbol)), Mem.Nullptr);

	}

}

[TestFixture]
[Parallelizable]
public class Tests_Magic
{

	[Test]
	public void Test1() { }

}

[TestFixture]
[Parallelizable]
public class Tests_Native
{

	[Test]
	[TestCase(@"C:\Symbols\coreclr.pdb")]
	public void SymbolsTest(string sss)
	{
		var m = new Win32SymbolReader(sss);

		if (!File.Exists(sss)) {
			Assert.Inconclusive();
		}

		var s = m.GetSymbol("g_pGCHeap");

		Assert.NotNull(s);
	}

	[Test]
	[TestCase(@"C:\Symbols\charmap.exe")]
	public void SymbolsTest2(string a)
	{
		var d = Win32SymbolReader.SymchkSymbolFileAsync(a);
		TestContext.WriteLine(d);

	}

	[Test]
	[TestCase('\u200b', "Zero Width Space")]
	public void UnicodeNamesTest(ushort id, string s)
	{
		Assert.AreEqual(Native.GetUnicodeName(id), s);
	}

}

[TestFixture]
public class Tests_RT
{

	[Test]
	public void Test1()
	{
		var o = (MyClass2) GCHeap.AllocUninitializedObject(typeof(MyClass2));
		Assert.True(GCHeap.IsHeapPointer(o));

		var o2 = (MyStruct) GCHeap.AllocUninitializedObject(typeof(MyStruct));
		Assert.True(GCHeap.IsHeapPointer(Mem.AddressOfData(ref o2)));
		// Assert.True(RuntimeProperties.IsBoxed(o2));

	}

	[Test]
	public void Test2()
	{
		object a = 1;
		Assert.True(RuntimeProperties.IsBoxed(a));
	}
}

[TestFixture]
public class Tests_ReflectionHelper
{

	[Test]
	public void Test1()
	{
		Assert.True(typeof(Implement1).ImplementsInterface(typeof(IInterface)));
		Assert.True(typeof(Subclass1).ExtendsType(typeof(Superclass1)));
	}

	[Test]
	public void Test2()
	{
		Assert.True(typeof(IInterface).GetAllInAssembly(InheritanceProperties.Interface).Contains(typeof(Implement1)));
		Assert.True(typeof(Superclass1).GetAllInAssembly(InheritanceProperties.Subclass).Contains(typeof(Subclass1)));
	}

	[Test]
	[TestCase(typeof(int), true, true, false)]
	[TestCase(typeof(float), true, false, true)]
	[TestCase(typeof(Half), true, false, true)]
	[TestCase(typeof(Int128), true, true, false)]
	public void Test3(Type t, bool isNum, bool isInt, bool isReal)
	{
		Assert.AreEqual(isNum, t.IsNumeric());
		Assert.AreEqual(isInt, t.IsInteger());
		Assert.AreEqual(isReal, t.IsReal());
	}

	[StructLayout(LayoutKind.Explicit, Size = 8)]
	struct substrate1 { }

	[Test]
	[TestCase(typeof(int), true)]
	[TestCase(typeof(uint), false)]
	[TestCase(typeof(Int128), true)]
	[TestCase(typeof(UInt128), false)]
	[TestCase(typeof(float), true)]
	public void Test4(Type t, bool s)
	{
		Assert.AreEqual(s, t.IsSigned());
	}

	[Test]
	[TestCase(typeof(substrate1), true)]
	[TestCase(typeof(Pointer<>), true)]
	[TestCase(typeof(Clazz), false)]
	public void Test5(Type t, bool b)
	{
		Assert.AreEqual(b, t.CanBePointerSurrogate());
	}

	[Test]
	public void Test4()
	{
		Assert.AreEqual(fieldof(() => new Subclass1().i),
		                typeof(Subclass1).GetRuntimeField(nameof(Subclass1.i)));
	}

	[Test]
	public void Test6()
	{
		const string foo = "foo";
		var          a   = new { s = foo, a = 321 };
		Assert.True(a.GetType().IsAnonymous());

	}

	[Test]
	public void Test7()
	{
		var a = new Clazz();
		var b = new Struct();

		var f = a.GetNullMembers();
		Assert.AreEqual(3, f.Length);
		Assert.AreEqual(1, b.GetNullMembers().Length);
	}

	[Test]
	public void Test8()
	{
		var a = new Clazz();
		var b = new Struct();
		var (s, g) = ReflectionHelper.GetAccessor(() => a.a);
		s(123);
		Assert.AreEqual(123, a.a);
		(s, g) = ReflectionHelper.GetAccessor(() => b.a);
		s(321);
		Assert.AreEqual(321, b.a);

	}

	[Test]
	[TestCase(typeof(string))]
	[TestCase(typeof(Clazz))]
	public void Test9(Type t)
	{
		Assert.True(t.GetRuntimeFields().SequenceEqual(t.GetAllFields()));
		Assert.True(t.GetRuntimeMethods().SequenceEqual(t.GetAllMethods()));

	}

}

[TestFixture]
public class Tests_Metadata
{

	[Test]
	[TestCase(typeof(string), ("_firstChar"))]
	public void FieldTest(Type t, string n)
	{
		var f  = t.GetAnyField(n);
		var mf = f.AsMetaField();

		Assert.AreEqual(mf.Info, f);
		Assert.AreEqual(mf.Token, f.MetadataToken);
		Assert.AreEqual(mf.Attributes, f.Attributes);
		Assert.AreEqual(mf.IsStatic, f.IsStatic);

	}

	[Test]
	public void FieldTest2()
	{
		Clazz c = new();

		var p = Mem.AddressOfField(c, nameof(Clazz.prop));

		Assert.True(!p.IsNull);

		p.Value = 123;

		Assert.AreEqual(c.prop, p.Value);

	}

	[Test]
	public void FieldTest4()
	{
		Clazz c = new();
		var   m = c.GetType().GetAnyResolvedField(nameof(Clazz.prop));
		Assert.NotNull(m);

	}

	[Test]
	public void FieldTest3()
	{
		var p2 = Mem.AddressOfField<int>(typeof(Clazz), nameof(Clazz.sprop));

		Assert.True(!p2.IsNull);

		p2.Value = 123;

		Assert.AreEqual(Clazz.sprop, p2.Value);

	}

	[Test]
	public void TypeTest2()
	{
		var t  = typeof(string);
		var mt = t.AsMetaType();

		Assert.AreEqual(mt.RuntimeType, t);
		Assert.AreEqual(mt.Token, t.MetadataToken);
		Assert.AreEqual(mt.Attributes, t.Attributes);
		Assert.True(mt.HasComponentSize);
		Assert.True(mt.IsString);
		Assert.True(mt.IsStringOrArray);
		Assert.AreEqual(mt.ComponentSize, sizeof(char));

		TestUtil.AssertAll(!mt.IsInteger,
		                   !mt.IsUnmanaged,
		                   !mt.IsNumeric,
		                   !mt.IsReal,
		                   !mt.IsAnyPointer,
		                   !mt.IsArray);

		var mti = typeof(int).AsMetaType();

		TestUtil.AssertAll(mti.IsInteger,
		                   !mti.IsReal,
		                   mti.IsNumeric);

		var mtf = typeof(float).AsMetaType();

		TestUtil.AssertAll(!mtf.IsInteger,
		                   mtf.IsReal,
		                   mtf.IsNumeric);

		var rg1 = new int[1];

		var mtrg1 = rg1.GetType().AsMetaType();
		Assert.AreEqual(rg1.Rank, mtrg1.ArrayRank);
		Assert.True(mtrg1.HasComponentSize);
		Assert.AreEqual(mtrg1.ComponentSize, sizeof(int));
		Assert.AreEqual(rg1.GetType().GetElementType(), mtrg1.ElementTypeHandle.RuntimeType);

		var rg2 = new int[1, 1];

		var mtrg2 = rg2.GetType().AsMetaType();
		Assert.AreEqual(rg2.Rank, mtrg2.ArrayRank);

		Assert.AreEqual(rg2.GetType().GetElementType(), mtrg2.ElementTypeHandle.RuntimeType);

		unsafe {
			fixed (int* i = rg1) {
				Assert.AreEqual((ulong) i, Mem.AddressOfHeap(rg1, OffsetOptions.ArrayData).ToUInt64());
			}
		}
	}

	[Test]
	[TestCase(typeof(string))]
	[TestCase(typeof(object))]
	[TestCase(typeof(object[]))]
	public void TypeTest(Type t)
	{
		var mt = t.AsMetaType();

		Assert.AreEqual(mt.RuntimeType, t);
		Assert.AreEqual(mt.Token, t.MetadataToken);
		Assert.AreEqual(mt.Attributes, t.Attributes);

		var b1 = typeof(RuntimeHelpers).GetMethod("IsReferenceOrContainsReferences").CallGeneric(t, null);

		Assert.AreEqual(mt.IsReferenceOrContainsReferences, b1);
	}

	[Test]
	public void MethodTest()
	{
		var m = typeof(Clazz).GetAnyMethod(nameof(Clazz.SayHi)).AsMetaMethod();

		Assert.False(m.IsPointingToNativeCode);

		m.Prepare();

		Assert.True(m.IsPointingToNativeCode);

		m.Reset();

		Assert.False(m.IsPointingToNativeCode);
	}

	[Test]
	public void ArrayTest()
	{
		var rg = new int[0, 0];
		var t  = rg.GetMetaType();

		Assert.AreEqual(t.ArrayRank, 2);
		Assert.AreEqual(t.ArrayRank, rg.Rank);
	}

	[Test]
	public void StringTest()
	{
		string s  = "foo";
		var    mt = s.GetType().AsMetaType();

		Assert.AreEqual(Mem.HeapSizeOf(s), 28);
		Assert.AreEqual(mt.BaseSize, 22); // NOT 20; SOS is wrong
		Assert.AreEqual(mt.ComponentSize, 2);

		Assert.AreEqual(mt.InstanceFieldsCount, 2);
		Assert.AreEqual(mt.StaticFieldsCount, 1);
		Assert.AreEqual(mt.InstanceFieldsSize, sizeof(char) + sizeof(int));
	}

	[Test]
	public void StaticTest()
	{
		static_clazz.i = 1;
		var p = (Pointer<int>) typeof(static_clazz).GetAnyField(nameof(static_clazz.i)).AsMetaField().StaticAddress;

		Assert.AreEqual(p.Value, 1);

		p.Value = 25;
		Assert.AreEqual(p.Value, 25);

	}

	[Test]
	[TestCase(nameof(SayHi2), true)]
	[TestCase(nameof(SayHi), false)]
	[TestCase(nameof(Tiny), null)]
	public void ILTest(string name, bool? init)
	{
		var m = typeof(Tests_Metadata).GetAnyMethod(name);

		var il = m.AsMetaMethod().ILHeader;
		var mb = m.GetMethodBody();

		Assert.AreEqual(mb.MaxStackSize, il.MaxStackSize);
		Assert.AreEqual(mb.LocalSignatureMetadataToken, il.LocalVarSigToken);
		Assert.True(mb.GetILAsByteArray().SequenceEqual(il.CodeIL));

		if (il.IsFat && init.HasValue) {
			Assert.AreEqual(il.Flags.HasFlag(CorILMethodFlags.InitLocals), init.Value);

		}

	}

	private static void Tiny() { }

	private static void SayHi2()
	{
		string s = "";
		Console.WriteLine(s);
	}

	[SkipLocalsInit]
	private static void SayHi()
	{
		string s = "";
		Console.WriteLine(s);
	}

}

[TestFixture()]
public class Tests_Runtime
{

	[SetUp]
	public void Setup()
	{
		TestExecutionContext.CurrentContext.IsSingleThreaded = false;
		TestExecutionContext.CurrentContext.ParallelScope    = ParallelScope.Fixtures;
	}

	[Test]
	public void PinnableBlittableTest()
	{
		Assert.True(RuntimeProperties.IsPinnable("g"));
		Assert.False(RuntimeProperties.IsBlittable("g"));

		Assert.True(RuntimeProperties.IsPinnable(new[] { 1, 2, 3 }));
		Assert.False(RuntimeProperties.IsBlittable(new[] { 1, 2, 3 }));
	}

	[Test]
	public void BoxedTest()
	{
		Assert.True(RuntimeProperties.IsBoxed((object) 1));
	}

	[Test]
	public void BlankTest()
	{
		Assert.True(RuntimeProperties.IsEmpty(Array.Empty<int>()));
	}

	[Test]
	public void PinnableTest()
	{
		string s = "foo";
		Assert.True(RuntimeProperties.IsPinnable(s));

		string[] rg = { "foo", "bar" };
		Assert.False(RuntimeProperties.IsPinnable(rg));

		var rg2 = new[] { 1, 2, 3 };
		Assert.True(RuntimeProperties.IsPinnable(rg2));

		Assert.True(RuntimeProperties.IsPinnable(1));

		Assert.False(RuntimeProperties.IsPinnable(new List<int> { 1, 2, 3 }));
	}

	[Test]
	public void DefaultTest()
	{
		string s = "foo";
		Assert.False(RuntimeProperties.IsDefault(s));

		string s2 = null;
		Assert.True(RuntimeProperties.IsDefault(s2));

		Assert.True(RuntimeProperties.IsDefault(default(int)));
	}

	[Test]
	[TestCaseSource(nameof(_rg0))]
	public void NullMemTest(object o, bool b)
	{
		Assert.AreEqual(b, RuntimeProperties.IsNull(o));
	}

	private static readonly object[] _rg0 =
	{
		new object[] { default(int), true },
		new object[] { null, true },
		new object[] { 1, false },

	};

	private static readonly object[] _rg1 =
	{
		new Struct(),
		new Clazz { a = 0, prop = 0, s = null }
	};

	private static readonly object[] _rg2 =
	{
		new Clazz { a = 1, prop = 3, s = "butt" }
	};

	[Test]
	[TestCaseSource(nameof(_rg2))]
	public void NullMemTest2(object o)
	{
		Assert.False(RuntimeProperties.IsNull(o));
	}

	[Test]
	[TestCaseSource(nameof(_rg1))]
	public void NullMemTest3(object o)
	{
		Assert.True(RuntimeProperties.IsNull(o));
	}

}

[TestFixture]
public class Tests_Runtime2
{

	[Test]
	[TestCase("foo")]
	[TestCase(new[] { 1, 2, 3 })]
	public void PinTest2(object s)
	{
		var p = Mem.AddressOfHeap(s);

		Mem.InvokeWhilePinned(s, o =>
		{
			Assert.False(AddPressure(p, o));
		});
	}

	[Test]
	[TestCase("foo")]
	[TestCase(new[] { 1, 2, 3 })]
	public void PinTest(object s)
	{

		//var g = GCHandle.Alloc(s, GCHandleType.Pinned);

		var p = Mem.AddressOfHeap(s);

		Mem.Pin(s);
		Assert.False(AddPressure(p, s));

		Mem.Unpin(s);
		// Assert.True(AddPressure(p, s));

	}

	[Test]
	[TestCase("foo")]
	[TestCase(new[] { 1, 2, 3 })]
	public void PinTestInv(object s)
	{

		//var g = GCHandle.Alloc(s, GCHandleType.Pinned);

		var p = Mem.AddressOfHeap(s);

		Mem.Pin(s);
		Assert.False(AddPressure(p, s));

		Mem.Unpin(s);
		Assert.True(AddPressure(p, s));

	}

	private static bool AddPressure(Pointer<byte> p, object s, long i1 = 5_000)
	{
		var random = new Random();

		for (int i = 0; i < i1; i++) {
			GC.AddMemoryPressure(i1);
			//GC.AddMemoryPressure(100000);
			var r = new object[i1];

			for (int j = 0; j < r.Length; j++) {
				r[j] = random.Next();

				if (p != Mem.AddressOfHeap(s)) {
					return true;
				}
			}

			GC.Collect();

			if (p != Mem.AddressOfHeap(s)) {
				return true;
			}

			// GC.RemoveMemoryPressure(i1);
		}

		return p != Mem.AddressOfHeap(s);
		// return false;
	}

}

[TestFixture]
public class Tests_FileSystem
{

	[Test]
	[TestCase("C:\\Users\\Deci\\Pictures\\Sample\\Koala.jpg", 3)]
	[TestCase("C:\\Users\\Deci\\Pictures\\Sample\\Deserts.jpg", 4)]
	public void FileSystemTest(string str, int i)
	{
		var p = new FileInfo(str);

		var a = FileSystem.GetRelativeParent(p.FullName, i);
		var b = FileSystem.GetParent(p.FullName, i);

		var d = new DirectoryInfo(a);
		Assert.AreEqual(d.FullName, b);
	}

	[Test]
	public void FileSystemTest2()
	{
		var fs = FileSystem.GetTempFileName(null, "jpg");
		Assert.True(fs.EndsWith(".jpg"));

		var fs2 = FileSystem.GetTempFileName("hello", "jpg");
		Assert.True(File.Exists(fs2));

		var fs3 = FileSystem.GetTempFileName("hello2");
		Assert.True(File.Exists(fs3));

		var fs4 = FileSystem.GetTempFileName();
		Assert.True(File.Exists(fs4));
	}

}

[TestFixture]
public class Tests_Allocator
{

	[Test]
	public void AllocatorTest()
	{
		var h = AllocManager.Alloc(256);

		Assert.True(AllocManager.IsAllocated(h));

		Assert.AreEqual((nuint) 256, AllocManager.GetSize(h));

		h = AllocManager.ReAlloc(h, 512);

		Assert.AreEqual((nuint) 512, AllocManager.GetSize(h));

		/*Assert.Throws<Exception>(() =>
		{
			AllocManager.ReAlloc(h, Mem.Invalid_u);
		});*/

		AllocManager.Free(h);

		Assert.False(AllocManager.IsAllocated(h));

		// Assert.True(AllocManager.ReAlloc(h, Mem.Invalid_u) == null);
	}

	/*[Test]
	public void AllocUTest()
	{
	    var s = SmartAllocator.AllocU<Clazz>();

	    //var obj = Mem.AllocRefOnStack<Clazz>(ref stack);

	    Assert.AreEqual(s.a, Clazz.i);
	}*/

}

[TestFixture]
public class Tests_Mem
{

	[SetUp]
	public void Setup() { }

	[Test]
	[TestCase("foo")]
	public void ByteTest<T>(T t)
	{
		Assert.AreEqual(t, Mem.ReadFromBytes<T>(Mem.GetBytes(t)));
	}

	[Test]
	public void CopyTest()
	{
		const string foo = "foo";
		var          a   = new MyClass { s = foo, a = 321 };
		var          a2  = Mem.CopyInstance(a);

		Assert.AreEqual(a2, a);
	}

	[Test]
	public void OffsetTest()
	{
		Assert.AreEqual(0, Mem.OffsetOf<MyStruct>(nameof(MyStruct.a)));
		Assert.AreEqual(sizeof(int), Mem.OffsetOf<MyStruct>(nameof(MyStruct.b)));

		Assert.AreEqual(0, Mem.OffsetOf<string>("_stringLength"));
		Assert.AreEqual(sizeof(int), Mem.OffsetOf<string>("_firstChar"));
	}

	[Test]
	public unsafe void AddressTest()
	{
		/*
		 * String
		 */

		var s = "foo";

		nint strFixed;

		fixed (char* c = s) {
			strFixed = (nint) c;
		}

		nint strPin;

		fixed (char* c = &s.GetPinnableReference()) {
			strPin = (nint) c;
		}

		var pin = s.AsMemory().Pin();

		var strPinHandle = (nint) pin.Pointer;

		var strMem = Mem.AddressOfHeap(s, OffsetOptions.StringData).Address;

		Assert.AreEqual(strFixed, strMem);
		Assert.AreEqual(strPin, strMem);
		Assert.AreEqual(strPinHandle, strMem);

		/*
		 * Array
		 */

		nint arrayFixed;

		var rg = new[] { 1, 2, 3 };

		fixed (int* c = rg) {
			arrayFixed = (nint) c;
		}

		var arrayPin = rg.AsMemory().Pin();

		var arrayPinHandle = (nint) arrayPin.Pointer;

		var arrayMem = Mem.AddressOfHeap(rg, OffsetOptions.ArrayData).Address;

		Assert.AreEqual(arrayFixed, arrayMem);
		Assert.AreEqual(arrayPinHandle, arrayMem);

		/*
		 * Object
		 */

		object obj = new();

		nint objFixed;

		fixed (byte* p = &Mem.GetPinningHelper(obj).Data) {
			objFixed = (nint) p;
		}

		var objMem = Mem.AddressOfHeap(obj, OffsetOptions.Fields).Address;

		Assert.AreEqual(objFixed, objMem);
	}

	[Test]
	public unsafe void SizeTest()
	{
		var intArray = new[] { 1, 2, 3 };
		Assert.AreEqual(Mem.SizeOf(intArray, SizeOfOptions.Heap), 36);

		var s = "foo";
		Assert.AreEqual(Mem.SizeOf(s, SizeOfOptions.Heap), 28);

		var o = new object();
		Assert.AreEqual(Mem.SizeOf(o, SizeOfOptions.Heap), 24);

		Assert.AreEqual(Mem.SizeOf<string>(), Mem.Size);

		Assert.AreEqual(Mem.SizeOf<string>(SizeOfOptions.BaseFields), sizeof(int) + sizeof(char));

		Assert.AreEqual(Mem.SizeOf<string>(SizeOfOptions.BaseInstance), 22); // NOT 20; SOS is wrong

		Assert.AreEqual(Mem.SizeOf(intArray, SizeOfOptions.Data),
		                RuntimeProperties.ArrayOverhead + (sizeof(int) * intArray.Length));

		Assert.AreEqual(Mem.SizeOf(s, SizeOfOptions.Data),
		                RuntimeProperties.StringOverhead + (sizeof(char) * s.Length));

		Assert.AreEqual(Mem.SizeOf<string>(SizeOfOptions.Heap), Native.INVALID);

		Assert.Throws<ArgumentNullException>(() =>
		{
			Mem.SizeOf<string>(null, SizeOfOptions.Heap);
		});

		Assert.AreEqual(Mem.SizeOf<bool>(SizeOfOptions.Native), Marshal.SizeOf<bool>());
		Assert.AreEqual(Mem.SizeOf<Point>(), sizeof(Point));
		Assert.AreEqual(Mem.SizeOf<Point>(SizeOfOptions.Intrinsic), sizeof(Point));
	}

	[Test]
	[TestCase("foo")]
	[TestCase(new[] { 1, 2, 3, 4, 5 })]
	public void SizeTest2(object value)
	{
		Assert.AreEqual(RuntimeProperties.GetRawObjDataSize(value), Mem.SizeOf(value, SizeOfOptions.Data));
	}

	[Test]
	public void StringTest()
	{
		string s = "foo";

		var pointer = Mem.AddressOfHeap(s, OffsetOptions.StringData);

		unsafe {
			fixed (char* p = s) {
				Assert.AreEqual((ulong) p, pointer.ToUInt64());
			}
		}

		var p2 = pointer.Cast<char>();
		p2[0] = 'g';

		Assert.AreEqual(s, "goo");
	}

	[Test]
	public void ArrayTest()
	{
		int[] rg = { 1, 2, 3 };

		var ptr = Mem.AddressOfHeap(rg, OffsetOptions.ArrayData);

		unsafe {
			fixed (int* p = rg) {
				Assert.AreEqual((ulong) p, ptr.ToUInt64());
			}
		}

		ptr[0] = 100;

		Assert.True(rg.SequenceEqual(new[] { 100, 2, 3 }));
	}

	[Test]
	public void KernelReadWriteTest()
	{
		/*
		 *
		 */
		string s1 = "foo";

		var proc = Process.GetCurrentProcess();
		var addr = Mem.AddressOf(ref s1);

		var s1New = "bar";

		Mem.WriteProcessMemory(proc, addr, s1New);
		Assert.AreEqual(s1, s1New);

		var s2 = Mem.ReadProcessMemory<string>(proc, addr);
		Assert.AreEqual(s2, s1New);

		/*
		 *
		 */

		int i1    = 123;
		var addr2 = Mem.AddressOf(ref i1);
		var i1New = 321;
		Mem.WriteProcessMemory(proc, addr2, i1New);

		Assert.AreEqual(i1, i1New);

		var i2 = Mem.ReadProcessMemory<int>(proc, addr2);

		Assert.AreEqual(i2, i1New);
	}

	[Test]
	public void InToRefTest()
	{
		int i = 123;
		Change(ref i);
		Assert.AreEqual(i, 321);

		string s = "foo";
		Change2(ref s);
		Assert.AreEqual(s, "bar");

		static void Change2(ref string ix)
		{
			ref string r = ref Unsafe.AsRef(ref ix);
			r = "bar";
		}

		static void Change(ref int ix)
		{
			ref int r = ref Unsafe.AsRef(ref ix);
			r = 321;
		}
	}

	[Test]
	public void FieldTest()
	{
		var a = new Clazz { s = "a" };

		var pointer = Mem.AddressOfField<object, string>(a, "s");

		Assert.AreEqual(a.s, pointer.Value);

		pointer.Value = "g";

		Assert.AreEqual(a.s, "g");

	}

	[Test]
	public void FieldTest2()
	{
		var a = new MyStruct() { a = 123, b = 321 };

		var ofs  = Mem.OffsetOf<MyStruct>(nameof(MyStruct.a));
		var ptr  = Mem.AddressOf(ref a);
		var ptr2 = (ptr + ofs).Cast<int>();
		Assert.AreEqual(ptr2.Value, a.a);

		var ptr3 = Mem.AddressOfField<MyStruct, int>(in a, nameof(MyStruct.a)).Cast<int>();
		Assert.AreEqual(ptr2, ptr3);

		var ptr4 = Mem.AddressOfField(in a, () => a.a);
		Assert.AreEqual(ptr2, ptr4);
	}

	[Test]
	public void SpecialReadTest()
	{
		var a = new Clazz { a  = 321 };
		var b = new Struct { a = 123 };

		var proc = Process.GetCurrentProcess();
		var a1   = Mem.AddressOf(ref a);
		var b1   = Mem.AddressOf(ref b);

		Assert.AreEqual(((Clazz) Mem.ReadProcessMemory(proc, a1, typeof(Clazz))).a, a.a);

		Assert.AreEqual(((Struct) Mem.ReadProcessMemory(proc, b1, typeof(Struct))).a, b.a);
	}

	[Test]
	public void RefTest()
	{
		object     a = 123;
		ref object b = ref Mem.ref_cast(a);
		Assert.AreEqual(a, b);
		b = 321;
		Assert.AreEqual(a, b);

	}

	[Test]
	public void AddrOfDataTest()
	{
		int i = 123;
		Assert.AreEqual(Mem.AddressOfData(ref i), Mem.AddressOfData2(in i));

		string s = "butt";
		Assert.AreEqual(Mem.AddressOfData(ref s), Mem.AddressOfData2(s));
	}

	[Test]
	public unsafe void AllocTest()
	{
		var sizeOf = Mem.SizeOf<MyClass>(SizeOfOptions.BaseInstance);

		Pointer<byte> p = stackalloc byte[sizeOf];

		ref var d = ref Mem.New<MyClass>(ref p, out var p2);

		const int i = 123;

		d.a = i;

		Assert.NotNull(d);
		Assert.False(GCHeap.IsHeapPointer(d));
		Assert.AreEqual(d.a, i);

	}

	[Test]
	public unsafe void AllocTest2()
	{
		var sizeOf = Mem.SizeOf<MyClass>(SizeOfOptions.BaseInstance);

		Pointer<byte> p = AllocManager.Alloc((nuint) sizeOf);

		ref var d = ref Mem.New<MyClass>(ref p, out var p2);

		const int    i = 123;
		const string s = "foo";

		d.a = i;
		d.s = s;

		Assert.NotNull(d);
		Assert.False(GCHeap.IsHeapPointer(d));
		Assert.AreEqual(d.a, i);
		Assert.AreEqual(d.s, s);

		AllocManager.Free(p2);
	}

}

[TestFixture]
public class Tests_Streams
{

	[Test]
	public unsafe void Test1()
	{
		var str = "hi\0"u8;

		fixed (byte* p = str) {
			var br = new BinaryReader(new UnmanagedMemoryStream(p, str.Length));
			var c  = br.BaseStream.ReadUntil((byte s) => s == '\0', f => (byte) f.ReadByte()).ToArray();
			var c2 = Encoding.ASCII.GetString(c);
			Assert.AreEqual(Encoding.ASCII.GetString(str).Trim('\0'), c2);
		}
	}

	[Test]
	[TestCase(new byte[] { 0b1010, 0b101010, 0b11001 })]
	public void Test2(byte[] rg)
	{
		var bs  = new BitStream(new MemoryStream(rg, 0, rg.Length), true);
		var sb  = new string[rg.Length];
		var sb2 = new string[rg.Length];

		for (int i = 0; i < rg.Length; i++) {
			sb[i] = Mem.ToBinaryString(rg[i]);

		}

		int j = 0;

		for (int i = 0; i < bs.Length * BitCalculator.BITS_PER_BYTE; i++) {
			sb2[unchecked(i / BitCalculator.BITS_PER_BYTE)] += bs.ReadBit() ? 1 : 0;

		}

		for (int i = 0; i < rg.Length; i++) {
			Assert.AreEqual(sb[i], sb2[i]);
		}
	}

	[Test]
	public unsafe void Test3()
	{
		var ms = new MyStruct() { a = 321, b = 0xBEEF };

		var bs = new BitStream(
			new UnmanagedMemoryStream(Mem.AddressOf(ref ms).ToPointer<byte>(), Mem.SizeOf<MyStruct>()), true);

		Assert.AreEqual(bs.Read<MyStruct>(), ms);
		var ms2 = new MyStruct() { a = Random.Shared.Next(), b = Random.Shared.Next() };
		bs.ResetPosition();
		bs.Write(ms2);
		bs.ResetPosition();
		Assert.AreEqual(bs.Read<MyStruct>(), ms2);
	}

	[Test]
	public unsafe void Test4()
	{
		/*byte   a = (byte)Random.Shared.Next();
		sbyte  b = (sbyte)Random.Shared.Next();
		short  c = (short)Random.Shared.Next();
		ushort d = (ushort)Random.Shared.Next();*/
		int e = (int) Random.Shared.Next();
		/*uint   f = (uint)Random.Shared.Next();
		long   g = Random.Shared.Next();
		ulong    h = (ulong) Random.Shared.Next();*/

		var bs = new BitStream(
			new UnmanagedMemoryStream(Mem.AddressOf(ref e).ToPointer<byte>(), Mem.SizeOf<int>()), true);

		Assert.AreEqual(bs.Read<int>(), e);
		bs.ResetPosition();
		var s2 = Random.Shared.Next();
		bs.Write(s2);
		bs.ResetPosition();
		Assert.AreEqual(bs.Read<int>(), s2);

	}

	[Test]
	public unsafe void Test5()
	{
		var s = Strings.CreateRandom(10);

		var bs = new BitStream(
			new UnmanagedMemoryStream(Mem.AddressOf(ref s).ToPointer<byte>(), Mem.SizeOf<string>()), true);

		Assert.AreEqual(bs.Read<string>(), s);
		bs.ResetPosition();
		var s2 = "foo";
		bs.Write(s2);
		bs.ResetPosition();

		Assert.AreEqual(bs.Read<string>(), s2);

	}

}

static class static_clazz
{

	public static int i;

}