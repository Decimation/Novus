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
using Novus.OS.Win32.Structures.Ntdll;
using static Novus.OS.Win32.Native;
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
	// stdcall        
	static int MyThreadProc(IntPtr param)
	{
		int pid = Process.GetCurrentProcess().Id;
		Console.WriteLine("Pid {0}: Inside my new thread!. Param={1}", pid, param.ToInt32());
		return 1;
	}

	// Helper to wait for a thread to exit and print its exit code
	static void WaitForThreadToExit(IntPtr hThread)
	{
		WaitForSingleObject(hThread, unchecked((uint) -1));

		uint exitCode;
		GetExitCodeThread(hThread, out exitCode);
		int pid = Process.GetCurrentProcess().Id;
		Console.WriteLine("Pid {0}: Thread exited with code: {1}", pid, exitCode);
	}

	private static void Main(string[] args)
	{
		int pid = Process.GetCurrentProcess().Id;

		if (args.Length == 0) {
			Console.WriteLine("Pid {0}:Started Parent process", pid);

			// Spawn the child
			string fileName = Process.GetCurrentProcess().MainModule.FileName.Replace(".vshost", "");

			// Get thread proc as an IntPtr, which we can then pass to the 2nd-process.
			// We must keep the delegate alive so that fpProc remains valid
			ThreadProc proc   = MyThreadProc;
			IntPtr     fpProc = Marshal.GetFunctionPointerForDelegate(proc);

			// Spin up the other process, and pass our pid and function pointer so that it can
			// use that to call CreateRemoteThraed
			string           arg  = String.Format("{0} {1}", pid, fpProc);
			ProcessStartInfo info = new ProcessStartInfo(fileName, arg);
			info.UseShellExecute = false; // share console, output is interlaces.
			Process processChild = Process.Start(info);

			processChild.WaitForExit();
			GC.KeepAlive(proc); // keep the delegate from being collected
			return;
		}
		else {
			Console.WriteLine("Pid {0}:Started Child process", pid);
			uint   pidParent = uint.Parse(args[0]);
			IntPtr fpProc    = new IntPtr(uint.Parse(args[1]));

			IntPtr hProcess = OpenProcess(ProcessAccess.All, false, (int) pidParent);

			uint dwThreadId;

			// Create a thread in the first process.
			IntPtr hThread = CreateRemoteThread(
				hProcess,
				IntPtr.Zero,
				0,
				fpProc, new IntPtr(6789),
				0,
				out dwThreadId);
			WaitForThreadToExit(hThread);
			return;
		}
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