// Author: Deci | Project: Novus | Name: Native.AdvApi32.cs
// Date: 2025/02/05 @ 14:02:43

using Novus.Win32.Structures.AdvApi32;
using System.Runtime.InteropServices;

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

}