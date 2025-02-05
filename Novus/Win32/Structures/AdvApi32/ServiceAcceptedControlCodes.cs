// Author: Deci | Project: Novus | Name: ServiceAcceptedControlCodes.cs
// Date: 2025/02/05 @ 14:02:23

namespace Novus.Win32.Structures.AdvApi32;

[Flags]
public enum ServiceAcceptedControlCodes : uint
{
	/// <summary>The service can be stopped. This control code allows the service to receive SERVICE_CONTROL_STOP notifications.</summary>
	SERVICE_ACCEPT_STOP = 0x00000001,

	/// <summary>
	/// The service can be paused and continued. This control code allows the service to receive SERVICE_CONTROL_PAUSE and
	/// SERVICE_CONTROL_CONTINUE notifications.
	/// </summary>
	SERVICE_ACCEPT_PAUSE_CONTINUE = 0x00000002,

	/// <summary>
	/// The service is notified when system shutdown occurs. This control code allows the service to receive SERVICE_CONTROL_SHUTDOWN
	/// notifications. Note that ControlService and ControlServiceEx cannot send this notification; only the system can send it.
	/// </summary>
	SERVICE_ACCEPT_SHUTDOWN = 0x00000004,

	/// <summary>
	/// The service can reread its startup parameters without being stopped and restarted. This control code allows the service to
	/// receive SERVICE_CONTROL_PARAMCHANGE notifications.
	/// </summary>
	SERVICE_ACCEPT_PARAMCHANGE = 0x00000008,

	/// <summary>
	/// The service is a network component that can accept changes in its binding without being stopped and restarted. This control
	/// code allows the service to receive SERVICE_CONTROL_NETBINDADD, SERVICE_CONTROL_NETBINDREMOVE, SERVICE_CONTROL_NETBINDENABLE,
	/// and SERVICE_CONTROL_NETBINDDISABLE notifications.
	/// </summary>
	SERVICE_ACCEPT_NETBINDCHANGE = 0x00000010,

	/// <summary>
	/// The service is notified when the computer's hardware profile has changed. This enables the system to send
	/// SERVICE_CONTROL_HARDWAREPROFILECHANGE notifications to the service.
	/// </summary>
	SERVICE_ACCEPT_HARDWAREPROFILECHANGE = 0x00000020,

	/// <summary>
	/// The service is notified when the computer's power status has changed. This enables the system to send
	/// SERVICE_CONTROL_POWEREVENT notifications to the service.
	/// </summary>
	SERVICE_ACCEPT_POWEREVENT = 0x00000040,

	/// <summary>
	/// The service is notified when the computer's session status has changed. This enables the system to send
	/// SERVICE_CONTROL_SESSIONCHANGE notifications to the service.
	/// </summary>
	SERVICE_ACCEPT_SESSIONCHANGE = 0x00000080,

	/// <summary>
	/// The service can perform preshutdown tasks. This control code enables the service to receive SERVICE_CONTROL_PRESHUTDOWN
	/// notifications. Note that ControlService and ControlServiceEx cannot send this notification; only the system can send it.
	/// Windows Server 2003 and Windows XP: This value is not supported.
	/// </summary>
	SERVICE_ACCEPT_PRESHUTDOWN = 0x00000100,

	/// <summary>
	/// The service is notified when the system time has changed. This enables the system to send SERVICE_CONTROL_TIMECHANGE
	/// notifications to the service. Windows Server 2008, Windows Vista, Windows Server 2003 and Windows XP: This control code is
	/// not supported.
	/// </summary>
	SERVICE_ACCEPT_TIMECHANGE = 0x00000200,

	/// <summary>
	/// The service is notified when an event for which the service has registered occurs. This enables the system to send
	/// SERVICE_CONTROL_TRIGGEREVENT notifications to the service. Windows Server 2008, Windows Vista, Windows Server 2003 and
	/// Windows XP: This control code is not supported.
	/// </summary>
	SERVICE_ACCEPT_TRIGGEREVENT = 0x00000400,

	/// <summary>
	/// The services is notified when the user initiates a reboot. Windows Server 2008 R2, Windows 7, Windows Server 2008, Windows
	/// Vista, Windows Server 2003 and Windows XP: This control code is not supported.
	/// </summary>
	SERVICE_ACCEPT_USER_LOGOFF = 0x00000800,

	/// <summary>Undocumented.</summary>
	SERVICE_ACCEPT_LOWRESOURCES = 0x00002000,

	/// <summary>Undocumented.</summary>
	SERVICE_ACCEPT_SYSTEMLOWRESOURCES = 0x00004000,
}