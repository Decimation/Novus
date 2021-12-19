using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Novus.OS.Win32;
using Novus.OS.Win32.Structures;

// ReSharper disable UnusedMember.Global

// ReSharper disable EmptyDestructor

namespace Novus.OS;

public sealed class KeyboardListener : IDisposable
{
	public KeyboardListener() : this(IntPtr.Zero) { }

	public KeyboardListener(string windowName, ISet<VirtualKey> whitelist = null) : this(
		Native.FindWindow(windowName), whitelist) { }

	public KeyboardListener(IntPtr handle, ISet<VirtualKey> whitelist = null)
	{
		m_thread = new Thread(Listen)
		{
			Priority     = ThreadPriority.AboveNormal,
			IsBackground = false,
		};

		m_keyHistory = new ConcurrentDictionary<VirtualKey, KeyEventArgs>();
		ScopeHandle  = handle;
		KeyWhitelist = whitelist ?? new HashSet<VirtualKey>();
	}

	public void Start()
	{
		IsActive = true;
		m_thread.Start();
	}

	public void Stop()
	{
		IsActive = false;
		m_thread.Join();
	}

	public static bool IsKeyDown(VirtualKey k)
	{
		short keyState = Native.GetKeyState(k);
		return IsVkDown(keyState);
	}

	private static bool IsVkPrevious(short k)
	{
		unsafe {
			byte* b = (byte*) &k;
			return b[0] == K_PREV;
		}

	}

	private static bool IsVkDown(short keyState)
	{
		unsafe {
			byte* b = (byte*) &keyState;
			return (b[1] & K_DOWN) != 0;
		}
	}
	

	private void HandleKey(VirtualKey keyShort)
	{
		if ((KeyWhitelist.Count > 0 && !KeyWhitelist.Contains(keyShort))) {
			return;
		}

		short shift = Native.GetAsyncKeyState(VirtualKey.SHIFT);
		short ctrl  = Native.GetAsyncKeyState(VirtualKey.CONTROL);
		short alt   = Native.GetAsyncKeyState(VirtualKey.MENU);

		bool shiftDown = IsVkDown(shift);
		bool ctrDown   = IsVkDown(ctrl);
		bool altDown   = IsVkDown(alt);

		ConsoleModifiers c = 0;

		if (shiftDown) {
			c |= ConsoleModifiers.Shift;
		}

		if (altDown) {
			c |= ConsoleModifiers.Alt;
		}

		if (ctrDown) {
			c |= ConsoleModifiers.Control;
		}

		short key     = Native.GetAsyncKeyState(keyShort);
		bool  keyPrev = IsVkPrevious(key);
		bool  keyDown = IsVkDown(key);

		bool stroke = m_keyHistory.ContainsKey(keyShort)
		              && m_keyHistory[keyShort].IsDown
		              && !keyDown;

		var args = new KeyEventArgs
		{
			Key        = keyShort,
			IsDown     = keyDown,
			IsPrevious = keyPrev,
			IsStroke   = stroke,
			Value      = key,
			Modifiers  = c
		};

		KeyEvent?.Invoke(null, args);

		if (args.IsStroke) {
			KeyStroke?.Invoke(null, keyShort);
		}

		if (args.IsDown) {
			KeyDown?.Invoke(null, keyShort);
		}


		m_keyHistory[args.Key] = args;
	}

	private void Listen()
	{
		while (IsActive) {

			if (ScopeHandle != IntPtr.Zero && Native.GetForegroundWindow() != ScopeHandle) {
				continue;
			}

			var rg = new byte[VK_COUNT];

			Native.GetKeyboardState(rg);

			for (int i = 0; i < rg.Length; i++) {
				HandleKey((VirtualKey) i);
			}

		}
	}

	public void Dispose()
	{
		Stop();
		m_keyHistory.Clear();
		KeyWhitelist.Clear();
		ScopeHandle = IntPtr.Zero;

	}

	public bool IsActive { get; private set; }

	/// <summary>
	/// When set, restricts listening to this handle
	/// </summary>
	public IntPtr ScopeHandle { get; set; }

	public ISet<VirtualKey> KeyWhitelist { get; set; }

	/// <summary>
	/// Keyboard monitor thread
	/// </summary>
	private readonly Thread m_thread;

	private readonly ConcurrentDictionary<VirtualKey, KeyEventArgs> m_keyHistory;

	#region Events

	public event EventHandler<KeyEventArgs> KeyEvent;

	public event EventHandler<VirtualKey> KeyDown;

	public event EventHandler<VirtualKey> KeyStroke;

	#endregion


	#region Flags

	private const int K_DOWN = 0x80;

	private const int K_PREV = 1;

	private const int VK_COUNT = 256;

	#endregion
}

public sealed class KeyEventArgs : EventArgs
{
	public VirtualKey Key { get; internal init; }

	public bool IsDown { get; internal init; }

	public bool IsPrevious { get; internal init; }

	public bool IsStroke { get; internal init; }

	public short Value { get; internal set; }

	public ConsoleModifiers Modifiers { get; internal init; }

	public override string ToString()
	{
		return $"{nameof(Key)}: {Key}, " +
		       $"{nameof(IsDown)}: {IsDown}," +
		       $" {nameof(IsPrevious)}: {IsPrevious}, " +
		       $"{nameof(IsStroke)}: {IsStroke}, " +
		       $"{nameof(Value)}: {Value}, " +
		       $"{nameof(Modifiers)}: {Modifiers}";
	}
}