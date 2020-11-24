using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using SimpleCore.Diagnostics;
using SimpleCore.Utilities;

[assembly: InternalsVisibleTo("Test")]
namespace Novus
{
	public static class Global
	{
		public static void Setup()
		{
		
			bool c = IsCompatible();

			if (!c) {
				Guard.Fail();
			}
		}

		public static void DumpDependencies()
		{
			var rg = new[]
			{
				//
				typeof(Global).Assembly,
				//
				Assembly.GetCallingAssembly()
			};


			foreach (var assembly in rg) {

				try {
					var name         = assembly.GetName();
					var dependencies = RuntimeInfo.GetUserDependencies(assembly).ToList();

					Console.WriteLine("{0}: ", name.Name);

					foreach (var asmDependency in dependencies) {
						var ver = File.GetLastWriteTime(RuntimeInfo.GetAssemblyByName(asmDependency.FullName).Location);

						Console.WriteLine("\t{0}:\t({1}) (modified {2})", asmDependency.Name, asmDependency.Version,
							ver);
					}

					Console.WriteLine();
				}
				catch (Exception e) {
					Console.WriteLine(e);
					throw;
				}
			}


		}

		public static readonly Version ClrVersion = new(5, 0, 0);

		public static bool IsCompatible()
		{
			bool ver = Environment.Version == ClrVersion;
			bool b   = !GCSettings.IsServerGC;

			return ver && b;
		}
	}
}