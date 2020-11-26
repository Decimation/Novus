using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using SimpleCore.Utilities;

// ReSharper disable ConvertIfStatementToReturnStatement

// ReSharper disable UnusedMember.Global

#nullable enable


namespace Novus.Win32.FileSystem
{
	/// <summary>
	///     Utilities for working with the file system, files, etc.
	/// </summary>
	/// <seealso cref="File" />
	/// <seealso cref="FileInfo" />
	/// <seealso cref="Directory" />
	/// <seealso cref="DirectoryInfo" />
	/// <seealso cref="Path" />
	public static class Files
	{
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

			if (fileBytes.StartsWith(jpegJfifExif)  ||
			    fileBytes.StartsWith(jpegJfifExif2) ||
			    fileBytes.StartsWith(jpegJfifExif3)) {
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
}