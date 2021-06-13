using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Novus.Win32.Structures;

namespace Novus.Win32
{
	public sealed class KeyboardListener : IDisposable
	{
		public event EventHandler<KeyEventArgs> KeyPress;

		public KeyboardListener()
		{
			m_thread = new Thread(Start)
			{
				Priority     = ThreadPriority.AboveNormal,
				IsBackground = false,

			};

			m_previous = new ConcurrentDictionary<VirtualKey, KeyEventArgs>();
		}

		public void Stop()
		{
			Active = false;
			m_thread.Join();
		}

		public void Run()
		{
			Active = true;
			m_thread.Start();
		}

		public IntPtr Restrict { get; set; }

		public bool Active { get; private set; }

		private readonly Thread m_thread;

		private readonly ConcurrentDictionary<VirtualKey, KeyEventArgs> m_previous;

		private void Start()
		{
			// todo: there must be a better way of doing this

			while (Active) {

				if (Restrict != IntPtr.Zero && Native.GetForegroundWindow() != Restrict) {

					continue;
				}

				byte[] rg = new byte[256];
				Native.GetKeyboardState(rg);

				for (int i = 0; i < rg.Length; i++) {
					var keyShort = (VirtualKey) (i);

					short keyState = Native.GetAsyncKeyState(keyShort);
					//keyState != 0 && keyShort != 0
					byte[] krg = BitConverter.GetBytes(keyState);

					bool prev = krg[0] == 1;
					bool down = krg[1] == 0x80;


					bool stroke = m_previous.ContainsKey(keyShort)
					              && m_previous[keyShort].IsDown && !down;

					var args = new KeyEventArgs
					{
						Key      = keyShort,
						IsDown   = down,
						Previous = prev,
						Stroke   = stroke,
					};

					//args.Stroke = prevArg != null && (prevArg.VirtualKey == args.VirtualKey
					//                                  && args.IsPrevious && !prevArg.IsPrevious
					//                                  && args.IsDown     && prevArg.IsDown);


					KeyPress?.Invoke(null, args);

					m_previous[args.Key] = args;
				}

			}
		}

		~KeyboardListener()
		{
			// ...
		}

		public void Dispose()
		{
			Stop();
			GC.SuppressFinalize(this);
		}
	}

	public sealed class KeyEventArgs : EventArgs
	{
		public VirtualKey Key { get; init; }

		public bool IsDown { get; init; }

		public bool Previous { get; init; }

		public bool Stroke { get; init; }
	}
}