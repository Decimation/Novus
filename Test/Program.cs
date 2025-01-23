// // ReSharper disable LocalizableElement
// ReSharper disable RedundantUsingDirective
// ReSharper disable RedundantUnsafeContext

global using Pointer = Novus.Memory.Pointer<byte>;
global using Native = Novus.Win32.Native;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Concurrent;
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
using Novus.FileTypes;
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
using Test.TestTypes;
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
using Novus.FileTypes.Uni;
using Novus.Memory.Types;

// ReSharper disable UnusedMember.Local

#pragma warning disable NV0001
#pragma warning disable CS0649

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable PossibleNullReferenceException
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable UnusedType.Local
// ReSharper disable InconsistentNaming
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
/*
 * https://github.com/IS4Code/SharpUtils
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
 * TypeHandle
 * https://github.com/dotnet/runtime/blob/master/src/coreclr/vm/typehandle.h
 * https://github.com/dotnet/runtime/blob/master/src/coreclr/vm/typehandle.cpp
 * https://github.com/dotnet/runtime/blob/master/src/coreclr/vm/typehandle.inl
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

public static class Program
{

	static Program()
	{
		Global.Clr.LoadImports(typeof(Program));
	}

	private static unsafe void Main(string[] args)
	{

		var clazz = new TestTypes.Clazz3();
		var mm    = clazz.GetType().GetAnyMethod("SayHi").AsMetaMethod();
		var mm2    = clazz.GetType().GetAnyMethod("SayButt").AsMetaMethod();
		clazz.SayHi();
		clazz.SayButt();
		mm.Reset();
		mm.EntryPoint = (void*) mm2.Function;
		clazz.SayHi();

		// delegate* unmanaged<void> au = &clazz.SayButt;

		return;
	}

	private static void Test1()
	{
		MyClass mc = new MyClass() { a = 321, s = "butt" };
		var     rg = Mem.GetBytes(mc);
		Console.WriteLine(rg.FormatJoin("X", delim: " "));
		var mc2 = Mem.ReadFromBytes<object>(rg);
		Console.WriteLine(mc);
		Console.WriteLine(mc2);
	}

	private static void run1()
	{
		string s = "foo";
	}

	private static void run2()
	{
		int i = 123;
	}

	private static unsafe void run3()
	{
		Pointer p = stackalloc byte[Mem.Size];
		any     a = new();

	}

	public static unsafe delegate* managed<int> f;

}