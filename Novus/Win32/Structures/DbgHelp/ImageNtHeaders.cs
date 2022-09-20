using System.Runtime.InteropServices;

// ReSharper disable StructCanBeMadeReadOnly
namespace Novus.Win32.Structures.DbgHelp;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct ImageNtHeaders
{
	public uint Signature { get; }

	public ImageFileHeader FileHeader { get; }

	public ImageOptionalHeader OptionalHeader { get; }
}