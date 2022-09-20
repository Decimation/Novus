// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
namespace Novus.Win32.Structures.Kernel32;

public enum DllCharacteristics : ushort
{
	RES_0                                          = 0x0001,
	RES_1                                          = 0x0002,
	RES_2                                          = 0x0004,
	RES_3                                          = 0x0008,
	IMAGE_DLL_CHARACTERISTICS_DYNAMIC_BASE         = 0x0040,
	IMAGE_DLL_CHARACTERISTICS_FORCE_INTEGRITY      = 0x0080,
	IMAGE_DLL_CHARACTERISTICS_NX_COMPAT            = 0x0100,
	IMAGE_DLLCHARACTERISTICS_NO_ISOLATION          = 0x0200,
	IMAGE_DLLCHARACTERISTICS_NO_SEH                = 0x0400,
	IMAGE_DLLCHARACTERISTICS_NO_BIND               = 0x0800,
	RES_4                                          = 0x1000,
	IMAGE_DLLCHARACTERISTICS_WDM_DRIVER            = 0x2000,
	IMAGE_DLLCHARACTERISTICS_TERMINAL_SERVER_AWARE = 0x8000
}