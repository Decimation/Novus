using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Novus.Memory;
using Novus.Runtime;
using Novus.Win32;
using Novus.Win32.Structures.Kernel32;
using Novus.Win32.Structures.Ntdll;
using Novus.Win32.Wrappers;

// ReSharper disable UnusedMember.Global
#pragma warning disable CA1416

namespace Novus.Utilities;

public static class ProcessHelper
{

	extension(Process p)
	{

		/// <summary>
		///     Finds a <see cref="ProcessModule" /> in the current process with the <see cref="ProcessModule.ModuleName" /> of
		///     <paramref name="moduleName" />
		/// </summary>
		/// <param name="moduleName">
		///     <see cref="ProcessModule.ModuleName" />
		/// </param>
		/// <returns>The found <see cref="ProcessModule" />; <c>null</c> otherwise</returns>
		[CBN]
		public ProcessModule FindModule(string moduleName) => p.GetModules().FirstOrDefault(module => module.ModuleName == moduleName);

		public IEnumerable<ProcessModule> GetModules() => p.Modules.Cast<ProcessModule>();

		[CBN]
		public Process GetParent() => Process.GetParent(p.Handle, out _);

	}

	extension(Process)
	{

		[CBN]
		[SupportedOSPlatform(RuntimeInformationExtensions.OS_WIN)]
		public static Process GetParent(nint handle, out ProcessBasicInformation pbi)
		{
			int returnLength;
			pbi = default;

			unsafe {
				var pbiBuf = new ProcessBasicInformation();

				var status = Native.NtQueryInformationProcess(handle, 0, &pbiBuf,
				                                              Marshal.SizeOf(pbiBuf), out returnLength);
				pbi = pbiBuf;

				if (status != NtStatus.SUCCESS) {
					return null;
				}

				try {
					return Process.GetProcessById(pbi.InheritedFromUniqueProcessId.ToInt32());
				}
				catch (ArgumentException) {

					return null;
				}

			}
		}

	}


	[SupportedOSPlatform(RuntimeInformationExtensions.OS_WIN)]
	public static (ModuleEntry32, ImageSectionInfo) FindInProcessMemory(Process proc, Pointer<byte> ptr)
	{
		var modules = Native.EnumProcessModules((uint) proc.Id);

		foreach (var m in modules) {
			nint size = (nint) m.modBaseSize;
			var  b    = ptr >= m.modBaseAddr && ptr <= (m.modBaseAddr + (size));

			if (!b) {
				continue;
			}

			var pe = Native.GetPESectionInfo(m.hModule);

			// var seg = pe.FirstOrDefault(e => Mem.IsAddressInRange(ptr, e.Address, e.Address + e.Size));

			foreach (var e in pe) {
				var b2 = ptr >= e.Address && ptr <= (e.Address + size);

				if (b2) {
					return (m, e);

				}
			}
		}

		return (default, default);
	}

}