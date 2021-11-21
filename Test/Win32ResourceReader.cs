using System;
using System.Runtime.InteropServices;
using System.Text;
using Novus.Win32;
// ReSharper disable UnusedMember.Global

// ReSharper disable PossibleNullReferenceException

namespace Test;
#pragma warning disable IDE0060

/*
 * https://stackoverflow.com/questions/2087682/finding-out-unicode-character-name-in-net
 */

public sealed class Win32ResourceReader : IDisposable
{
	private IntPtr m_hModule;

	public Win32ResourceReader(string filename)
	{
		m_hModule = Native.LoadLibraryEx(filename, Native.LoadLibraryFlags.AsDataFile | Native.LoadLibraryFlags.AsImageResource);

		if (m_hModule == IntPtr.Zero) {
			Native.FailWin32();
		}
	}

	public string GetString(uint id)
	{
		var buffer = new StringBuilder(1024);

		Native.LoadString(m_hModule, id, buffer, buffer.Capacity);

		if (Marshal.GetLastWin32Error() != 0) {
			Native.FailWin32();
		}

		return buffer.ToString();
	}

	~Win32ResourceReader()
	{
		Dispose(false);
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	public void Dispose(bool disposing)
	{
		if (m_hModule != IntPtr.Zero) {
			Native.FreeLibrary(m_hModule);
		}

		m_hModule = IntPtr.Zero;
	}
}