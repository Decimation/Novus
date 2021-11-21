using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Test;

public class Win32ResourceReader : IDisposable
{
	private IntPtr _hModule;

	public Win32ResourceReader(string filename)
	{
		_hModule = LoadLibraryEx(filename, IntPtr.Zero, LoadLibraryFlags.AsDataFile | LoadLibraryFlags.AsImageResource);
		if (_hModule == IntPtr.Zero)
			throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
	}

	public string GetString(uint id)
	{
		var buffer = new StringBuilder(1024);
		LoadString(_hModule, id, buffer, buffer.Capacity);
		if (Marshal.GetLastWin32Error() != 0)
			throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
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
		if (_hModule != IntPtr.Zero)
			FreeLibrary(_hModule);
		_hModule = IntPtr.Zero;
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	static extern int LoadString(IntPtr hInstance, uint uID, StringBuilder lpBuffer, int nBufferMax);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hReservedNull, LoadLibraryFlags dwFlags);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	static extern bool FreeLibrary(IntPtr hModule);

	[Flags]
	enum LoadLibraryFlags : uint
	{
		AsDataFile      = 0x00000002,
		AsImageResource = 0x00000020
	}
}