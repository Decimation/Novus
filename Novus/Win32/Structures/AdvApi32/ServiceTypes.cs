// Author: Deci | Project: Novus | Name: ServiceTypes.cs
// Date: 2025/02/05 @ 14:02:17

namespace Novus.Win32.Structures.AdvApi32;

/// <summary>
///     Used by the
///     <see
///         cref="ChangeServiceConfig(SC_HANDLE, ServiceTypes, ServiceStartType, ServiceErrorControlType, string, string,
/// out uint, string[], string, string, string)" />
///     function.
/// </summary>
[Flags]
public enum ServiceTypes : uint
{

	/// <summary>Makes no change for this setting.</summary>
	SERVICE_NO_CHANGE = 0xFFFFFFFF,

	/// <summary>Driver service.</summary>
	SERVICE_KERNEL_DRIVER = 0x00000001,

	/// <summary>File system driver service.</summary>
	SERVICE_FILE_SYSTEM_DRIVER = 0x00000002,

	/// <summary>Reserved</summary>
	SERVICE_ADAPTER = 0x00000004,

	/// <summary>Reserved</summary>
	SERVICE_RECOGNIZER_DRIVER = 0x00000008,

	/// <summary>Combination of SERVICE_KERNEL_DRIVER | SERVICE_FILE_SYSTEM_DRIVER | SERVICE_RECOGNIZER_DRIVER</summary>
	SERVICE_DRIVER = SERVICE_KERNEL_DRIVER | SERVICE_FILE_SYSTEM_DRIVER | SERVICE_RECOGNIZER_DRIVER,

	/// <summary>Service that runs in its own process.</summary>
	SERVICE_WIN32_OWN_PROCESS = 0x00000010,

	/// <summary>Service that shares a process with other services.</summary>
	SERVICE_WIN32_SHARE_PROCESS = 0x00000020,

	/// <summary>Combination of SERVICE_WIN32_OWN_PROCESS | SERVICE_WIN32_SHARE_PROCESS</summary>
	SERVICE_WIN32 = SERVICE_WIN32_OWN_PROCESS | SERVICE_WIN32_SHARE_PROCESS,

	/// <summary>The service user service</summary>
	SERVICE_USER_SERVICE = 0x00000040,

	/// <summary>The service userservice instance</summary>
	SERVICE_USERSERVICE_INSTANCE = 0x00000080,

	/// <summary>Combination of SERVICE_USER_SERVICE | SERVICE_WIN32_SHARE_PROCESS</summary>
	SERVICE_USER_SHARE_PROCESS = SERVICE_USER_SERVICE | SERVICE_WIN32_SHARE_PROCESS,

	/// <summary>Combination of SERVICE_USER_SERVICE | SERVICE_WIN32_OWN_PROCESS</summary>
	SERVICE_USER_OWN_PROCESS = SERVICE_USER_SERVICE | SERVICE_WIN32_OWN_PROCESS,

	/// <summary>The service can interact with the desktop.</summary>
	SERVICE_INTERACTIVE_PROCESS = 0x00000100,

	/// <summary>The service PKG service</summary>
	SERVICE_PKG_SERVICE = 0x00000200,

	/// <summary>Combination of all service types</summary>
	SERVICE_TYPE_ALL = SERVICE_WIN32 | SERVICE_ADAPTER | SERVICE_DRIVER | SERVICE_INTERACTIVE_PROCESS
					   | SERVICE_USER_SERVICE | SERVICE_USERSERVICE_INSTANCE | SERVICE_PKG_SERVICE

}