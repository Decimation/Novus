using System;
using System.Runtime.InteropServices;

// ReSharper disable UnusedMember.Global

namespace Novus.Win32.Structures
{
	/*[StructLayout(LayoutKind.Sequential)]
	public struct MemoryBasicInformation64
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
	}*/

	[StructLayout(LayoutKind.Sequential)]
	public struct MemoryBasicInformation
	{
		public IntPtr           BaseAddress;
		public IntPtr           AllocationBase;
		public MemoryProtection AllocationProtect;
		public IntPtr           RegionSize;
		public AllocationType   State;
		public MemoryProtection Protect;
		public MemType          Type;

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{nameof(BaseAddress)}: {BaseAddress:X},\n" +
			       $"{nameof(AllocationBase)}: {AllocationBase:X},\n" +
			       $"{nameof(AllocationProtect)}: {AllocationProtect},\n" +
			       $"{nameof(RegionSize)}: {RegionSize},\n" +
			       $"{nameof(State)}: {State},\n" +
			       $"{nameof(Protect)}: {Protect},\n" +
			       $"{nameof(Type)}: {Type}";
		}
	}

	public enum MemType : uint
	{
		MEM_IMAGE   = 0x1000000,
		MEM_MAPPED  = 0x40000,
		MEM_PRIVATE = 0x20000
	}
}