using System;
using System.Runtime.InteropServices;

// ReSharper disable UnusedMember.Global

namespace Novus.Win32.Structures
{
	[StructLayout(LayoutKind.Sequential)]
	public struct SystemInfo
	{
		public ProcessorArchitecture wProcessorArchitecture;
		public ushort                wReserved;
		public uint                  dwPageSize;
		public IntPtr                lpMinimumApplicationAddress;
		public IntPtr                lpMaximumApplicationAddress;
		public IntPtr                dwActiveProcessorMask;
		public uint                  dwNumberOfProcessors;
		public uint                  dwProcessorType;
		public uint                  dwAllocationGranularity;
		public ushort                wProcessorLevel;
		public ushort                wProcessorRevision;
	}

	public enum ProcessorArchitecture : ushort
	{
		AMD64   = 9,
		ARM     = 5,
		ARM64   = 12,
		IA64    = 6,
		Intel   = 0,
		Unknown = 0xFFFF
	}
}