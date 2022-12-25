using System.Runtime.InteropServices;
using Novus.Imports.Attributes;

// ReSharper disable StructCanBeMadeReadOnly
namespace Novus.Win32.Structures.DbgHelp;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
[NativeStructure]
internal struct ImageNtHeaders
{
	public uint Signature { get; }

	public ImageFileHeader FileHeader { get; }

	public ImageOptionalHeader OptionalHeader { get; }
}