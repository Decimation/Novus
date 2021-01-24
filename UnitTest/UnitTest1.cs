using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Novus;
using Novus.Memory;
using Novus.Runtime;
using Novus.Runtime.Meta;
using Novus.Runtime.VM;
using Novus.Utilities;
using Novus.Win32;
using NUnit.Framework;
// ReSharper disable InconsistentNaming

namespace UnitTest
{
	public static class TestUtil
	{
		public static void AssertAll<T>(T t, params Func<T, bool>[] fn)
		{
			foreach (var func in fn) {
				Assert.True(func(t));
			}
		}

		public static void AssertAll(params bool[] cond)
		{
			foreach (bool b in cond) {
				Assert.True(b);
			}
		}
	}

	[TestFixture]
	public class Tests
	{
		[SetUp]
		public void Setup() { }

		[StructLayout(LayoutKind.Explicit)]
		private struct MyStruct
		{
			[FieldOffset(default)] public int a;

			[FieldOffset(sizeof(int))] public int b;
		}
		
		
		[Test]
		public unsafe void TestMem1()
		{
			
			
			Assert.AreEqual(0, Mem.OffsetOf<MyStruct>(nameof(MyStruct.a)));
			Assert.AreEqual(sizeof(int), Mem.OffsetOf<MyStruct>( nameof(MyStruct.b)));

			
			Assert.AreEqual(0, Mem.OffsetOf<string>("_stringLength"));
			Assert.AreEqual(sizeof(int), Mem.OffsetOf<string>("_firstChar"));
		}
		
		
		[Test]
		public unsafe void TestAddresses()
		{
			/*
			 * String
			 */


			var s = "foo";

			IntPtr strFixed;

			fixed (char* c = s) {
				strFixed = (IntPtr) c;
			}

			var strMem = Mem.AddressOfHeap(s, OffsetOptions.StringData).Address;

			Assert.AreEqual(strFixed, strMem);

			/*
			 * Array
			 */

			IntPtr arrayFixed;

			var rg = new int[] {1, 2, 3};

			fixed (int* c = rg) {
				arrayFixed = (IntPtr) c;
			}

			var arrayMem = Mem.AddressOfHeap(rg, OffsetOptions.ArrayData).Address;

			Assert.AreEqual(arrayFixed, arrayMem);
			
			
			/*
			 * Object
			 */

			object obj = new object();

			IntPtr objFixed;
			
			fixed(byte* p = &RuntimeInfo.GetPinningHelper(obj).Data) {
				objFixed = (IntPtr) p;
			}

			var objMem = Mem.AddressOfHeap(obj, OffsetOptions.Fields).Address;
			
			Assert.AreEqual(objFixed, objMem);

		}


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
		public void InspectorTest()
		{
			string s = "foo";
			Assert.False(RuntimeInfo.IsNil(s));

			string s2 = null;
			Assert.True(RuntimeInfo.IsNil(s2));


			Assert.True(RuntimeInfo.IsNil(default(int)));


			Assert.True(RuntimeInfo.IsPinnable(s));

			string[] rg = {"foo", "bar"};
			Assert.False(RuntimeInfo.IsPinnable(rg));

			var rg2 = new int[] {1, 2, 3};
			Assert.True(RuntimeInfo.IsPinnable(rg2));
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


			unsafe {
				fixed (char* p = s) {
					Assert.AreEqual((ulong) p, Mem.AddressOfHeap(s, OffsetOptions.StringData).ToUInt64());
				}
			}

		}

		[Test]
		public void AllocatorTest()
		{
			Pointer<byte> h = Allocator.Alloc(256);
			
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

			Assert.True(Allocator.ReAlloc(h, -1)==null);


			
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
		}
	}
}