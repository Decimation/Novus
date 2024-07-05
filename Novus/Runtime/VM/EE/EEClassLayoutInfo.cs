using System.Runtime.InteropServices;
using Novus.Imports;
using Novus.Imports.Attributes;
using Novus.Win32;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable InconsistentNaming

namespace Novus.Runtime.VM.EE;

[NativeStructure]
[StructLayout(LayoutKind.Sequential)]
public unsafe struct EEClassLayoutInfo
{
		
	internal int ManagedSize { get; set; }

	// 1,2,4 or 8: this is equal to the largest of the alignment requirements
	// of each of the EEClass's members. If the NStruct extends another NStruct,
	// the base NStruct is treated as the first member for the purpose of
	// this calculation.

	// Post V1.0 addition: This is the equivalent of m_LargestAlignmentRequirementOfAllMember
	// for the managed layout.

	// Alias: ManagedLargestAlignmentRequirementOfAllMembers

	internal byte ManagedMaxAlignReqOfAll { get; set; }

	internal LayoutFlags Flags { get; set; }

	internal byte PackingSize { get; set; }

}