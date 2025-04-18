﻿// Author: Deci | Project: Novus | Name: MetaType.cs
// Date: 2020/11/02 @ 00:11:00

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Kantan.Diagnostics;
using Novus.Memory;
using Novus.Runtime.Meta.Base;
using Novus.Runtime.VM;
using Novus.Runtime.VM.EE;
using Novus.Runtime.VM.Tokens;
using Novus.Utilities;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

#nullable enable
namespace Novus.Runtime.Meta;

public static class MetaExtensions
{

	public static MetaType GetMetaType(this object o)
		=> o.GetType().AsMetaType();

	public static MetaMethod AsMetaMethod(this MethodInfo t)
		=> new(t);

	public static MetaField AsMetaField(this FI t)
		=> new(t);

	public static MetaType AsMetaType(this Type t)
		=> new(t);

}

/// <summary>
///     <list type="bullet">
///         <item>
///             <description>
///                 CLR structure: <see cref="VM.MethodTable" />, <see cref="VM.EE.EEClass" />,
///                 <see cref="VM.TypeHandle" />, <see cref="VM.EE.EEClassLayoutInfo" />, <see cref="VM.EE.ArrayClass" />
///             </description>
///         </item>
///         <item>
///             <description>Reflection structure: <see cref="Type" /></description>
///         </item>
///     </list>
/// </summary>
public unsafe class MetaType : MetaClrStructure<MethodTable>
{

	public MetaType(Pointer<MethodTable> mt) : base(mt) { }

	public MetaType(Type t) : base(t) { }

	/// <summary>
	///     Equals <see cref="Type.Attributes" />
	/// </summary>
	public TypeAttributes Attributes => EEClass.Reference.Attributes;

	/// <summary>
	///     Size of the padding in <see cref="BaseSize" />
	/// </summary>
	public int BaseSizePadding => EEClass.Reference.BaseSizePadding;

	// public bool FieldsArePacked => EEClass.Reference.FieldsArePacked;

	public int FieldsCount => EEClass.Reference.FieldListLength;

	// public int FixedEEClassFields => EEClass.Reference.FixedEEClassFields;

	public int InstanceFieldsCount => EEClass.Reference.NumInstanceFields;

	/// <summary>
	///     Size of instance fields
	/// </summary>
	public int InstanceFieldsSize => BaseSize - BaseSizePadding;

	public CorInterfaceType InterfaceType => EEClass.Reference.InterfaceType;

	public int MethodsCount => EEClass.Reference.NumMethods;

	/// <summary>
	///     Equals <see cref="Marshal.SizeOf(Type)" />, <see cref="Marshal.SizeOf{T}()" />
	/// </summary>
	public int NativeSize => (int) NativeLayoutInfo.Reference.Size;

	public int NonVirtualSlotsCount => EEClass.Reference.NumNonVirtualSlots;

	public CorElementType NormType => EEClass.Reference.NormType;

	public IEnumerable<MetaField> Fields
		=> RuntimeType.GetRuntimeFields()
			.Select(f => new MetaField(f));

	public IEnumerable<MetaMethod> Methods
		=> RuntimeType.GetRuntimeMethods()
			.Select(m => new MetaMethod(m));

	private Pointer<EEClassNativeLayoutInfo> NativeLayoutInfo
	{
		get
		{
			Require.Assert(HasLayout);

			return Value.Reference.NativeLayoutInfo;
		}
	}

	public bool IsMarshalable => NativeLayoutInfo.Reference.IsMarshalable;

	public int NativeFieldsCount => (int) NativeLayoutInfo.Reference.NumFields;

	private Pointer<EEClassLayoutInfo> LayoutInfo
	{
		get
		{
			Require.Assert(HasLayout);

			return EEClass.Reference.LayoutInfo;
		}
	}

	public int ManagedSize => LayoutInfo.Reference.ManagedSize;

	public LayoutFlags LayoutFlags => LayoutInfo.Reference.Flags;

	public int PackingSize => LayoutInfo.Reference.PackingSize;

	/// <summary>
	///     Number of fields that are not <see cref="FieldInfo.IsLiteral" /> but <see cref="MetaField.IsStatic" />
	/// </summary>
	public int StaticFieldsCount => EEClass.Reference.NumStaticFields;

	public VMFlags VMFlags => EEClass.Reference.VMFlags;

	public bool ContainsPointers => TypeFlags.HasFlag(TypeFlags.ContainsPointers);

	public bool HasComponentSize => TypeFlags.HasFlag(TypeFlags.HasComponentSize);

	/// <summary>
	///     Whether this <see cref="EEClass" /> has a <see cref="EEClassLayoutInfo" />
	/// </summary>
	public bool HasLayout => VMFlags.HasFlag(VMFlags.HasLayout);

