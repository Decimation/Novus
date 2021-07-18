// ReSharper disable LocalizableElement
// ReSharper disable RedundantUsingDirective
// ReSharper disable RedundantUnsafeContext

#pragma warning disable IDE0005

using System;
using System.Buffers;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Xml;
using Novus;
using Novus.Imports;
using Novus.Memory;
using Novus.Runtime;
using Novus.Runtime.Meta;
using Novus.Runtime.VM;
using Novus.Utilities;
using Novus.Win32;
using Novus.Win32.Structures;
using Novus.Win32.Wrappers;
using Kantan.Diagnostics;
using Kantan.Utilities;
using static Novus.Utilities.ReflectionOperatorHelpers;
using Console = System.Console;

// ReSharper disable UnusedParameter.Local
#nullable disable
#pragma warning disable IDE0060

namespace Test
{
	/*
	 * C:\Program Files\dotnet\shared\Microsoft.NETCore.App\5.0.6
	 * C:\Program Files\dotnet\shared\Microsoft.NETCore.App\5.0.5
	 * C:\Program Files\dotnet\shared\Microsoft.NETCore.App\5.0.4
	 * C:\Program Files\dotnet\shared\Microsoft.NETCore.App\5.0.2
	 * C:\Program Files\dotnet\shared\Microsoft.NETCore.App\5.0.0
	 * C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.9
	 * C:\Windows\Microsoft.NET\Framework64\v4.0.30319
	 *
	 * symchk "input" /s SRV*output*http://msdl.microsoft.com/download/symbols
	 *
	 * todo: integrate pdbex
	 * todo: IL, ILSupport
	 *
	 */


	/*
	 * Novus				https://github.com/Decimation/Novus
	 * NeoCore				https://github.com/Decimation/NeoCore
	 * RazorSharp			https://github.com/Decimation/RazorSharp
	 * 
	 * Kantan			https://github.com/Decimation/Kantan
	 * SimpleSharp			https://github.com/Decimation/SimpleSharp
	 *
	 * Memkit				https://github.com/Decimation/Memkit
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


	public static unsafe class Program
	{
		private static void Main(string[] args)
		{
			//KeyboardListener k = new KeyboardListener();
			//k.Run();
			//k.KeyPress += (sender, eventArgs) =>
			//{
			//	if (eventArgs.Key ==  VirtualKey.KEY_G) {
			//		Console.WriteLine(eventArgs);

			//	}
			//};

			//Native.SetConsoleOutputCP((uint) Encoding.UTF8.CodePage);

			//Console.WriteLine(StringConstants.CHECK_MARK);

			//var s = StringConstants.CHECK_MARK.ToString();


			//int size = Native.WideCharToMultiByte(Encoding.UTF8.CodePage, 0, s, (int) s.Length, null, 0, IntPtr.Zero, IntPtr.Zero);

			//byte[] s2 = new byte[size];

			//Native.WideCharToMultiByte(Encoding.UTF8.CodePage, 0, s, (int) s.Length, s2, size, IntPtr.Zero, IntPtr.Zero);

			//Console.WriteLine(s2.FormatJoin("X"));

			//var builder = new StringBuilder(Encoding.UTF8.GetString(s2));
			//Console.WriteLine(builder);

			//var consoleOutput = Native.GetStdOutputHandle();

			//var xb = Native.WriteConsoleOutputCharacter(consoleOutput,
			//                                            builder, (uint)builder.Length,Native.GetConsoleCursorPosition(consoleOutput),
			//                                            out uint tc);
			//Console.WriteLine(xb);
			//Console.WriteLine(tc);

			//Console.WriteLine(Global.Clr);

			//Console.WriteLine(Global.Clr.Symbols.Value.GetSymbol("g_pGCHeap"));
			var ptr = Native.SearchForWindow("VLC media player");
			Console.WriteLine(ptr);
			var v = Native.SendMessage(ptr, 0x0100, (int) VirtualKey.SPACE, (IntPtr) 0x002C0001);
			Console.WriteLine(v);
		}
	}
}