using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Novus.CoreClr.VM;
using Novus.Memory;

namespace Novus.CoreClr.Meta
{
	/// <summary>
	/// Describes a <see cref="StandardClrStructure{TClr}"/> that is enclosed by an accompanying <see cref="MethodTable"/>
	/// </summary>
	/// <typeparam name="TClr">CLR structure type</typeparam>
	public abstract unsafe class EmbeddedClrStructure<TClr> : StandardClrStructure<TClr> where TClr : unmanaged
	{
		#region MethodTable

		public abstract MetaType EnclosingType { get; }

		#endregion

		

		#region Constructors

		protected EmbeddedClrStructure(Pointer<TClr> ptr) : base(ptr) { }

		protected EmbeddedClrStructure(MemberInfo info) : base(info) { }

		#endregion
	}
}
