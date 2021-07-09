using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Novus.Win32.Structures;

// ReSharper disable UnusedMember.Global

// ReSharper disable EmptyDestructor

namespace Novus.Win32
{
	public sealed class KeyboardListener : IDisposable
	{
		public KeyboardListener() : this(IntPtr.Zero) { }

		public KeyboardListener(string s) : this(Native.FindWindow(s)) { }

		public KeyboardListener(IntPtr h)
		{
			m_thread = new Thread(Listen)
			{
				Priority     = ThreadPriority.AboveNormal,
				IsBackground = false,

			};

			m_keyHistory    = new ConcurrentDictionary<VirtualKey, KeyEventArgs>();
			m_restriction = h;
		}

		public void Stop()
		{
			IsActive = false;
			m_thread.Join();
		}

		public void Start()
		{
			IsActive = true;
			m_thread.Start();
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

		public bool IsActive { get; private set; }

		/// <summary>
		/// When set, restricts listening to this handle
		/// </summary>
		private readonly IntPtr m_restriction;

		/// <summary>
		/// Keyboard monitor thread
		/// </summary>
		private readonly Thread m_thread;

		private readonly ConcurrentDictionary<VirtualKey, KeyEventArgs> m_keyHistory;

		#region Flags

		private const int K_DOWN = 0x80;

		private const int K_PREV = 1;

		#endregion

		public event EventHandler<KeyEventArgs> KeyPress;

		public event EventHandler<VirtualKey> KeyStroke;

		private void Listen()
		{

			while (IsActive) {

				if (m_restriction != IntPtr.Zero && Native.GetForegroundWindow() != m_restriction) {
					continue;
				}

				byte[] rg = new byte[256];
				Native.GetKeyboardState(rg);

				for (int i = 0; i < rg.Length; i++) {
					var keyShort = (VirtualKey) (i);

					short keyState = Native.GetAsyncKeyState(keyShort);
					//keyState != 0 && keyShort != 0
					//byte[] krg = BitConverter.GetBytes(keyState);

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
					};
					
					KeyPress?.Invoke(null, args);

					if (args.IsStroke) {
						KeyStroke?.Invoke(null, keyShort);
					}

					m_keyHistory[args.Key] = args;
				}

			}
		}


		public void Dispose()
		{
			Stop();
		}
	}

	public sealed class KeyEventArgs : EventArgs
	{
		public VirtualKey Key { get; init; }

		public bool IsDown { get; init; }

		public bool IsPrevious { get; init; }

		public bool IsStroke { get; init; }

		public override string ToString()
		{
			return $"{nameof(Key)}: {Key}, "                +
			       $"{nameof(IsDown)}: {IsDown},"           +
			       $" {nameof(IsPrevious)}: {IsPrevious}, " +
			       $"{nameof(IsStroke)}: {IsStroke}";
		}
	}
}