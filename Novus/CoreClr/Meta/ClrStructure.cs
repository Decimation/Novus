using System;
using System.Reflection;
using Novus.Memory;
using Novus.Win32;

namespace Novus.CoreClr.Meta
{
	/// <summary>
	/// Describes a CLR structure that doesn't have an accompanying token or <see cref="MemberInfo"/>
	/// </summary>
	/// <typeparam name="TClr">CLR structure type</typeparam>
	public abstract unsafe class ClrStructure<TClr> where TClr : unmanaged
	{
		#region Fields

		/// <summary>
		/// Points to the internal CLR structure representing this instance
		/// </summary>
		public Pointer<TClr> Value { get; }

		/// <summary>
		/// The native, built-in form of <see cref="Value"/>
		/// </summary>
		protected internal TClr* NativePointer => Value.ToPointer<TClr>();

		#endregion

		#region Constructors

		/// <summary>
		/// Root constructor
		/// </summary>
		/// <param name="ptr">Metadata structure handle</param>
		protected ClrStructure(Pointer<TClr> ptr)
		{
			Value = ptr;
		}
		protected ClrStructure(MemberInfo member) : this(RuntimeInfo.ResolveHandle(member)) { }
		#endregion


		#region ToString

		public override string ToString()
		{
			return String.Format("Handle: {0}", Value);
		}

		#endregion

		#region Equality

		public override bool Equals(object? obj)
		{
			if (ReferenceEquals(null, obj))
				return false;

			if (ReferenceEquals(this, obj))
				return true;

			if (obj.GetType() != this.GetType())
				return false;

			return Equals((ClrStructure<TClr>) obj);
		}

		public bool Equals(ClrStructure<TClr> other)
		{
			return this.Value == other.Value;
		}

		public override int GetHashCode()
		{
			return Native.INVALID;
		}

		public static bool operator ==(ClrStructure<TClr> left, ClrStructure<TClr> right) =>
			Equals(left, right);

		public static bool operator !=(ClrStructure<TClr> left, ClrStructure<TClr> right) =>
			!Equals(left, right);

		#endregion
	}
}