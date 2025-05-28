// Author: Deci | Project: Novus | Name: ScManagerAccessTypes.cs
// Date: 2025/02/05 @ 14:02:55

using Novus.Win32.Structures.Other;
#pragma warning disable CS1574, CS1584, CS1581, CS1580

namespace Novus.Win32.Structures.AdvApi32;

[Flags]
public enum ScManagerAccessTypes : uint
{
	/// <summary>Required to connect to the service control manager.</summary>
	SC_MANAGER_CONNECT = 0x0001,

	/// <summary>Required to call the CreateService function to create a service object and add it to the database.</summary>
	SC_MANAGER_CREATE_SERVICE = 0x0002,

	/// <summary>
	/// Required to call the <see cref="EnumServicesStatus(SC_HANDLE, ServiceTypes, SERVICE_STATE)"/> or <see cref="EnumServicesStatusEx(SC_HANDLE, ServiceTypes, SERVICE_STATE, string)"/> function to list the services
	/// that are in the database. Required to call the <see cref="NotifyServiceStatusChange"/> function to receive notification when
	/// any service is created or deleted.
	/// </summary>
	SC_MANAGER_ENUMERATE_SERVICE = 0x0004,

	/// <summary>Required to call the <see cref="LockServiceDatabase"/> function to acquire a lock on the database.</summary>
	SC_MANAGER_LOCK = 0x0008,

	/// <summary>
	/// Required to call the <see cref="QueryServiceLockStatus"/> function to retrieve the lock status information for the database.
	/// </summary>
	SC_MANAGER_QUERY_LOCK_STATUS = 0x0010,

	/// <summary>Required to call the <see cref="NotifyBootConfigStatus"/> function.</summary>
	SC_MANAGER_MODIFY_BOOT_CONFIG = 0x0020,

	/// <summary>Includes <see cref="AccessMask.STANDARD_RIGHTS_REQUIRED"/>, in addition to all access rights in this table.</summary>
	SC_MANAGER_ALL_ACCESS = AccessMask.STANDARD_RIGHTS_REQUIRED |
	                        SC_MANAGER_CONNECT                   |
	                        SC_MANAGER_CREATE_SERVICE            |
	                        SC_MANAGER_ENUMERATE_SERVICE         |
	                        SC_MANAGER_LOCK                      |
	                        SC_MANAGER_QUERY_LOCK_STATUS         |
	                        SC_MANAGER_MODIFY_BOOT_CONFIG
}