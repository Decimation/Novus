using System.Runtime.InteropServices;
using Novus.Interop;

namespace Novus.Runtime.VM
{
	[NativeStructure]
	[StructLayout(LayoutKind.Sequential)]
	public struct ObjHeader
	{
		private nint Value { get; }
	}
}
