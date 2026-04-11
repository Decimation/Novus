// Author: Deci | Project: Novus | Name: ObjectProxy.cs
// Date: 2026/04/11 @ 12:04:22

using Novus.Memory;
// ReSharper disable ClassCannotBeInstantiated

namespace Novus.Runtime.VM;

/// <summary>
/// Reference type equivalent of <see cref="ClrObject"/>.
///     <para>Also used assist with unsafe pinning of arbitrary objects. The typical usage pattern is:</para>
///     <code>
///  fixed (byte* pData = &amp;PinHelper.GetPinProxy(value).Data)
///  {
///  }
///  </code>
///     <remarks>
///         <para>Memory layout is equivalent to <see cref="ClrObject" /></para>
///         <para><c>pData</c> is what <c>Object::GetData()</c> returns in VM.</para>
///         <para><c>pData</c> is also equal to offsetting the pointer by <see cref="OffsetOptions.Fields" />. </para>
///         <para>From <see cref="System.Runtime.CompilerServices.JitHelpers" />. </para>
///     </remarks>
/// </summary>
public sealed class ObjectProxy
{

	/// <summary>
	///     Represents the first field in an object.
	/// </summary>
	/// <remarks>
	///     <para>Equals <see cref="Mem.AddressOfHeap{T}(T,OffsetOptions)" /> with <see cref="OffsetOptions.Fields" />.</para>
	///     <para>Equals <see cref="ClrObject.Data" /></para>
	/// </remarks>
	public byte Data;

	private ObjectProxy() { }

}