#nullable enable
using System;
using System.Reflection;
using Novus.Memory;
using Novus.Win32;

namespace Novus.CoreClr.Meta.Base
{
	/// <summary>
	/// Describes a CLR structure that has metadata information.
	/// </summary>
	/// <typeparam name="TClr">CLR structure type</typeparam>
	public abstract unsafe class MetaClrStructure<TClr> : BaseClrStructure<TClr>
		where TClr : unmanaged
	{
		// TODO: Maybe add MemberInfo type parameter
		
		public virtual string Name => Info.Name;

		public abstract MemberInfo Info { get; }

		public abstract int Token { get; }

		internal MetaClrStructure(Pointer<TClr> ptr) : base(ptr) { }

		protected MetaClrStructure(MemberInfo member) : base(member) { }


		public override string ToString()
		{
			return String.Format("Handle: {0} | Name: {1}", Value, Name);
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
}
