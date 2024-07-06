using System.Runtime.InteropServices;
using Novus.Imports;
using Novus.Imports.Attributes;
using Novus.Numerics;
using Novus.Win32;

namespace Novus.Runtime.VM.EE;

/// <summary>
/// Substructure of <see cref="EEClass"/>
/// </summary>
[NativeStructure]
[StructLayout(LayoutKind.Sequential)]
public struct ArrayClass
{

	internal byte Rank { get; set; }

	/// <summary>
	/// Cache of <see cref="MethodTable.ElementTypeHandle"/>
	/// </summary>
	internal CorElementType ElementType { get; set; }

}