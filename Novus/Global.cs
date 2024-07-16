// ReSharper disable RedundantUsingDirective.Global

#pragma warning disable IDE0060, IDE0079, IDE0005

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
using Microsoft.Extensions.Logging;
using Novus.Imports;
using Novus.Win32;

// ReSharper disable InconsistentNaming

// ReSharper disable LocalizableElement

// ReSharper disable UnusedMember.Global
// [assembly: InternalsVisibleTo("Ultrakiller")]
[assembly: InternalsVisibleTo("Test")]
[assembly: InternalsVisibleTo("TestBenchmark")]
[assembly: InternalsVisibleTo("UnitTest")]
#nullable disable

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
///                 <see cref="RuntimeProperties" />
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
///                 <see cref="Win32SymbolReader" />
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
[DAM(DAMT.All)]
public static class Global
{

	/// <summary>
	/// Name of this library
	/// </summary>
	public const string LIB_NAME = "Novus";

	/// <summary>
	///     Runtime CLR module name
	/// </summary>
	public const string CLR_MODULE = "coreclr.dll";

	public const string CLR_PDB = "coreclr.pdb";

	/// <summary>
	///     Runtime CLR version
	/// </summary>
	public static readonly Version ClrVersion = Version.Parse(EmbeddedResources.RequiredVersion);

	public static string ClrPdb { get; set; } = GetPdbFile();

	/// <summary>
	///     Runtime CLR resources
	/// </summary>
	public static RuntimeResource Clr { get; private set; }

	public static bool IsSetup { get; private set; }

	/*public static string ProgramData { get; } =
		Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), LIB_NAME);*/

	public static readonly string DataFolder =
		Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), LIB_NAME);

	public static readonly Assembly Assembly = Assembly.GetExecutingAssembly();

	// internal static readonly ILoggerFactory LoggerFactory = new LoggerFactory();

	internal static readonly ILoggerFactory LoggerFactory =
		Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddDebug());

	internal static readonly ILogger Logger = LoggerFactory.CreateLogger(LIB_NAME);

	static Global()
	{

		if (!Directory.Exists(DataFolder)) {
			Directory.CreateDirectory(DataFolder);
		}

	}

	[CBN]
	public static string GetPdbFile()
	{
		// var pdbFile = Path.Join(DataFolder, CLR_PDB);
		// File.WriteAllBytes(pdbFile, EmbeddedResources.coreclr);

		var pdbFile = Win32SymbolReader.EnumerateSymbolPath(CLR_PDB).FirstOrDefault();

		return pdbFile;
	}

	/// <summary>
	///     Module initializer
	/// </summary>
	[ModuleInitializer]
	public static void Setup()
	{
		if (IsSetup) {
			return;
		}

		/*
		 * Setup
		 */

		Logger.LogTrace($"[{LIB_NAME}] Module init");
		Logger.LogTrace($"[{LIB_NAME}]");

		if (!IsWindows) {
			Logger.LogWarning("Not on Windows!");
			return;
		}

		//todo
		Clr = new RuntimeResource(CLR_MODULE, ClrPdb);

		/* try {
			DateTime dt = default;

			dt = File.GetLastWriteTime(Assembly.Location);
			var version = Assembly.GetName().Version;
			Trace.WriteLine($"{version} @ ~{dt}");
		}
		catch (TypeInitializationException e) {
			Debug.WriteLine($"{e}");
		} */

		if (!IsCompatible) {
			Logger.LogCritical($"[{LIB_NAME}] Compatibility check failed! " +
			                   $"(Runtime: {Environment.Version} | Target: {ClrVersion})", C_ERROR);

			//Require.Fail();
		}

		IsSetup = true;

		/*
		 * Close
		 */

		AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
		{
			//Close();
		};

		Logger.LogDebug($"[{LIB_NAME}] CLR: ({Environment.Version})", C_INFO);

	}

	public static void Close()
	{
		AllocManager.Close();
		Clr.Dispose();
		Clr     = null;
		IsSetup = false;
	}

	[SupportedOSPlatformGuard(OS_WIN)]
	public static readonly bool IsWindows = OperatingSystem.IsWindows();

	[SupportedOSPlatformGuard(OS_LINUX)]
	public static readonly bool IsLinux = OperatingSystem.IsLinux();

	public static readonly bool IsWorkstationGC = !GCSettings.IsServerGC;

	public static readonly bool IsCorrectVersion = Environment.Version == ClrVersion;

	public static bool IsCompatible => IsCorrectVersion && IsWorkstationGC && IsWindows;

	internal const MethodImplOptions IMPL_OPTIONS =
		MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization;

	public const string OS_WIN = "windows";

	public const string OS_LINUX = "linux";
#if EXTRA
	#region QWrite

	internal static Action<object> DefaultQWriteFunction = Console.WriteLine;

	[StringFormatMethod(nameof(s))]
	internal static void QWrite(string s, params object[] args)
		=> QWrite(s, DefaultQWriteFunction, args: args);

	[StringFormatMethod(nameof(s))]
	internal static void QWrite(string s, Action<object> writeFunction = null, string category = null,
	                            [CallerArgumentExpression(nameof(s))] string sz = null, params object[] args)
	{
		writeFunction ??= DefaultQWriteFunction;

		var fmt = new object[args.Length];

		int i = 0;

		foreach (object obj in args) {
			string s2 = obj switch
			{
				object[] rg => rg.QuickJoin(),
				Array r     => r.CastObjectArray().QuickJoin(),
				string str  => str,
				_           => obj.ToString()
			};

			if (obj.GetType().IsAnyPointer()) {
				s = FormatHelper.ToHexString(obj);
			}

			else if (EnumerableHelper.TryCastDictionary(obj as IDictionary, out var kv)) {
				s = kv.Select(x => $"{x.Key} = {x.Value}")
					.QuickJoin("\n");
			}

			fmt[i++] = s2;

		}

		s = String.Format(s, fmt);

		if (category is { }) {
			s = $"[{category}] " + s;

		}

		if (sz is { }) {
			s = $"[{sz}] " + s;
		}

		writeFunction(s);

	}

	#endregion
#endif

	//CS8058
	internal const string DIAG_ID_EXPERIMENTAL = "NV0001";

}