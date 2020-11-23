using System;
using System.Diagnostics;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

#nullable enable
#pragma warning disable HAA0501 //
#pragma warning disable HAA0502 //
#pragma warning disable HAA0301 //
#pragma warning disable HAA0302 //


namespace Novus.Win32
{
	/// <summary>
	/// <seealso cref="OperatingSystem"/>
	/// </summary>
	public static class OS
	{
		/// <summary>
		///     Environment variable PATH
		/// </summary>
		public const string PATH_ENV = "PATH";

		/// <summary>
		///     Delimiter of environment variable <see cref="OS.PATH_ENV" />
		/// </summary>
		public const char PATH_DELIM = ';';

		public const string KERNEL32_DLL = "kernel32.dll";

		/// <summary>
		///     Environment variable target
		/// </summary>
		public static EnvironmentVariableTarget Target { get; set; } = EnvironmentVariableTarget.User;

		/// <summary>
		///     Directories of <see cref="EnvironmentPath" /> with environment variable target <see cref="Target" />
		/// </summary>
		public static string[] PathDirectories => EnvironmentPath.Split(PATH_DELIM);

		/// <summary>
		///     Environment variable <see cref="OS.PATH_ENV" /> with target <see cref="Target" />
		/// </summary>
		public static string EnvironmentPath
		{
			get
			{
				string? env = Environment.GetEnvironmentVariable(OS.PATH_ENV, Target);

				if (env == null) {
					throw new NullReferenceException();
				}

				return env;
			}
			set => Environment.SetEnvironmentVariable(OS.PATH_ENV, value, Target);
		}

		/// <summary>
		///     Removes <paramref name="location" /> from <see cref="PathDirectories" />
		/// </summary>
		public static void RemoveFromPath(string location)
		{
			string oldValue = EnvironmentPath;

			EnvironmentPath = oldValue.Replace(PATH_DELIM + location, String.Empty);
		}

		/// <summary>
		///     Determines whether <paramref name="location" /> is in <see cref="PathDirectories" />
		/// </summary>
		public static bool IsFolderInPath(string location)
		{
			string? dir = Array.Find(PathDirectories, s => s == location);

			return !String.IsNullOrWhiteSpace(dir);
		}

		/// <summary>
		///     Forcefully kills a <see cref="Process" /> and ensures the process has exited.
		/// </summary>
		/// <param name="p"><see cref="Process" /> to forcefully kill.</param>
		/// <returns><c>true</c> if <paramref name="p" /> was killed; <c>false</c> otherwise</returns>
		public static bool ForceKill(this Process p)
		{
			p.WaitForExit();
			p.Dispose();

			try {
				if (!p.HasExited) {
					p.Kill();
				}

				return true;
			}
			catch (Exception) {

				return false;
			}
		}
	}
}