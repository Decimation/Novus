
global using FS=Novus.OS.FileSystem;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kantan.Collections;
using Kantan.Diagnostics;
using Kantan.Utilities;
using Novus.OS.Win32;
using Novus.OS.Win32.Structures;
using Novus.OS.Win32.Structures.Kernel32;

#pragma warning disable 8603, CA1416

// ReSharper disable SuggestVarOrType_BuiltInTypes
// ReSharper disable TailRecursiveCall

// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator

// ReSharper disable ConvertIfStatementToReturnStatement

// ReSharper disable UnusedMember.Global

#nullable enable

namespace Novus.OS;

/// <summary>
///     Utilities for working with the file system, files, etc.
/// </summary>
/// <seealso cref="File" />
/// <seealso cref="FileInfo" />
/// <seealso cref="Directory" />
/// <seealso cref="DirectoryInfo" />
/// <seealso cref="Path" />
/// <seealso cref="OperatingSystem"/>
public static class FileSystem
{
	#region KnownFolder

	/// <remarks><a href="https://stackoverflow.com/questions/10667012/getting-downloads-folder-in-c">Adapted from here</a></remarks>
	private static readonly string[] KnownFolderGuids =
	{
		"{56784854-C6CB-462B-8169-88E350ACB882}", // Contacts
		"{B4BFCC3A-DB2C-424C-B029-7FE99A87C641}", // Desktop
		"{FDD39AD0-238F-46AF-ADB4-6C85480369C7}", // Documents
		"{374DE290-123F-4565-9164-39C4925E467B}", // Downloads
		"{1777F761-68AD-4D8A-87BD-30B759FA33DD}", // Favorites
		"{BFB9D5E0-C6A9-404C-B2B2-AE6DB6AF4968}", // Links
		"{4BD8D571-6D19-48D3-BE97-422220080E43}", // Music
		"{33E28130-4E1E-4676-835A-98395C3BC3BB}", // Pictures
		"{4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4}", // SavedGames
		"{7D1D3A04-DEBB-4115-95CF-2F29DA2920DA}", // SavedSearches
		"{18989B1D-99B5-455B-841C-AB7C74E4DDFC}", // Videos
	};

	/// <summary>
	/// Gets the current path to the specified known folder as currently configured. This does
	/// not require the folder to be existent.
	/// </summary>
	/// <param name="knownFolder">The known folder which current path will be returned.</param>
	/// <returns>The default path of the known folder.</returns>
	/// <exception cref="System.Runtime.InteropServices.ExternalException">Thrown if the path
	///     could not be retrieved.</exception>
	public static string GetPath(KnownFolder knownFolder) => GetPath(knownFolder, false);

	/// <summary>
	/// Gets the current path to the specified known folder as currently configured. This does
	/// not require the folder to be existent.
	/// </summary>
	/// <param name="knownFolder">The known folder which current path will be returned.</param>
	/// <param name="defaultUser">Specifies if the paths of the default user (user profile
	///     template) will be used. This requires administrative rights.</param>
	/// <returns>The default path of the known folder.</returns>
	/// <exception cref="System.Runtime.InteropServices.ExternalException">Thrown if the path
	///     could not be retrieved.</exception>
	public static string GetPath(KnownFolder knownFolder, bool defaultUser)
		=> GetPath(knownFolder, KnownFolderFlags.DontVerify, defaultUser);

	private static string GetPath(KnownFolder knownFolder, KnownFolderFlags flags, bool defaultUser)
	{
		int result = Native.SHGetKnownFolderPath(new Guid(KnownFolderGuids[(int) knownFolder]),
		                                         (uint) flags, new IntPtr(defaultUser ? -1 : 0),
		                                         out var outPath);

		if (result >= 0) {
			string path = Marshal.PtrToStringUni(outPath)!;
			Marshal.FreeCoTaskMem(outPath);
			return path;
		}

		throw new ExternalException(
			"Unable to retrieve the known folder path. It may not be available on this system.", result);
	}

	#endregion

