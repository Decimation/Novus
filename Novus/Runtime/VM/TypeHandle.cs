// global using TADDR = nuint;

using Novus.Imports;
using Novus.Imports.Attributes;
using Novus.Memory;
using Novus.Runtime.VM.Tokens;
using Novus.Win32;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable FieldCanBeMadeReadOnly.Local

#pragma warning disable IDE0251
namespace Novus.Runtime.VM;

/// <summary>
/// Represents the <see cref="TypeHandle"/> as defined in the CLR
/// </summary>
[NativeStructure]
[StructLayout(LayoutKind.Explicit)]
public unsafe struct TypeHandle
{

	static TypeHandle()
	{
		Global.Clr.LoadImports(typeof(TypeHandle));
	}

	/// <summary>
	/// The address of the current type handle object.
	/// </summary>
	[FieldOffset(0)]
	private void* m_asTAddr;

	[MethodImpl(MImplO.AggressiveInlining)]
	internal TypeHandle(void* tAddr)
	{
		m_asTAddr = tAddr;
	}

	/// <summary>
	/// Gets whether the current instance wraps a <see langword="null"/> pointer.
	/// </summary>
	public bool IsNull => m_asTAddr is null;

	/// <summary>
	/// Gets whether or not this <see cref="TypeHandle"/> wraps a <c>TypeDesc</c> pointer.
	/// Only if this returns <see langword="false"/> it is safe to call <see cref="AsMethodTable"/>.
	/// </summary>
	public bool IsTypeDesc
	{
		[MethodImpl(MImplO.AggressiveInlining)]
		get => ((nint) m_asTAddr & 2) != 0;
	}

	/// <summary>
	/// Gets the <see cref="MethodTable"/> pointer wrapped by the current instance.
	/// </summary>
	/// <remarks>This is only safe to call if <see cref="IsTypeDesc"/> returned <see langword="false"/>.</remarks>
	[MethodImpl(MImplO.AggressiveInlining)]
	public MethodTable* AsMethodTable()
	{
		Debug.Assert(!IsTypeDesc);

		return (MethodTable*) m_asTAddr;
	}

	/// <summary>
	/// Gets the <see cref="TypeDesc"/> pointer wrapped by the current instance.
	/// </summary>
	/// <remarks>This is only safe to call if <see cref="IsTypeDesc"/> returned <see langword="true"/>.</remarks>
	[MethodImpl(MImplO.AggressiveInlining)]
	public TypeDesc* AsTypeDesc()
	{
		Debug.Assert(IsTypeDesc);

		return (TypeDesc*) ((nint) m_asTAddr & ~2); // Drop the second lowest bit.
	}

	public static bool AreSameType(TypeHandle left, TypeHandle right) => left.m_asTAddr == right.m_asTAddr;

	internal CorElementType CorElementType => (CorElementType) Func_GetCorElemType(m_asTAddr);

#region

	/// <summary>
	/// <see cref="MethodTable"/>
	/// </summary>
	[field: ImportClr("Sym_GetMethodTable", ImportType.Symbol)]
	private static delegate* unmanaged<TypeHandle*, MethodTable*> Func_GetMethodTable { get; }

	/// <summary>
	/// <see cref="CorElementType"/>
	/// </summary>
	[field: ImportClr("Sig_GetCorElemType")]
	private static delegate* unmanaged<void*, int> Func_GetCorElemType { get; }

#endregion

}

[Flags]
public enum TypeHandleBits
{

	MethodTable = 0,
	TypeDesc    = 2,
	ValidMask   = 2 //todo?

}