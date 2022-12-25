using System.Runtime.InteropServices;
using Novus.Imports.Attributes;
using Novus.Win32.Structures.Kernel32;

// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable StructCanBeMadeReadOnly
namespace Novus.Win32.Structures.DbgHelp;

[StructLayout(LayoutKind.Sequential)]
[NativeStructure]
internal struct ImageFileHeader
{
	/// WORD->unsigned short
	public MachineType Machine { get; }

	/// WORD->unsigned short
	public ushort NumberOfSections { get; }

	/// DWORD->unsigned int
	public uint TimeDateStamp { get; }

	/// DWORD->unsigned int
	public uint PointerToSymbolTable { get; }

	/// DWORD->unsigned int
	public uint NumberOfSymbols { get; }

	/// WORD->unsigned short
	public ushort SizeOfOptionalHeader { get; }

	/// WORD->unsigned short
	public ushort Characteristics { get; }
}