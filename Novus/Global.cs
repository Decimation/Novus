using System;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.CompilerServices;
using Novus.Memory;
using Novus.Properties;
using Novus.Runtime;
using Novus.Win32;
using SimpleCore.Diagnostics;

// ReSharper disable UnusedMember.Global
[assembly: InternalsVisibleTo("Ultrakiller")]
[assembly: InternalsVisibleTo("Test")]
[assembly: InternalsVisibleTo("TestBenchmark")]
#nullable enable

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
	///                 <see cref="EmbeddedResources" />
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 <see cref="Resource" />
	///             </description>
	///         </item>
	/// <item>
	///             <description>
	///                 <see cref="SigScanner" />
	///             </description>
	///         </item>
	///     </list>
	///     OS utilities:
	///     <list type="bullet">
	/// <item>
	///             <description>
	///                 <see cref="Native" />
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

		/// <summary>
		///     Runtime CLR version
		/// </summary>
		public static readonly Version ClrVersion = new(5, 0, 6);

		/// <summary>
		///     Runtime CLR resources
		/// </summary>
		public static Resource Clr { get; } = new(CLR_MODULE);

		public static bool IsSetup { get; private set; }

		/// <summary>
		///     Module initializer
		/// </summary>
		[ModuleInitializer]
		public static void Setup()
		{
			/*
			 * Setup
			 */

			Trace.WriteLine($">>> Module init <<<");
			
			bool compatible = IsCompatible();

			if (!compatible) {
				//Guard.Fail();
				Trace.WriteLine($"[WARNING] compatibility check failed!");
			}
			

			IsSetup = true;
			
			/*
			 * Close
			 */
			
			AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
			{
				//Close();
			};

			Trace.WriteLine($">>> {LIB_NAME} loaded <<<");
		}

		public const string LIB_NAME = "Novus";

		public static void Close()
		{
			Allocator.Close();
			Resource.Close();

			IsSetup = false;
		}
		
		// TODO: .NET 5.0.6 coreclr symbols (pdb) file

		public static bool IsCompatible()
		{
			bool ver = Environment.Version == ClrVersion;
			bool gc  = !GCSettings.IsServerGC;
			bool os  = OperatingSystem.IsWindows();
		
			return ver && gc && os;
		}
	}
}