using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Novus.Win32
{
	public static class RegistryHelper
	{
		public static void WriteRegistryFile(string[] reg, string dest)
		{
			
			var rg = new List<string>()
			{
				"Windows Registry Editor Version 5.00",
				"\n"
			};

			rg.AddRange(reg);


			File.WriteAllLines(dest, rg);
		}
		
		public static void Remove(string k)
		{
			// bypass

			string[] removeCode =
			{
				"@echo off",
				$@"reg.exe delete {k} /f >nul"
			};

			Command.RunBatch(removeCode,true);
		}

		public static void Install(string reg)
		{
			var proc = new Process();

			proc.StartInfo = new ProcessStartInfo()
			{
				FileName        = "regedit.exe",
				Arguments       = $"/s \"{reg}\"",
				Verb            = "runas",
				UseShellExecute = true,

			};
			proc.Start();
			proc.WaitForExit();
		}
	}
}