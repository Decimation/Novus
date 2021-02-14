using System.Runtime.InteropServices;
using Novus.Imports;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable InconsistentNaming

namespace Novus.Runtime.VM.EE
{
	[NativeStructure]
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct EEClassLayoutInfo
	{
		
		internal int ManagedSize { get; }

		// 1,2,4 or 8: this is equal to the largest of the alignment requirements
		// of each of the EEClass's members. If the NStruct extends another NStruct,
		// the base NStruct is treated as the first member for the purpose of
		// this calculation.

		

		// Post V1.0 addition: This is the equivalent of m_LargestAlignmentRequirementOfAllMember
		// for the managed layout.

		// Alias: ManagedLargestAlignmentRequirementOfAllMembers
		internal byte ManagedMaxAlignReqOfAll { get; }

		internal LayoutFlags Flags { get; }

		internal byte PackingSize { get; }

		
	}
}