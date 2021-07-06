using Novus.Memory;
using Novus.Runtime;
using Novus.Runtime.Meta;
using Novus.Utilities;
using Novus.Win32;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Novus;
using Novus.Imports;
using Novus.Runtime.VM;
using Novus.Runtime.VM.IL;
using SimpleCore.Numeric;
using UnitTest.TestTypes;

// ReSharper disable StringLiteralTypo

// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
#pragma warning disable 0649, IDE0044

namespace UnitTest
{
	[TestFixture]
	public unsafe class Tests_Pointer
	{
		[Test]
		public void Test1()
		{
			int i = 256;

			var ptr1 = Mem.AddressOf(ref i);

			Assert.AreEqual(ptr1.Value, i);
			Assert.AreEqual(ptr1.Reference, i);

			var          rg   = new int[] {1, 2, 3};
			Pointer<int> ptr2 = Mem.AddressOfHeap(rg, OffsetOptions.ArrayData);

			fixed (int* p1 = rg) {
				int* cpy = p1;

				for (int j = 0; j < rg.Length; j++) {
					Assert.True(ptr2.Address == Marshal.UnsafeAddrOfPinnedArrayElement(rg, j));
					Assert.True(cpy++        == ptr2++);
				}
			}


		}
	}


	[TestFixture]
	public unsafe class Tests_Resources
	{
		private const string s  = "C:\\Users\\Deci\\VSProjects\\SandboxLibrary\\x64\\Release\\SandboxLibrary.dll";
		private const string s2 = "SandboxLibrary.dll";

		[ImportUnmanaged(s2, UnmanagedImportType.Signature, "89 54 24 10 89 4C 24 08")]
		private static delegate* unmanaged<int, int, int> doSomething;

		[Test]
		public void Test()
		{

			using var r = Resource.LoadModule(s);
			r.LoadImports(typeof(Tests_Resources));

			int c = doSomething(1, 1);

			Assert.AreEqual(c, 2);


		}
	}


	[TestFixture]
	public class Tests_Native
	{
		[Test]
		public void SymbolsTest()
		{
			var m = new SymbolLoader(@"C:\Symbols\coreclr.pdb");

			var s = m.GetSymbol("g_pGCHeap");

			Assert.NotNull(s);
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
			Assert.True(typeof(IInterface).GetAllImplementations().Contains(typeof(Implement1)));
			Assert.True(typeof(Superclass1).GetAllSubclasses().Contains(typeof(Subclass1)));
		}

		[Test]
		[TestCase(typeof(int), true, true, false)]
		[TestCase(typeof(float), true, false, true)]
		[TestCase(typeof(Half), true, false, true)]
		public void Test3(Type t, bool isNum, bool isInt, bool isReal)
		{
			Assert.AreEqual(isNum, t.IsNumeric());
			Assert.AreEqual(isInt, t.IsInteger());
			Assert.AreEqual(isReal, t.IsReal());
		}

		[StructLayout(LayoutKind.Explicit, Size = 8)]
		struct substrate { }

		[Test]
		[TestCase(typeof(int), true)]
		[TestCase(typeof(uint), false)]
		public void Test4(Type t, bool s)
		{
			Assert.AreEqual(s, t.IsSigned());
		}

		[Test]
		[TestCase(typeof(substrate), true)]
		[TestCase(typeof(Pointer<>),true)]
		[TestCase(typeof(Clazz), false)]
		public void Test5(Type t, bool b)
		{
			Assert.AreEqual(b, t.CanBePointerSurrogate());
		}

