using System;
using System.Text;
using Novus.OS.Win32;
using Novus.OS.Win32.Structures;

namespace Novus.OS;

/*
 * Adapted from
 * https://stackoverflow.com/questions/2087682/finding-out-unicode-character-name-in-net
 */


public sealed class Win32ResourceReader : IDisposable
{
	private IntPtr m_hModule;

	public Win32ResourceReader(string filename)
	{
		m_hModule = Native.LoadLibraryEx(filename, LoadLibraryFlags.AsDataFile | LoadLibraryFlags.AsImageResource);
	}

	public string GetString(uint id)
	{
		var buffer = new StringBuilder(Native.SIZE_1);
		Native.LoadString(m_hModule, id, buffer, buffer.Capacity);

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