using System;
using System.Diagnostics;
using System.IO;
using System.Runtime;
using System.Runtime.CompilerServices;
using Kantan.Cli;
using Novus.Memory;
using Novus.Properties;
using Novus.Runtime;
using Novus.Win32;
using Kantan.Diagnostics;
using Kantan.Utilities;
using static Kantan.Diagnostics.LogCategories;
// ReSharper disable LocalizableElement

// ReSharper disable UnusedMember.Global
[assembly: InternalsVisibleTo("Ultrakiller")]
[assembly: InternalsVisibleTo("Test")]
[assembly: InternalsVisibleTo("TestBenchmark")]
#nullable disable

namespace Novus
{
	/// <summary>
	///     Core functionality and global resources.
	/// </summary>
	/// <remarks>
	///     Central runtime utilities:
	///     <list type="bullet">
	///         <item>
	///             <description>
	///                 <see cref="Global" />
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 <see cref="Resource" />
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 <see cref="EmbeddedResources" />
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 <see cref="Mem" />
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 <see cref="RuntimeInfo" />
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 <see cref="Inspector" />
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 <see cref="Pointer{T}" />
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 <see cref="SigScanner" />
	///             </description>
	///         </item>
	///     </list>
	///     OS utilities:
	///     <list type="bullet">
	///         <item>
	///             <description>
	///                 <see cref="Native" />
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 <see cref="SymbolLoader" />
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 <see cref="FileSystem" />
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 <see cref="Command" />
	///             </description>
	///         </item>
	///     </list>
	/// </remarks>
	public static class Global
	{
		/// <summary>
		///     Runtime CLR module name
		/// </summary>
		public const string CLR_MODULE = "coreclr.dll";

		public const string LIB_NAME = "Novus";

		/// <summary>
		///     Runtime CLR version
		/// </summary>
		public static readonly Version ClrVersion = Version.Parse(EmbeddedResources.RequiredVersion);


		/// <summary>
		///     Runtime CLR resources
		/// </summary>
		public static Resource Clr { get; } = new(CLR_MODULE);

		public static bool IsSetup { get; private set; }

		public static string ProgramData { get; } =
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), LIB_NAME);


		/// <summary>
		///     Module initializer
		/// </summary>
		[ModuleInitializer]
		public static void Setup()
		{
			/*
			 * Setup
			 */


			Trace.WriteLine(">>> Module init <<<", C_INFO);

			bool compatible = IsCompatible();

			if (!compatible) {
				Trace.WriteLine("Compatibility check failed!", C_WARN);
				Guard.Fail();
			}

			
			IsSetup = true;

			/*
			 * Close
			 */

			AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
			{
				//Close();
			};

			Trace.WriteLine($">>> {LIB_NAME} loaded <<<", C_INFO);

		}

		public static void Close()
		{
			Allocator.Close();
			Clr.Dispose();

			IsSetup = false;
		}


		public static bool IsCompatible()
		{
			bool ver = Environment.Version == ClrVersion;
			bool gc  = !GCSettings.IsServerGC;
			bool os  = OperatingSystem.IsWindows();

			return ver && gc && os;
		}


		public static void QWrite(string s, string u,Action<object> obj, params object[] args)
		{
			//todo
		}
	}
}