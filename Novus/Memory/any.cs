// Author: Deci | Project: Novus | Name: any.cs
// Date: 2024/09/06 @ 03:09:58


// ReSharper disable InconsistentNaming

using Novus.Memory.Allocation;

#pragma warning disable CS1574
namespace Novus.Memory;

public struct any
{

	private Pointer m_address;

	public Pointer Address
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

	public any(Pointer address, Type type = null)
	{
		m_address = address;
		m_type    = type;
	}

	public static explicit operator any(Pointer p)
		=> new(p, null);

	/*public static any Alloc<T>()
	{
		AllocManager.New<>()
	}*/

}