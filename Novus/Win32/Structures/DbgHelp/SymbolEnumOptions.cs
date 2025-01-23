// Author: Deci | Project: Novus | Name: SYMENUM.cs
// Date: 2025/01/22 @ 14:01:48

// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

namespace Novus.Win32.Structures.DbgHelp;

/// <summary>Indicates possible options.</summary>
/// <remarks>Name: <c>SYMENUM</c></remarks>
[Flags]
public enum SymbolEnumOptions
{

	/// <summary>Use the default options.</summary>
	SYMENUM_OPTIONS_DEFAULT = 0x00000001,

	/// <summary>Enumerate inline symbols.</summary>
	SYMENUM_OPTIONS_INLINE = 0x00000002

}