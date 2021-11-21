using System.Reflection;
using Novus.Memory;
using Novus.Runtime.VM;
// ReSharper disable UnusedMemberInSuper.Global

namespace Novus.Runtime.Meta.Base;

/// <summary>
/// Describes a <see cref="MetaClrStructure{TClr}"/> that is enclosed by an accompanying <see cref="MethodTable"/>
/// </summary>
/// <typeparam name="TClr">CLR structure type</typeparam>
public abstract unsafe class EmbeddedClrStructure<TClr> : MetaClrStructure<TClr> where TClr : unmanaged
{
	public abstract MetaType EnclosingType { get; }


	protected EmbeddedClrStructure(Pointer<TClr> ptr) : base(ptr) { }

	protected EmbeddedClrStructure(MemberInfo info) : base(info) { }
}