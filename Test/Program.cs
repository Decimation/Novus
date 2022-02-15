// ReSharper disable LocalizableElement
// ReSharper disable RedundantUsingDirective
// ReSharper disable RedundantUnsafeContext

global using Native = Novus.OS.Win32.Native;
using System.Buffers;
using System.Diagnostics;
using System.Dynamic;
using System.Runtime.Intrinsics.X86;
using Kantan.Cli;
using Novus.OS;
using Novus.OS.Win32;
using Novus.OS.Win32.Structures.Kernel32;
#pragma warning disable IDE0005, CS0436, CS0469
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

using Novus.Runtime;
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable UnusedType.Local
// ReSharper disable InconsistentNaming
#pragma warning disable CS0649

// ReSharper disable UnusedParameter.Local
#nullable disable

namespace Test;

/*
 * C:\Program Files\dotnet\shared\Microsoft.NETCore.App\6.x.x
 * C:\Windows\Microsoft.NET\Framework64\v4.0.30319
 *
 * symchk "input" /s SRV*output*http://msdl.microsoft.com/download/symbols
 *
 * todo: integrate pdbex
 * todo: IL, ILSupport
 */

/*
 * ◆ Novus				https://github.com/Decimation/Novus
 * ⨉ NeoCore			https://github.com/Decimation/NeoCore
 * ⨉ RazorSharp			https://github.com/Decimation/RazorSharp
 * 
 * ◆ Kantan				https://github.com/Decimation/Kantan
 * 
 */

/* Runtime
 *
 * https://github.com/dotnet/runtime
 *
 *
 *
 * Field
 * https://github.com/dotnet/runtime/blob/master/src/coreclr/vm/field.h
 * https://github.com/dotnet/runtime/blob/master/src/coreclr/vm/field.cpp
 *
 * Method
 * https://github.com/dotnet/runtime/blob/master/src/coreclr/vm/method.hpp
 * https://github.com/dotnet/runtime/blob/master/src/coreclr/vm/method.cpp
 * https://github.com/dotnet/runtime/blob/master/src/coreclr/vm/method.inl
 *
 * EEClass
 * https://github.com/dotnet/runtime/blob/master/src/coreclr/vm/class.h
 * https://github.com/dotnet/runtime/blob/master/src/coreclr/vm/class.cpp
 * https://github.com/dotnet/runtime/blob/master/src/coreclr/vm/class.inl
 *
 * MethodTable
 * https://github.com/dotnet/runtime/blob/master/src/coreclr/vm/methodtable.h
 * https://github.com/dotnet/runtime/blob/master/src/coreclr/vm/methodtable.cpp
 * https://github.com/dotnet/runtime/blob/master/src/coreclr/vm/methodtable.inl
 *
 * TypeIntPtr
 * https://github.com/dotnet/runtime/blob/master/src/coreclr/vm/typeIntPtr.h
 * https://github.com/dotnet/runtime/blob/master/src/coreclr/vm/typeIntPtr.cpp
 * https://github.com/dotnet/runtime/blob/master/src/coreclr/vm/typeIntPtr.inl
 *
 * Marshal Native
 * https://github.com/dotnet/runtime/blob/master/src/coreclr/vm/marshalnative.h
 * https://github.com/dotnet/runtime/blob/master/src/coreclr/vm/marshalnative.cpp
 *
 * Other
 * https://github.com/dotnet/runtime/blob/master/src/coreclr/vm/ecalllist.h
 * https://github.com/dotnet/runtime/blob/master/src/coreclr/vm/gcheaputilities.h
 * https://github.com/dotnet/runtime/blob/master/src/coreclr/gc/gcinterface.h
 */


public static unsafe class Program
{
	private static void Main(string[] args)
	{
		//

		var p = Process.GetProcessById(14400);
		Console.WriteLine(p.Id);
		
		

	}


	struct MyStruct
	{
		public int   a;
		public float f;

		public override string ToString()
		{
			return $"{nameof(a)}: {a}, {nameof(f)}: {f}";
		}
	}

	private static void Test1()
	{
		dynamic o = new ExpandoObject();
		// o.a = (Func<int>) (() => { return 1; });
		var dictionary = (IDictionary<string, object>) o;
		dictionary.Add("a", 1);

		Console.WriteLine(o);
		Console.WriteLine(o.a);

		var kl = new KeyboardListener()
		{
			KeyWhitelist =
			{
				VirtualKey.LBUTTON
			}
		};

		kl.KeyStroke += (sender, key) =>
		{
			Console.WriteLine($"! {key}");

		};
		kl.Start();
		Thread.Sleep(TimeSpan.FromSeconds(10));
	}
}