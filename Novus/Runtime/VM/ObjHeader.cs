using System.Runtime.InteropServices;
using Novus.Imports;

namespace Novus.Runtime.VM
{
	[NativeStructure]
	[StructLayout(LayoutKind.Sequential)]
	public struct ObjHeader
	{
		private nint Value { get; }
	}
}
