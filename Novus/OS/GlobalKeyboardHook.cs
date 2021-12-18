using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Novus.OS.Win32;

namespace Novus.OS;

//NOTE: Adapted from https://gist.github.com/dudikeleti/a0ce3044b683634793cf297addbf5f11

/// <summary>
///     Provide a way to handle a global keyboard hooks
///     <remarks>
///         This hook is called in the context of the thread that installed it.
///         The call is made by sending a message to the thread that installed the hook.
///         Therefore, the thread that installed the hook must have a message loop.
///     </remarks>
/// </summary>
public sealed class GlobalKeyboardHook : IDisposable
{
	private const int WH_KEYBOARD_LL = 13;
	private const int WM_KEYDOWN     = 0x0100;
	private const int WM_KEYUP       = 0x0101;

	private LowLevelKeyboardProc _proc;

	private readonly IntPtr _hookId = IntPtr.Zero;

	private static GlobalKeyboardHook _instance;

	private Dictionary<int, KeyValuePair<KeyCombination, HookActions>> _hookEvents;
	private bool                                                       _disposed;
	private KeyCombination                                             _pressedKeys;

	/// <summary>
	///     Return a singleton instance of <see cref="GlobalKeyboardHook" />
	/// </summary>
	public static GlobalKeyboardHook Instance
	{
		get
		{
			Interlocked.CompareExchange(ref _instance, new GlobalKeyboardHook(), null);
			return _instance;
		}
	}

	private GlobalKeyboardHook()
	{
		_proc        = HookCallback;
		_hookEvents  = new Dictionary<int, KeyValuePair<KeyCombination, HookActions>>();
		_hookId      = SetHook(_proc);
		_pressedKeys = new KeyCombination();
	}

	/// <summary>
	///     Register a keyboard hook event
	/// </summary>
	/// <param name="keys">The short keys. minimum is two keys</param>
	/// <param name="execute">The action to run when the key ocmbination has pressed</param>
	/// <param name="message">Empty if no error occurred otherwise error message</param>
	/// <param name="runAsync">
	///     True if the action should execute in the background. -Be careful from thread affinity- Default
	///     is false
	/// </param>
	/// <param name="dispose">An action to run when unsubscribing from keyboard hook. can be null</param>
	/// <returns>Event id to use when unregister</returns>
	public int Hook(List<ushort> keys, Action execute, out string message, bool runAsync = false,
	                Action<object> dispose = null)
	{
		if (_hookEvents == null) {
			message = "Can't register";
			return -1;
		}

		if (keys == null || execute == null) {
			message = "'keys' and 'execute' can't be null";
			return -1;
		}

		if (keys.Count < 2) {
			message = "You must provide at least two keys";
			return -1;
		}

		if (!ValidateKeys(keys)) {
			message = "Illegal key. Only 'shift', 'ctrl' and 'a' - 'z' are allowed";
			return -1;
		}

		var kc = new KeyCombination(keys);
		int id = kc.GetHashCode();

		if (_hookEvents.ContainsKey(id)) {
			message = "The key combination is already exist it the application";
			return -1;
		}

		// if the action should run async, wrap it with Task
		Action asyncAction = null;

		if (runAsync)
			asyncAction = () => Task.Run(() => execute);

		_hookEvents[id] =
			new KeyValuePair<KeyCombination, HookActions>(kc, new HookActions(asyncAction ?? execute, dispose));
		message = String.Empty;
		return id;
	}

	private static bool ValidateKeys(IEnumerable<ushort> keys)
	{
		return keys.All(t => IsKeyValid((int) t));
	}

	private static bool IsKeyValid(int key)
	{
		// 'alt' is sys key and hence is disallowed.
		// a - z and shift, ctrl. 
		return key is >= 44 and <= 69 or >= 116 and <= 119;
	}

	/// <summary>
	///     Un register a keyboard hook event
	/// </summary>
	public void UnHook(int id, object obj = null)
	{
		if (_hookEvents == null || id < 0 || !_hookEvents.ContainsKey(id)) return;

		var hook = _hookEvents[id];

		if (hook.Value is { Dispose: { } }) {
			try {
				hook.Value.Dispose(obj);
			}
			catch (Exception) { }
		}

		_hookEvents.Remove(id);
	}


