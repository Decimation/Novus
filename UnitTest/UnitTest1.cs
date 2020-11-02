using Novus;
using Novus.CoreClr.Meta;
using Novus.Memory;
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
		public void Setup()
		{
		}
		[Test]
		public void Test2()
		{
			string s  = "foo";
			var    mt = s.GetType().AsMetaType();

			Assert.AreEqual(Mem.HeapSizeOf(s), 28);
			//Assert.AreEqual(mt.BaseSize,0x14); 
			Assert.AreEqual(mt.ComponentSize, 0x2);

			Assert.AreEqual(mt.InstanceFieldsCount,2);
			Assert.AreEqual(mt.StaticFieldsCount, 1);
		}
		[Test]
		public void Test1()
		{
			Assert.Pass();
		}
	}
}