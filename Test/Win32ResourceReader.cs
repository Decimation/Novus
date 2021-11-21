using System;
using System.Runtime.InteropServices;
using System.Text;
using Novus.Win32;
using Novus.Win32.Structures;

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

	public Win32ResourceReader(string filename) : this(
		Native.LoadLibraryEx(filename, LoadLibraryFlags.AsDataFile | LoadLibraryFlags.AsImageResource)) { }

	public Win32ResourceReader(IntPtr h)
	{
		m_hModule = h;

		if (m_hModule == IntPtr.Zero) {
			Native.FailWin32Error();
		}
	}

	public string GetString(uint id)
	{
		return Native.LoadString(m_hModule, id).ToString();
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