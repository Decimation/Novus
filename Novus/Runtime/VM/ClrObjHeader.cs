// using System.Runtime.InteropServices;

using System.Runtime.InteropServices;
using Novus.Imports;
using Novus.Imports.Attributes;
using Novus.Win32;

// ReSharper disable InconsistentNaming

namespace Novus.Runtime.VM;

/// <summary>
/// Represents the header of an <see cref="object"/> (<see cref="ClrObject"/>) as defined in the CLR
/// </summary>
[NativeStructure]
[StructLayout(LayoutKind.Sequential)]
public struct ClrObjHeader
{

	public nint Value { get; set; }

}