using System.Runtime.InteropServices;
using Novus.Imports;
using Novus.Imports.Attributes;
using Novus.Win32;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable InconsistentNaming

namespace Novus.Runtime.VM.EE;

public enum LayoutType : byte
{

	/*FF enum EEClassLayoutInfo::LayoutType : __int8
	FF { // XREF: MethodTableBuilder::bmtLayoutInfo/r
	FF   // EEClassLayoutInfo/r
	FF     Auto       = 0x0,
	FF     Sequential = 0x1,
	FF     Explicit   = 0x2,
	FF };*/

	Auto       = 0x0,
	Sequential = 0x1,
	Explicit   = 0x2,

}

[NativeStructure]
[StructLayout(LayoutKind.Sequential)]
public unsafe struct EEClassLayoutInfo
{

/*
 *
   00000000 struct EEClassLayoutInfo // sizeof=0x4
   00000000 {                                       // XREF: LayoutEEClass/r
   00000000     EEClassLayoutInfo::LayoutType m_LayoutType;
   00000001     unsigned __int8 m_ManagedLargestAlignmentRequirementOfAllMembers;
   00000002     unsigned __int8 m_bFlags;
   00000003     unsigned __int8 m_cbPackingSize;
   00000004 };
 *
 */

	internal LayoutType LayoutType { get; set; }

	// 1,2,4 or 8: this is equal to the largest of the alignment requirements
	// of each of the EEClass's members. If the NStruct extends another NStruct,
	// the base NStruct is treated as the first member for the purpose of
	// this calculation.

	// Post V1.0 addition: This is the equivalent of m_LargestAlignmentRequirementOfAllMember
	// for the managed layout.

	// Alias: ManagedLargestAlignmentRequirementOfAllMembers

	internal byte ManagedLargestAlignReqOfAll { get; set; }

	internal LayoutFlags Flags { get; set; }

	internal byte PackingSize { get; set; }

}