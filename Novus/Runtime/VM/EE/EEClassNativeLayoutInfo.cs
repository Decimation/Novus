using System.Runtime.InteropServices;
using Novus.Imports;
using Novus.Imports.Attributes;
using Novus.Win32;

// ReSharper disable InconsistentNaming

namespace Novus.Runtime.VM.EE;

[NativeStructure]
[StructLayout(LayoutKind.Sequential)]
public unsafe struct EEClassNativeLayoutInfo
{

	/*struct __cppobj EEClassNativeLayoutInfo // sizeof=0x10;variable_size
	00000000 {
	00000000     unsigned __int8 m_alignmentRequirement;
	00000001     bool            m_isMarshalable;
	00000002 // padding byte
	00000003 // padding byte
	00000004     unsigned int m_size;
	00000008     unsigned int m_numFields;
	0000000C // padding byte
	0000000D // padding byte
	0000000E // padding byte
	0000000F // padding byte
	00000010     NativeFieldDescriptor m_nativeFieldDescriptors[];
	00000010 };*/

	internal byte Alignment { get; set; }

	internal bool IsMarshalable { get; set; }

	private fixed byte Padding1[2];

	internal uint Size { get; set; }

	internal uint NumFields { get; set; }

	private fixed byte Padding2[4];

	internal void* NativeFieldDescriptors { get; set; }

}