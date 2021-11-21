using System.Runtime.InteropServices;
// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable StructCanBeMadeReadOnly
namespace Novus.Win32.Structures;

[StructLayout(LayoutKind.Sequential)]
internal struct ImageDataDirectory
{
	public uint VirtualAddress;
	public uint Size;
}