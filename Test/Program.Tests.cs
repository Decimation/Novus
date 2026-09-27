// Author: Deci | Project: Test | Name: Program.Tests.cs
// Date: 2026/09/26 @ 23:09:04

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using Kantan.Text;
using Novus;
using Novus.FileTypes.Media;
using Novus.FileTypes.Resolvers;
using Novus.Memory;
using Novus.Memory.Allocation;
using Novus.OS;
using Novus.Runtime;
using Novus.Runtime.Meta;
using Novus.Runtime.VM;
using Novus.Streams;
using Novus.Utilities;
using Novus.Win32;
using Novus.Win32.Structures.AdvApi32;
using Novus.Win32.Structures.DbgHelp;
using UnitTest.TestTypes;

namespace Test;

public static partial class Program
{

	private static async Task TestMediaType2(Stream s)
	{
		var (mt, stream) = await ((DatabaseResolver) DatabaseResolver.Instance).SniffAsync(s);
		Console.WriteLine($"{mt}, {stream.Length}");
	}

	private static async Task TestMediaType(string u1)
	{
		var req    = await u1.WithHeaders(new { User_Agent = ER.UserAgent }).GetAsync();
		var stream = await req.GetStreamAsync();

		var hdrStr = await stream.GetHeaderAsync(MediaTypeUtilities.RSRC_HEADER_LEN);
		var hdr    = await hdrStr.ReadHeaderAsync(MediaTypeUtilities.RSRC_HEADER_LEN);

		for (int i = 0; i < hdr.Length; i++) {
			Console.Write($"{hdr[i]:X} ");
		}

		Console.WriteLine();

		Console.WriteLine(MediaTypeUtilities.IsBinaryResource(hdr));
		Console.WriteLine(DatabaseResolver.Instance.Resolve(hdr));
	}

	private static void TestResType()
	{
		foreach (var val0 in MediaTypeUtilities.ApacheBugContentTypes) {
			Console.WriteLine(val0);
			var rg = Encoding.Default.GetBytes(val0.ToString());

			for (int i = 0; i < rg.Length; i++) {
				Console.Write($"{rg[i]:X} ");
			}

			Console.WriteLine();
		}

		var bstr      = "74 65 78 74 2F 70 6C 61 69 6E 3B 20 63 68 61 72 73 65 74 3D 49 53 4F 2D 38 38 35 39 2D 31";
		var aobString = Mem.ParseAOBString(bstr);
		Console.WriteLine(aobString);
		var s = Encoding.Default.GetString(aobString);
		Console.WriteLine(s);
		Console.WriteLine(s == MediaTypeUtilities.ApacheBugContentTypes[1].ToString());

		foreach (var type in ((DatabaseResolver) DatabaseResolver.Instance).All) {
			Console.WriteLine(type);
			Console.WriteLine($"{type.Signatures.Length}");

			foreach (var signature in type.Signatures) {
				Console.WriteLine($"\t{signature}");
			}
		}

		Console.WriteLine(((DatabaseResolver) DatabaseResolver.Instance).Find("image/png"));
	}

	private static void TestSym2()
	{
		var fileName = Process.GetCurrentProcess().FindModule("coreclr.dll").FileName;
		var pdb      = @"C:\Symbols\coreclr.pdb\85DECBA7C49F4EDF8283BF735FB7D7C21\coreclr.pdb";
		var sym      = new SymbolHandler(fileName);

		var hProcess = new IntPtr(0x1337);

		Native.SymSetOptions(SymbolOptions.DEFERRED_LOADS | SymbolOptions.UNDNAME);
		Native.SymInitialize(hProcess, null, false);
		var baseAddr = Native.SymLoadModuleEx(hProcess, IntPtr.Zero, fileName, null, 0, 0, IntPtr.Zero, 0);
		Console.WriteLine(baseAddr);

		var s = new ImageHelpModule64();
		s.SizeOfStruct = (uint) Marshal.SizeOf<ImageHelpModule64>();
		Native.SymGetModuleInfoW64(hProcess, baseAddr, ref s);
	}

	private static void TestAlloc1()
	{
		var ls = ObjectUtility.New<List<int>>();
		ls.Add(1);
		Console.WriteLine(ls);
		Console.WriteLine(GCHeap.IsHeapPointer(ls));
		AllocManager.Free(ls);

		var obj     = "foo";
		var clrObj  = obj.AsClrObject();
		var clrObj2 = Unsafe.As<string, Pointer<ClrObject>>(ref obj);

		Console.WriteLine(clrObj);
		Console.WriteLine(clrObj2);
	}

	private static void TestSym1()
	{
		var sb  = new StringBuilder(2048);
		var sb2 = new StringBuilder(4096);

		var hProcess = Random.Shared.Next();

		var ok = Native.SymInitialize(hProcess);

		Console.WriteLine(ok);

		ok |= Native.SymGetSymbolFile(hProcess: hProcess, ImageFile: "coreclr.pdb", Type: IMAGEHLP_SF_TYPE.sfImage, SymbolFile: sb, cSymbolFile: sb.Capacity,
		                              DbgFile: sb2, cDbgFile: sb2.Capacity);
		Console.WriteLine(ok);
		Console.WriteLine(sb);
		Console.WriteLine(sb2);

		var peReader = new PEReader(File.OpenRead(Global.Clr.Module.FileName));

		var codeViewEntry = peReader.ReadDebugDirectory()
		                            .First(entry => entry.Type == DebugDirectoryEntryType.CodeView);

		var pdbData = peReader.ReadCodeViewDebugDirectoryData(codeViewEntry);
		Console.WriteLine(pdbData.Path);

		/*var mem = AllocManager.New<List<int>>();
		Console.WriteLine(mem);
		mem.Add(1);
		Console.WriteLine(mem.Count);*/
	}

