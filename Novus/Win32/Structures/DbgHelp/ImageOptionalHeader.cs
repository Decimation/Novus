using System.Runtime.InteropServices;
using Novus.Imports.Attributes;
using Novus.Win32.Structures.Kernel32;

// ReSharper disable StructCanBeMadeReadOnly
// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
namespace Novus.Win32.Structures.DbgHelp;

[StructLayout(LayoutKind.Explicit)]
[NativeStructure]
internal struct ImageOptionalHeader
{
	[field: FieldOffset(0)]
	public MagicType Magic { get; }

	[field: FieldOffset(2)]
	public byte MajorLinkerVersion { get; }

	[field: FieldOffset(3)]
	public byte MinorLinkerVersion { get; }

	[field: FieldOffset(4)]
	public uint SizeOfCode { get; }

	[field: FieldOffset(8)]
	public uint SizeOfInitializedData { get; }

	[field: FieldOffset(12)]
	public uint SizeOfUninitializedData { get; }

	[field: FieldOffset(16)]
	public uint AddressOfEntryPoint { get; }

	[field: FieldOffset(20)]
	public uint BaseOfCode { get; }

	[field: FieldOffset(24)]
	public ulong ImageBase { get; }

	[field: FieldOffset(32)]
	public uint SectionAlignment { get; }

	[field: FieldOffset(36)]
	public uint FileAlignment { get; }

	[field: FieldOffset(40)]
	public ushort MajorOperatingSystemVersion { get; }

	[field: FieldOffset(42)]
	public ushort MinorOperatingSystemVersion { get; }

	[field: FieldOffset(44)]
	public ushort MajorImageVersion { get; }

	[field: FieldOffset(46)]
	public ushort MinorImageVersion { get; }

	[field: FieldOffset(48)]
	public ushort MajorSubsystemVersion { get; }

	[field: FieldOffset(50)]
	public ushort MinorSubsystemVersion { get; }

	[field: FieldOffset(52)]
	public uint Win32VersionValue { get; }

	[field: FieldOffset(56)]
	public uint SizeOfImage { get; }

	[field: FieldOffset(60)]
	public uint SizeOfHeaders { get; }

	[field: FieldOffset(64)]
	public uint CheckSum { get; }

	[field: FieldOffset(68)]
	public SubSystemType Subsystem { get; }

	[field: FieldOffset(70)]
	public DllCharacteristics DllCharacteristics { get; }

	[field: FieldOffset(72)]
	public ulong SizeOfStackReserve { get; }

	[field: FieldOffset(80)]
	public ulong SizeOfStackCommit { get; }

	[field: FieldOffset(88)]
	public ulong SizeOfHeapReserve { get; }

	[field: FieldOffset(96)]
	public ulong SizeOfHeapCommit { get; }

	[field: FieldOffset(104)]
	public uint LoaderFlags { get; }

	[field: FieldOffset(108)]
	public uint NumberOfRvaAndSizes { get; }

	[field: FieldOffset(112)]
	public ImageDataDirectory ExportTable { get; }

	[field: FieldOffset(120)]
	public ImageDataDirectory ImportTable { get; }

	[field: FieldOffset(128)]
	public ImageDataDirectory ResourceTable { get; }

	[field: FieldOffset(136)]
	public ImageDataDirectory ExceptionTable { get; }

	[field: FieldOffset(144)]
	public ImageDataDirectory CertificateTable { get; }

	[field: FieldOffset(152)]
	public ImageDataDirectory BaseRelocationTable { get; }

	[field: FieldOffset(160)]
	public ImageDataDirectory Debug { get; }

	[field: FieldOffset(168)]
	public ImageDataDirectory Architecture { get; }

	[field: FieldOffset(176)]
	public ImageDataDirectory GlobalPtr { get; }

	[field: FieldOffset(184)]
	public ImageDataDirectory TLSTable { get; }

	[field: FieldOffset(192)]
	public ImageDataDirectory LoadConfigTable { get; }

	[field: FieldOffset(200)]
	public ImageDataDirectory BoundImport { get; }

	[field: FieldOffset(208)]
	public ImageDataDirectory IAT { get; }

	[field: FieldOffset(216)]
	public ImageDataDirectory DelayImportDescriptor { get; }

	[field: FieldOffset(224)]
	public ImageDataDirectory CLRRuntimeHeader { get; }

	[field: FieldOffset(232)]
	public ImageDataDirectory Reserved { get; }
}