		[Test]
		public void Test4()
		{
			Assert.AreEqual(ReflectionOperatorHelpers.fieldof(() => new Subclass1().i),
			                typeof(Subclass1).GetRuntimeField(nameof(Subclass1.i)));
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

			var b1 = ReflectionHelper.CallGeneric(
				typeof(RuntimeHelpers).GetMethod("IsReferenceOrContainsReferences"), t, null);

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
		public unsafe void GCTest()
		{
			var s   = "bar";
			var ptr = Mem.AddressOfHeap(s).ToPointer();
			Assert.True(GCHeap.IsHeapPointer(ptr));

			var p = new Point();
			Assert.False(GCHeap.IsHeapPointer(&p));

			Assert.True(GCHeap.IsHeapPointer(s));

			var o = GCHeap.AllocObject(typeof(List<int>).AsMetaType()) as List<int>;

			Assert.NotNull(o);
			o.Add(1);

			Assert.True(o.Contains(1));
		}


		[Test]
		[TestCase(nameof(SayHi2), true)]
		[TestCase(nameof(SayHi), false)]
		[TestCase(nameof(Tiny), null)]
		public unsafe void ILTest(string name, bool? init)
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

	[TestFixture]
	public class Tests_Runtime
	{
		[Test]
		public void PinnableBlittableTest()
		{
			Assert.True(RuntimeInfo.IsPinnable("g"));
			Assert.False(RuntimeInfo.IsBlittable("g"));


			Assert.True(RuntimeInfo.IsPinnable(new int[] {1, 2, 3}));
			Assert.False(RuntimeInfo.IsBlittable(new int[] {1, 2, 3}));
		}


		[Test]
		public void BoxedTest()
		{
			Assert.True(RuntimeInfo.IsBoxed((object) 1));
		}

		[Test]
		public void BlankTest()
		{
			Assert.True(RuntimeInfo.IsBlank(Array.Empty<int>()));
		}

		[Test]
		public void PinnableTest()
		{
			string s = "foo";
			Assert.True(RuntimeInfo.IsPinnable(s));

			string[] rg = {"foo", "bar"};
			Assert.False(RuntimeInfo.IsPinnable(rg));

			var rg2 = new[] {1, 2, 3};
			Assert.True(RuntimeInfo.IsPinnable(rg2));

			Assert.True(RuntimeInfo.IsPinnable(1));

			Assert.False(RuntimeInfo.IsPinnable(new List<int> {1, 2, 3}));
		}

		[Test]
		public void NilTest()
		{
			string s = "foo";
			Assert.False(RuntimeInfo.IsNil(s));

			string s2 = null;
			Assert.True(RuntimeInfo.IsNil(s2));

			Assert.True(RuntimeInfo.IsNil(default(int)));
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
		[TestCase(@"C:\Users\Deci\Pictures\Sample\Penguins.jpg", nameof(FileFormatType.JPEG))]
		[TestCase(@"C:\Users\Deci\Pictures\Sample\Penguins.gif", nameof(FileFormatType.GIF))]
		[TestCase(@"C:\Users\Deci\Pictures\Sample\Penguins.bmp", nameof(FileFormatType.BMP))]
		[TestCase(@"C:\Users\Deci\Pictures\Sample\Penguins.png", nameof(FileFormatType.PNG))]
		[TestCase(@"C:\Users\Deci\Pictures\Icons\terminal.ico", nameof(FileFormatType.ICO))]
		public void FileTypeTest(string s, string n)
		{
			//C:\Users\Deci\Pictures\Camera Roll

			var t = FileSystem.ResolveFileType(s);

			Assert.AreEqual(t.Name, n);

			var m = FileSystem.ResolveMimeType(File.ReadAllBytes(s));

			TestContext.WriteLine(m);

			Assert.Throws<ArgumentNullException>(() =>
			{
				FileSystem.ResolveMimeType(dataBytes: null);
			});
		}

		[TestCase("https://i.imgur.com/QtCausw.png", nameof(FileFormatType.JPEG))]
		public void FileTypeTest2(string s, string n)
		{
			var sx = new WebClient();
			var rg = sx.DownloadData(s);
			var s2 = new MemoryStream(rg);
			var t  = FileSystem.ResolveFileType(s2);

			Assert.AreEqual(t.Name, n);

			//var m = FileSystem.ResolveMimeType(File.ReadAllBytes(s));

			//TestContext.WriteLine(m);

			//Assert.Throws<ArgumentNullException>(() =>
			//{
			//	FileSystem.ResolveMimeType(dataBytes: null);
			//});
		}
	}

	[TestFixture]
	public class Tests_Allocator
	{
		[Test]
		public void AllocatorTest()
		{
			var h = Allocator.Alloc(256);

			Assert.AreEqual(true, Allocator.IsAllocated(h));

			Assert.AreEqual(256, Allocator.GetAllocSize(h));

			h = Allocator.ReAlloc(h, 512);

			Assert.AreEqual(512, Allocator.GetAllocSize(h));

			Assert.Throws<ArgumentException>(() =>
			{
				Allocator.ReAlloc(h, -1);
			});

			Allocator.Free(h);

			Assert.AreEqual(false, Allocator.IsAllocated(h));

			Assert.True(Allocator.ReAlloc(h, -1) == null);
		}

		[Test]
		public void AllocUTest()
		{
			var s = Allocator.AllocU<Clazz>();

			//var obj = Mem.AllocRefOnStack<Clazz>(ref stack);

			Assert.AreEqual(s.a, Clazz.i);
		}
	}

	[TestFixture]
	public class Tests_Mem
	{
		[SetUp]
		public void Setup() { }

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

			IntPtr strFixed;

			fixed (char* c = s) {
				strFixed = (IntPtr) c;
			}

			IntPtr strPin;

			fixed (char* c = &s.GetPinnableReference()) {
				strPin = (IntPtr) c;
			}

			var pin = s.AsMemory().Pin();

			var strPinHandle = (IntPtr) pin.Pointer;

			var strMem = Mem.AddressOfHeap(s, OffsetOptions.StringData).Address;

			Assert.AreEqual(strFixed, strMem);
			Assert.AreEqual(strPin, strMem);
			Assert.AreEqual(strPinHandle, strMem);

			/*
			 * Array
			 */

			IntPtr arrayFixed;

			var rg = new[] {1, 2, 3};

			fixed (int* c = rg) {
				arrayFixed = (IntPtr) c;
			}

			var arrayPin = rg.AsMemory().Pin();

			var arrayPinHandle = (IntPtr) arrayPin.Pointer;

			var arrayMem = Mem.AddressOfHeap(rg, OffsetOptions.ArrayData).Address;

			Assert.AreEqual(arrayFixed, arrayMem);
			Assert.AreEqual(arrayPinHandle, arrayMem);

			/*
			 * Object
			 */

			object obj = new();

			IntPtr objFixed;

			fixed (byte* p = &RuntimeInfo.GetPinningHelper(obj).Data) {
				objFixed = (IntPtr) p;
			}

			var objMem = Mem.AddressOfHeap(obj, OffsetOptions.Fields).Address;

			Assert.AreEqual(objFixed, objMem);
		}

		[Test]
		public unsafe void SizeTest()
		{
			var intArray = new[] {1, 2, 3};
			Assert.AreEqual(Mem.SizeOf(intArray, SizeOfOptions.Heap), 36);

			var s = "foo";
			Assert.AreEqual(Mem.SizeOf(s, SizeOfOptions.Heap), 28);

			var o = new object();
			Assert.AreEqual(Mem.SizeOf(o, SizeOfOptions.Heap), 24);

			Assert.AreEqual(Mem.SizeOf<string>(), Mem.Size);

			Assert.AreEqual(Mem.SizeOf<string>(SizeOfOptions.BaseFields), sizeof(int) + sizeof(char));

			Assert.AreEqual(Mem.SizeOf<string>(SizeOfOptions.BaseInstance), 22); // NOT 20; SOS is wrong

			Assert.AreEqual(Mem.SizeOf(intArray, SizeOfOptions.Data),
			                RuntimeInfo.ArrayOverhead + (sizeof(int) * intArray.Length));

			Assert.AreEqual(Mem.SizeOf(s, SizeOfOptions.Data),
			                RuntimeInfo.StringOverhead + (sizeof(char) * s.Length));

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
			int[] rg = {1, 2, 3};

			var ptr = Mem.AddressOfHeap(rg, OffsetOptions.ArrayData);

			unsafe {
				fixed (int* p = rg) {
					Assert.AreEqual((ulong) p, ptr.ToUInt64());
				}
			}

			ptr[0] = 100;

			Assert.True(rg.SequenceEqual(new[] {100, 2, 3}));
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
		public void SpecialReadTest()
		{
			var a = new Clazz {a  = 321};
			var b = new Struct {a = 123};

			var proc = Process.GetCurrentProcess();
			var a1   = Mem.AddressOf(ref a);
			var b1   = Mem.AddressOf(ref b);

			Assert.AreEqual(((Clazz) Mem.ReadProcessMemory(proc, a1, typeof(Clazz))).a, a.a);

			Assert.AreEqual(((Struct) Mem.ReadProcessMemory(proc, b1, typeof(Struct))).a, b.a);
		}
	}
}