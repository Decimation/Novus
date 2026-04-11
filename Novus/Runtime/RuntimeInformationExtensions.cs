using System.Reflection;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

// ReSharper disable InconsistentNaming

namespace Novus.Runtime;

public static class RuntimeInformationExtensions
{

	/*
	 * https://github.com/dotnet/BenchmarkDotNet/blob/master/src/BenchmarkDotNet/Portability/RuntimeInformation.cs
	 *
	 */

	private static bool s_isAot { get; } = IsAotMethod(); // This allocates, so we only want to call it once statically.

	private static bool IsAotMethod()
	{
		Type runtimeFeature = Type.GetType("System.Runtime.CompilerServices.RuntimeFeature");

		if (runtimeFeature != null) {
			MI methodInfo = runtimeFeature.GetProperty("IsDynamicCodeCompiled", BindingFlags.Public | BindingFlags.Static)?.GetMethod;

			if (methodInfo != null) {
				return !(bool) methodInfo.Invoke(null, null);
			}
		}

		return false;
	}

	extension(RuntimeInformation)
	{

		[PublicAPI]
		public static bool IsNetNative => RuntimeInformation.FrameworkDescription.StartsWith(".NET Native", StringComparison.OrdinalIgnoreCase);

		public static bool IsNetCore => ((Environment.Version.Major >= 5)
		                                 || RuntimeInformation.FrameworkDescription.StartsWith(".NET Core", StringComparison.OrdinalIgnoreCase))
		                                && !String.IsNullOrEmpty(AppContext.BaseDirectory);

		public static bool IsWasm => RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER"));

		public static bool IsNativeAOT => Environment.Version.Major >= 5
		                                  && String.IsNullOrEmpty(AppContext.BaseDirectory) // it's merged to a single .exe and .Location returns null
		                                  && !RuntimeInformation.IsWasm;    // Wasm also returns "" for assembly locations

		public static bool IsAot => s_isAot;

		public static bool IsInteractiveHost => Assembly.GetEntryAssembly()?.FullName?.Contains("InteractiveHost") ?? false;

	}


	public const string OS_WIN = "windows";

	public const string OS_LINUX = "linux";

}