using System;
using System.Runtime.InteropServices;

// ReSharper disable StructCanBeMadeReadOnly

namespace Novus.Win32.Structures
{
	[StructLayout(LayoutKind.Explicit)]
	internal struct ImageSectionHeader
	{
		private const int IMAGE_SIZEOF_SHORT_NAME = 8;

		[field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = IMAGE_SIZEOF_SHORT_NAME)]
		[field: FieldOffset(0)]
		public string Name { get; }

		[field: FieldOffset(8)]
		public UInt32 PhysicalAddress { get; }

		[field: FieldOffset(8)]
		public UInt32 VirtualSize { get; }

		[field: FieldOffset(12)]
		public UInt32 VirtualAddress { get; }

		[field: FieldOffset(14)]
		public UInt32 SizeOfRawData { get; }

		[field: FieldOffset(18)]
		public UInt32 PointerToRawData { get; }

		[field: FieldOffset(22)]
		public UInt32 PointerToRelocations { get; }

		[field: FieldOffset(26)]
		public UInt32 PointerToLineNumbers { get; }

		[field: FieldOffset(30)]
		public UInt16 NumberOfRelocations { get; }

		[field: FieldOffset(32)]
		public UInt16 NumberOfLineNumbers { get; }

		[field: FieldOffset(36)]
		public ImageSectionCharacteristics Characteristics { get; }
	}
}