using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
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
	public static class FileSystem
	{
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
				string path = Marshal.PtrToStringUni(outPath);
				Marshal.FreeCoTaskMem(outPath);
				return path;
			}
			else {
				throw new ExternalException("Unable to retrieve the known folder path. It may not "
				                            + "be available on this system.", result);
			}
		}


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

			return GetParent(Directory.GetParent(fi).FullName, --n);
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

			rg.AddRange(OS.PathDirectories);

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
		public static long GetFileSize(string file)
		{
			var f = new FileInfo(file);

			return f.Length;
		}

		//todo

		/// <summary>
		///     Attempts to determine the file format (type) given a file.
		/// </summary>
		/// <param name="file">File whose type to resolve</param>
		/// <returns>
		///     The best <see cref="FileFormatType" /> match; <see cref="FileFormatType.Unknown" /> if the type could not be
		///     determined.
		/// </returns>
		public static FileFormatType ResolveFileType(string file) => ResolveFileType(File.ReadAllBytes(file));

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
			// todo: FileIdentity, FileSequence, etc

			/*
			 * JPEG RAW
			 */

			var jpegStart = new byte[] {0xFF, 0xD8, 0xFF, 0xDB};
			var jpegEnd   = new byte[] {0xFF, 0xD9};

			if (fileBytes.StartsWith(jpegStart) && fileBytes.EndsWith(jpegEnd)) {
				return FileFormatType.JPEG_RAW;
			}

			/*
			 * JPEG JFIF
			 */

			//var jpegJfif = new byte[] {0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01};
			var jpegJfifExif  = new byte[] {0xFF, 0xD8, 0xFF, 0xDB};
			var jpegJfifExif2 = new byte[] {0xFF, 0xD8, 0xFF, 0xE1};
			var jpegJfifExif3 = new byte[] {0xFF, 0xD8, 0xFF, 0xE0};

			// Photoshop unique signature
			var jpegJfifExif4 = new byte[] {0xFF, 0xD8, 0xFF, 0xED};

			if (fileBytes.StartsWith(jpegJfifExif)  ||
			    fileBytes.StartsWith(jpegJfifExif2) ||
			    fileBytes.StartsWith(jpegJfifExif3) ||
			    fileBytes.StartsWith(jpegJfifExif4)) {
				return FileFormatType.JPEG_JFIF_EXIF;
			}

			/*
			 * PNG
			 */

			var png = new byte[] {0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A};

			if (fileBytes.StartsWith(png)) {
				return FileFormatType.PNG;
			}

			/*
			 * GIF
			 */

			var gif = new byte[] {0x47, 0x49, 0x46, 0x38};

			if (fileBytes.StartsWith(gif)) {
				return FileFormatType.GIF;
			}

			/*
			 * BMP
			 */

			var bmp = new byte[] {0x42, 0x4D};

			if (fileBytes.StartsWith(bmp)) {
				return FileFormatType.BMP;
			}

			/*
			 * Unknown
			 */

			return FileFormatType.Unknown;
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