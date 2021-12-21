// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
namespace Novus.OS.Win32.Structures.Kernel32;

public enum SubSystemType : ushort
{
	IMAGE_SUBSYSTEM_UNKNOWN                 = 0,
	IMAGE_SUBSYSTEM_NATIVE                  = 1,
	IMAGE_SUBSYSTEM_WINDOWS_GUI             = 2,
	IMAGE_SUBSYSTEM_WINDOWS_CUI             = 3,
	IMAGE_SUBSYSTEM_POSIX_CUI               = 7,
	IMAGE_SUBSYSTEM_WINDOWS_CE_GUI          = 9,
	IMAGE_SUBSYSTEM_EFI_APPLICATION         = 10,
	IMAGE_SUBSYSTEM_EFI_BOOT_SERVICE_DRIVER = 11,
	IMAGE_SUBSYSTEM_EFI_RUNTIME_DRIVER      = 12,
	IMAGE_SUBSYSTEM_EFI_ROM                 = 13,
	IMAGE_SUBSYSTEM_XBOX                    = 14
}