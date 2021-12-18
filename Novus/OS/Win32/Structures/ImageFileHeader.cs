using System.Runtime.InteropServices;

// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable StructCanBeMadeReadOnly
namespace Novus.OS.Win32.Structures;

[StructLayout(LayoutKind.Sequential)]
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