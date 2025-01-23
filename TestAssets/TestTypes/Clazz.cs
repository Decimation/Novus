// ReSharper disable InconsistentNaming
// ReSharper disable CA1822
// ReSharper disable LocalizableElement

using System;
using System.Runtime.CompilerServices;

#pragma warning disable CA1822
[assembly: InternalsVisibleTo("Test")]
[assembly: InternalsVisibleTo("UnitTest")]

namespace Test.TestTypes;

public class Clazz2
{

	public static unsafe int Func()
	{
		return default;
	}

}

public class Clazz3
{

	public int a;

	public const int i = 123_321;

	public string s;

	public int prop { get; set; }

	public static int sprop { get; set; }

	public Clazz3()
	{
		a = i;
	}

	public Clazz3(int a, string s, int prop)
	{
		this.a    = a;
		this.s    = s;
		this.prop = prop;
	}

	public void SayHi()
		=> Console.WriteLine("hi");
	public void SayButt()
		=> Console.WriteLine("butt");
	public override string ToString()
	{
		return $"{nameof(a)}: {a}, {nameof(s)}: {s}, {nameof(prop)}: {prop}";
	}

}

public class Clazz
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

	public void SayHi()
		=> Console.WriteLine("hi");

	public override string ToString()
	{
		return $"{a} | {i} | {s} | {prop} | {sprop}";
	}

}