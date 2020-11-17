using System;
using System.Collections.Generic;
using System.Linq;
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


	public static class Program
	{
		private static void Main(string[] args)
		{
			Console.WriteLine(Environment.Version);
			Global.Setup();

			CascadingContextMenuEntry x = new("myactiontoplevel");
			
			

			var e = x.GetStub();
			e.Base.Main    = "My action 1";
			e.Command.Main = @"C:\\Users\\Deci\\Desktop\\SmartImage.exe " + "--priority-engines All " + "\\\"%1\\\"";
			x.Items.Add(e);
			x.Base.Values.Add("MUIVerb","My tool");
			x.Base.Values.Add("Icon", @"C:\\Users\\Deci\\Desktop\\SmartImage.exe");


			foreach (string s in x.ToRegistry()) {
				Console.WriteLine(s);
			}

			var f = @"C:\Users\Deci\Desktop\regtest.reg";

			RegistryOperations.WriteRegistryFile(x.ToRegistry(), f);
			RegistryOperations.Install(f);
			Console.ReadLine();
			RegistryOperations.Remove(@"HKEY_CLASSES_ROOT\*\shell\myactiontoplevel\");
			Console.ReadLine();
		}
	}
}
