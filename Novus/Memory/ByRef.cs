using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
// ReSharper disable UnusedMember.Global
namespace Novus.Memory
{
	// NOTE: Experimental

	/// <summary>
	/// <remarks><c>ByReference</c></remarks>
	/// </summary>
	public readonly unsafe ref struct ByRef<T>
	{
		private readonly void* m_data;

		public ref T Value => ref Unsafe.AsRef<T>(m_data);

		public ByRef(ref T t)
		{
			m_data = Unsafe.AsPointer(ref t);
		}

		public override string ToString()
		{
			return $"{typeof(T)}& = {Value}";
		}
	}
}