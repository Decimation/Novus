
// ReSharper disable InconsistentNaming
// ReSharper disable CA1822
// ReSharper disable LocalizableElement

using System;
using System.Runtime.CompilerServices;

#pragma warning disable CA1822
[assembly: InternalsVisibleTo("Test")]
[assembly: InternalsVisibleTo("UnitTest")]
namespace TestTypes;

internal class Clazz
{
	public int a;

	public const int i = 123_321;

	public string s;

	public int prop { get; set; }

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