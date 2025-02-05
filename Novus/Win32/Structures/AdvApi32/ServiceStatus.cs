// Author: Deci | Project: Novus | Name: SERVICE_STATUS.cs
// Date: 2025/02/05 @ 14:02:25

using Win32Error = uint;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace Novus.Win32.Structures.AdvApi32;

[StructLayout(LayoutKind.Sequential)]
public struct ServiceStatus
{

	/// <summary>
	///     <para>The type of service. This member can be one of the following values.</para>
	///     <list type="table">
	///         <listheader>
	///             <term>Value</term>
	///             <term>Meaning</term>
	///         </listheader>
	///         <item>
	///             <term>SERVICE_FILE_SYSTEM_DRIVER 0x00000002</term>
	///             <term>The service is a file system driver.</term>
	///         </item>
	///         <item>
	///             <term>SERVICE_KERNEL_DRIVER 0x00000001</term>
	///             <term>The service is a device driver.</term>
	///         </item>
	///         <item>
	///             <term>SERVICE_WIN32_OWN_PROCESS 0x00000010</term>
	///             <term>The service runs in its own process.</term>
	///         </item>
	///         <item>
	///             <term>SERVICE_WIN32_SHARE_PROCESS 0x00000020</term>
	///             <term>The service shares a process with other services.</term>
	///         </item>
	///         <item>
	///             <term>SERVICE_USER_OWN_PROCESS 0x00000050</term>
	///             <term>The service runs in its own process under the logged-on user account.</term>
	///         </item>
	///         <item>
	///             <term>SERVICE_USER_SHARE_PROCESS 0x00000060</term>
	///             <term>
	///                 The service shares a process with one or more other services that run under the logged-on user
	///                 account.
	///             </term>
	///         </item>
	///     </list>
	///     <para>
	///         If the service type is either SERVICE_WIN32_OWN_PROCESS or SERVICE_WIN32_SHARE_PROCESS, and the service is
	///         running in the
	///         context of the LocalSystem account, the following type may also be specified.
	///     </para>
	///     <list type="table">
	///         <listheader>
	///             <term>Value</term>
	///             <term>Meaning</term>
	///         </listheader>
	///         <item>
	///             <term>SERVICE_INTERACTIVE_PROCESS 0x00000100</term>
	///             <term>The service can interact with the desktop. For more information, see Interactive Services.</term>
	///         </item>
	///     </list>
	/// </summary>
	public ServiceTypes dwServiceType;

	/// <summary>
	///     <para>The current state of the service. This member can be one of the following values.</para>
	///     <list type="table">
	///         <listheader>
	///             <term>Value</term>
	///             <term>Meaning</term>
	///         </listheader>
	///         <item>
	///             <term>SERVICE_CONTINUE_PENDING 0x00000005</term>
	///             <term>The service continue is pending.</term>
	///         </item>
	///         <item>
	///             <term>SERVICE_PAUSE_PENDING 0x00000006</term>
	///             <term>The service pause is pending.</term>
	///         </item>
	///         <item>
	///             <term>SERVICE_PAUSED 0x00000007</term>
	///             <term>The service is paused.</term>
	///         </item>
	///         <item>
	///             <term>SERVICE_RUNNING 0x00000004</term>
	///             <term>The service is running.</term>
	///         </item>
	///         <item>
	///             <term>SERVICE_START_PENDING 0x00000002</term>
	///             <term>The service is starting.</term>
	///         </item>
	///         <item>
	///             <term>SERVICE_STOP_PENDING 0x00000003</term>
	///             <term>The service is stopping.</term>
	///         </item>
	///         <item>
	///             <term>SERVICE_STOPPED 0x00000001</term>
	///             <term>The service is not running.</term>
	///         </item>
	///     </list>
	/// </summary>
	public ServiceState dwCurrentState;

