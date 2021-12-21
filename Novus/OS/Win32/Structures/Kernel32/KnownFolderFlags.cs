using System;

// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace Novus.OS.Win32.Structures.Kernel32;

[Flags]
internal enum KnownFolderFlags : uint
{
	SimpleIDList              = 0x00000100,
	NotParentRelative         = 0x00000200,
	DefaultPath               = 0x00000400,
	Init                      = 0x00000800,
	NoAlias                   = 0x00001000,
	DontUnexpand              = 0x00002000,
	DontVerify                = 0x00004000,
	Create                    = 0x00008000,
	NoAppcontainerRedirection = 0x00010000,
	AliasOnly                 = 0x80000000
}