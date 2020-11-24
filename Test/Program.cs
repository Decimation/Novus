using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Novus;
using Novus.CoreClr.Meta;
using Novus.Memory;
using Novus.Utilities;
using Novus.Win32;
using Novus.Win32.ContextMenu;

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

	/*
	 * TODO: Registry
	 */

	public static unsafe class Program
	{
		private static void Main(string[] args)
		{
			
			nint n;
			Console.WriteLine(sizeof(nint));
			var mt = typeof(string).AsMetaType();
			Console.WriteLine(mt.InstanceFieldsSize);
			Console.WriteLine();
			Global.DumpDependencies();

			var mm = typeof(Program).GetAnyMethod(nameof(sayhi)).AsMetaMethod();
			Console.WriteLine(mm);
			Console.WriteLine(mm.IsPointingToNativeCode);
			RuntimeHelpers.PrepareMethod(mm.MethodInfo.MethodHandle);
			Console.WriteLine(mm.IsPointingToNativeCode);

		}

		public static void sayhi()
		{
			Console.WriteLine("h");
		}

		private static void test1()
		{
			Console.WriteLine(Environment.Version);
			Global.Setup();

			CascadingContextMenuEntry x = new("myactiontoplevel");
			


			var e = x.GetStub();
			e.Base.Main    = "My action 1";
			e.Command.Main = @"C:\\Users\\Deci\\Desktop\\SmartImage.exe " + "--priority-engines All " + "\\\"%1\\\"";
			x.Items.Add(e);
			x.Base.Values.Add("MUIVerb", "My tool");
			x.Base.Values.Add("Icon", @"C:\\Users\\Deci\\Desktop\\SmartImage.exe");


			foreach (string s in x.ToRegistry())
			{
				Console.WriteLine(s);
			}

			var f = @"C:\Users\Deci\Desktop\regtest.reg";

			RegistryHelper.WriteRegistryFile(x.ToRegistry(), f);
			RegistryHelper.Install(f);
			Console.ReadLine();
			RegistryHelper.Remove(@"HKEY_CLASSES_ROOT\*\shell\myactiontoplevel\");
			Console.ReadLine();
		}
	}
}
