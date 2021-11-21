using System;
// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
namespace Novus.Win32.Structures;

[Flags]
public enum ProcessAccess : uint
{
	All                     = 0x1F0FFF,
	Terminate               = 0x000001,
	CreateThread            = 0x000002,
	VmOperation             = 0x00000008,
	VmRead                  = 0x000010,
	VmWrite                 = 0x000020,
	DupHandle               = 0x00000040,
	CreateProcess           = 0x000080,
	SetInformation          = 0x00000200,
	QueryInformation        = 0x000400,
	QueryLimitedInformation = 0x001000,
	Synchronize             = 0x00100000
}