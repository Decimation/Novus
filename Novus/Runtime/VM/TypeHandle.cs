﻿// global using TADDR = nuint;

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
[StructLayout(LayoutKind.Explicit)]
public unsafe struct TypeHandle
{

	static TypeHandle()
	{
		Global.Clr.LoadImports(typeof(TypeHandle));
	}

	[field: FieldOffset(0)]
	public void* Value { get; set; }

	[field: FieldOffset(0)]
	public ulong TAddr { get; set; }

	[field: FieldOffset(0)]
	public MethodTable* AsMethodTable { get; set; }

	[field: FieldOffset(0)]
	public TypeDesc* AsTypeDesc { get; set; }

	internal Pointer<MethodTable> MethodTable
	{
		get
		{
			fixed (TypeHandle* p = &this) {
				return Func_GetMethodTable(p);
			}
		}
	}

	/*internal bool IsTypeDesc
	{
		get { return (((nuint) Value) & 2) != 0; }
	}*/

	internal TypeDesc* AsTypeDesc1 => (TypeDesc*) ((nuint) Value - 2);

	internal bool IsMethodTable
		=> ((TypeHandleBits) (nuint) Value & TypeHandleBits.ValidMask) == TypeHandleBits.MethodTable;

	internal bool IsTypeDesc => ((TypeHandleBits) (nuint) Value & TypeHandleBits.ValidMask) == TypeHandleBits.TypeDesc;

	/// <summary>
	/// <see cref="TypeHandle.MethodTable"/>
	/// </summary>
	[field: ImportClr("Sig_GetMethodTable")]
	private static delegate* unmanaged[Thiscall]<TypeHandle*, MethodTable*> Func_GetMethodTable { get; }

}

[Flags]
public enum TypeHandleBits
{

	MethodTable = 0,
	TypeDesc    = 2,
	ValidMask   = 2

}