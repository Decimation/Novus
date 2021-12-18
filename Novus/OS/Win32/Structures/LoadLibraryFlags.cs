using System;

// ReSharper disable UnusedMember.Global

namespace Novus.OS.Win32.Structures;

[Flags]
public enum LoadLibraryFlags : uint
{
	AsDataFile      = 0x00000002,
	AsImageResource = 0x00000020
}