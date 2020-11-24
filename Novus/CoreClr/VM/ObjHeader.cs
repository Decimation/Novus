using System.Runtime.InteropServices;
using Novus.Interop;

namespace Novus.CoreClr.VM
{
	[NativeStructure]
	[StructLayout(LayoutKind.Sequential)]
	public struct ObjHeader
	{
		private nint Value { get; }
	}
}
