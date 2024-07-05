// ReSharper disable InconsistentNaming
// ReSharper disable CA1822
// ReSharper disable LocalizableElement

using Novus.Memory.Allocation;
using Novus.Memory;
using Novus.Runtime.Meta;
using Novus.Runtime;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Novus.OS;
using Novus.Properties;
using Novus.Utilities;
using Novus.Win32.Structures.Kernel32;
using Novus.Win32.Structures.User32;
using Novus.Win32;
using Novus;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Runtime.InteropServices;
using TestTypes;

#pragma warning disable CA1822
[assembly: InternalsVisibleTo("Test")]
[assembly: InternalsVisibleTo("UnitTest")]

namespace TestTypes;

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