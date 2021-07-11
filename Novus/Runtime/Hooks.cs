using System.Collections.Generic;
using Novus.Memory;
using Novus.Runtime.Meta;

namespace Novus.Runtime
{
	public static class Hooks
	{
		private struct HookBuffer
		{
			internal MetaMethod Destination { get; init; }

			internal Pointer<byte> Original { get; init; }
		}

		private static readonly Dictionary<MetaMethod, HookBuffer> Dictionary = new();

		public static void Set(MetaMethod src, MetaMethod dest)
		{
			src.Prepare();

			Dictionary.Add(src, new HookBuffer()
			{
				Destination = dest,
				Original    = src.EntryPoint
			});
			src.EntryPoint = dest.EntryPoint;
		}

		public static void Restore(MetaMethod src)
		{
			src.Reset();
			src.EntryPoint = Dictionary[src].Original;
			src.Prepare();
		}
	}
}