using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Novus;
using Novus.Memory;
using Novus.Runtime;
using Novus.Runtime.Meta;
using Novus.Utilities;
using Novus.Win32;
using Novus.Win32.ContextMenu;
// ReSharper disable LocalizableElement

namespace Test
{
	/*
	 * C:\Program Files\dotnet\shared\Microsoft.NETCore.App\5.0.0
	 * C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.9
	 * C:\Windows\Microsoft.NET\Framework64\v4.0.30319
	 *
	 * symchk "input" /s SRV*output*http://msdl.microsoft.com/download/symbols
	 *
	 * todo: integrate pdbex
	 * todo: IL, ILSupport
	 * todo: use resources
	 * todo: fully migrate NeoCore and RazorSharp
	 *
	 * 
	 * 
	 *
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

	/*
	 * TODO: Registry
	 */

	
	
	public static unsafe class Program
	{
		private static void Main(string[] args)
		{
			
			var p = new Point();


			Inspector.DumpLayout(ref p);

			

			var s = "foo";
			Inspector.DumpLayout(ref s);
			Inspector.DumpInfo(ref s);

			var t = typeof(Program).GetAllFields();

			foreach (var info in t) {
				Console.WriteLine("{0} {1}",info, info.FieldType);
			}

			var rg = new string[] {"foo"};
			
			Console.WriteLine(Functions.Func_IsPinnable(rg));
			
			Console.WriteLine(Functions.Func_IsPinnable(1));
		}
	}
}