	private static void TestClipboard1()
	{
		Console.WriteLine(Clipboard.Open());

		var fmt = Clipboard.EnumFormats();

		foreach (uint u in fmt) {
			Console.WriteLine($"[{Clipboard.GetFormatName(u)}]");
		}

		var data = Clipboard.GetData((uint) ClipboardFormat.PNG3);
		Console.WriteLine(data);
	}

	private static unsafe void Test7()
	{
		MyClass obj = new MyClass { s = "foo", a = 123 };
		Console.WriteLine(obj.s);

		var mt  = obj.GetMetaType();
		var mf  = mt.GetField(nameof(MyClass.s));
		var buf = stackalloc byte[8];
		mf.GetInstanceField(obj, buf);

		var fld = Mem.AddressOfField(obj, nameof(MyClass.s)).Cast<string>();
		Console.WriteLine(fld.ReadPointer());
		var ptr = (ulong*) buf;
		Console.WriteLine($"{*ptr:X}");

		var str2    = "bar";
		var str2Val = Mem.AddressOf(ref str2);
		Console.WriteLine(str2Val);

		mf.SetInstanceField(obj, str2Val);

		Console.WriteLine(obj.s);
	}

	private static unsafe void Test6()
	{
		// var mc=new MyClass2b();

		MyClass obj   = new MyClass { s = "foo", a = 123 };
		var     addr  = (Pointer<byte>) Unsafe.AsPointer(ref obj);
		var     addr2 = (Pointer<byte>) addr.ReadPointer();
		Console.WriteLine($"&obj = {addr} -> {addr2}");

		var heap = Mem.AddressOfHeap(obj);
		Console.WriteLine($"Heap: {heap}");

		Pointer<MethodTable> mt = ObjectUtility.GetMethodTable(obj);

		// Type                 t   = typeof(MyClass);
		// TypeHandle           mt2 = ObjectUtility.GetTypeHandle(t);

		Console.WriteLine($"MT: {mt}");

		int hSize = Mem.SizeOf(obj, SizeOfOption.Heap);

		Console.WriteLine($"Heap size: {hSize}");

		for (int i = 0; i < hSize; i++) {
			Console.Write($"{heap[i]:X} ");
		}

		Debugger.Break();
	}

	private static void Test4()
	{
		ProcessModuleCollection pss = Process.GetCurrentProcess().Modules;

		foreach (ProcessModule processModule in pss) {
			Console.WriteLine(processModule);

		}

		foreach (string s in FileSystem.EnumerateInPath("coreclr.pdb", EnvironmentVariableTarget.Machine)) {
			Console.WriteLine(s);
		}

		foreach (string s in SymbolHandler.EnumerateSymbolPath("coreclr.pdb")) {
			Console.WriteLine(s);
		}
	}

	private static IDictionary<string, Func<int, object>> fns = new Dictionary<string, Func<int, object>>
		{ }.AsReadOnly();

	public static IDictionary<string, int> Get<T>(T t = default)
	{
		Dictionary<string, int> vals = [];

		foreach (SizeOfOption i in Enum.GetValues<SizeOfOption>()) {

			vals.Add(i.ToString(), Mem.SizeOf<T>(t, i));
		}

		unsafe {
			vals.Add("sizeof", sizeof(T));
		}

		return vals;
	}

	private static void Test3()
	{
		ScHandle s  = Native.OpenSCManager(null, null, ScManagerAccessTypes.SC_MANAGER_ALL_ACCESS);
		ScHandle s2 = Native.OpenService(s, "NvContainerLocalSystem", ServiceAccessTypes.SERVICE_ALL_ACCESS);
		Native.ControlService(s2, ServiceControl.SERVICE_CONTROL_STOP, out ServiceStatus ss);
		Console.WriteLine(ss);
		Native.CloseServiceHandle(s);
		Native.CloseServiceHandle(s2);
	}

	private static unsafe void Test2()
	{
		Clazz3     clazz = new Clazz3();
		MetaMethod mm    = clazz.GetType().GetAnyMethod("SayHi").AsMetaMethod();
		MetaMethod mm2   = clazz.GetType().GetAnyMethod("SayBar").AsMetaMethod();
		clazz.SayHi();
		clazz.SayBar();
		mm.Reset();
		mm.EntryPoint = (void*) mm2.Function;
		clazz.SayHi();

		// delegate* unmanaged<void> au = &clazz.SayBar;
	}

	private static void Test1()
	{
		MyClass mc = new MyClass { a = 321, s = "bar" };
		byte[]  rg = ObjectUtility.GetBytes(mc);
		Console.WriteLine(rg.FormatJoin("X", delim: " "));
		object mc2 = ObjectUtility.ReadFromBytes<object>(rg);
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

	public static unsafe delegate* managed<int> f;

}