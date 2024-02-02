#nullable enable
using System;
using System.Reflection;
using Novus.Memory;
using Novus.Win32;

// ReSharper disable UnusedMemberInSuper.Global
#pragma warning disable CA1416

namespace Novus.Runtime.Meta.Base;

/// <summary>
/// Describes a CLR structure that has metadata information.
/// </summary>
/// <typeparam name="TClr">CLR structure type</typeparam>
public abstract unsafe class MetaClrStructure<TClr> : BaseClrStructure<TClr>
	where TClr : unmanaged
{
	// NOTE: Maybe add MemberInfo type parameter
		
	public virtual string Name => Info.Name;

	public abstract MemberInfo Info { get; }

	public abstract int Token { get; }

	internal MetaClrStructure(Pointer<TClr> ptr) : base(ptr) { }

	protected MetaClrStructure(MemberInfo member) : base(member) { }

	public override string ToString()
	{
		return $"Handle: {Value} | Name: {Name}";
	}

	public override bool Equals(object? obj)
	{
		return obj != null && base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return Native.INVALID;
	}

	public static bool operator ==(MetaClrStructure<TClr> left, MetaClrStructure<TClr> right) =>
		Equals(left, right);

	public static bool operator !=(MetaClrStructure<TClr> left, MetaClrStructure<TClr> right) =>
		!Equals(left, right);
}