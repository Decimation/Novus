using System;
using System.Collections.Generic;
using SimpleCore.Model;
using SimpleCore.Utilities;

// ReSharper disable UnusedMember.Global

// ReSharper disable InconsistentNaming
#nullable enable


namespace Novus.Win32
{
	/*
	 * https://github.com/cdgriffith/puremagic/blob/master/puremagic/magic_data.json
	 */


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
		/// Signatures
		/// </summary>
		public List<byte[]> Signature { get; private init; }

		/// <summary>
		/// (Unknown)
		/// </summary>
		public static readonly FileFormatType Unknown = new(0, nameof(Unknown))
		{
			Extension = null,
			Type      = FileType.Unknown,

		};

		/// <summary>
		/// Joint Photographic Experts Group
		/// </summary>
		/// <remarks>JPEG/JPG/JFIF</remarks>
		public static readonly FileFormatType JPEG = new(1, nameof(JPEG))
		{
			Extension = new[] {".jpeg", ".jpg", ".jfif"},
			Type      = FileType.Image,
			Signature = new List<byte[]>
			{
				// DB, E1, E0, ED

				new byte[] {0xFF, 0xD8, 0xFF},
			},
		};


		/// <summary>
		/// Portable Network Graphics
		/// </summary>
		/// <remarks>PNG</remarks>
		public static readonly FileFormatType PNG = new(2, nameof(PNG))
		{
			Extension = new[] {".png"},
			Type      = FileType.Image,
			Signature = new List<byte[]>
			{
				new byte[] {0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A},
			},
		};

		/// <summary>
		/// Graphical Interchange Format
		/// </summary>
		/// <remarks>GIF</remarks>
		public static readonly FileFormatType GIF = new(3, nameof(GIF))
		{
			Extension = new[] {".gif"},
			Type      = FileType.Image,
			Signature = new List<byte[]>
			{
				new byte[] {0x47, 0x49, 0x46, 0x38}
			},
		};

		/// <summary>
		/// Bitmap
		/// </summary>
		/// <remarks>BMP</remarks>
		public static readonly FileFormatType BMP = new(4, nameof(BMP))
		{
			Extension = new[] {".bmp"},
			Type      = FileType.Image,
			Signature = new List<byte[]>
			{
				new byte[] {0x42, 0x4D},
			},
		};

		/// <summary>
		/// Executable
		/// </summary>
		/// <remarks>EXE</remarks>
		public static readonly FileFormatType EXE = new(5, nameof(EXE))
		{
			Extension = new[] {".exe"},
			Type      = FileType.Executable,
			Signature = new List<byte[]>
			{
				new byte[] {0x4D, 0x5A},
			},
		};

		/// <summary>
		/// Icon
		/// </summary>
		/// <remarks>ICO</remarks>
		public static readonly FileFormatType ICO = new(6, nameof(ICO))
		{
			Extension = new[] {".ico"},
			Type      = FileType.Image,
			Signature = new List<byte[]>
			{
				new byte[] {0x00, 0x00, 0x01, 0x00},
			},
		};

		private FileFormatType(int id, string name) : base(id, name)
		{
			Signature = new List<byte[]>();
		}

		public static IEnumerable<FileFormatType> GetAll() => GetAll<FileFormatType>();

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

		/// <summary>
		/// Executable
		/// </summary>
		Executable,
	}
}