	/// <summary>
	/// Expands environment variables and, if unqualified, locates the exe in the working directory
	/// or the environment's path.
	/// </summary>
	/// <param name="f">The name of the executable file</param>
	/// <returns>The fully-qualified path to the file</returns>
	/// <exception cref="System.IO.FileNotFoundException">Raised when the exe was not found</exception>
	public static string? FindInPath(string f)
	{
		f = Environment.ExpandEnvironmentVariables(f);

		if (!File.Exists(f)) {
			if (Path.GetDirectoryName(f) == String.Empty) {
				var split = (Environment.GetEnvironmentVariable(PATH_ENV) ?? String.Empty)
					.Split(PATH_DELIM);

				foreach (string test in split) {
					string path = test.Trim();

					if (!String.IsNullOrEmpty(path) && File.Exists(path = Path.Combine(path, f))) {
						return Path.GetFullPath(path);
					}
				}
			}

			return null;
			// throw new FileNotFoundException(new FileNotFoundException().Message, f);
		}

		return Path.GetFullPath(f);
	}

	public static string GetRootDirectory()
		=> Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));

	public static string GetShortPath(string dir)
	{
		unsafe {

			var buf = stackalloc char[Native.SIZE_1];

			var l = Native.GetShortPathName(dir, buf, Native.SIZE_1);

			if (l != Native.ERROR_SUCCESS) {
				throw new Win32Exception((int) l);
			}

			return new string(buf);
		}
	}

	private const string RELATIVE_PATH = "..";

	public static string GetRelativeParent(string fi, int n)
	{

		var rg = new string[n + 1];

		Array.Fill(rg, RELATIVE_PATH);
		rg[0] = fi;
		string p = Path.Combine(rg);

		return p;
	}

	public static string GetParent(string fi, int n)
	{
		if (n == 0) {
			return fi;
		}

		return GetParent(Directory.GetParent(fi)!.FullName, --n);
	}

	public static string GetRandomName() => Path.GetFileNameWithoutExtension(Path.GetRandomFileName());

	public static string CreateTempFile(string fname, string[] data)
	{
		string file = Path.Combine(Path.GetTempPath(), fname);

		File.WriteAllLines(file, data);

		return file;
	}

	public static bool ExistsInFolder(string folder, string exeStr, out string folderExe)
	{
		string folderExeFull = Path.Combine(folder, exeStr);
		bool   inFolder      = File.Exists(folderExeFull);

		folderExe = folderExeFull;
		return inFolder;
	}

	public static bool ExploreFile(string filePath)
	{
		// https://stackoverflow.com/questions/13680415/how-to-open-explorer-with-a-specific-file-selected
		if (!File.Exists(filePath)) {
			return false;
		}

		//Clean up file path so it can be navigated OK
		filePath = Path.GetFullPath(filePath);
		using var p = Process.Start(Native.EXPLORER_EXE, $"/select,\"{filePath}\"");

		p.WaitForExit();

		return true;
	}

	public static string SanitizeFilename(string s)
	{
		//https://stackoverflow.com/questions/309485/c-sharp-sanitize-file-name

		var invalids = Path.GetInvalidFileNameChars();
		var newName  = String.Join("_", s.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');

		return newName;
	}

	/// <summary>
	///     Determines the file size (not size on disk) of <paramref name="file" />
	/// </summary>
	/// <param name="file">File location</param>
	/// <returns>Size of the file, in bytes</returns>
	public static long GetFileSize(string file) => new FileInfo(file).Length;

	#region File types

	#endregion

	public static Task<string> CreateRandomFileAsync(long cb, string? f = null)
	{
		return Task.Run(() =>
		{
			f ??= Path.GetTempFileName();
			var s = File.OpenWrite(f);

			for (long i = 0; i < cb; i++) {
				s.WriteByte((byte) (i ^ cb));
			}

			return f;
		});
	}

	#region Path

	public static string? FindExecutableLocation(string exe)
	{

		// https://stackoverflow.com/questions/6041332/best-way-to-get-application-folder-path
		// var exeLocation1 = Assembly.GetEntryAssembly().Location;
		// var exeLocation2 = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
		// var exeLocation3 = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;
		// var exeLocation = AppDomain.CurrentDomain.BaseDirectory;

		//

		var rg = new List<string>
		{
			/* Current directory */
			Environment.CurrentDirectory,

			// Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase!

			/* Executing directory */
			Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory
			                               .Replace("file:///", String.Empty)
			                               .Replace("/", "\\"))!,

			//Assembly.GetCallingAssembly().Location
		};

		rg.AddRange(GetEnvironmentPathDirectories());

		//

		foreach (string loc in rg) {
			if (ExistsInFolder(loc, exe, out var folder)) {
				return folder;
			}
		}

		return null;
	}

	public static string ResolvePath(string s)
	{
		var p = SearchInPath(s);

		if (p is not { }) {
			return s;
		}

		return s;
	}

	public static string? SearchInPath(string s)
	{
		string[] path = GetEnvironmentPathDirectories();

		foreach (string directory in path) {
			if (Directory.Exists(directory)) {
				foreach (string file in Directory.EnumerateFiles(directory)) {
					if (Path.GetFileName(file) == s) {
						//rg.Add(file);
						return file;
					}
				}
			}
		}

		return null;
	}

	/// <summary>
	///     Environment variable PATH
	/// </summary>
	public const string PATH_ENV = "PATH";

	/// <summary>
	///     Delimiter of environment variable <see cref="PATH_ENV" />
	/// </summary>
	public const char PATH_DELIM = ';';

	public static string[] GetEnvironmentPathDirectories() => GetEnvironmentPathDirectories(Target);

	public static string[] GetEnvironmentPathDirectories(EnvironmentVariableTarget t)
		=> GetEnvironmentPath(t).Split(PATH_DELIM);

	public static string GetEnvironmentPath() => GetEnvironmentPath(Target);

	public static string GetEnvironmentPath(EnvironmentVariableTarget t)
	{
		return Environment.GetEnvironmentVariable(PATH_ENV, t);
	}

	public static void SetEnvironmentPath(string s) => SetEnvironmentPath(s, Target);

	public static void SetEnvironmentPath(string s, EnvironmentVariableTarget t)
	{
		Environment.SetEnvironmentVariable(PATH_ENV, s, t);
	}

	/// <summary>
	///     Removes <paramref name="location" /> from <see cref="GetEnvironmentPathDirectories()" />
	/// </summary>
	public static void RemoveFromPath(string location)
	{
		string oldValue = GetEnvironmentPath();

		SetEnvironmentPath(oldValue.Replace(PATH_DELIM + location, String.Empty));
	}

	/// <summary>
	///     Determines whether <paramref name="location" /> is in <see cref="GetEnvironmentPathDirectories()" />
	/// </summary>
	public static bool IsFolderInPath(string location)
	{
		return GetEnvironmentPathDirectories().Contains(location);
	}

	#endregion

	public static string AppendToFilename(string filename, string append)
	{
		var withoutExtension = Path.GetFileNameWithoutExtension(filename);
		var extension        = Path.GetExtension(filename);
		return withoutExtension + append + extension;
	}

	/// <summary>
	///     Environment variable target
	/// </summary>
	public static EnvironmentVariableTarget Target { get; set; } = EnvironmentVariableTarget.User;

	public static string? SymbolPath
		=> Environment.GetEnvironmentVariable("_NT_SYMBOL_PATH", EnvironmentVariableTarget.Machine);

	public static bool IsAdministrator()
	{
		using var identity = WindowsIdentity.GetCurrent();

		var principal = new WindowsPrincipal(identity);

		return principal.IsInRole(WindowsBuiltInRole.Administrator);

	}
}

/// <summary>
/// Standard folders registered with the system. These folders are installed with Windows Vista
/// and later operating systems, and a computer will have only folders appropriate to it
/// installed.
/// </summary>
public enum KnownFolder
{
	Contacts,
	Desktop,
	Documents,
	Downloads,
	Favorites,
	Links,
	Music,
	Pictures,
	SavedGames,
	SavedSearches,
	Videos
}