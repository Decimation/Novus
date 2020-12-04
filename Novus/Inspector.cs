using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Novus.CoreClr;
using Novus.CoreClr.Meta;
using Novus.Memory;
using Novus.Utilities;

// ReSharper disable UnusedMember.Global

namespace Novus
{
	/// <summary>
	///     Utilities for inspecting and analyzing objects, data, etc.
	/// </summary>
	/// 
	/// <seealso cref="Mem" />
	/// <seealso cref="RuntimeInfo" />
	/// <seealso cref="ReflectionHelper" />
	public static class Inspector
	{
		public enum InspectorOptions
		{
			// todo
		}
		
		
		public static void DumpInfo<T>([NotNull] ref T t)
		{
			var sb = new StringBuilder();

			var addr = Mem.AddressOf(ref t);
			sb.AppendFormat("Address: {0}\n", addr);

			if (Mem.TryGetAddressOfHeap(t, out var heap)) {
				sb.AppendFormat("Address (heap): {0}\n", heap);
			}

			sb.AppendFormat("Pinnable: {0}\n", RuntimeInfo.IsPinnable(t));
			sb.AppendFormat("Boxed: {0}\n", RuntimeInfo.IsBoxed(t));
			sb.AppendFormat("Nil: {0}\n", RuntimeInfo.IsNil(t));

			var type = t.GetMetaType();


			Console.WriteLine(sb);
		}

		public static void DumpLayout<T>(ref T t)
		{
			var sb = new StringBuilder();
			var mt = t.GetMetaType();
			var f  = mt.Fields.Where(x => !x.IsStatic);
			int s  = Mem.SizeOf(t, SizeOfOptions.Auto);
			var p  = Mem.AddressOf(ref t);
			
			
			sb.AppendLine($"{mt.Name} ({s}) @ {p}:\n");

			foreach (var metaField in f) {
				sb.AppendLine(
					$"0x{metaField.Offset:X} | {metaField.Size} | ({metaField.FieldType.Name}) {metaField.Name}");
			}


			Console.WriteLine(sb);
		}
	}
}