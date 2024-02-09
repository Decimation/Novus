// ReSharper disable LocalizableElement
// ReSharper disable RedundantUsingDirective
// ReSharper disable RedundantUnsafeContext

global using Native = Novus.Win32.Native;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.SymbolStore;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Kantan.Collections;
using Kantan.Text;
using Microsoft.VisualBasic.FileIO;
using Novus;
using Novus.FileTypes.Impl;
using Novus.Imports;
using Novus.Imports.Attributes;
using Novus.Memory;
using Novus.Memory.Allocation;
using Novus.OS;
using Novus.Properties;
using Novus.Runtime.Meta;
using Novus.Runtime.VM;
using Novus.Streams;
using Novus.Utilities;
using Novus.Win32;
using Novus.Win32.Structures.DbgHelp;
using Novus.Win32.Structures.Kernel32;
using Novus.Win32.Structures.User32;
using Novus.Win32.Wrappers;
using TestTypes;
using static Novus.Win32.Structures.User32.WindowMessage;
using FileSystem = Novus.OS.FileSystem;
using FileType = Novus.FileTypes.FileType;
#pragma warning disable IDE0005, CS0436, CS0469
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Novus.Runtime;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Novus.Numerics;
using static System.Net.WebRequestMethods;
using Novus.FileTypes.Uni;

// ReSharper disable ClassNeverInstantiated.Local

// ReSharper disable PossibleNullReferenceException

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
 *
 *
 */

/*
 * 🌟 Novus				https://github.com/Decimation/Novus
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

	public delegate ref int Del(in int o);

	private static async Task Main(string[] args)
	{
		foreach (Delegate @delegate in UniSource.Register) {
			Console.WriteLine(@delegate);
		}

		var m = new MemoryStream([1, 2, 3]);
		var f = @"C:\Users\Deci\Pictures\Epic anime\0c4c80957134d4304538c27499d84dbe.jpeg";
		var u = (Url) "https://us.rule34.xxx//images/4777/eb5d308334c52a2ecd4b0b06846454e4.jpeg?5440124";

		foreach (var v in new object[]{m,f,u}) {
			Console.WriteLine(UniSource.GetUniType(v, out var v2));
		}

		;
	}

	static void t1()
	{
		DynamicMethod d = new DynamicMethod("ref_cast2", null, new[] { typeof(int).MakeByRefType() },
		                                    typeof(int).MakeByRefType());

		var il = d.GetILGenerator();
		il.Emit(OpCodes.Ret);
		var f = (Del) d.CreateDelegate(typeof(Del));
		int i = 0;
		Console.WriteLine(i);
		ref int rf = ref f(in i);
		t2(ref rf);
		Console.WriteLine(i);

	}

	static void t2(ref int r)
	{
		r = 321;
	}

	private static void Test2()
	{
		var o = (MyClass2) GCHeap.AllocUninitializedObject(typeof(MyClass2));
		Console.WriteLine(GCHeap.IsHeapPointer(o));
		Console.WriteLine(RuntimeProperties.IsBoxed(o));

		var o2 = (MyStruct) GCHeap.AllocUninitializedObject(typeof(MyStruct));
		Console.WriteLine(GCHeap.IsHeapPointer(&o2));
		Console.WriteLine(RuntimeProperties.IsBoxed(o2));
		Console.WriteLine(Mem.AddressOfData(ref o2));

		Console.WriteLine(RuntimeProperties.Box(o2));

		var o3 = new MyStruct() { };
		Console.WriteLine(RuntimeProperties.IsBoxed(o3));
		Console.WriteLine(GCHeap.IsHeapPointer(&o3));
		Console.WriteLine(RuntimeProperties.IsBoxed(RuntimeProperties.Box(o3)));
		Console.WriteLine(GCHeap.IsHeapPointer(Mem.AddressOf(ref o3)));

		int i = 1;
		Console.WriteLine(GCHeap.IsHeapPointer(&i));
		Console.WriteLine(GCHeap.IsHeapPointer(Mem.AddressOf(ref i)));
	}

	public static void HandleHotKey(IntPtr hWnd, int id)
	{
		// Handle the hotkey event
		Console.WriteLine("Hotkey pressed. ID: " + id);

	}

	private static void TestAlloc1()
	{
		var v = AllocManager.New<Clazz>(ctor: [3, "foo", 1]);
		Console.WriteLine(v);

		AllocManager.Free(v);
	}

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

		public Clazz(int a, string s, int prop)
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

	static void Run() { }

	static Program()
	{
		Global.Clr.LoadImports(typeof(Program));
		Global.Setup();

	}

}