using System.Runtime.InteropServices;

namespace Novus.CoreClr.VM
{
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