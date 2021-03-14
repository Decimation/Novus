using System.Runtime.InteropServices;
// ReSharper disable UnusedMember.Global

namespace Novus.Win32.Structures
{
	[StructLayout(LayoutKind.Sequential)]
	public struct MemoryBasicInformation64//todo
	{
		public ulong BaseAddress;
		public ulong AllocationBase;
		public int   AllocationProtect;
		public int   __alignment1;
		public ulong RegionSize;
		public int   State;
		public int   Protect;
		public int   Type;
		public int   __alignment2;
	}
}