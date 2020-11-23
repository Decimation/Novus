using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Novus.Win32
{
	public static class Native
	{
		private const string KERNEL32_DLL = "Kernel32.dll";
		
		public const string CMD_EXE      = "cmd.exe";
		public const string EXPLORER_EXE = "explorer.exe";
		public const int    INVALID      = -1;

		[DllImport(KERNEL32_DLL)]
		internal static extern uint LocalSize(IntPtr p);

		[DllImport(KERNEL32_DLL)]
		internal static extern IntPtr OpenProcess(ProcessAccess dwDesiredAccess, bool bInheritHandle, int dwProcessId);

		[DllImport(KERNEL32_DLL, SetLastError = true, PreserveSig = true)]
		internal static extern bool CloseHandle(IntPtr obj);

		[DllImport(KERNEL32_DLL)]
		internal static extern bool ReadProcessMemory(IntPtr proc, IntPtr baseAddr, IntPtr buffer,
			int size, out int numBytesRead);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		internal static extern bool WriteProcessMemory(IntPtr proc, IntPtr baseAddr, IntPtr buffer,
			int size, out int numberBytesWritten);

		internal static IntPtr OpenProcess(Process proc) => OpenProcess(ProcessAccess.All, false, proc.Id);

		public const int STD_ERROR_HANDLE = -12;

		public const int STD_INPUT_HANDLE = -10;

		public const int STD_OUTPUT_HANDLE = -11;

		/// <param name="nStdHandle">
		///     <see cref="STD_INPUT_HANDLE" />,
		///     <see cref="STD_OUTPUT_HANDLE" />,
		///     <see cref="STD_ERROR_HANDLE" />
		/// </param>
		[DllImport(KERNEL32_DLL, SetLastError = true)]
		public static extern IntPtr GetStdHandle(int nStdHandle);

		[Flags]
		public enum ProcessAccess : uint
		{
			All                     = 0x1F0FFF,
			Terminate               = 0x000001,
			CreateThread            = 0x000002,
			VmOperation             = 0x00000008,
			VmRead                  = 0x000010,
			VmWrite                 = 0x000020,
			DupHandle               = 0x00000040,
			CreateProcess           = 0x000080,
			SetInformation          = 0x00000200,
			QueryInformation        = 0x000400,
			QueryLimitedInformation = 0x001000,
			Synchronize             = 0x00100000
		}
	}
}