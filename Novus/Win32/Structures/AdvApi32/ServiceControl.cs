// Author: Deci | Project: Novus | Name: ServiceControl.cs
// Date: 2025/02/05 @ 14:02:13

namespace Novus.Win32.Structures.AdvApi32;

/// <summary>
///     Service control codes to be used with <see cref="Native.ControlService" /> and <see cref="ControlServiceEx" />
/// </summary>
public enum ServiceControl : uint
{

	/// <summary>
	///     Notifies a service that it should pause. The hService handle must have the SERVICE_PAUSE_CONTINUE access
	///     right.
	/// </summary>
	SERVICE_CONTROL_PAUSE = 0x00000002,

	/// <summary>
	///     Notifies a paused service that it should resume. The hService handle must have the SERVICE_PAUSE_CONTINUE access
	///     right.
	/// </summary>
	SERVICE_CONTROL_CONTINUE = 0x00000003,

	/// <summary>
	///     Notifies a service that it should report its current status information to the service control manager. The
	///     hService handle
	///     must have the SERVICE_INTERROGATE access right. Note that this control is not generally useful as the SCM is aware
	///     of the
	///     current state of the service
	/// </summary>
	SERVICE_CONTROL_INTERROGATE = 0x00000004,

	/// <summary>
	///     Notifies a service that its startup parameters have changed. The hService handle must have the
	///     SERVICE_PAUSE_CONTINUE access right.
	/// </summary>
	SERVICE_CONTROL_PARAMCHANGE = 0x00000006,

	/// <summary>
	///     Notifies a network service that there is a new component for binding. The hService handle must have the
	///     SERVICE_PAUSE_CONTINUE access right. However, this control code has been deprecated; use Plug and Play
	///     functionality instead.
	/// </summary>
	SERVICE_CONTROL_NETBINDADD = 0x00000007,

	/// <summary>
	///     Notifies a network service that a component for binding has been removed. The hService handle must have the
	///     SERVICE_PAUSE_CONTINUE access right. However, this control code has been deprecated; use Plug and Play
	///     functionality instead.
	/// </summary>
	SERVICE_CONTROL_NETBINDREMOVE = 0x00000008,

	/// <summary>
	///     Notifies a network service that a disabled binding has been enabled. The hService handle must have the
	///     SERVICE_PAUSE_CONTINUE
	///     access right. However, this control code has been deprecated; use Plug and Play functionality instead.
	/// </summary>
	SERVICE_CONTROL_NETBINDENABLE = 0x00000009,

	/// <summary>
	///     Notifies a network service that one of its bindings has been disabled. The hService handle must have the
	///     SERVICE_PAUSE_CONTINUE access right. However, this control code has been deprecated; use Plug and Play
	///     functionality instead.
	/// </summary>
	SERVICE_CONTROL_NETBINDDISABLE = 0x0000000A,

	//#define SERVICE_CONTROL_USER_LOGOFF  = 0x00000011
	//reserved for internal use            = 0x00000021
	//reserved for internal use            = 0x00000050

	/// <summary></summary>
	SERVICE_CONTROL_LOWRESOURCES = 0x00000060,

	/// <summary></summary>
	SERVICE_CONTROL_SYSTEMLOWRESOURCES = 0x00000061,

	/// <summary>
	///     Notifies a service that the system will be shutting down. Services that need additional time to perform cleanup
	///     tasks beyond
	///     the tight time restriction at system shutdown can use this notification. The service control manager sends this
	///     notification
	///     to applications that have registered for it before sending a SERVICE_CONTROL_SHUTDOWN notification to applications
	///     that have
	///     registered for that notification.
	///     <para>
	///         A service that handles this notification blocks system shutdown until the service stops or the pre-shutdown
	///         time-out interval
	///         specified through SERVICE_PRESHUTDOWN_INFO expires. Because this affects the user experience, services should
	///         use this
	///         feature only if it is absolutely necessary to avoid data loss or significant recovery time at the next system
	///         start.
	///     </para>
	///     <para>Windows Server 2003 and Windows XP: This value is not supported.</para>
	/// </summary>
	SERVICE_CONTROL_PRESHUTDOWN = 0x0000000F,

	/// <summary>
	///     Notifies a service that the system is shutting down so the service can perform cleanup tasks. Note that services
	///     that
	///     register for SERVICE_CONTROL_PRESHUTDOWN notifications cannot receive this notification because they have already
	///     stopped.
	///     <para>
	///         If a service accepts this control code, it must stop after it performs its cleanup tasks and return NO_ERROR.
	///         After the SCM
	///         sends this control code, it will not send other control codes to the service.
	///     </para>
	///     <para>For more information, see the Remarks section of this topic.</para>
	/// </summary>
	SERVICE_CONTROL_SHUTDOWN = 0x00000005,

	/// <summary>
	///     Notifies a service that it should stop.
	///     <para>
	///         If a service accepts this control code, it must stop upon receipt and return NO_ERROR. After the SCM sends this
	///         control code,
	///         it will not send other control codes to the service. Windows XP: If the service returns NO_ERROR and continues
	///         to run, it
	///         continues to receive control codes. This behavior changed starting with Windows Server 2003 and Windows XP with
	///         SP2.
	///     </para>
	///     <para>
	///         This parameter can also be one of the following extended control codes. Note that these control codes are not
	///         supported by
	///         the Handler function.
	///     </para>
	/// </summary>
	SERVICE_CONTROL_STOP = 0x00000001,

	/// <summary>
	///     Notifies a service of device events. (The service must have registered to receive these notifications using the
	///     RegisterDeviceNotification function.) The dwEventType and lpEventData parameters contain additional information.
	/// </summary>
	SERVICE_CONTROL_DEVICEEVENT = 0x0000000B,

	/// <summary>
	///     Notifies a service that the computer's hardware profile has changed. The dwEventType parameter contains additional
	///     information.
	/// </summary>
	SERVICE_CONTROL_HARDWAREPROFILECHANGE = 0x0000000C,

	/// <summary>
	///     Notifies a service of system power events. The dwEventType parameter contains additional information. If
	///     dwEventType is
	///     PBT_POWERSETTINGCHANGE, the lpEventData parameter also contains additional information.
	/// </summary>
	SERVICE_CONTROL_POWEREVENT = 0x0000000D,

	/// <summary>
	///     Notifies a service of session change events. Note that a service will only be notified of a user logon if it is
	///     fully loaded
	///     before the logon attempt is made. The dwEventType and lpEventData parameters contain additional information.
	/// </summary>
	SERVICE_CONTROL_SESSIONCHANGE = 0x0000000E,

	/// <summary>
	///     Notifies a service that the system time has changed. The lpEventData parameter contains additional information. The
	///     dwEventType parameter is not used.
	///     <para>Windows Server 2008, Windows Vista, Windows Server 2003 and Windows XP: This control code is not supported.</para>
	/// </summary>
	SERVICE_CONTROL_TIMECHANGE = 0x00000010,

	/// <summary>
	///     Notifies a service registered for a service trigger event that the event has occurred.
	///     <para>Windows Server 2008, Windows Vista, Windows Server 2003 and Windows XP: This control code is not supported.</para>
	/// </summary>
	SERVICE_CONTROL_TRIGGEREVENT = 0x00000020,

	/// <summary>
	///     Notifies a service that the user has initiated a reboot.
	///     <para>
	///         Windows Server 2008 R2, Windows 7, Windows Server 2008, Windows Vista, Windows Server 2003 and Windows XP: This
	///         control code
	///         is not supported.
	///     </para>
	/// </summary>
	SERVICE_CONTROL_USERMODEREBOOT = 0x00000040,

}