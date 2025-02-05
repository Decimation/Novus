// Author: Deci | Project: Novus | Name: ServiceAccessTypes.cs
// Date: 2025/02/05 @ 14:02:27

using Novus.Win32.Structures.Other;

namespace Novus.Win32.Structures.AdvApi32;

[Flags]
public enum ServiceAccessTypes : uint
{

	/// <summary>
	/// Required to call the <see cref="QueryServiceConfig"/> and <see cref="QueryServiceConfig2"/> functions to query the service configuration.
	/// </summary>
	SERVICE_QUERY_CONFIG = 0x0001,

	/// <summary>
	/// Required to call the <see cref="ChangeServiceConfig(SC_HANDLE, ServiceTypes, ServiceStartType, ServiceErrorControlType,
	/// string, string, nint, string[], string, string, string)"/> or <see cref="ChangeServiceConfig2"/> function to change the
	/// service configuration. Because this grants the caller the right to change the executable file that the system runs, it should
	/// be granted only to administrators.
	/// </summary>
	SERVICE_CHANGE_CONFIG = 0x0002,

	/// <summary>
	/// Required to call the <see cref="QueryServiceStatus"/> or <see cref="QueryServiceStatusEx"/> function to ask the service
	/// control manager about the status of the service. Required to call the <see cref="NotifyServiceStatusChange"/> function to
	/// receive notification when a service changes status.
	/// </summary>
	SERVICE_QUERY_STATUS = 0x0004,

	/// <summary>
	/// Required to call the <see cref="EnumDependentServices(SC_HANDLE, SERVICE_STATE)"/> function to enumerate all the services dependent on the service.
	/// </summary>
	SERVICE_ENUMERATE_DEPENDENTS = 0x0008,

	/// <summary>Required to call the <see cref="StartService"/> function to start the service.</summary>
	SERVICE_START = 0x0010,

	/// <summary>Required to call the <see cref="ControlService"/> function to stop the service.</summary>
	SERVICE_STOP = 0x0020,

	/// <summary>Required to call the <see cref="ControlService"/> function to pause or continue the service.</summary>
	SERVICE_PAUSE_CONTINUE = 0x0040,

	/// <summary>Required to call the <see cref="ControlService"/> function to ask the service to report its status immediately.</summary>
	SERVICE_INTERROGATE = 0x0080,

	/// <summary>Required to call the <see cref="ControlService"/> function to specify a user-defined control code.</summary>
	SERVICE_USER_DEFINED_CONTROL = 0x0100,

	/// <summary>Includes <see cref="AccessMask.STANDARD_RIGHTS_REQUIRED"/> in addition to all access rights in this table.</summary>
	SERVICE_ALL_ACCESS = AccessMask.STANDARD_RIGHTS_REQUIRED |
						 SERVICE_QUERY_CONFIG |
						 SERVICE_CHANGE_CONFIG |
						 SERVICE_QUERY_STATUS |
						 SERVICE_ENUMERATE_DEPENDENTS |
						 SERVICE_START |
						 SERVICE_STOP |
						 SERVICE_PAUSE_CONTINUE |
						 SERVICE_INTERROGATE |
						 SERVICE_USER_DEFINED_CONTROL

}