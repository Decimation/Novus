using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Novus.Win32.Structures;
using SimpleCore.Diagnostics;
using SimpleCore.Utilities;

// ReSharper disable ConvertIfStatementToReturnStatement

// ReSharper disable UnusedMember.Global

#nullable enable


namespace Novus.Win32
{
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
		public static string GetPath(KnownFolder knownFolder, bool defaultUser) =>
			GetPath(knownFolder, KnownFolderFlags.DontVerify, defaultUser);

		private static string GetPath(KnownFolder knownFolder, KnownFolderFlags flags, bool defaultUser)
		{
			int result = Native.SHGetKnownFolderPath(new Guid(KnownFolderGuids[(int) knownFolder]),
				(uint) flags, new IntPtr(defaultUser ? -1 : 0), out IntPtr outPath);

			if (result >= 0) {
				string path = Marshal.PtrToStringUni(outPath)!;
				Marshal.FreeCoTaskMem(outPath);
				return path;
			}
			else {
				throw new ExternalException(
					"Unable to retrieve the known folder path. It may not be available on this system.", result);
			}
		}

		#endregion


		public static string GetRelativeParent([NotNull] string fi, int n)
		{
			var i = new string[n + 1];

			Array.Fill(i, "..");
			i[0] = fi;
			var p = Path.Combine(i);

			return p;
		}

		public static string GetParent([NotNull] string fi, int n)
		{
			if (n == 0) {
				return fi;
			}

			return GetParent(Directory.GetParent(fi)!.FullName, --n);
		}


		// public static DirectoryInfo GetParentLevel([NotNull] this DirectoryInfo fi, int n)
		// {
		// 	if (n == 0) {
		// 		return fi;
		// 	}
		//
		// 	return fi.Parent.GetParentLevel(--n);
		// }

		public static string CreateRandomName() => Path.GetFileNameWithoutExtension(Path.GetRandomFileName());

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
			Process.Start(Native.EXPLORER_EXE, $"/select,\"{filePath}\"");
			return true;
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
				Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory!
					.Replace("file:///", String.Empty)
					.Replace("/", "\\"))!,


			};

			rg.AddRange(PathDirectories);

			//

			foreach (string loc in rg) {
				if (ExistsInFolder(loc, exe, out var folder)) {
					return folder;
				}
			}


			return null;
		}

		/// <summary>
		///     Determines the file size (not size on disk) of <paramref name="file" />
		/// </summary>
		/// <param name="file">File location</param>
		/// <returns>Size of the file, in bytes</returns>
		public static long GetFileSize(string file) => new FileInfo(file).Length;

		#region File types
		//todo
		public static string ResolveMimeType(string file, string? mimeProposed = null) =>
			ResolveMimeType(File.ReadAllBytes(file), mimeProposed);


		public static string ResolveMimeType(byte[] dataBytes, string? mimeProposed = null)
		{
			//https://stackoverflow.com/questions/2826808/how-to-identify-the-extension-type-of-the-file-using-c/2826884#2826884
			//https://stackoverflow.com/questions/18358548/urlmon-dll-findmimefromdata-works-perfectly-on-64bit-desktop-console-but-gener
			//https://stackoverflow.com/questions/11547654/determine-the-file-type-using-c-sharp
			//https://github.com/GetoXs/MimeDetect/blob/master/src/Winista.MimeDetect/URLMONMimeDetect/urlmonMimeDetect.cs

			Guard.AssertArgumentNotNull(dataBytes, nameof(dataBytes));


			string mimeRet = String.Empty;

			if (!string.IsNullOrEmpty(mimeProposed)) {
				//suggestPtr = Marshal.StringToCoTaskMemUni(mimeProposed); // for your experiments ;-)
				mimeRet = mimeProposed;
			}

			int ret = Native.FindMimeFromData(IntPtr.Zero,
				null, dataBytes, dataBytes.Length, mimeProposed, 0, out var outPtr, 0);

			if (ret == 0 && outPtr != IntPtr.Zero) {

				var str = Marshal.PtrToStringUni(outPtr)!;

				Marshal.FreeHGlobal(outPtr);

				return str;
			}

			return mimeRet;
		}

		/// <summary>
		///     Attempts to determine the file format (type) given a file.
		/// </summary>
		/// <param name="file">File whose type to resolve</param>
		/// <returns>
		///     The best <see cref="FileFormatType" /> match; <see cref="FileFormatType.Unknown" /> if the type could not be
		///     determined.
		/// </returns>
		public static FileFormatType ResolveFileType(string file) => ResolveFileType(File.ReadAllBytes(file));


		private static readonly Dictionary<List<byte[]>, FileFormatType> FileTypeSignatures = new()
		{
			{
				new List<byte[]>
				{
					new byte[] {0xFF, 0xD8, 0xFF, 0xDB},
					new byte[] {0xFF, 0xD8, 0xFF, 0xE1},
					new byte[] {0xFF, 0xD8, 0xFF, 0xE0},

					// Photoshop
					new byte[] {0xFF, 0xD8, 0xFF, 0xED},
				},
				FileFormatType.JPEG

			},

			{
				new List<byte[]>
				{
					new byte[] {0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A},
				},
				FileFormatType.PNG
			},

			{
				new List<byte[]>
				{
					new byte[] {0x47, 0x49, 0x46, 0x38}
				},
				FileFormatType.GIF
			},

			{
				new List<byte[]>
				{
					new byte[] {0x42, 0x4D},
				},
				FileFormatType.BMP
			}
		};

		/// <summary>
		///     Attempts to determine the file format (type) given the raw bytes of a file
		///     by comparing file format magic bytes.
		/// </summary>
		/// <param name="fileBytes">Raw file bytes</param>
		/// <returns>
		///     The best <see cref="FileFormatType" /> match; <see cref="FileFormatType.Unknown" /> if the type could not be
		///     determined.
		/// </returns>
		public static FileFormatType ResolveFileType(byte[] fileBytes)
		{

			//Map.Keys.Any(k=>k.Any(b=>fileBytes.StartsWith(b)))

			foreach (var map in FileTypeSignatures) {
				foreach (byte[] bytes in map.Key) {
					if (fileBytes.StartsWith(bytes)) {
						return map.Value;
					}
				}
			}

			return FileFormatType.Unknown;
		}

		#endregion

		/// <summary>
		///     Environment variable PATH
		/// </summary>
		public const string PATH_ENV = "PATH";

		/// <summary>
		///     Delimiter of environment variable <see cref="PATH_ENV" />
		/// </summary>
		public const char PATH_DELIM = ';';

		/// <summary>
		///     Environment variable target
		/// </summary>
		public static EnvironmentVariableTarget Target { get; set; } = EnvironmentVariableTarget.User;

		/// <summary>
		///     Directories of <see cref="EnvironmentPath" /> with environment variable target <see cref="Target" />
		/// </summary>
		public static string[] PathDirectories => EnvironmentPath.Split(PATH_DELIM);

		/// <summary>
		///     Environment variable <see cref="PATH_ENV" /> with target <see cref="Target" />
		/// </summary>
		public static string EnvironmentPath
		{
			get
			{
				string? env = Environment.GetEnvironmentVariable(PATH_ENV, Target);

				if (env == null) {
					throw new NullReferenceException();
				}

				return env;
			}
			set => Environment.SetEnvironmentVariable(PATH_ENV, value, Target);
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
}