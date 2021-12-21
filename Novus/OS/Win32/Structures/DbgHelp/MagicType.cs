// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
namespace Novus.OS.Win32.Structures.DbgHelp;

public enum MagicType : ushort
{
	IMAGE_NT_OPTIONAL_HDR32_MAGIC = 0x10b,
	IMAGE_NT_OPTIONAL_HDR64_MAGIC = 0x20b
}