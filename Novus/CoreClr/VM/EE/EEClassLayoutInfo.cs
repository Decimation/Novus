using System.Runtime.InteropServices;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable InconsistentNaming

namespace Novus.CoreClr.VM.EE
{
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct EEClassLayoutInfo
	{
		#region Fields

		// why is there an m_cbNativeSize in EEClassLayoutInfo and EEClass?
		internal int NativeSize { get; }

		internal int ManagedSize { get; }

		// 1,2,4 or 8: this is equal to the largest of the alignment requirements
		// of each of the EEClass's members. If the NStruct extends another NStruct,
		// the base NStruct is treated as the first member for the purpose of
		// this calculation.

		// Alias: LargestAlignmentRequirementOfAllMembers
		internal byte MaxAlignReqOfAll { get; }

		// Post V1.0 addition: This is the equivalent of m_LargestAlignmentRequirementOfAllMember
		// for the managed layout.

		// Alias: ManagedLargestAlignmentRequirementOfAllMembers
		internal byte ManagedMaxAlignReqOfAll { get; }

		internal LayoutFlags Flags { get; }

		internal byte PackingSize { get; }

		/// <summary>
		///     # of fields that are of the calltime-marshal variety.
		/// </summary>
		internal int NumCTMFields { get; }

		internal void* FieldMarshalers { get; }

		#endregion
	}
}