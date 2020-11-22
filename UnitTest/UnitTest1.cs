using System;
using System.Diagnostics;
using System.Reflection;
using Novus;
using Novus.CoreClr.Meta;
using Novus.Memory;
using Novus.Utilities;
using NUnit.Framework;


#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
#pragma warning disable HAA0602 // Delegate on struct instance caused a boxing allocation
#pragma warning disable HAA0603 // Delegate allocation from a method group
#pragma warning disable HAA0604 // Delegate allocation from a method group

#pragma warning disable HAA0501 // Explicit new array type allocation
#pragma warning disable HAA0502 // Explicit new reference type allocation
#pragma warning disable HAA0503 // Explicit new reference type allocation
#pragma warning disable HAA0504 // Implicit new array creation allocation
#pragma warning disable HAA0505 // Initializer reference type allocation
#pragma warning disable HAA0506 // Let clause induced allocation

#pragma warning disable HAA0301 // Closure Allocation Source
#pragma warning disable HAA0302 // Display class allocation to capture closure
#pragma warning disable HAA0303 // Lambda or anonymous method in a generic method allocates a delegate instance


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

			Assert.AreEqual(mf.Attributes, f.Attributes);
			Assert.AreEqual(mf.IsStatic, f.IsStatic);
			Assert.AreEqual(mf.Token, f.MetadataToken);
			Assert.AreEqual(mf.FieldInfo, f);
		}

		[Test]
		[TestCase(typeof(string))]
		public void TypeTest(Type t)
		{
			var mt = t.AsMetaType();

			Assert.AreEqual(mt.RuntimeType, t);
			Assert.AreEqual(mt.Token, t.MetadataToken);
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
			Assert.AreEqual(mt.StaticFieldsCount, 1);

		}
	}
}