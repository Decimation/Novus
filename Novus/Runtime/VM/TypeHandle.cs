// global using TADDR = nuint;

using System.Runtime.InteropServices;
using Novus.Imports;
using Novus.Imports.Attributes;
using Novus.Memory;
using Novus.Win32;

// ReSharper disable InconsistentNaming

// ReSharper disable UnusedMember.Global

// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace Novus.Runtime.VM;

[NativeStructure]
[StructLayout(LayoutKind.Sequential)]
public unsafe struct TypeHandle
{

	static TypeHandle()
	{
		Global.Clr.LoadImports(typeof(TypeHandle));
	}

	private void* Value { get; set; }

	internal Pointer<MethodTable> MethodTable
	{
		get
		{
			fixed (TypeHandle* p = &this) {

				return Func_GetMethodTable(p);
			}
		}
	}

	/// <summary>
	/// <see cref="TypeHandle.MethodTable"/>
	/// </summary>
	[field: ImportClr("Sig_GetMethodTable")]
	private static delegate* unmanaged[Thiscall]<TypeHandle*, MethodTable*> Func_GetMethodTable { get; }

}