	/// <summary>
	///     <para>
	///         The control codes the service accepts and processes in its handler function (see Handler and HandlerEx). A user
	///         interface
	///         process can control a service by specifying a control command in the ControlService or ControlServiceEx
	///         function. By default,
	///         all services accept the <c>SERVICE_CONTROL_INTERROGATE</c> value.
	///     </para>
	///     <para>
	///         To accept the <c>SERVICE_CONTROL_DEVICEEVENT</c> value, the service must register to receive device events by
	///         using the
	///         RegisterDeviceNotification function.
	///     </para>
	///     <para>The following are the control codes.</para>
	///     <list type="table">
	///         <listheader>
	///             <term>Control code</term>
	///             <term>Meaning</term>
	///         </listheader>
	///         <item>
	///             <term>SERVICE_ACCEPT_NETBINDCHANGE 0x00000010</term>
	///             <term>
	///                 The service is a network component that can accept changes in its binding without being stopped and
	///                 restarted. This control
	///                 code allows the service to receive SERVICE_CONTROL_NETBINDADD, SERVICE_CONTROL_NETBINDREMOVE,
	///                 SERVICE_CONTROL_NETBINDENABLE,
	///                 and SERVICE_CONTROL_NETBINDDISABLE notifications.
	///             </term>
	///         </item>
	///         <item>
	///             <term>SERVICE_ACCEPT_PARAMCHANGE 0x00000008</term>
	///             <term>
	///                 The service can reread its startup parameters without being stopped and restarted. This control code
	///                 allows the service to
	///                 receive SERVICE_CONTROL_PARAMCHANGE notifications.
	///             </term>
	///         </item>
	///         <item>
	///             <term>SERVICE_ACCEPT_PAUSE_CONTINUE 0x00000002</term>
	///             <term>
	///                 The service can be paused and continued. This control code allows the service to receive
	///                 SERVICE_CONTROL_PAUSE and
	///                 SERVICE_CONTROL_CONTINUE notifications.
	///             </term>
	///         </item>
	///         <item>
	///             <term>SERVICE_ACCEPT_PRESHUTDOWN 0x00000100</term>
	///             <term>
	///                 The service can perform preshutdown tasks. This control code enables the service to receive
	///                 SERVICE_CONTROL_PRESHUTDOWN
	///                 notifications. Note that ControlService and ControlServiceEx cannot send this notification; only the
	///                 system can send it.
	///                 Windows Server 2003 and Windows XP: This value is not supported.
	///             </term>
	///         </item>
	///         <item>
	///             <term>SERVICE_ACCEPT_SHUTDOWN 0x00000004</term>
	///             <term>
	///                 The service is notified when system shutdown occurs. This control code allows the service to receive
	///                 SERVICE_CONTROL_SHUTDOWN
	///                 notifications. Note that ControlService and ControlServiceEx cannot send this notification; only the
	///                 system can send it.
	///             </term>
	///         </item>
	///         <item>
	///             <term>SERVICE_ACCEPT_STOP 0x00000001</term>
	///             <term>
	///                 The service can be stopped. This control code allows the service to receive SERVICE_CONTROL_STOP
	///                 notifications.
	///             </term>
	///         </item>
	///     </list>
	///     <para>
	///         This member can also contain the following extended control codes, which are supported only by HandlerEx. (Note
	///         that these
	///         control codes cannot be sent by ControlService or ControlServiceEx.)
	///     </para>
	///     <list type="table">
	///         <listheader>
	///             <term>Control code</term>
	///             <term>Meaning</term>
	///         </listheader>
	///         <item>
	///             <term>SERVICE_ACCEPT_HARDWAREPROFILECHANGE 0x00000020</term>
	///             <term>
	///                 The service is notified when the computer's hardware profile has changed. This enables the system to
	///                 send
	///                 SERVICE_CONTROL_HARDWAREPROFILECHANGE notifications to the service.
	///             </term>
	///         </item>
	///         <item>
	///             <term>SERVICE_ACCEPT_POWEREVENT 0x00000040</term>
	///             <term>
	///                 The service is notified when the computer's power status has changed. This enables the system to send
	///                 SERVICE_CONTROL_POWEREVENT notifications to the service.
	///             </term>
	///         </item>
	///         <item>
	///             <term>SERVICE_ACCEPT_SESSIONCHANGE 0x00000080</term>
	///             <term>
	///                 The service is notified when the computer's session status has changed. This enables the system to send
	///                 SERVICE_CONTROL_SESSIONCHANGE notifications to the service.
	///             </term>
	///         </item>
	///         <item>
	///             <term>SERVICE_ACCEPT_TIMECHANGE 0x00000200</term>
	///             <term>
	///                 The service is notified when the system time has changed. This enables the system to send
	///                 SERVICE_CONTROL_TIMECHANGE
	///                 notifications to the service. Windows Server 2008, Windows Vista, Windows Server 2003 and Windows XP:
	///                 This control code is
	///                 not supported.
	///             </term>
	///         </item>
	///         <item>
	///             <term>SERVICE_ACCEPT_TRIGGEREVENT 0x00000400</term>
	///             <term>
	///                 The service is notified when an event for which the service has registered occurs. This enables the
	///                 system to send
	///                 SERVICE_CONTROL_TRIGGEREVENT notifications to the service. Windows Server 2008, Windows Vista, Windows
	///                 Server 2003 and
	///                 Windows XP: This control code is not supported.
	///             </term>
	///         </item>
	///         <item>
	///             <term>SERVICE_ACCEPT_USERMODEREBOOT 0x00000800</term>
	///             <term>
	///                 The services is notified when the user initiates a reboot. Windows Server 2008 R2, Windows 7, Windows
	///                 Server 2008, Windows
	///                 Vista, Windows Server 2003 and Windows XP: This control code is not supported.
	///             </term>
	///         </item>
	///     </list>
	/// </summary>
	public ServiceAcceptedControlCodes dwControlsAccepted;

