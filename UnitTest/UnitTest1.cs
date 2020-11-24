using System;
using System.Diagnostics;
using System.Reflection;
using Novus;
using Novus.CoreClr.Meta;
using Novus.Memory;
using Novus.Utilities;
using NUnit.Framework;


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


			Assert.AreEqual(mf.FieldInfo, f);
			Assert.AreEqual(mf.Token, f.MetadataToken);
			Assert.AreEqual(mf.Attributes, f.Attributes);
			Assert.AreEqual(mf.IsStatic, f.IsStatic);
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
		public void Test2()
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
		public void Test1()
		{
			string s  = "foo";
			var    mt = s.GetType().AsMetaType();

			Assert.AreEqual(Mem.HeapSizeOf(s), 28);
			//Assert.AreEqual(mt.BaseSize,0x14); 
			Assert.AreEqual(mt.ComponentSize, 0x2);

			Assert.AreEqual(mt.InstanceFieldsCount, 2);
			Assert.AreEqual(mt.StaticFieldsCount, 1);
			Assert.AreEqual(mt.InstanceFieldsSize, sizeof(char) + sizeof(int));

		}
	}
}