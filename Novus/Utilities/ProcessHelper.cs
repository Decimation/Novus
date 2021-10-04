using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
// ReSharper disable UnusedMember.Global

namespace Novus.Utilities
{
	public static class ProcessHelper
	{
		/// <summary>
		///     Finds a <see cref="ProcessModule" /> in the current process with the <see cref="ProcessModule.ModuleName" /> of
		///     <paramref name="moduleName" />
		/// </summary>
		/// <param name="moduleName">
		///     <see cref="ProcessModule.ModuleName" />
		/// </param>
		/// <returns>The found <see cref="ProcessModule" />; <c>null</c> otherwise</returns>
		[CanBeNull]
		public static ProcessModule FindModule(string moduleName)
		{
			return FindModule(Process.GetCurrentProcess(), moduleName);

		}

		/// <summary>
		///     Finds a <see cref="ProcessModule" /> in the current process with the <see cref="ProcessModule.ModuleName" /> of
		///     <paramref name="moduleName" />
		/// </summary>
		/// <param name="p">Process from which to load modules</param>
		/// <param name="moduleName">
		///     <see cref="ProcessModule.ModuleName" />
		/// </param>
		/// <returns>The found <see cref="ProcessModule" />; <c>null</c> otherwise</returns>
		[CanBeNull]
		public static ProcessModule FindModule(Process p,string moduleName)
		{

			return p.GetModules().FirstOrDefault(module => module.ModuleName == moduleName);

		}
		public static ProcessModule[] GetModules(this Process p)
		{
			var modules = p.Modules;

			return modules.Cast<ProcessModule>().Where(module => module != null).ToArray();

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
