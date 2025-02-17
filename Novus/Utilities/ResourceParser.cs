﻿using System;
using System.Runtime.Versioning;
using System.Text;
using Novus.OS;
using Novus.Win32;
using Novus.Win32.Structures;
using Novus.Win32.Structures.Kernel32;

namespace Novus.Utilities;

/*
 * Adapted from
 * https://stackoverflow.com/questions/2087682/finding-out-unicode-character-name-in-net
 */

[SupportedOSPlatform(FileSystem.OS_WIN)]
public sealed class ResourceParser : IDisposable
{
	private nint m_hModule;

	public ResourceParser(string filename)
	{
		m_hModule = Native.LoadLibraryEx(filename, LoadLibraryFlags.AsDataFile | LoadLibraryFlags.AsImageResource);
	}

	public string GetString(uint id)
	{
		var buffer = new StringBuilder(Native.SIZE_1024);
		Native.LoadString(m_hModule, id, buffer, buffer.Capacity);

		return buffer.ToString();
	}

	~ResourceParser()
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