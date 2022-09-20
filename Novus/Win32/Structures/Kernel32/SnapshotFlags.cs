
// ReSharper disable UnusedMember.Global

namespace Novus.Win32.Structures.Kernel32;

[Flags]
public enum SnapshotFlags : uint
{
	HeapList = 0x00000001,
	Process  = 0x00000002,
	Thread   = 0x00000004,
	Module   = 0x00000008,
	Module32 = 0x00000010,
	Inherit  = 0x80000000,
	All      = 0x0000001F
}