using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Novus;
using Novus.Memory;
using Novus.OS;
using Novus.Properties;
using Novus.Runtime.Meta;
using Novus.Utilities;
using Novus.Win32;
using Novus.Win32.Structures.Kernel32;
using Novus.Win32.Structures.User32;

namespace Test
{
	internal class Tests
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
			var currentProcess = Mem.Locate(ptr, Process.GetCurrentProcess());
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
			Console.WriteLine(Clipboard.GetData());

		}

		private static void Test3()
		{
			var type = new MyStruct2();

			foreach (var nullMember in ReflectionHelper.GetNullMembers(type)) {
				Console.WriteLine($"{nullMember.Field.Name} {nullMember.IsNull}");
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
}
