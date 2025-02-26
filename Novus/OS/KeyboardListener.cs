using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using Novus.Win32;
using Novus.Win32.Structures.User32;

// ReSharper disable UnusedMember.Global

// ReSharper disable EmptyDestructor

namespace Novus.OS;

[SupportedOSPlatform(FileSystem.OS_WIN)]
public sealed class KeyboardListener : IDisposable
{
	public KeyboardListener() : this(IntPtr.Zero) { }

	public KeyboardListener(string windowName, ISet<VirtualKey> whitelist = null)
		: this(Native.FindWindow(windowName), whitelist) { }

	public KeyboardListener(nint handle, ISet<VirtualKey> whitelist = null)
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

	public bool IsGlobal => ScopeHandle == IntPtr.Zero;

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
		/*unsafe {
			byte* b = (byte*) &k;
			return b[0] == K_PREV;
		}*/

		return k < 0;
	}

	private static bool IsVkDown(short keyState)
	{
		unsafe {
			// byte* b = (byte*) &keyState;
			return (keyState & K_DOWN) != 0;
		}
	}

	public static (short key, bool isDown) GetVkDownState(VirtualKey k)
	{
		var s = Native.GetAsyncKeyState(k);
		return (s, IsVkDown(s));
	}

	private void HandleKey(VirtualKey keyShort)
	{
		if ((KeyWhitelist.Count > 0 && !KeyWhitelist.Contains(keyShort))) {
			return;
		}

		KeyModifiers k = 0;

		/*var (lshift, lshiftD) = GetDownState(VirtualKey.LSHIFT);
		var (rshift, rshiftD) = GetDownState(VirtualKey.RSHIFT);

		var (lctrl, lctrld) = GetDownState(VirtualKey.LCONTROL);
		var (rctrl, rctrlD) = GetDownState(VirtualKey.RCONTROL);

		var (lalt, laltD) = GetDownState(VirtualKey.LMENU);
		var (ralt, raltD) = GetDownState(VirtualKey.RMENU);

		var (lwin, lwinD) = GetDownState(VirtualKey.LWIN);
		var (rwin, rwinD) = GetDownState(VirtualKey.RWIN);*/

		var (shift, shiftD) = GetVkDownState(VirtualKey.SHIFT);
		var (ctrl, ctrld)   = GetVkDownState(VirtualKey.CONTROL);
		var (alt, altD)     = GetVkDownState(VirtualKey.MENU);
		var (lwin, lwinD)   = GetVkDownState(VirtualKey.LWIN);
		var (rwin, rwinD)   = GetVkDownState(VirtualKey.RWIN);

		if (shiftD) k |= KeyModifiers.Shift;
		if (ctrld) k  |= KeyModifiers.Ctrl;
		if (altD) k   |= KeyModifiers.Alt;
		if (lwinD) k  |= KeyModifiers.LWin;
		if (rwinD) k  |= KeyModifiers.RWin;

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
			Modifiers  = k
		};

		KeyEvent?.Invoke(null, args);

		if (args.IsStroke) {
			KeyStroke?.Invoke(null, args);
		}

		if (args.IsDown) {
			KeyDown?.Invoke(null, args);
		}

		m_keyHistory[args.Key] = args;
	}

	private void Listen()
	{
		var rg = new byte[VK_COUNT];

		while (IsActive) {
			if (!IsGlobal) {
				if (Native.GetForegroundWindow() != ScopeHandle) {
					continue;
				}

			}

			if (Native.GetKeyboardState(rg)) {
				for (int i = 0; i < rg.Length; i++) {
					HandleKey((VirtualKey) i);
				}

			}

			// Thread.Sleep(TimeSpan.FromMilliseconds(300));
		}

	}

	private void Listen2(in byte[] rg)
	{
		unsafe {

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
		// ScopeHandle = IntPtr.Zero;

	}

	public bool IsActive { get; private set; }

	/// <summary>
	/// When set, restricts listening to this handle
	/// </summary>
	public nint ScopeHandle { get; init; }

	public ISet<VirtualKey> KeyWhitelist { get; init; }

	/// <summary>
	/// Keyboard monitor thread
	/// </summary>
	private readonly Thread m_thread;

	private readonly ConcurrentDictionary<VirtualKey, KeyEventArgs> m_keyHistory;

	#region Events

	public event EventHandler<KeyEventArgs> KeyEvent;

	public event EventHandler<KeyEventArgs> KeyDown;

	public event EventHandler<KeyEventArgs> KeyStroke;

	#endregion

	#region Flags

	private const int K_DOWN = 0x8000;

	private const int K_PREV = 1;

	private const int VK_COUNT = 256;

	#endregion
}

[Flags]
public enum KeyModifiers
{
	None  = 0,
	Shift = 1 << 0,
	Ctrl  = 1 << 1,
	Alt   = 1 << 2,
	LWin  = 1 << 3,
	RWin  = 1 << 4,

	Win = LWin | RWin
}

public sealed class KeyEventArgs : EventArgs
{
	public VirtualKey Key { get; internal init; }

	public bool IsDown { get; internal init; }

	public bool IsPrevious { get; internal init; }

	public bool IsStroke { get; internal init; }

	public short Value { get; internal set; }

	public KeyModifiers Modifiers { get; internal init; }

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