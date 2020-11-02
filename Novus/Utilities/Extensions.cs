using System;
using System.Diagnostics;
using System.IO;
using System.Text;

// ReSharper disable UnusedMember.Global

namespace Novus.Utilities
{
	public static class Extensions
	{
		public static string ReadCString(this BinaryReader br, int count)
		{
			var s = Encoding.ASCII.GetString(br.ReadBytes(count)).TrimEnd('\0');


			return s;
		}

		/// <summary>
		///     Forcefully kills a <see cref="Process" /> and ensures the process has exited.
		/// </summary>
		/// <param name="p"><see cref="Process" /> to forcefully kill.</param>
		/// <returns><c>true</c> if <paramref name="p" /> was killed; <c>false</c> otherwise</returns>
		public static bool ForceKill(this Process p)
		{
			p.WaitForExit();
			p.Dispose();

			try {
				if (!p.HasExited) {
					p.Kill();
				}

				return true;
			}
			catch (Exception) {

				return false;
			}
		}
	}
}