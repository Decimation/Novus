using System;
using System.Collections.Generic;
using SimpleCore.Model;
using SimpleCore.Utilities;

// ReSharper disable InconsistentNaming
#nullable enable


namespace Novus.Win32
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
		/// <remarks>JPEG/JPG/JFIF</remarks>
		public static readonly FileFormatType JPEG = new(1, nameof(JPEG))
		{
			Extension = new[] {".jpeg", ".jpg", ".jfif"},
			Type      = FileType.Image
		};


		/// <summary>
		/// Portable Network Graphics
		/// </summary>
		/// <remarks>PNG</remarks>
		public static readonly FileFormatType PNG = new(2, nameof(PNG))
		{
			Extension = new[] {".png"},
			Type      = FileType.Image
		};

		/// <summary>
		/// Graphical Interchange Format
		/// </summary>
		/// <remarks>GIF</remarks>
		public static readonly FileFormatType GIF = new(3, nameof(GIF))
		{
			Extension = new[] {".gif"},
			Type      = FileType.Image
		};

		/// <summary>
		/// Bitmap
		/// </summary>
		/// <remarks>BMP</remarks>
		public static readonly FileFormatType BMP = new(4, nameof(BMP))
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
			return $"{base.ToString()} {Extension?.QuickJoin()} {Type}";
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