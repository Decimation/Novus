using System.Runtime.InteropServices;

// ReSharper disable UnassignedGetOnlyAutoProperty

// ReSharper disable StructCanBeMadeReadOnly
// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
namespace Novus.OS.Win32.Structures;

[StructLayout(LayoutKind.Explicit)]
internal struct ImageSectionHeader
{
	private const int IMAGE_SIZEOF_SHORT_NAME = 8;

	[field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = IMAGE_SIZEOF_SHORT_NAME)]
	[field: FieldOffset(0)]
	public string Name { get; }

	[field: FieldOffset(8)]
	public uint PhysicalAddress { get; }

	[field: FieldOffset(8)]
	public uint VirtualSize { get; }

	[field: FieldOffset(12)]
	public uint VirtualAddress { get; }

	[field: FieldOffset(14)]
	public uint SizeOfRawData { get; }

	[field: FieldOffset(18)]
	public uint PointerToRawData { get; }

	[field: FieldOffset(22)]
	public uint PointerToRelocations { get; }

	[field: FieldOffset(26)]
	public uint PointerToLineNumbers { get; }

	[field: FieldOffset(30)]
	public ushort NumberOfRelocations { get; }

	[field: FieldOffset(32)]
	public ushort NumberOfLineNumbers { get; }

	[field: FieldOffset(36)]
	public ImageSectionCharacteristics Characteristics { get; }
}