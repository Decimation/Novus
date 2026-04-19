// ReSharper disable RedundantUsingDirective.Global

#pragma warning disable IDE0060, IDE0079, IDE0005

#region

global using UI = JetBrains.Annotations.UsedImplicitlyAttribute;
global using MIU = JetBrains.Annotations.MeansImplicitUseAttribute;
global using MDR = JetBrains.Annotations.MustDisposeResourceAttribute;
global using MURV = JetBrains.Annotations.MustUseReturnValueAttribute;
global using NN = JetBrains.Annotations.NotNullAttribute;
global using DNR = System.Diagnostics.CodeAnalysis.DoesNotReturnAttribute;
global using DNRI = System.Diagnostics.CodeAnalysis.DoesNotReturnIfAttribute;
global using MA = System.Runtime.InteropServices.MarshalAsAttribute;
global using UT = System.Runtime.InteropServices.UnmanagedType;
global using PE = System.Linq.Expressions.ParameterExpression;
global using BE = System.Linq.Expressions.BinaryExpression;
global using NNINN = System.Diagnostics.CodeAnalysis.NotNullIfNotNullAttribute;
global using CBN = JetBrains.Annotations.CanBeNullAttribute;
global using ICBN = JetBrains.Annotations.ItemCanBeNullAttribute;
global using MNNW = System.Diagnostics.CodeAnalysis.MemberNotNullWhenAttribute;
global using MNN = System.Diagnostics.CodeAnalysis.MemberNotNullAttribute;
global using MN = System.Diagnostics.CodeAnalysis.MaybeNullAttribute;
global using NNW = System.Diagnostics.CodeAnalysis.NotNullWhenAttribute;
global using MNW = System.Diagnostics.CodeAnalysis.MaybeNullWhenAttribute;
global using DAM = System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembersAttribute;
global using DAMT = System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes;
global using Opt = System.Runtime.InteropServices.OptionalAttribute;

#endregion

// global using Native = Novus.Win32.Native;
// global using ReflectionHelper = Novus.Utilities.ReflectionHelper;
// global using U = System.Runtime.CompilerServices.Unsafe;
// global using M = Novus.Memory.Mem;
using static Kantan.Diagnostics.LogCategories;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using JetBrains.Annotations;
using Kantan.Collections;
using Kantan.Text;
using Novus.Memory;
using Novus.Memory.Allocation;
using Novus.OS;
using Novus.Properties;
using Novus.Runtime;
using Novus.Utilities;
using System.Xml.Linq;
using Kantan.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Novus.FileTypes.Impl;
using Novus.Imports;
using Novus.Win32;

// ReSharper disable InconsistentNaming
// ReSharper disable LocalizableElement
// ReSharper disable UnusedMember.Global
#pragma warning disable CA1873
#nullable disable

[assembly: InternalsVisibleTo("Test")]
[assembly: InternalsVisibleTo("TestBenchmark")]
[assembly: InternalsVisibleTo("UnitTest")]

namespace Novus;

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
///                 <see cref="RuntimeResource" />
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
///                 <see cref="ObjectUtility" />
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
///                 <see cref="SymbolReader" />
///             </description>
///         </item>
///         <item>
///             <description>
///                 <see cref="FileSystem" />
///             </description>
///         </item>
///     </list>
/// </remarks>
[DAM(DAMT.All)]
public static class Global
{

	/// <summary>
	/// Name of this library
	/// </summary>
	public const string LIB_NAME = "Novus";

	#region 

	/// <summary>
	///     Runtime CLR module name
	/// </summary>
	public const string CLR_MODULE = "coreclr.dll";

	public const string CLR_MODULE_SO = "libcoreclr.so";

	public const string CLR_PDB = "coreclr.pdb";

	/// <summary>
	///     Runtime CLR version (target)
	/// </summary>
	public static readonly Version Version;

	/*[MN]
	public static string ClrPdb { get; set; }*/

	/// <summary>
	///     Runtime CLR resources
	/// </summary>
	public static RuntimeResource Clr { get; private set; }

	#endregion

	public static bool IsSetup { get; private set; }

