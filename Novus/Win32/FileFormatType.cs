using System;
using SimpleCore.Model;
using SimpleCore.Utilities;

// ReSharper disable InconsistentNaming
#nullable enable


#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
#pragma warning disable HAA0602 // Delegate on struct instance caused a boxing allocation
#pragma warning disable HAA0603 // Delegate allocation from a method group
#pragma warning disable HAA0604 // Delegate allocation from a method group

#pragma warning disable HAA0501 // Explicit new array type allocation
#pragma warning disable HAA0502 // Explicit new reference type allocation
#pragma warning disable HAA0503 // Explicit new reference type allocation
#pragma warning disable HAA0504 // Implicit new array creation allocation
#pragma warning disable HAA0505 // Initializer reference type allocation
#pragma warning disable HAA0506 // Let clause induced allocation

#pragma warning disable HAA0301 // Closure Allocation Source
#pragma warning disable HAA0302 // Display class allocation to capture closure
#pragma warning disable HAA0303 // Lambda or anonymous method in a generic method allocates a delegate instance

namespace Novus.Win32
{
	/// <summary>
	/// Represents a file format or type.
	/// </summary>
	public class FileFormatType : Enumeration
	{
		public string[]? Extension { get; private set; }

		public FileType Type { get; private set; }

		/// <summary>
		/// (Unknown)
		/// </summary>
		public static readonly FileFormatType Unknown = new FileFormatType(0, nameof(Unknown))
		{
			Extension = null,
			Type      = FileType.Unknown
		};

		/// <summary>
		/// Joint Photographic Experts Group
		/// </summary>
		/// <remarks>JPEG/JPG</remarks>
		public static readonly FileFormatType JPEG_RAW = new FileFormatType(1, nameof(JPEG_RAW))
		{
			Extension = new[] {".jpeg", ".jpg"},
			Type      = FileType.Image
		};

		/// <summary>
		/// Joint Photographic Experts Group
		/// </summary>
		/// <remarks>JPEG/JPG/JFIF</remarks>
		public static readonly FileFormatType JPEG_JFIF_EXIF = new FileFormatType(2, nameof(JPEG_JFIF_EXIF))
		{
			Extension = new[] {JPEG_RAW.Extension![0], JPEG_RAW.Extension![1], ".jfif"},
			Type      = FileType.Image
		};

		/// <summary>
		/// Portable Network Graphics
		/// </summary>
		/// <remarks>PNG</remarks>
		public static readonly FileFormatType PNG = new FileFormatType(3, nameof(PNG))
		{
			Extension = new[] {".png"},
			Type      = FileType.Image
		};

		/// <summary>
		/// Graphical Interchange Format
		/// </summary>
		/// <remarks>GIF</remarks>
		public static readonly FileFormatType GIF = new FileFormatType(4, nameof(GIF))
		{
			Extension = new[] {".gif"},
			Type      = FileType.Image
		};

		/// <summary>
		/// Bitmap
		/// </summary>
		/// <remarks>BMP</remarks>
		public static readonly FileFormatType BMP = new FileFormatType(5, nameof(BMP))
		{
			Extension = new[] {".bmp"},
			Type      = FileType.Image
		};

		private FileFormatType(int id, string name) : base(id, name) { }

		public override string ToString()
		{
			return String.Format("{0} {1} {2}", base.ToString(), Extension?.QuickJoin(), Type);
		}
	}

	public enum FileType
	{
		Unknown,
		Image,
	}
}