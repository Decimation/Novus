using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml;
using Novus;
using Novus.Memory;
using Novus.Runtime;
using Novus.Runtime.Meta;
using Novus.Runtime.Meta.Jit;
using Novus.Runtime.VM;
using Novus.Utilities;
using Novus.Win32;
using Novus.Win32.Structures;
using SimpleCore.Utilities;
#nullable enable
// ReSharper disable LocalizableElement

namespace Test
{
	/*
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
	 * todo: fully migrate NeoCore and RazorSharp
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


		Native.GetSymbol(Native.GetCurrentProcess(),@"C:\Users\Deci\Desktop\coreclr.pdb", "g_pGCHeap");
			
		}
	}
}