	private static IntPtr SetHook(LowLevelKeyboardProc proc)
	{
		using Process       curProcess = Process.GetCurrentProcess();
		using ProcessModule curModule  = curProcess.MainModule;

		return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);

	}

	private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

	private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
	{
		if (nCode < 0)
			return CallNextHookEx(_hookId, nCode, wParam, lParam);

		var result = new IntPtr(0);

		if (wParam == (IntPtr) WM_KEYDOWN) {
			// var k = k.KeyFromVirtualKey(Marshal.ReadInt32(lParam));

			var k = Marshal.ReadInt32(lParam);

			_pressedKeys.Add(
				(ushort) k); // vkCode (in KBDLLHOOKSTRUCT) is DWORD (actually it can be 0-254)

			if (_pressedKeys.Count >= 2) {
				var (_, value) = _hookEvents.Values.FirstOrDefault(val =>
					                                                   val.Key.Equals(_pressedKeys));

				if (value != null) {
					value.Exceute();
					// don't try to get the action again after the execute becasue it may removed already
					result = new IntPtr(1);
				}
			}
		}
		else if (wParam == (IntPtr) WM_KEYUP) {
			_pressedKeys.Clear();
		}

		// in case we processed the message, prevent the system from passing the message to the rest of the hook chain
		// return result.ToInt32() == 0 ? CallNextHookEx(_hookId, nCode, wParam, lParam) : result;
		return CallNextHookEx(_hookId, nCode, wParam, lParam);
	}

	[DllImport(Native.USER32_DLL, CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod,
	                                              uint dwThreadId);

	[DllImport(Native.USER32_DLL, CharSet = CharSet.Auto, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool UnhookWindowsHookEx(IntPtr hhk);

	[DllImport(Native.USER32_DLL, CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

	[DllImport(Native.KERNEL32_DLL, CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr GetModuleHandle(string lpModuleName);

	private void Dispose(bool dispose)
	{
		try {
			if (_disposed)
				return;

			UnhookWindowsHookEx(_hookId);

			if (dispose) {
				_proc        = null;
				_hookEvents  = null;
				_pressedKeys = null;
				GC.SuppressFinalize(this);
			}

			_disposed = true;
		}
		// ReSharper disable once EmptyGeneralCatchClause
		catch { }
	}

	public void Dispose()
	{
		Dispose(true);
	}

	~GlobalKeyboardHook()
	{
		Dispose(false);
	}

	private class HookActions
	{
		public HookActions(Action excetue, Action<object> dispose = null)
		{
			Exceute = excetue;
			Dispose = dispose;
		}

		public Action         Exceute { get; set; }
		public Action<object> Dispose { get; set; }
	}

	private class KeyCombination : IEquatable<KeyCombination>
	{
		private readonly bool _canModify;

		public KeyCombination(List<ushort> keys)
		{
			_keys = keys ?? new List<ushort>();
		}

		public KeyCombination()
		{
			_keys      = new List<ushort>();
			_canModify = true;
		}

		public void Add(ushort key)
		{
			if (_canModify) {
				_keys.Add(key);
			}
		}

		public void Remove(ushort key)
		{
			if (_canModify) {
				_keys.Remove(key);
			}
		}

		public void Clear()
		{
			if (_canModify) {
				_keys.Clear();
			}
		}

		public int Count
		{
			get { return _keys.Count; }
		}

		private readonly List<ushort> _keys;

		public bool Equals(KeyCombination other)
		{
			return other._keys != null && _keys != null && KeysEqual(other._keys);
		}

		private bool KeysEqual(List<ushort> keys)
		{
			if (keys == null || _keys == null || keys.Count != _keys.Count) return false;

			for (int i = 0; i < _keys.Count; i++) {
				if (_keys[i] != keys[i])
					return false;
			}

			return true;
		}

		public override bool Equals(object obj)
		{
			if (obj is KeyCombination)
				return Equals((KeyCombination) obj);
			return false;
		}

		public override int GetHashCode()
		{
			if (_keys == null) return 0;

			//http://stackoverflow.com/a/263416
			//http://stackoverflow.com/a/8094931
			//assume keys not going to modify after we use GetHashCode
			unchecked {
				int hash = 19;

				for (int i = 0; i < _keys.Count; i++) {
					hash = hash * 31 + _keys[i].GetHashCode();
				}

				return hash;
			}
		}

		public override string ToString()
		{
			if (_keys == null)
				return String.Empty;

			var sb = new StringBuilder((_keys.Count - 1) * 4 + 10);

			for (int i = 0; i < _keys.Count; i++) {
				if (i < _keys.Count - 1)
					sb.Append(_keys[i] + " , ");
				else
					sb.Append(_keys[i]);
			}

			return sb.ToString();
		}
	}
}