	public static readonly string DataFolder;

	public static readonly Assembly Assembly = Assembly.GetExecutingAssembly();

	internal static readonly ILoggerFactory LoggerFactoryInt;

	private static readonly ILogger s_logger;

	private static readonly IConfigurationRoot s_config;

	public static readonly bool IsWorkstationGC = !GCSettings.IsServerGC;

	public static readonly bool IsCorrectVersion;

	public static readonly bool IsCompatible;


	/// <summary>
	/// Root static initializer, followed by <see cref="Setup"/>
	/// </summary>
	static Global()
	{
		s_config = new ConfigurationBuilder()
		           .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
		           .AddEnvironmentVariables()
		           .Build();

		LoggerFactoryInt = LoggerFactory.Create(builder =>
		{
			builder.AddDebug();
			builder.AddTraceSource("TRACE");
			builder.SetMinimumLevel(LogLevel.Trace);
		});

		s_logger = LoggerFactoryInt.CreateLogger(LIB_NAME);
		s_logger.LogTrace($"{nameof(Global)} invoked");

		// Assembly = Assembly.GetExecutingAssembly();

#if NCLRI
#warning NCLRI mode
		s_logger.LogInformation("{Build} build!", BUILD_NCLRI);
#endif

		Version = Version.Parse(ER.RequiredVersion);

		IsCorrectVersion = Environment.Version == Version;
		IsCompatible     = IsCorrectVersion && IsWorkstationGC && OperatingSystem.IsWindows();
		DataFolder       = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), LIB_NAME);

		if (!Directory.Exists(DataFolder)) {
			Directory.CreateDirectory(DataFolder);
		}

		s_logger.LogInformation("Data folder: {Folder}", DataFolder);
	}

	/// <summary>
	///     Root initializer
	/// </summary>
	[ModuleInitializer]
	public static void Setup()
	{
#if NCLRI
#warning Not running Setup!
		return;
#endif

		if (IsSetup) {
			return;
		}

		/*
		 * Setup
		 */

		using var setupScope = s_logger.BeginScope(nameof(Setup));

		s_logger.LogTrace("Module init :: {Name}", ER.Name);
		s_logger.LogTrace("Runtime: {EnvVer} | Target: {ClrVer}", Environment.Version, Version);

		if (RuntimeInformation.IsInteractiveHost) {
			s_logger.LogWarning("Interactive host");
		}

		if (!OperatingSystem.IsWindows()) {
			s_logger.LogWarning("Not on Windows!");

			return;
		}

		if (!IsCompatible) {
			s_logger.LogWarning("Compatibility check failed!");
		}

		NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), DllImportResolver);

		Clr = new RuntimeResource(CLR_MODULE);

		/*
		 * Close
		 */

		AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
		{
			//Close();
		};

		IsSetup = true;

		s_logger.LogInformation("Loaded runtime resource {ClrRR}", Clr);
	}

	public static nint DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
	{
		if (libraryName == MagicNative.MAGIC_LIB_PATH) {
			return NativeLibrary.Load(Path.Combine(DataFolder, MagicNative.MAGIC_LIB_PATH), assembly, searchPath);
		}

		return IntPtr.Zero;
	}

	/*[CBN]
	[SupportedOSPlatform(RuntimeInformationExtensions.OS_WIN)]
	public static string GetPdbFile()
	{
		// var pdbFile = Path.Join(DataFolder, CLR_PDB);
		// File.WriteAllBytes(pdbFile, EmbeddedResources.coreclr);

		// var path=FileSystem.EnumerateInPath(CLR_PDB);

		var path = SymbolReader.EnumerateSymbolPath(CLR_PDB);

		if (path == null) {
			return null;
		}

		var pdbFile = path.FirstOrDefault();

		return pdbFile;

		// return CLR_PDB;
	}*/

	public static void Close()
	{
		AllocManager.Close();
		Clr.Dispose();
		Clr     = null;
		IsSetup = false;
	}


	//CS8058
	internal const string BUILD_NCLRI          = "NCLRI";
	internal const string DIAG_ID_EXPERIMENTAL = "NV0001";

}