using System.Runtime.InteropServices;

// ReSharper disable StructCanBeMadeReadOnly
namespace Novus.OS.Win32.Structures;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct ImageNtHeaders
{
	public uint Signature { get; }

	public ImageFileHeader FileHeader { get; }

	public ImageOptionalHeader OptionalHeader { get; }
}