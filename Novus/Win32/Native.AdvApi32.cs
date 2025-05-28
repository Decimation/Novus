// Author: Deci | Project: Novus | Name: Native.AdvApi32.cs
// Date: 2025/02/05 @ 14:02:43

using Novus.Win32.Structures.AdvApi32;
using System.Runtime.InteropServices;
#pragma warning disable CS1574, CS1584, CS1581, CS1580

namespace Novus.Win32;

public static partial class Native
{

	[DllImport(ADVAPI32_DLL, SetLastError = true, ExactSpelling = true)]
	[return: MA(UT.Bool)]
	public static extern bool ControlService(ScHandle hService, ServiceControl dwControl,
	                                         out ServiceStatus lpServiceStatus);

	[DllImport(ADVAPI32_DLL, SetLastError = true, CharSet = CharSet.Auto)]
	public static extern ScHandle OpenService(ScHandle hSCManager, string lpServiceName,
	                                          ServiceAccessTypes dwDesiredAccess);

	[DllImport(ADVAPI32_DLL, SetLastError = true, CharSet = CharSet.Auto)]
	public static extern ScHandle OpenSCManager(string lpMachineName, string lpDatabaseName,
	                                            ScManagerAccessTypes dwDesiredAccess);

	[DllImport(ADVAPI32_DLL, SetLastError = true, ExactSpelling = true)]
	[return: MA(UT.Bool)]
	public static extern bool CloseServiceHandle(ScHandle hSCObject);

	[DllImport(ADVAPI32_DLL, SetLastError = true, CharSet = CharSet.Auto)]
	[return: MA(UT.Bool)]
	public static extern bool StartService(ScHandle hService, [Opt] int dwNumServiceArgs,
	                                       [CBN] [Opt] string[] lpServiceArgVectors);

	/// <summary>Stops a service using <see cref="ControlService"/> with <see cref="ServiceControl.SERVICE_CONTROL_STOP"/></summary>
	/// <param name="hService">
	/// A handle to the service. This handle is returned by the <see cref="OpenService"/> or
	/// <see cref="CreateService(SC_HANDLE, string, string, uint, ServiceTypes, ServiceStartType, ServiceErrorControlType, string, string, IntPtr, string[], string, string)"/> function. The
	/// access rights required for this handle depend on the <see cref="ServiceControl"/> code requested.
	/// </param>
	/// <param name="lpServiceStatus">
	/// A pointer to a <see cref="SERVICE_STATUS"/> structure that receives the latest service status information. The information
	/// returned reflects the most recent status that the service reported to the service control manager.
	/// </param>
	/// <returns></returns>
	public static bool StopService(ScHandle hService, out ServiceStatus lpServiceStatus) =>
		ControlService(hService, ServiceControl.SERVICE_CONTROL_STOP, out lpServiceStatus);

}