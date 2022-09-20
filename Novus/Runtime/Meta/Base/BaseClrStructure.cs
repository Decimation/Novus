#nullable enable
using System;
using System.Reflection;
using Novus.Memory;

// ReSharper disable UnusedMember.Global

namespace Novus.Runtime.Meta.Base;

/// <summary>
/// Describes a CLR structure that doesn't have an accompanying token or <see cref="MemberInfo"/>
/// </summary>
/// <typeparam name="TClr">CLR structure type</typeparam>
public abstract unsafe class BaseClrStructure<TClr> where TClr : unmanaged
{
	/// <summary>
	/// Points to the internal CLR structure representing this instance
	/// </summary>
	public Pointer<TClr> Value { get; }

	/// <summary>
	/// The native, built-in form of <see cref="Value"/>
	/// </summary>
	protected internal TClr* NativePointer => Value.ToPointer<TClr>();

	/// <summary>
	/// Root constructor
	/// </summary>
	/// <param name="ptr">Metadata structure handle</param>
	protected BaseClrStructure(Pointer<TClr> ptr)
	{
		Value = ptr;
	}

	protected BaseClrStructure(MemberInfo member) : this(RuntimeProperties.ResolveHandle(member)) { }

	public override string ToString()
	{
		return $"Handle: {Value}";
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(null, obj))
			return false;

		if (ReferenceEquals(this, obj))
			return true;

		if (obj.GetType() != GetType())
			return false;

		return Equals((BaseClrStructure<TClr>) obj);
	}

	public bool Equals(BaseClrStructure<TClr> other)
	{
		return Value == other.Value;
	}

	public override int GetHashCode()
	{
		return Native.INVALID;
	}

	public static bool operator ==(BaseClrStructure<TClr> left, BaseClrStructure<TClr> right) =>
		Equals(left, right);

	public static bool operator !=(BaseClrStructure<TClr> left, BaseClrStructure<TClr> right) =>
		!Equals(left, right);
}