using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Novus.CoreClr.VM;
using Novus.Properties;
using Novus.Utilities;
using SimpleCore.Diagnostics;
using SimpleCore.Internal;
using SimpleCore.Utilities;

[assembly: InternalsVisibleTo("Test")]

namespace Novus
{
	public static unsafe class Global
	{
		/// <summary>
		/// Module initializer
		/// </summary>
		[ModuleInitializer]
		public static void Setup()
		{
			Trace.WriteLine(">>> Module init <<<");

			bool c = IsCompatible();

			if (!c) {
				Guard.Fail();
			}

			foreach (var assemblyName in DumpDependencies()) {
				Trace.WriteLine(assemblyName.Name, "Dependency");
				//Write("{0}", assemblyName.Name);
			}
		}

		

		public static HashSet<AssemblyName> DumpDependencies()
		{
			var rg = new[]
			{
				//
				//typeof(Global).Assembly,
				//Assembly.GetExecutingAssembly(),
				//
				Assembly.GetCallingAssembly()
			};

			var asm = new HashSet<AssemblyName>();

			foreach (var assembly in rg) {

				var dependencies = RuntimeInfo.GetUserDependencies(assembly);

				asm.UnionWith(dependencies);

			}
			
			return asm;
		}

		public static readonly Version ClrVersion = new(5, 0, 0);

		public static bool IsCompatible()
		{
			bool ver = Environment.Version == ClrVersion;
			bool gc  = !GCSettings.IsServerGC;
			bool os  = OperatingSystem.IsWindows();

			return ver && gc && os;
		}
	}
}