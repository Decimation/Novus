using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Novus.Win32.Structures.Ntdll;

// ReSharper disable UnusedMember.Global

namespace Novus.Utilities;

public static class ProcessHelper
{
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
	public static ProcessModule FindModule(this Process p, string moduleName)
		=> p.GetModules().FirstOrDefault(module => module.ModuleName == moduleName);

	public static ProcessModule[] GetModules(this Process p)
		=> p.Modules.Cast<ProcessModule>().Where(module => module != null).ToArray();

#if DANGEROUS
	/// <summary>
	///     Forcefully kills a <see cref="Process" /> and ensures the process has exited.
	/// </summary>
	/// <param name="p"><see cref="Process" /> to forcefully kill.</param>
	/// <param name="ms"></param>
	/// <returns><c>true</c> if <paramref name="p" /> was killed; <c>false</c> otherwise</returns>
	public static bool Abort(this Process p, int ms = 0)
	{
		Task.Run(p.WaitForExit).Wait(ms);
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
#endif

	[CanBeNull]
	public static Process GetParent()
	{
		return GetParent(Process.GetCurrentProcess().Handle);
	}

	[CanBeNull]
	public static Process GetParent(int id)
	{
		var process = Process.GetProcessById(id);
		return GetParent(process.Handle);
	}

	[CanBeNull]
	public static Process GetParent(string s)
	{
		var process = Process.GetProcessesByName(s).First();
		return GetParent(process.Handle);
	}

	[CanBeNull]
	public static Process GetParent(this Process p)
		=> GetParent(p.Handle);

	[CanBeNull]
	public static Process GetParent(IntPtr handle)
	{
		int returnLength;

		unsafe {
			var pbi = new ProcessBasicInformation();

			var status = Native.NtQueryInformationProcess(handle, 0, &pbi,
			                                              Marshal.SizeOf(pbi), out returnLength);

			if (status != NtStatus.SUCCESS)
				throw new Win32Exception((int) status);

			try {
				return Process.GetProcessById(pbi.InheritedFromUniqueProcessId.ToInt32());
			}
			catch (ArgumentException) {

				return null;
			}

		}
	}
}