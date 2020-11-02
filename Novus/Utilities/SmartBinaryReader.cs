using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;

// ReSharper disable UnusedMember.Global

#nullable enable
namespace Novus.Utilities
{
	public unsafe class SmartBinaryReader : BinaryReader
	{
		public SmartBinaryReader([NotNull] Stream input)
			: base(input) { }

		public SmartBinaryReader([NotNull] Stream input, [NotNull] Encoding encoding)
			: base(input, encoding) { }

		public SmartBinaryReader([NotNull] Stream input, [NotNull] Encoding encoding, bool leaveOpen)
			: base(input, encoding, leaveOpen) { }

		public T ReadAny<T>()
		{
			var s  = Mem.SizeOf<T>();
			var rg = base.ReadBytes(s);
			
			T t;

			fixed (byte* p = rg) {
				t = Unsafe.Read<T>(p);
			}

			return t;
		}
	}
}