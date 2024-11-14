using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Novus;
using Novus.Memory;
using Novus.Memory.Allocation;
using Novus.OS;
using Novus.Properties;
using Novus.Runtime;
using Novus.Runtime.Meta;
using Novus.Utilities;
using Novus.Win32;
using Novus.Win32.Structures.Kernel32;
using Novus.Win32.Structures.User32;
using Test.TestTypes;

namespace Test;

public static class Tests2
{

	public delegate ref int Del(in int o);

	[UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = nameof(Clazz2.Func))]
	public static extern int Func1(Clazz2 c);

	static void t2(ref int r)
	{
		r = 321;
	}

	public static unsafe void Test2()
	{
		var o = (MyClass2) RuntimeHelpers.GetUninitializedObject(typeof(MyClass2));
		Console.WriteLine(GCHeap.IsHeapPointer(o));
		Console.WriteLine(RuntimeProperties.IsBoxed(o));

		var o2 = (MyStruct) RuntimeHelpers.GetUninitializedObject(typeof(MyStruct));
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

	public static unsafe void Test3()
	{
		Clazz2 o = (Clazz2) RuntimeHelpers.GetUninitializedObject(typeof(Clazz2));

		var f = Func1(o);

		Console.WriteLine(f);
		delegate*<Clazz2, int> fn = &Func1;

		Console.WriteLine((Pointer<byte>) fn);

		delegate* <int> fn2 = ((&Clazz2.Func));

		Console.WriteLine((Pointer<byte>) fn2);
		Console.WriteLine(Thread.CurrentThread.ManagedThreadId);

		// Debugger.Break();
		Console.ReadLine();
	}

	public static void TestAlloc1()
	{
		var v = AllocManager.New<Clazz3>(ctor: [3, "foo", 1]);
		Console.WriteLine(v);

		AllocManager.Free(v);
	}

}
internal class Tests1
{

	private static readonly IntPtr HWND_MESSAGE       = new IntPtr(-3);
	private const           int    WM_CLIPBOARDUPDATE = 0x031D;

	private delegate void ClipboardUpdateCallback();

	private static void WndProc(ref MSG m)
	{
		if (m.message == WM_CLIPBOARDUPDATE) {
			// The clipboard contents have changed
			OnClipboardChanged();
		}

		WndProc(ref m);
	}

	private static void OnClipboardChanged()
	{
		// Check if the clipboard contains a specific format
		bool hasFormat = Native.IsClipboardFormatAvailable((int) ClipboardFormat.CF_UNICODETEXT);

		if (hasFormat) {
			// Get the clipboard data and do something with it
			IntPtr clipboardData = Native.GetClipboardData((int) ClipboardFormat.CF_UNICODETEXT);

			if (clipboardData != IntPtr.Zero) {
				string clipboardText = Marshal.PtrToStringUni(clipboardData);

				// ...
				Console.WriteLine(clipboardText);
			}
		}
	}

	private static void Test5()
	{
		// MyClass2.doSomething2(1);
		Console.WriteLine("hi");

		Console.WriteLine("bye");

		int i   = 123;
		var ptr = GCHeap.GlobalHeap;

		Console.WriteLine(ptr);
		var currentProcess = Mem.FindInProcessMemory(Process.GetCurrentProcess(), ptr);
		Console.WriteLine($"{currentProcess.Item1}");
		Console.WriteLine($"{currentProcess.Item2}");

		var o = Global.Clr.Symbols.Value.GetSymbols(EmbeddedResources.Sym_IsHeapPointer);

		foreach (var symbol in o) {
			Console.WriteLine(symbol);
		}

		Console.WriteLine(GCHeap.IsHeapPointer("foo"));
	}

	private static void Test4()
	{
		bool c = Clipboard.Open();

		uint[] f = Clipboard.EnumFormats();

		nint   p = Native.GetClipboardData(49159);
		string s = Marshal.PtrToStringUni(p);
		Console.WriteLine(s);
		Console.WriteLine(Clipboard.GetData(Clipboard.EnumFormats().FirstOrDefault()));

	}

	private static void Test3()
	{
		var type = new MyStruct2();

		foreach (var nullMember in type.GetNullMembers()) {
			Console.WriteLine($"{nullMember.Key.Name} {nullMember}");
		}
	}

	private static void err()
	{
		throw new Exception();
	}

	private static int fn()
	{
		return 1;
	}

	private static int MyThreadProc(nint param)
	{
		var process = Process.GetCurrentProcess();
		int pid     = process.Id;

		Console.WriteLine("Pid {0}: Inside my new thread!. Param={1}", pid, param.ToInt32());

		return 1;
	}

	private static void WaitForThreadToExit(nint hThread)
	{
		uint c  = Native.WaitForSingleObject(hThread, unchecked((uint) -1));
		var  ex = Marshal.GetExceptionForHR((int) c);

		Native.GetExitCodeThread(hThread, out uint exitCode);

		var process = Process.GetCurrentProcess();
		int pid     = process.Id;

		Console.WriteLine("Pid {0}: Thread exited with code: {1}", pid, exitCode);
	}

	private static unsafe void Test2(string[] args, Process p)
	{
		uint pid = (uint) p.Id;

		if (args.Length == 0) {
			Console.WriteLine("Pid {0}:Started Parent process", pid);

			// Spawn the child
			string fileName = p.MainModule.FileName.Replace(".vshost", "");

			// Get thread proc as an IntPtr, which we can then pass to the 2nd-process.
			// We must keep the delegate alive so that fpProc remains valid

			Native.ThreadProc proc   = MyThreadProc;
			nint              fpProc = Marshal.GetFunctionPointerForDelegate(proc);

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

			uint  pidParent = UInt32.Parse(args[0]);
			nuint fpProc    = new nuint(UInt64.Parse(args[1]));

			nint hProcess = Native.OpenProcess(ProcessAccess.All, false, (int) pidParent);

			// Create a thread in the first process.
			nint hThread = Native.CreateRemoteThread(hProcess, IntPtr.Zero, 0,
			                                         (nint) fpProc.ToPointer(), new nint(6789),
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