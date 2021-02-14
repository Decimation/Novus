using System.Runtime.InteropServices;
using Novus.Imports;

namespace Novus.Runtime.VM.EE
{
	/// <summary>
	/// Substructure of <see cref="EEClass"/>
	/// </summary>
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