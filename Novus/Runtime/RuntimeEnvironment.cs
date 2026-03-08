using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using JetBrains.Annotations;

// ReSharper disable InconsistentNaming

namespace Novus.Runtime;

public static class RuntimeEnvironment
{

	/*
	 * https://github.com/dotnet/BenchmarkDotNet/blob/master/src/BenchmarkDotNet/Portability/RuntimeInformation.cs
	 *
	 */


	[SupportedOSPlatformGuard(OS_WIN)]
	public static readonly bool IsWindows = OperatingSystem.IsWindows();

	[SupportedOSPlatformGuard(OS_LINUX)]
	public static readonly bool IsLinux = OperatingSystem.IsLinux();

	public const string OS_WIN = "windows";

	public const string OS_LINUX = "linux";

	[PublicAPI]
	public static bool IsNetNative => RuntimeInformation.FrameworkDescription.StartsWith(".NET Native", StringComparison.OrdinalIgnoreCase);

	public static bool IsNetCore => ((Environment.Version.Major >= 5)
	                                 || RuntimeInformation.FrameworkDescription.StartsWith(".NET Core", StringComparison.OrdinalIgnoreCase))
	                                && !String.IsNullOrEmpty(AppContext.BaseDirectory);

	public static bool IsWasm => RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER"));

	public static bool IsNativeAOT => Environment.Version.Major >= 5
	                                  && String.IsNullOrEmpty(AppContext.BaseDirectory) // it's merged to a single .exe and .Location returns null
	                                  && !IsWasm;                                       // Wasm also returns "" for assembly locations

	public static bool IsAot { get; } = IsAotMethod(); // This allocates, so we only want to call it once statically.

	public static bool IsInteractiveHost => Assembly.GetEntryAssembly()?.FullName?.Contains("InteractiveHost") ?? false;

	private static bool IsAotMethod()
	{
		Type runtimeFeature = Type.GetType("System.Runtime.CompilerServices.RuntimeFeature");

		if (runtimeFeature != null) {
			MI methodInfo = runtimeFeature
				.GetProperty("IsDynamicCodeCompiled", BindingFlags.Public | BindingFlags.Static)?.GetMethod;

			if (methodInfo != null) {
				return !(bool) methodInfo.Invoke(null, null);
			}
		}

		return false;
	}

}