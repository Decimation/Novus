using System;

namespace UnitTest.TestTypes
{
	internal class Clazz
	{
		public       int a;
		public const int i = 123_321;

		public Clazz()
		{
			a = i;
		}

		public void SayHi() => Console.WriteLine("hi");

		public override string ToString()
		{
			return $"{a}";
		}
	}
}