using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleCore.Model;
using SimpleCore.Utilities;

// ReSharper disable InconsistentNaming
#nullable enable


namespace Novus.Win32.FileSystem
{
	/// <summary>
	/// Represents a file format or type.
	/// </summary>
	public class FileFormatType : Enumeration
	{
		/// <summary>
		/// File extensions associated with the described file type
		/// </summary>
		public string[]? Extension { get; private init; }

		/// <summary>
		/// File format type
		/// </summary>
		public FileType Type { get; private init; }

		/// <summary>
		/// (Unknown)
		/// </summary>
		public static readonly FileFormatType Unknown = new(0, nameof(Unknown))
		{
			Extension = null,
			Type      = FileType.Unknown
		};

		/// <summary>
		/// Joint Photographic Experts Group
		/// </summary>
		/// <remarks>JPEG/JPG</remarks>
		public static readonly FileFormatType JPEG_RAW = new(1, nameof(JPEG_RAW))
		{
			Extension = new[] {".jpeg", ".jpg"},
			Type      = FileType.Image
		};

		/// <summary>
		/// Joint Photographic Experts Group
		/// </summary>
		/// <remarks>JPEG/JPG/JFIF</remarks>
		public static readonly FileFormatType JPEG_JFIF_EXIF = new(2, nameof(JPEG_JFIF_EXIF))
		{
			Extension = new[] {JPEG_RAW.Extension![0], JPEG_RAW.Extension![1], ".jfif"},
			Type      = FileType.Image
		};

		/// <summary>
		/// Portable Network Graphics
		/// </summary>
		/// <remarks>PNG</remarks>
		public static readonly FileFormatType PNG = new(3, nameof(PNG))
		{
			Extension = new[] {".png"},
			Type      = FileType.Image
		};

		/// <summary>
		/// Graphical Interchange Format
		/// </summary>
		/// <remarks>GIF</remarks>
		public static readonly FileFormatType GIF = new(4, nameof(GIF))
		{
			Extension = new[] {".gif"},
			Type      = FileType.Image
		};

		/// <summary>
		/// Bitmap
		/// </summary>
		/// <remarks>BMP</remarks>
		public static readonly FileFormatType BMP = new(5, nameof(BMP))
		{
			Extension = new[] {".bmp"},
			Type      = FileType.Image
		};

		private FileFormatType(int id, string name) : base(id, name) { }

		public static IEnumerable<FileFormatType> GetAll()
		{
			return Enumeration.GetAll<FileFormatType>();
		}

		public override string ToString()
		{
			return String.Format("{0} {1} {2}", base.ToString(), Extension?.QuickJoin(), Type);
		}
	}

	/// <summary>
	/// File type
	/// </summary>
	public enum FileType
	{
		/// <summary>
		/// (Unknown)
		/// </summary>
		Unknown,

		/// <summary>
		/// Image
		/// </summary>
		Image,
	}
}