using System.Runtime.InteropServices;
using Novus.Interop;

namespace Novus.CoreClr.VM
{
	[NativeStructure]
	[StructLayout(LayoutKind.Sequential)]
	public struct ArrayClass
	{
		internal byte Rank { get; }

		/// <summary>
		/// Cache of <see cref="MethodTable.ElementTypeHandle"/>
		/// </summary>
		internal CorElementType ElementType { get; }
	}
}