using System;
using System.Runtime.InteropServices;

namespace Novus.Win32.Structures
{
	[StructLayout(LayoutKind.Sequential)]
	public struct MemoryBasicInformation
	{
		public IntPtr BaseAddress;
		public IntPtr AllocationBase;
		public uint   AllocationProtect;
		public IntPtr RegionSize;
		public uint   State;
		public uint   Protect;
		public uint   Type;
	}
}