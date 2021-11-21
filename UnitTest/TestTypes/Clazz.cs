using System;
// ReSharper disable InconsistentNaming
// ReSharper disable CA1822
// ReSharper disable LocalizableElement
#pragma warning disable CA1822
namespace UnitTest.TestTypes;

internal class Clazz
{
	public       int    a;
	public const int    i = 123_321;
	public       string s;

	public        int prop  { get; set; }
	public static int sprop { get; set; }

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