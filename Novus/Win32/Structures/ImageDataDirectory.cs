using System.Runtime.InteropServices;

// ReSharper disable StructCanBeMadeReadOnly
namespace Novus.Win32.Structures
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct ImageDataDirectory
	{
		public uint VirtualAddress;
		public uint Size;
	}
}