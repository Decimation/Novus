// ReSharper disable LocalizableElement
// ReSharper disable RedundantUsingDirective
// ReSharper disable RedundantUnsafeContext

global using Native = Novus.Win32.Native;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.SymbolStore;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Kantan.Text;
using Microsoft.VisualBasic.FileIO;
using Novus;
using Novus.FileTypes;
using Novus.Memory;
using Novus.OS;
using Novus.Runtime.Meta;
using Novus.Utilities;
using Novus.Win32;
using Novus.Win32.Structures.Kernel32;
using Novus.Win32.Structures.User32;
using FileType = Novus.FileTypes.FileType;
#pragma warning disable IDE0005, CS0436, CS0469
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Novus.Runtime;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

// ReSharper disable ClassNeverInstantiated.Local

// ReSharper disable PossibleNullReferenceException

// ReSharper disable ArrangeTypeModifiers
// ReSharper disable UnusedType.Local
// ReSharper disable InconsistentNaming
#pragma warning disable CS0649

// ReSharper disable UnusedParameter.Local
#nullable disable

namespace Test;

// # .NET 7
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

public static class Program
{
	private static async Task Main(string[] args)
	{

	}

	private static async Task Test5()
	{
		var fileType = FileType.Find("image").ToArray();

		var t = await UniFile.TryGetAsync("https://i.imgur.com/QtCausw.png", whitelist: fileType);
		Console.WriteLine(t);
		var f = await t.DownloadAsync();
		Console.WriteLine(f);

		var ft = Activator.CreateInstance<FileType>();
		Console.WriteLine(ft);
	}

	public interface IPtr<T>
	{
		static abstract ref T Ref { get; }
	}

	private static void Test4()
	{
		var c = Native.OpenClipboard();

		var f = Native.EnumClipboardFormats();

		var p = Native.GetClipboardData(49159);
		var s = Marshal.PtrToStringUni(p);
		Console.WriteLine(s);
		Console.WriteLine(Native.GetClipboard());
		Console.WriteLine(Native.GetClipboardFileName());

	}

	private static void Test3()
	{
		var type = new MyStruct();

		foreach (var nullMember in type.GetNullMembers()) {
			Console.WriteLine($"{nullMember.Field.Name} {nullMember.IsNull}");
		}
	}

	static void err()
	{
		throw new Exception();
	}

	static int fn()
	{
		return 1;
	}

	private struct MyStruct
	{
		public int a;

		public float f { get; set; }

		public override string ToString()
		{
			return $"{nameof(a)}: {a}, {nameof(f)}: {f}";
		}
	}

	private class MyClass
	{
		public int a;

		public float f { get; set; }

		public override string ToString()
		{
			return $"{nameof(a)}: {a}, {nameof(f)}: {f}";
		}
	}

	private static int MyThreadProc(IntPtr param)
	{
		var process = Process.GetCurrentProcess();
		int pid     = process.Id;

		Console.WriteLine("Pid {0}: Inside my new thread!. Param={1}", pid, param.ToInt32());

		return 1;
	}

	private static void WaitForThreadToExit(IntPtr hThread)
	{
		var c  = Native.WaitForSingleObject(hThread, unchecked((uint) -1));
		var ex = Marshal.GetExceptionForHR((int) c);

		Native.GetExitCodeThread(hThread, out uint exitCode);

		var process = Process.GetCurrentProcess();
		int pid     = process.Id;

		Console.WriteLine("Pid {0}: Thread exited with code: {1}", pid, exitCode);
	}

	private static unsafe void Test2(string[] args, Process p)
	{
		var pid = (uint) p.Id;

		if (args.Length == 0) {
			Console.WriteLine("Pid {0}:Started Parent process", pid);

			// Spawn the child
			string fileName = p.MainModule.FileName.Replace(".vshost", "");

			// Get thread proc as an IntPtr, which we can then pass to the 2nd-process.
			// We must keep the delegate alive so that fpProc remains valid

			Native.ThreadProc proc   = MyThreadProc;
			IntPtr            fpProc = Marshal.GetFunctionPointerForDelegate(proc);

			// Spin up the other process, and pass our pid and function pointer so that it can
			// use that to call CreateRemoteThread

			string arg = $"{pid} {fpProc}";

			var info = new ProcessStartInfo(fileName, arg)
			{
				// share console, output is interlaces.
				UseShellExecute = false
			};

			var processChild = Process.Start(info);

			processChild.WaitForExit();
			GC.KeepAlive(proc); // keep the delegate from being collected
			return;
		}
		else {
			Console.WriteLine("Pid {0}:Started Child process", pid);

			uint pidParent = UInt32.Parse(args[0]);
			var  fpProc    = new UIntPtr(UInt64.Parse(args[1]));

			IntPtr hProcess = Native.OpenProcess(ProcessAccess.All, false, (int) pidParent);

			// Create a thread in the first process.
			IntPtr hThread = Native.CreateRemoteThread(hProcess, IntPtr.Zero, 0,
			                                           (IntPtr) fpProc.ToPointer(), new IntPtr(6789),
			                                           0, out uint dwThreadId);
			WaitForThreadToExit(hThread);
			return;
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