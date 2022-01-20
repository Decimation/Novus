// ReSharper disable RedundantUsingDirective.Global

#pragma warning disable IDE0060, IDE0079, IDE0005
global using NN = JetBrains.Annotations.NotNullAttribute;

global using Native = Novus.OS.Win32.Native;
global using ReflectionHelper = Novus.Utilities.ReflectionHelper;

global using U = System.Runtime.CompilerServices.Unsafe;
global using M = Novus.Memory.Mem;

global using MA = System.Runtime.InteropServices.MarshalAsAttribute;
global using UT = System.Runtime.InteropServices.UnmanagedType;
global using PE = System.Linq.Expressions.ParameterExpression;
global using BE = System.Linq.Expressions.BinaryExpression;
global using NNINN = System.Diagnostics.CodeAnalysis.NotNullIfNotNullAttribute;
using CBN = JetBrains.Annotations.CanBeNullAttribute;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text.Json;
using JetBrains.Annotations;
using Kantan.Cli;
using Kantan.Collections;
using Novus.Memory;
using Novus.Properties;
using Novus.Runtime;
using Kantan.Diagnostics;
using Kantan.Text;
using Kantan.Utilities;
using Novus.Memory.Allocation;
using Novus.OS;
using Novus.OS.Win32;
using Novus.Utilities;
using static Kantan.Diagnostics.LogCategories;
using static Novus.Utilities.ReflectionHelper;

// ReSharper disable LocalizableElement

// ReSharper disable UnusedMember.Global
// [assembly: InternalsVisibleTo("Ultrakiller")]
[assembly: InternalsVisibleTo("Test")]
[assembly: InternalsVisibleTo("TestBenchmark")]
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
///                 <see cref="SymbolReader" />
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
	/// Name of this library
	/// </summary>
	public const string LIB_NAME = "Novus";

	/// <summary>
	///     Runtime CLR module name
	/// </summary>
	public const string CLR_MODULE = "coreclr.dll";

	/// <summary>
	///     Runtime CLR version
	/// </summary>
	public static readonly Version ClrVersion = Version.Parse(EmbeddedResources.RequiredVersion);

	/// <summary>
	///     Runtime CLR resources
	/// </summary>
	public static Resource Clr { get; } = new(CLR_MODULE);

	public static bool IsSetup { get; private set; }

	internal static string ProgramData { get; } =
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

		Trace.WriteLine($"[{LIB_NAME}] Module init", C_INFO);

		bool compatible = IsCompatible();

		if (!compatible) {
			Trace.WriteLine($"[{LIB_NAME}] Compatibility check failed! " +
			                $"({Environment.Version}, {ClrVersion})", C_ERROR);
			//Guard.Fail();
		}

		IsSetup = true;

		/*
		 * Close
		 */

		AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
		{
			//Close();
		};


		Trace.WriteLine($"[{LIB_NAME}] Loaded ({Environment.Version})", C_INFO);

	}

	public static void Close()
	{
		AllocManager.Close();
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

	#region QWrite

	

	private const string QWRITE_STR_FMT_ARG = "s";

	internal static Action<object> DefaultQWriteFunction = Console.WriteLine;

	
	[StringFormatMethod(QWRITE_STR_FMT_ARG)]
	internal static void QWrite(string s, params object[] args) => QWrite(s, DefaultQWriteFunction, args: args);

	[StringFormatMethod(QWRITE_STR_FMT_ARG)]
	internal static void QWrite(string s, Action<object> writeFunction = null, string category = null,
								[CallerArgumentExpression("s")] string sz = null, params object[] args)
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
				s = Strings.ToHexString(obj);
			}

			else if (EnumerableHelper.TryCastDictionary(obj as IDictionary, out var kv)) {
				s = kv.Select(x => $"{x.Key} = {x.Value}")
				      .QuickJoin("\n");
			}

			fmt[i++] = s2;

		}

		s = string.Format(s, fmt);

		if (category is { }) {
			s = $"[{category}] " + s;

		}

		if (sz is { }) {
			s = $"[{sz}] " + s;
		}

		writeFunction(s);

	}

	#endregion
}