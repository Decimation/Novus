﻿// global using FS = Novus.OS.FileSystem;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kantan.Collections;
using Kantan.Diagnostics;
using Kantan.Utilities;
using Novus.Win32;
using Novus.Win32.Structures;
using Novus.Win32.Structures.Kernel32;
using Novus.Win32.Structures.User32;

#pragma warning disable 8603
// #pragma warning disable CA1416 //todo

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

	[SupportedOSPlatformGuard(OS_WIN)]
	public static readonly bool IsWindows = OperatingSystem.IsWindows();

	[SupportedOSPlatformGuard(OS_LINUX)]
	public static readonly bool IsLinux = OperatingSystem.IsLinux();

	public const string OS_WIN = "windows";

	public const string OS_LINUX = "linux";

	static FileSystem()
	{
		if (IsLinux) {
			PathDelimiter = ':';
		}
		else {
			PathDelimiter = ';';
		}
	}

	#region KnownFolder

	/// <remarks><a href="https://stackoverflow.com/questions/10667012/getting-downloads-folder-in-c">Adapted from here</a></remarks>
	[SupportedOSPlatform(OS_WIN)]
	private static readonly string[] KnownFolderGuids =
	[
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
		"{18989B1D-99B5-455B-841C-AB7C74E4DDFC}"  // Videos
	];

	/// <summary>
	/// Gets the current path to the specified known folder as currently configured. This does
	/// not require the folder to be existent.
	/// </summary>
	/// <param name="knownFolder">The known folder which current path will be returned.</param>
	/// <returns>The default path of the known folder.</returns>
	/// <exception cref="System.Runtime.InteropServices.ExternalException">Thrown if the path
	///     could not be retrieved.</exception>
	[SupportedOSPlatform(OS_WIN)]
	public static string GetPath(KnownFolder knownFolder)
		=> GetPath(knownFolder, false);

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
	[SupportedOSPlatform(OS_WIN)]
	public static string GetPath(KnownFolder knownFolder, bool defaultUser)
		=> GetPath(knownFolder, KnownFolderFlags.DontVerify, defaultUser);

	[SupportedOSPlatform(OS_WIN)]
	private static string GetPath(KnownFolder knownFolder, KnownFolderFlags flags, bool defaultUser)
	{
		int result = Native.SHGetKnownFolderPath(new Guid(KnownFolderGuids[(int) knownFolder]),
		                                         (uint) flags, new nint(defaultUser ? -1 : 0),
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

	public static string GetRootDirectory()
		=> Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));

	[SupportedOSPlatform(OS_WIN)]
	public static string GetShortPath(string dir)
	{
		unsafe {

			var buf = stackalloc char[Native.SIZE_1024];

			var l = Native.GetShortPathName(dir, buf, Native.SIZE_1024);

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

	#region

	public static string GetRandomName()
		=> Path.GetFileNameWithoutExtension(Path.GetRandomFileName());

	public static string GetTempFileName(string? fn = null, string? ext = null)
	{
		string tmp = Path.GetTempFileName();

		if (fn != null) {
			var ext1 = Path.GetExtension(fn);

			/*if (ext != null && ext1 != ext) {
					fn = Path.ChangeExtension(fn, ext);
				}*/
			if (ext == null && ext1 != null) {
				ext = ext1;
			}

			var tmp3 = Path.ChangeExtension(Path.Combine(Path.GetDirectoryName(tmp), fn), "tmp");
			File.Move(tmp, tmp3, true);
			tmp = tmp3;
		}
		else { }

		if (ext != null) {
			var tmp2 = Path.ChangeExtension(tmp, ext);

			if (tmp2 != tmp) {
				File.Move(tmp, tmp2, true);
				tmp = tmp2;
			}
		}

		return tmp;
	}

	public static string CreateTempFile(string fname, string[] data)
	{
		string file = Path.Combine(Path.GetTempPath(), fname);

		File.WriteAllLines(file, data);

		return file;
	}

	public static Task<string> CreateRandomFileAsync(long cb, string? f = null)
	{
		return Task.Run(() =>
		{
			f ??= Path.GetTempFileName();
			using var s = File.OpenWrite(f);

			for (long i = 0; i < cb; i++) {
				s.WriteByte((byte) (i ^ cb));
			}

			return f;
		});
	}

	#endregion

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

		using var p = Process.Start(ER.E_Explorer, $"/select,\"{filePath}\"");

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
	public static long GetFileSize(string file)
		=> new FileInfo(file).Length;

	public static bool Open(string s)
	{
		var r = Open(s, out var p);
		p?.Dispose();
		return r;
	}

	public static bool Open(string s, out Process? p)
	{
		try {
			p = Process.Start(new ProcessStartInfo
			{
				FileName        = s,
				UseShellExecute = true
			});
			return true;
		}
		catch (Exception ex) {
			// Handle any exceptions that might occur
			Trace.WriteLine($"Error opening: {ex.Message}");
			p = null;
			return false;
		}
	}

	[SupportedOSPlatform(OS_WIN)]
	public static bool SendFileToRecycleBin(string filePath)
	{
		if (String.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
			return false;

		var fileOp = new SHFILEOPSTRUCT
		{
			wFunc = Native.FO_DELETE,
			pFrom = filePath + '\0', // The path should be null-terminated
			// Set appropriate flags to enable Undo and avoid confirmation prompt
			fFlags = Native.FOF_ALLOWUNDO | Native.FOF_NOCONFIRMATION
		};

		// Perform the operation
		var result = Native.SHFileOperation(ref fileOp);

		// Check if the operation was successful
		return result;
	}

	#region Path

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
					.Split(PathDelimiter);

				// ReSharper disable once LoopCanBeConvertedToQuery
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

	public static string? SearchInPath(string s, EnvironmentVariableTarget t = EnvironmentVariableTarget.User)
	{
		string[] path = GetEnvironmentPathDirectories(t);

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


	public static readonly char PathDelimiter;

	public static string[] GetEnvironmentPathDirectories(EnvironmentVariableTarget t = EnvironmentVariableTarget.User)
	{
		return GetEnvironmentPath(t).Split(PathDelimiter);
	}

	public static string GetEnvironmentPath(EnvironmentVariableTarget t = EnvironmentVariableTarget.User)
	{
		return Environment.GetEnvironmentVariable(PATH_ENV, t);
	}

	public static void SetEnvironmentPath(string s, EnvironmentVariableTarget t = EnvironmentVariableTarget.User)
	{
		Environment.SetEnvironmentVariable(PATH_ENV, s, t);
	}

	/// <summary>
	///     Removes <paramref name="location" /> from <see cref="GetEnvironmentPathDirectories" />
	/// </summary>
	public static void RemoveFromPath(string location, EnvironmentVariableTarget t = EnvironmentVariableTarget.User)
	{
		string oldValue = GetEnvironmentPath(t);

		SetEnvironmentPath(oldValue.Replace(PathDelimiter + location, String.Empty), t);
	}

	/// <summary>
	///     Determines whether <paramref name="location" /> is in <see cref="GetEnvironmentPathDirectories" />
	/// </summary>
	public static bool IsFolderInPath(string location, EnvironmentVariableTarget t = EnvironmentVariableTarget.User)
	{
		return GetEnvironmentPathDirectories(t).Contains(location);
	}

	#endregion

	public static string AppendToFilename(string filename, string append)
	{
		var withoutExtension = Path.GetFileNameWithoutExtension(filename);
		var extension        = Path.GetExtension(filename);
		return withoutExtension + append + extension;
	}

	[SupportedOSPlatform(OS_WIN)]
	public static string? SymbolPath
		=> Environment.GetEnvironmentVariable("_NT_SYMBOL_PATH", EnvironmentVariableTarget.Machine);

	[SupportedOSPlatform(OS_WIN)]
	public static bool IsAdministrator()
	{
		using var identity = WindowsIdentity.GetCurrent();

		var principal = new WindowsPrincipal(identity);

		return principal.IsInRole(WindowsBuiltInRole.Administrator);

	}

	public static bool IsRoot
	{
		get
		{
			if (IsLinux) {
				return Environment.UserName == "root";
			}
			else if (IsWindows) {
				return IsAdministrator();

			}

			throw new InvalidOperationException();

		}
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