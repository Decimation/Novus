// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
namespace Novus.Win32.Structures;

public enum MachineType : ushort
{
	Native  = 0,
	I386    = 0x014c,
	Itanium = 0x0200,
	x64     = 0x8664
}