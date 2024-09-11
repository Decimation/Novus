// Author: Deci | Project: Novus | Name: TypeDesc.cs
// Date: 2024/09/11 @ 14:09:35

using System.Runtime.InteropServices;
using Novus.Imports.Attributes;
using Novus.Numerics;

namespace Novus.Runtime.VM;

[NativeStructure]
[StructLayout(LayoutKind.Sequential)]
public unsafe struct TypeDesc
{

	// Low-order 8 bits of this flag are used to store the CorElementType, which
	// discriminates what kind of TypeDesc we are
	private uint m_typeAndFlags;

	private void* m_hExposedClassObject;

	internal CorElementType CorElementType
	{
		get { return (CorElementType) (m_typeAndFlags & 0xFF); }
	}

}