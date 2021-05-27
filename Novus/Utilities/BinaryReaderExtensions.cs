using System.IO;
using System.Runtime.CompilerServices;
using Novus.Memory;

// ReSharper disable UnusedMember.Global

#nullable enable
namespace Novus.Utilities
{
	public static unsafe class BinaryReaderExtensions
	{
		public static T ReadAny<T>(this BinaryReader br)
		{
			var s  = Mem.SizeOf<T>();
			var rg = br.ReadBytes(s);

			T t;

			fixed (byte* p = rg) {
				t = Unsafe.Read<T>(p);
			}

			return t;
		}
	}
}