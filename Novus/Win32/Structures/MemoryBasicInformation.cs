using System;
using System.Runtime.InteropServices;

namespace Novus.Win32.Structures
{
	[StructLayout(LayoutKind.Sequential)]
	public struct MemoryBasicInformation
	{
		public IntPtr BaseAddress;
		public IntPtr AllocationBase;
		public MemoryProtection   AllocationProtect;
		public IntPtr RegionSize;
		public AllocationType   State;
		public MemoryProtection   Protect;
		public TypeEnum   Type;
	}
	public enum TypeEnum : uint//todo
	{
		MEM_IMAGE   = 0x1000000,
		MEM_MAPPED  = 0x40000,
		MEM_PRIVATE = 0x20000
	}
}