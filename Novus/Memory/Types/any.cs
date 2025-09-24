// Author: Deci | Project: Novus | Name: any.cs
// Date: 2024/09/06 @ 03:09:58


// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;
using Novus.Memory.Allocation;

#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.

#pragma warning disable CS1574
namespace Novus.Memory.Types;

[Experimental(Global.DIAG_ID_EXPERIMENTAL)]
public struct any
{

	private Pointer<byte> m_address;

	public Pointer<byte> Address
	{
		get => m_address;
	}

	private Type m_type;

	[MN]
	public Type Type
	{
		get { return m_type; }
		set { }
	}

	[MNNW(true, nameof(Type))]
	public bool HasType => Type != null;

	public any(Pointer<byte> address, Type type = null)
	{
		m_address = address;
		m_type    = type;
	}

	public static explicit operator any(Pointer<byte> p)
		=> new(p, null);


	/*public static any Alloc<T>()
	{
		AllocManager.New<>()
	}*/

}