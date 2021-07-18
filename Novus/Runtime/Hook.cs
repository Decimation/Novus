using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Novus.Memory;
using Novus.Runtime.Meta;
using Kantan.Utilities;
using static Kantan.Diagnostics.LogCategories;
// ReSharper disable UnusedMember.Global

namespace Novus.Runtime
{
	public static class Hook
	{
		private static Dictionary<MetaMethod, HookBuffer> Map { get; } = new();


		public static HookBuffer Get(MetaMethod m) => Map[m];

		public static void RestoreAll()
		{
			var array = Map.Keys.ToArray();

			for (int i = Map.Count - 1; i >= 0; i--) {
				Restore(array[i]);
			}
		}

		public static void Set(MetaMethod src, MetaMethod dest)
		{
			src.Prepare();

			Map.Add(src, new HookBuffer
			{
				Destination = dest,
				Original    = src.EntryPoint,
				Target = src
			});

			src.EntryPoint = dest.EntryPoint;

			Debug.WriteLine($"[{nameof(Hook)}] Set: {src.Name} \u2192 {dest.EntryPoint}", C_DEBUG);
		}

		public static void Restore(MetaMethod src)
		{
			src.Reset();
			src.EntryPoint = Map[src].Original;
			src.Prepare();
			Map.Remove(src);

			Debug.WriteLine($"[{nameof(Hook)}] Restored: {src.Name}", C_DEBUG);
		}

	}

	public readonly struct HookBuffer
	{
		public MetaMethod Target    { get; internal init; }

		public MetaMethod Destination { get; internal init; }

		public Pointer<byte> Original { get; internal init; }
	}
}