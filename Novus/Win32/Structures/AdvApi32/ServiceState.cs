// Author: Deci | Project: Novus | Name: ServiceState.cs
// Date: 2025/02/05 @ 14:02:14

namespace Novus.Win32.Structures.AdvApi32;

public enum ServiceState : uint
{
	/// <summary>The service continue is pending.</summary>
	SERVICE_CONTINUE_PENDING = 0x00000005,

	/// <summary>The service pause is pending.</summary>
	SERVICE_PAUSE_PENDING = 0x00000006,

	/// <summary>The service is paused.</summary>
	SERVICE_PAUSED = 0x00000007,

	/// <summary>The service is running.</summary>
	SERVICE_RUNNING = 0x00000004,

	/// <summary>The service is starting.</summary>
	SERVICE_START_PENDING = 0x00000002,

	/// <summary>The service is stopping.</summary>
	SERVICE_STOP_PENDING = 0x00000003,

	/// <summary>The service is not running.</summary>
	SERVICE_STOPPED = 0x00000001,
}