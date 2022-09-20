
// ReSharper disable UnusedMember.Global

namespace Novus.Win32.Structures.Kernel32;

[Flags]
public enum LoadLibraryFlags : uint
{
	AsDataFile      = 0x00000002,
	AsImageResource = 0x00000020
}