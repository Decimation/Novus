using System.Runtime.InteropServices;
using Novus.Imports.Attributes;

// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable StructCanBeMadeReadOnly
namespace Novus.Win32.Structures.DbgHelp;

[StructLayout(LayoutKind.Sequential)]
[NativeStructure]
internal struct ImageDataDirectory
{
	public uint VirtualAddress;
	public uint Size;
}