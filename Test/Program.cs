// ReSharper disable LocalizableElement
// ReSharper disable RedundantUsingDirective
// ReSharper disable RedundantUnsafeContext

#pragma warning disable IDE0005

using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
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
using SimpleCore.Diagnostics;
using SimpleCore.Utilities;
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
	 * SimpleCore			https://github.com/Decimation/SimpleCore
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

			Console.WriteLine(s);

			var r = Resource.LoadModule(s);
			var sig=r.FindSignature("89 54 24 10 89 4C 24 08");
			Console.WriteLine(sig);

			Console.WriteLine(call<int>(sig,1,1));
		}
		private const string s  = "C:\\Users\\Deci\\VSProjects\\SandboxLibrary\\x64\\Release\\SandboxLibrary.dll";
		private const string s2 = "SandboxLibrary.dll";
		public static int MultiplySum(int a, int b, int c, int d, int e, int x)
		{
			return a + b + c + d + e+x;
		}

		static T call<T>(Pointer<byte> p, params object[] args)
		{
			var           types = args.Select(s => s.GetType()).ToArray();
			DynamicMethod m              = new DynamicMethod("Call", typeof(T), types);
			var           il             = m.GetILGenerator();

			Console.WriteLine(types.QuickJoin());

			//for (int i = 0; i < args.Length; i++)
			//{
			//	il.Emit(OpCodes.Ldarg_S, i + 1);
			//}

			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldarg_2);

			/*
			 * IL_0003:  ldarg.0
	  IL_0004:  conv.i
	  IL_0005:  calli      void*(void*,void*,!!T3)
	  IL_000a:  ret
			 */
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Conv_I);
			il.EmitCalli(OpCodes.Calli, CallingConvention.StdCall, typeof(T), types);
			il.Emit(OpCodes.Ret);

			//var types2 = types.ToList();
			//types2.Add(typeof(T));

			//var rx     =Expression.GetDelegateType(types2.ToArray());
			//Console.WriteLine(rx);

			var f  = m.CreateDelegate<Func<long,int,int,int>>();
			var di = f(p.ToInt64(),(int)args[0], (int)args[1]);

			return (T)(object)di;
		}
	}
}