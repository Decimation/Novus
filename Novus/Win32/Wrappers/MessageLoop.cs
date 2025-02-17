﻿using System.Runtime.Versioning;
using Novus.OS;
using Novus.Win32.Structures.User32;

namespace Novus.Win32.Wrappers;

/// <summary>
/// <para>
/// This class encapsulates the management of a message loop for an application. It supports queuing a callback to the application via
/// the message loop to enable the app to return from a call and continue processing that call later. This behavior is needed when
/// implementing a shell verb as verbs must not block the caller.
/// </para>
/// <note type="note">The ComObject derived class should call QueueNonBlockingCallback in its invoke function, for example
/// IExecuteCommand::Execute() or IDropTarget::Drop() passing a method that will complete the initialization work.</note>
/// </summary>
[SupportedOSPlatform(FileSystem.OS_WIN)]
public class MessageLoop
{
	//https://github.com/dahall/Vanara

	private readonly uint curThreadId;
	private Action<object> appCallback;
	private uint callbackMsg;
	private object callbackObj;
	private nint timeoutTimerId; // timer id used to exit the app if the app is not called back within a certain time

	/// <summary>Initializes a new instance of the <see cref="MessageLoop"/> class.</summary>
	public MessageLoop() => curThreadId = Native.GetCurrentThreadId();

	/// <summary>Occurs when a new message is available.</summary>
	public event EventHandler<MessageEventArgs> ProcessMessage;

	/// <summary>Gets a value indicating whether this <see cref="MessageLoop"/> is running.</summary>
	/// <value><see langword="true"/> if running; otherwise, <see langword="false"/>.</value>
	public virtual bool Running { get; private set; } = false;

	/// <summary>
	/// Cancel the timeout timer. This should be called when the application knows that it wants to keep running, for example when it
	/// receives the incoming call to invoke the verb.
	/// </summary>
	public virtual void CancelTimeout()
	{
		if (timeoutTimerId == default) return;
		Native.KillTimer(default, timeoutTimerId);
		timeoutTimerId = default;
	}

	/// <summary>
	/// Queues a one-time callback function via the message loop. This method is not intended to be used simultaneously by multiple callers.
	/// </summary>
	/// <param name="callback">The callback delegate method.</param>
	/// <param name="tag">An optional argument that will be passed to the callback.</param>
	/// <exception cref="InvalidOperationException">Another callback is currently queued.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="callback"/> cannot be <see langword="null"/>.</exception>
	public virtual void QueueCallback(Action<object> callback, object tag = null)
	{
		if (appCallback != null) throw new InvalidOperationException("Another callback is currently queued.");
		appCallback = callback ?? throw new ArgumentNullException(nameof(callback));
		callbackObj = tag;

		if (callbackMsg == 0)
			callbackMsg = Native.RegisterWindowMessage(GetType().FullName + "+Callback");
		Native.PostThreadMessage(curThreadId, callbackMsg);
	}

	public virtual void Quit(int exitCode = 0)
	{
		CancelTimeout();

		if (Running)
			Native.PostQuitMessage(exitCode);
	}

	/// <summary>Runs the message loop.</summary>
	/// <param name="timeout">
	/// The time span after which the message loop will be terminated. If this value equals TimeSpan.Zero or is not specified, the
	/// message loop will run until the <see cref="Quit"/> method is called or the message loop receives a quit message.
	/// </param>
	public virtual void Run(TimeSpan timeout = default)
	{
		const uint WM_TIMER = 0x0113;

		if (timeout != TimeSpan.Zero)
			timeoutTimerId = Native.SetTimer(uElapse: (uint)timeout.TotalMilliseconds);

		Running = true;
		MSG msg = new MSG();
		while (Native.GetMessage(ref msg))
		{
			System.Diagnostics.Debug.WriteLine($"Message loop: message={msg.message}");

			try { ProcessMessage?.Invoke(this, new MessageEventArgs(msg)); }
			catch { }

			if (msg.message == WM_TIMER)
			{
				Native.KillTimer(default, msg.wParam);

				if (msg.wParam == timeoutTimerId)
				{
					timeoutTimerId = IntPtr.Zero;
					Native.PostQuitMessage(0);
				}
			}
			else if (msg.message == callbackMsg)
			{
				try { appCallback?.Invoke(callbackObj); }
				catch { }

				appCallback = null;
				callbackObj = null;
			}

			Native.TranslateMessage(ref msg);
			Native.DispatchMessage(ref msg);
		}

		Running = false;
	}

	/// <summary>Holds a copy of the MSG instance retrieved by GetMessage.</summary>
	/// <seealso cref="EventArgs"/>
	public class MessageEventArgs : EventArgs
	{
		internal MessageEventArgs(MSG msg) => MSG = msg;

		/// <summary>Gets or sets the MSG.</summary>
		/// <value>The MSG.</value>
		public MSG MSG { get; set; }
	}
}