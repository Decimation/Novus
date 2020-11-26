using System.Runtime.InteropServices;
using Novus.Interop;

// ReSharper disable InconsistentNaming

namespace Novus.CoreClr.VM.EE
{
	[NativeStructure]
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct LayoutEEClass
	{
		// Note: This offset should be 72 or sizeof(EEClass)
		[field: FieldOffset(0x48)] internal EEClassLayoutInfo LayoutInfo;

		[field: FieldOffset(0x50)] internal EEClassNativeLayoutInfo* NativeLayoutInfo;
	}
}