using System;
using System.Reflection;
using Novus.Memory;
using Novus.Win32;

namespace Novus.CoreClr.Meta.Base
{
	/// <summary>
	/// Describes a CLR structure that has metadata information.
	/// </summary>
	/// <typeparam name="TClr">CLR structure type (<see cref="IClrStructure"/>)</typeparam>
	public abstract unsafe class StandardClrStructure<TClr> : BaseClrStructure<TClr>
		where TClr : unmanaged
	{
		public virtual string Name => Info?.Name;

		public abstract MemberInfo Info { get; }

		public abstract int Token { get; }

		internal StandardClrStructure(Pointer<TClr> ptr) : base(ptr) { }

		protected StandardClrStructure(MemberInfo member) : base(member) { }


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

		public static bool operator ==(StandardClrStructure<TClr> left, StandardClrStructure<TClr> right) =>
			Equals(left, right);

		public static bool operator !=(StandardClrStructure<TClr> left, StandardClrStructure<TClr> right) =>
			!Equals(left, right);
	}
}
