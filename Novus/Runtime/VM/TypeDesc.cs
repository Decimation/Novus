// Author: Deci | Project: Novus | Name: TypeDesc.cs
// Date: 2024/09/11 @ 14:09:35

using System.Runtime.InteropServices;
using Novus.Imports.Attributes;
using Novus.Numerics;
using Novus.Runtime.VM.Tokens;

namespace Novus.Runtime.VM;

[NativeStructure]
[StructLayout(LayoutKind.Sequential)]
public unsafe struct TypeDesc
{

	/// <summary>
	/// Low-order 8 bits of this flag are used to store the CorElementType, which
	/// discriminates what kind of TypeDesc we are
	/// </summary>
	internal uint m_typeAndFlags;

	internal void* m_hExposedClassObject;

	internal CorElementType CorElementType => (CorElementType) (m_typeAndFlags & 0xFF);

}