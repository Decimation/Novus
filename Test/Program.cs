using System;
using System.Linq;
using Novus;
using Novus.CoreClr.Meta;
using Novus.Memory;
using Novus.Utilities;

namespace Test
{
	/*
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
	 * https://github.com/sidristij/dotnetex
	 *
	 * https://github.com/wbenny/pdbex
	 *
	 * https://github.com/ins0mniaque/ILSupport
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


	public static class Program
	{
		private static void Main(string[] args)
		{
			Global.Setup();
			

			string s = "foo";
			Console.WriteLine(Mem.HeapSizeOf(s));

			Console.WriteLine(Mem.AddressOfHeap(s));

			foreach (var f in s.GetType().GetAllFields()) {
				var mf = f.AsMetaField();

				Console.WriteLine($"{mf.Size} {mf.Offset} {mf}");
			}

			Console.WriteLine(sizeof(bool));
			Console.WriteLine(Mem.SizeOf2<bool>(SizeOfOptions.Native));

			Console.WriteLine(Mem.AddressOfHeap(s, OffsetOptions.StringData).Cast<char>()[0]);
			Console.ReadLine();
		}
	}
}
