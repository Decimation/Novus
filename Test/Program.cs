// // ReSharper disable LocalizableElement
// ReSharper disable RedundantUsingDirective
// ReSharper disable RedundantUnsafeContext

#pragma warning disable IDE0005, CS0436, CS0469
#pragma warning disable NV0001
#pragma warning disable CS0649


global using Native = Novus.Win32.Native;
global using ER = Novus.Properties.EmbeddedResources;
using Flurl;
using Flurl.Http;
using Kantan.Collections;
using Kantan.Text;
using Novus;
using Novus.FileTypes.Resolvers;
using Novus.FileTypes.Uni;
using Novus.Imports;
using Novus.Imports.Attributes;
using Novus.Memory;
using Novus.Memory.Allocation;
using Novus.Numerics;
using Novus.OS;
using Novus.Properties;
using Novus.Runtime;
using Novus.Runtime.Meta;
using Novus.Runtime.VM;
using Novus.Runtime.VM.EE;
using Novus.Streams;
using Novus.Utilities;
using Novus.Win32;
using Novus.Win32.Structures.AdvApi32;
using Novus.Win32.Structures.DbgHelp;
using Novus.Win32.Structures.Kernel32;
using Novus.Win32.Structures.User32;
using Novus.Win32.Wrappers;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.SymbolStore;
using System.Drawing;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnitTest.TestTypes;
using UnitTest.TestTypes;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Novus.FileTypes.Media;


// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable PossibleNullReferenceException
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable UnusedType.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedParameter.Local

#nullable disable

namespace Test;

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

public static partial class Program
{

	static Program()
	{
		Global.Clr.LoadImports(typeof(Program));
	}


	private static async Task Main(string[] args)
	{
		ref var obj = ref ObjectUtility.New<MyClass>(ctorArgs: ["foo",321]);

		Console.WriteLine(obj);
	}

}