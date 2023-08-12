using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Novus.Imports;
using Novus.Imports.Attributes;
using Novus.Memory;
using Novus.Numerics;
using Novus.Win32;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable CommentTypo

// ReSharper disable StructCanBeMadeReadOnly

namespace Novus.Runtime.VM;

[NativeStructure]
[StructLayout(LayoutKind.Sequential)]
public unsafe struct FieldDesc
{
	static FieldDesc()
	{
		Global.Clr.LoadImports(typeof(FieldDesc));
	}

	private Pointer<MethodTable> EnclosingMethodTableStub { get; }

	/// <summary>
	/// <c>DWORD</c> #1
	///     <para>unsigned m_mb : 24;</para>
	///     <para>unsigned m_isStatic : 1;</para>
	///     <para>unsigned m_isThreadLocal : 1;</para>
	///     <para>unsigned m_isRVA : 1;</para>
	///     <para>unsigned m_prot : 3;</para>
	///     <para>unsigned m_requiresFullMbValue : 1;</para>
	/// </summary>
	private uint UInt1 { get; }

	/// <summary>
	/// <c>DWORD</c> #2
	///     <para>unsigned m_dwOffset : 27;</para>
	///     <para>unsigned m_type : 5;</para>
	/// </summary>
	private uint UInt2 { get; }

	internal int Size
	{
		get
		{
			fixed (FieldDesc* p = &this) {
				return Func_GetSize(p);
			}
		}
	}

	internal Pointer<MethodTable> ApproxEnclosingMethodTable
	{
		get
		{
			// m_pMTOfEnclosingClass.GetValue(PTR_HOST_MEMBER_TADDR(FieldDesc, this, m_pMTOfEnclosingClass));

			const int MT_FIELD_OFS = 0;

			//return Mem.OffsetField((MethodTable*) EnclosingMethodTableStub, MT_FIELD_OFS);

			return EnclosingMethodTableStub.AddBytes(MT_FIELD_OFS);
		}
	}

	internal int Offset => BitCalculator.ReadBits((int) UInt2, 0, 27);

	internal int Token
	{
		get
		{
			int rawToken = (int) (UInt1 & 0xFFFFFF);

			// Check if this FieldDesc is using the packed mb layout
			if (!BitFlags.HasFlag(FieldBitFlags.RequiresFullMBValue))
				return Tokens.TokenFromRid(rawToken & (int) PackedLayoutMask.MBMask, CorTokenType.FieldDef);

			return Tokens.TokenFromRid(rawToken, CorTokenType.FieldDef);
		}
	}

	internal CorElementType Element => (CorElementType) (int) ((UInt2 >> 27) & 0x7FFFFFF);

	internal AccessModifiers Access => (AccessModifiers) (int) ((UInt1 >> 26) & 0x3FFFFFF);

	internal bool IsPointer => Element == CorElementType.Ptr;

	internal FieldBitFlags BitFlags => (FieldBitFlags) UInt1;

	internal void* StaticAddress
	{
		get
		{
			fixed (FieldDesc* p = &this) {
				return Func_StaticAddress(p);
			}
		}
	}

	/// <summary>
	/// <see cref="FieldDesc.Size"/>
	/// </summary>
	[field: ImportClr("Sig_GetSize")]
	private static delegate* unmanaged[Thiscall]<FieldDesc*, int> Func_GetSize { get; }

	/// <summary>
	/// <see cref="FieldDesc.StaticAddress"/>
	/// </summary>
	[field: ImportClr("Sig_GetStaticAddr")]
	private static delegate* unmanaged[Thiscall]<FieldDesc*, void*> Func_StaticAddress { get; }

}

/// <summary>
/// Describes access modifiers for <see cref="FieldDesc.Access"/>
/// </summary>
public enum AccessModifiers
{
	/// <summary>
	///     <remarks>Equals <see cref="FieldInfo.IsPrivate" /></remarks>
	/// </summary>
	Private = 2,

	/// <summary>
	///     <remarks>Equals <see cref="FieldInfo.IsFamilyAndAssembly" /></remarks>
	/// </summary>
	PrivateProtected = 4,

	/// <summary>
	///     <remarks>Equals <see cref="FieldInfo.IsAssembly" /></remarks>
	/// </summary>
	Internal = 6,

	/// <summary>
	///     <remarks>Equals <see cref="FieldInfo.IsFamily" /></remarks>
	/// </summary>
	Protected = 8,

	/// <summary>
	///     <remarks>Equals <see cref="FieldInfo.IsFamilyOrAssembly" /></remarks>
	/// </summary>
	ProtectedInternal = 10,

	/// <summary>
	///     <remarks>Equals <see cref="FieldInfo.IsPublic" /></remarks>
	/// </summary>
	Public = 12
}

/// <summary>
/// Flags for <see cref="FieldDesc.UInt1"/>
/// </summary>
[Flags]
public enum FieldBitFlags
{
	// <summary>
	// <c>DWORD</c> #1
	//     <para>unsigned m_mb : 24;</para>
	//     <para>unsigned m_isStatic : 1;</para>
	//     <para>unsigned m_isThreadLocal : 1;</para>
	//     <para>unsigned m_isRVA : 1;</para>
	//     <para>unsigned m_prot : 3;</para>
	//     <para>unsigned m_requiresFullMbValue : 1;</para>
	// </summary>

	None = 0,

	Static = 1 << 24,

	ThreadLocal = 1 << 25,

	RVA = 1 << 26,

	RequiresFullMBValue = 1 << 31
}

/// <summary>
///     Packed MB layout masks
/// </summary>
internal enum PackedLayoutMask
{
	MBMask       = 0x01FFFF,
	NameHashMask = 0xFE0000
}