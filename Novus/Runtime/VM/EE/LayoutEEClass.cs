using System.Runtime.InteropServices;
using Novus.Imports;

// ReSharper disable InconsistentNaming

namespace Novus.Runtime.VM.EE;

/// <summary>
/// Substructure of <see cref="EEClass"/>
/// </summary>
[NativeStructure]
[StructLayout(LayoutKind.Explicit)]
public unsafe struct LayoutEEClass
{
	// Note: This offset should be 72 or sizeof(EEClass)
	[field: FieldOffset(0x48)]
	internal EEClassLayoutInfo LayoutInfo;

	[field: FieldOffset(0x50)]
	internal EEClassNativeLayoutInfo* NativeLayoutInfo;
}