using System.Runtime.InteropServices;
using Novus.Imports;
using Novus.Imports.Attributes;
using Novus.Win32;

namespace Novus.Runtime.VM;

[NativeStructure]
[StructLayout(LayoutKind.Sequential)]
public struct ObjHeader
{
	private nint Value { get;set; }
}