using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using Novus.Utilities;
using SimpleCore.Diagnostics;

// ReSharper disable UnusedMember.Global

[assembly: InternalsVisibleTo("Test")]
#nullable enable
namespace Novus
{
	/// <summary>
	///     Global and resources
	/// </summary>
	public static class Global
	{
		

		public static Resource Clr { get; } = new("coreclr.dll");

		public static readonly Version ClrVersion = new(5, 0, 0);

		/// <summary>
		///     Module initializer
		/// </summary>
		[ModuleInitializer]
		public static void Setup()
		{
			/*
			 *
			 */

			Debug.WriteLine(">>> Module init <<<");

			bool compatible = IsCompatible();

			if (!compatible) {
				Guard.Fail();
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

				var dependencies = ReflectionHelper.GetUserDependencies(assembly);

				asm.UnionWith(dependencies);

			}

			return asm;
		}

		public static bool IsCompatible()
		{
			bool ver = Environment.Version == ClrVersion;
			bool gc  = !GCSettings.IsServerGC;
			bool os  = OperatingSystem.IsWindows();


			return ver && gc && os;
		}
	}
}