	/// <summary>
	///     <a href="https://docs.microsoft.com/en-us/dotnet/framework/interop/blittable-and-non-blittable-types">
	///         Blittable types
	///     </a>
	/// </summary>
	public bool IsBlittable => HasLayout && LayoutInfo.Reference.Flags.HasFlag(LayoutFlags.Blittable);

	public bool IsArray => (TypeFlags & TypeFlags.ArrayMask) == TypeFlags.Array;

	public bool IsDelegate => VMFlags.HasFlag(VMFlags.Delegate);

	/// <summary>
	///     Equals <see cref="RuntimeHelpers.IsReferenceOrContainsReferences{T}" />
	/// </summary>
	public bool IsReferenceOrContainsReferences => !RuntimeType.IsValueType || ContainsPointers;

	public bool IsString => HasComponentSize && !IsArray && RawGetComponentSize() == sizeof(char);

	public bool IsStringOrArray => HasComponentSize;

	public override MMI Info => RuntimeType;

	/// <summary>
	///     Equals <see cref="MemberInfo.MetadataToken" />
	/// </summary>
	public override int Token => TokenHelper.TokenFromRid(Value.Reference.RawToken, CorTokenType.TypeDef);

	public CorElementType CorElementType => RuntimeType.GetCorElementType();

	public int BaseDataSize
	{
		get
		{
			if (RuntimeType.IsValueType) {

				var mi = typeof(Mem).GetRuntimeMethods().First(static mi2 =>
				{
					var infos = mi2.GetParameters();

					return mi2.Name                    == nameof(Mem.SizeOf)
					       && infos.Length           == 2
					       && infos[1].ParameterType == typeof(SizeOfOption);
				});

				return (int) mi.CallGeneric(RuntimeType, null, null, SizeOfOption.Intrinsic);
			}

			// Subtract the size of the ObjHeader and MethodTable*
			return InstanceFieldsSize;
		}
	}

	/// <summary>
	///     Equals <see cref="Array.Rank" />
	/// </summary>
	public int ArrayRank => EEClass.Reference.ArrayRank;

	public int BaseSize => Value.Reference.BaseSize;

	public int ComponentSize => Value.Reference.ComponentSize;

	private Pointer<EEClass> EEClass => Value.Reference.EEClass;

	public MetaType ElementTypeHandle => Value.Reference.ElementTypeHandle;

	public GenericsFlags GenericFlags => Value.Reference.GenericsFlags;

	public int InterfacesCount => Value.Reference.NumInterfaces;

	public Pointer<byte> Module => Value.Reference.Module;

	public MetaType ParentType => (Pointer<MethodTable>) Value.Reference.Parent;

	public CorElementType ArrayElementType => EEClass.Reference.ArrayElementType;

	public Type RuntimeType => RuntimeProperties.ResolveType(Value.Cast<MethodTable>());

	public MethodTableFlags2 SlotsFlags => Value.Reference.Flags2;

	public TypeFlags TypeFlags => Value.Reference.TypeFlags;

	public int VirtualsCount => Value.Reference.NumVirtuals;

	public bool IsInteger => RuntimeType.IsInteger();

	public bool IsReal => RuntimeType.IsReal();

	public bool IsNumeric => RuntimeType.IsNumeric();

	public bool IsUnmanaged => RuntimeType.IsUnmanaged();

	public bool IsAnyPointer => RuntimeType.IsAnyPointer();

	public MetaField GetField(string name)
		=> RuntimeType.GetAnyField(name);

	public MetaMethod GetMethod(string name)
		=> RuntimeType.GetAnyMethod(name);

	// returns random combination of flags if this doesn't have a component size
	private ushort RawGetComponentSize()
		=> (ushort) ComponentSize;

	public bool Equals(MetaType other)
	{
		return base.Equals(other); /*&& RuntimeType == other.RuntimeType*/
	}

	public override bool Equals(object? obj)
	{
		if (obj is null)
			return false;

		if (ReferenceEquals(this, obj))
			return true;

		return obj.GetType() == GetType() && Equals((MetaType) obj);

	}

	public override int GetHashCode()
	{
		unchecked {
			return (base.GetHashCode() * 397) ^ RuntimeType.GetHashCode();
		}
	}

	public override string ToString()
		=> ($"{base.ToString()} | {nameof(EEClass)}: {EEClass}");

	public static implicit operator MetaType(Pointer<MethodTable> ptr)
		=> new(ptr);

	public static implicit operator MetaType(Type t)
		=> new(t);

	public static bool operator !=(MetaType left, MetaType right)
	{
		return !Equals(left, right);
	}

	public static bool operator ==(MetaType left, MetaType right)
	{
		return Equals(left, right);
	}

}