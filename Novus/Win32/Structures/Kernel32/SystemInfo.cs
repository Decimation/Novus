using System.Runtime.InteropServices;

// ReSharper disable UnusedMember.Global

namespace Novus.Win32.Structures.Kernel32;

[StructLayout(LayoutKind.Sequential)]
public struct SystemInfo
{
	public ProcessorArchitecture ProcessorArchitecture;

	public ushort Reserved;
	public uint   PageSize;

	public nint MinimumApplicationAddress;
	public nint MaximumApplicationAddress;

	public nint ActiveProcessorMask;
	public uint   NumberOfProcessors;
	public uint   ProcessorType;

	public uint AllocationGranularity;

	public ushort ProcessorLevel;
	public ushort ProcessorRevision;
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