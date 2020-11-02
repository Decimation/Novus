using System.Runtime.InteropServices;

namespace Novus.CoreClr.VM.EE
{
	[StructLayout(LayoutKind.Explicit)]
	public struct LayoutEEClass
	{
		// Note: This offset should be 72 or sizeof(EEClass)
		// 		 but I'm keeping it at 0 to minimize size usage,
		//		 so I'll just offset the pointer by 72 bytes
		[field: FieldOffset(0)]
		internal EEClassLayoutInfo LayoutInfo { get; }
	}
}