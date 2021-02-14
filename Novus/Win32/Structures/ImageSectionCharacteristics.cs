using System;
// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
namespace Novus.Win32.Structures
{
	[Flags]
	public enum ImageSectionCharacteristics : uint
	{
		// Reserved 0x00000000
		// Reserved 0x00000001
		// Reserved 0x00000002
		// Reserved 0x00000004

		/// <summary>
		///     The section should not be padded to the next boundary. This flag is obsolete and is replaced by
		///     <see cref="ALIGN_1BYTES" />
		/// </summary>
		TYPE_NO_PAD = 0x00000008,

		// Reserved 0x00000010

		/// <summary>
		///     The section contains executable code.
		/// </summary>
		CNT_CODE = 0x00000020,

		/// <summary>
		///     The section contains initialized data.
		/// </summary>
		CNT_INITIALIZED_DATA = 0x00000040,

		/// <summary>
		///     The section contains uninitialized data.
		/// </summary>
		CNT_UNINITIALIZED_DATA = 0x00000080,

		/// <summary>
		///     Reserved.
		/// </summary>
		LNK_OTHER = 0x00000100,

		/// <summary>
		///     The section contains comments or other information. This is valid only for object files.
		/// </summary>
		LNK_INFO = 0x00000200,

		// Reserved 0x00000400

		/// <summary>
		///     The section will not become part of the image. This is valid only for object files.
		/// </summary>
		LNK_REMOVE = 0x00000800,

		/// <summary>
		///     The section contains COMDAT data. This is valid only for object files.
		/// </summary>
		LNK_COMDAT = 0x00001000,

		// Reserved 0x00002000

		/// <summary>
		///     Reset speculative exceptions handling bits in the TLB entries for this section.
		/// </summary>
		NO_DEFER_SPEC_EXC = 0x00004000,

		/// <summary>
		///     The section contains data referenced through the global pointer.
		/// </summary>
		GPREL = 0x00008000,

		/// <summary>
		///     Reserved.
		/// </summary>
		MEM_PURGEABLE = 0x00020000,

		/// <summary>
		///     Reserved.
		/// </summary>
		MEM_LOCKED = 0x00040000,

		/// <summary>
		///     Reserved.
		/// </summary>
		MEM_PRELOAD = 0x00080000,

		/// <summary>
		///     Align data on a 1-byte boundary. This is valid only for object files.
		/// </summary>
		ALIGN_1BYTES = 0x00100000,

		/// <summary>
		///     Align data on a 2-byte boundary. This is valid only for object files.
		/// </summary>
		ALIGN_2BYTES = 0x00200000,

		/// <summary>
		///     Align data on a 4-byte boundary. This is valid only for object files.
		/// </summary>
		ALIGN_4BYTES = 0x00300000,

		/// <summary>
		///     Align data on a 8-byte boundary. This is valid only for object files.
		/// </summary>
		ALIGN_8BYTES = 0x00400000,

		/// <summary>
		///     Align data on a 16-byte boundary. This is valid only for object files.
		/// </summary>
		ALIGN_16BYTES = 0x00500000,

		/// <summary>
		///     Align data on a 32-byte boundary. This is valid only for object files.
		/// </summary>
		ALIGN_32BYTES = 0x00600000,

		/// <summary>
		///     Align data on a 64-byte boundary. This is valid only for object files.
		/// </summary>
		ALIGN_64BYTES = 0x00700000,

		/// <summary>
		///     Align data on a 128-byte boundary. This is valid only for object files.
		/// </summary>
		ALIGN_128BYTES = 0x00800000,

		/// <summary>
		///     Align data on a 256-byte boundary. This is valid only for object files.
		/// </summary>
		ALIGN_256BYTES = 0x00900000,

		/// <summary>
		///     Align data on a 512-byte boundary. This is valid only for object files.
		/// </summary>
		ALIGN_512BYTES = 0x00A00000,

		/// <summary>
		///     Align data on a 1024-byte boundary. This is valid only for object files.
		/// </summary>
		ALIGN_1024BYTES = 0x00B00000,

		/// <summary>
		///     Align data on a 2048-byte boundary. This is valid only for object files.
		/// </summary>
		ALIGN_2048BYTES = 0x00C00000,

		/// <summary>
		///     Align data on a 4096-byte boundary. This is valid only for object files.
		/// </summary>
		ALIGN_4096BYTES = 0x00D00000,

		/// <summary>
		///     Align data on a 8192-byte boundary. This is valid only for object files.
		/// </summary>
		ALIGN_8192BYTES = 0x00E00000,

		/// <summary>
		///     The section contains extended relocations.
		///     The count of relocations for the section exceeds the 16 bits that is reserved for it in the section header.
		///     If the <see cref="ImageSectionHeader.NumberOfRelocations" /> field in the section header is 0xffff,
		///     the actual relocation count is stored in the <see cref="ImageSectionHeader.VirtualAddress" /> field of the first
		///     relocation.
		///     It is an error if <see cref="LNK_NRELOC_OVFL" /> is set and there are fewer than 0xffff relocations in
		///     the section.
		/// </summary>
		LNK_NRELOC_OVFL = 0x01000000,

		/// <summary>
		///     The section can be discarded as needed.
		/// </summary>
		MEM_DISCARDABLE = 0x02000000,

		/// <summary>
		///     The section cannot be cached.
		/// </summary>
		MEM_NOT_CACHED = 0x04000000,

		/// <summary>
		///     The section cannot be paged.
		/// </summary>
		MEM_NOT_PAGED = 0x08000000,

		/// <summary>
		///     The section can be shared in memory.
		/// </summary>
		MEM_SHARED = 0x10000000,

		/// <summary>
		///     The section can be executed as code.
		/// </summary>
		MEM_EXECUTE = 0x20000000,

		/// <summary>
		///     The section can be read.
		/// </summary>
		MEM_READ = 0x40000000,

		/// <summary>
		///     The section can be written to.
		/// </summary>
		MEM_WRITE = 0x80000000

		// Reserved 0x00010000
	}
}