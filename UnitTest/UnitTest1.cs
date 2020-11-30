using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using Novus;
using Novus.CoreClr.Meta;
using Novus.Memory;
using Novus.Utilities;
using Novus.Win32;
using NUnit.Framework;
using Novus.CoreClr.Meta;

namespace UnitTest
{
	public class Tests
	{
		[SetUp]
		public void Setup() { }

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


			AssertAll(!mt.IsInteger, 
				!mt.IsUnmanaged, 
				!mt.IsNumeric, 
				!mt.IsReal,
				!mt.IsAnyPointer, 
				!mt.IsArray);


			var mti = typeof(int).AsMetaType();

			AssertAll(mti.IsInteger, 
				!mti.IsReal, 
				mti.IsNumeric);

			var mtf = typeof(float).AsMetaType();

			AssertAll(!mtf.IsInteger, 
				mtf.IsReal, 
				mtf.IsNumeric);

			var rg1 = new int[1];

			var mtrg1 = rg1.GetType().AsMetaType();
			Assert.AreEqual(rg1.Rank, mtrg1.ArrayRank);
			Assert.True(mtrg1.HasComponentSize);
			Assert.AreEqual(mtrg1.ComponentSize, sizeof(int));
			Assert.AreEqual(rg1.GetType().GetElementType(), mtrg1.ElementTypeHandle.RuntimeType);


			var rg2   = new int[1,1];

			var mtrg2 = rg2.GetType().AsMetaType();
			Assert.AreEqual(rg2.Rank, mtrg2.ArrayRank);

			Assert.AreEqual(rg2.GetType().GetElementType(), mtrg2.ElementTypeHandle.RuntimeType);
		}

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

		[Test]
		[TestCase(typeof(string))]
		public void TypeTest(Type t)
		{
			var mt = t.AsMetaType();


			Assert.AreEqual(mt.RuntimeType, t);
			Assert.AreEqual(mt.Token, t.MetadataToken);
			Assert.AreEqual(mt.Attributes, t.Attributes);


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

			Assert.Throws<ArgumentException>(() =>
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
			Assert.False(Inspector.IsNil(s));

			string s2 = null;
			Assert.True(Inspector.IsNil(s2));


			Assert.True(Inspector.IsNil(default(int)));


			Assert.True(Inspector.IsPinnable(s));

			string[] rg = {"foo", "bar"};
			Assert.False(Inspector.IsPinnable(rg));

			var rg2 = new int[] {1, 2, 3};
			Assert.True(Inspector.IsPinnable(rg2));
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
	}
}