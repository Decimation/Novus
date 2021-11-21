using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Novus.Win32.Structures;

// ReSharper disable UnusedMember.Global

// ReSharper disable EmptyDestructor

namespace Novus.Win32;

public sealed class KeyboardListener : IDisposable
{
	public KeyboardListener() : this(IntPtr.Zero) { }

	public KeyboardListener(string s, ISet<VirtualKey> k = null) : this(Native.FindWindow(s), k) { }

	public KeyboardListener(IntPtr h, ISet<VirtualKey> k = null)
	{
		m_thread = new Thread(Listen)
		{
			Priority     = ThreadPriority.AboveNormal,
			IsBackground = false,

		};

		m_keyHistory = new ConcurrentDictionary<VirtualKey, KeyEventArgs>();
		ScopeHandle  = h;
		KeyWhitelist = k ?? new HashSet<VirtualKey>();
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

		short keyState = Native.GetAsyncKeyState(keyShort);
			
		bool prev = IsVkPrevious(keyState);
		bool down = IsVkDown(keyState);


		bool stroke = m_keyHistory.ContainsKey(keyShort)
		              && m_keyHistory[keyShort].IsDown && !down;

		var args = new KeyEventArgs
		{
			Key        = keyShort,
			IsDown     = down,
			IsPrevious = prev,
			IsStroke   = stroke,
			Raw        = keyState
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

			byte[] rg = new byte[VK_COUNT];

			Native.GetKeyboardState(rg);

			for (int i = 0; i < rg.Length; i++) {
				HandleKey((VirtualKey) i);
			}

		}
	}

	public void Dispose()
	{
		Stop();
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

	private const int K_PREV   = 1;
	private const int VK_COUNT = 256;

	#endregion
}

public sealed class KeyEventArgs : EventArgs
{
	public VirtualKey Key { get; init; }

	public bool IsDown { get; init; }

	public bool IsPrevious { get; init; }

	public bool  IsStroke { get; init; }
	public short Raw      { get; set; }

	public override string ToString()
	{
		return $"{nameof(Key)}: {Key}, "                +
		       $"{nameof(IsDown)}: {IsDown},"           +
		       $" {nameof(IsPrevious)}: {IsPrevious}, " +
		       $"{nameof(IsStroke)}: {IsStroke}, "      + $"{nameof(Raw)}: {Raw}";
	}
}