	/// <summary>
	///     <para>
	///         The error code the service uses to report an error that occurs when it is starting or stopping. To return an
	///         error code
	///         specific to the service, the service must set this value to <c>ERROR_SERVICE_SPECIFIC_ERROR</c> to indicate
	///         that the
	///         <c>dwServiceSpecificExitCode</c> member contains the error code. The service should set this value to
	///         <c>NO_ERROR</c> when it
	///         is running and on normal termination.
	///     </para>
	/// </summary>
	public Win32Error dwWin32ExitCode;

	/// <summary>
	///     <para>
	///         A service-specific error code that the service returns when an error occurs while the service is starting or
	///         stopping. This
	///         value is ignored unless the <c>dwWin32ExitCode</c> member is set to <c>ERROR_SERVICE_SPECIFIC_ERROR</c>.
	///     </para>
	/// </summary>
	public Win32Error dwServiceSpecificExitCode;

	/// <summary>
	///     <para>
	///         The check-point value the service increments periodically to report its progress during a lengthy start, stop,
	///         pause, or
	///         continue operation. For example, the service should increment this value as it completes each step of its
	///         initialization when
	///         it is starting up. The user interface program that invoked the operation on the service uses this value to
	///         track the progress
	///         of the service during a lengthy operation. This value is not valid and should be zero when the service does not
	///         have a start,
	///         stop, pause, or continue operation pending.
	///     </para>
	/// </summary>
	public uint dwCheckPoint;

	/// <summary>
	///     <para>
	///         The estimated time required for a pending start, stop, pause, or continue operation, in milliseconds. Before
	///         the specified
	///         amount of time has elapsed, the service should make its next call to the SetServiceStatus function with either
	///         an incremented
	///         <c>dwCheckPoint</c> value or a change in <c>dwCurrentState</c>. If the amount of time specified by
	///         <c>dwWaitHint</c> passes,
	///         and <c>dwCheckPoint</c> has not been incremented or <c>dwCurrentState</c> has not changed, the service control
	///         manager or
	///         service control program can assume that an error has occurred and the service should be stopped. However, if
	///         the service
	///         shares a process with other services, the service control manager cannot terminate the service application
	///         because it would
	///         have to terminate the other services sharing the process as well.
	///     </para>
	/// </summary>
	public uint dwWaitHint;

}