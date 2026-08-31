// Author: Deci | Project: Novus | Name: OffsetOptions.cs
// Date: 2026/08/30 @ 16:08:43

using System.Runtime.InteropServices;
using Novus.Runtime;
using Novus.Runtime.VM;

namespace Novus.Memory;

#pragma warning disable CS1574

/// <summary>
///     Offset options for <see cref="Mem.AddressOfHeap{T}(T,OffsetOptions)" />
/// </summary>
public enum OffsetOptions
{

	/// <summary>
	///     Return the pointer offset by <c>-</c><see cref="ObjectUtility.OffsetToData" />,
	///     so it points to the object's <see cref="ClrObjHeader" />.
	/// </summary>
	Header,

	/// <summary>
	///     If the type is a <see cref="string" />, return the
	///     pointer offset by <see cref="ObjectUtility.OffsetToStringData" /> so it
	///     points to the string's characters.
	///     <remarks>
	///         Note: Equal to <see cref="GCHandle.AddrOfPinnedObject" /> and <c>fixed</c>.
	///     </remarks>
	/// </summary>
	StringData,

	/// <summary>
	///     If the type is an array, return
	///     the pointer offset by <see cref="ObjectUtility.OffsetToArrayData" /> so it points
	///     to the array's elements.
	///     <remarks>
	///         Note: Equal to <see cref="GCHandle.AddrOfPinnedObject" /> and <c>fixed</c>
	///     </remarks>
	/// </summary>
	ArrayData,

	/// <summary>
	///     If the type is a reference type, return
	///     the pointer offset by <see cref="ObjectUtility.OffsetToData" /> so it points
	///     to the object's fields.
	/// </summary>
	Fields,

	/// <summary>
	///     Don't offset the heap pointer at all, so it
	///     points to the <see cref="TypeHandle" />
	/// </summary>
	None

}