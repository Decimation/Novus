using System.Linq;
using JetBrains.Annotations;
using Novus.Memory;
using Novus.Runtime.Meta;
using Novus.Utilities;
using SimpleCore.Utilities;

// ReSharper disable UnusedMember.Global

namespace Novus.Runtime
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
			/*
			 *
			 */

			var addrTable = new ConsoleTable("-", "Address");

			var addr = Mem.AddressOf(ref t);
			addrTable.AddRow("Address", addr);

			if (Mem.TryGetAddressOfHeap(t, out var heap)) {
				addrTable.AddRow("Address (heap)", heap);
			}

			addrTable.Write();

			/*
			 *
			 */

			var propTable = new ConsoleTable("-", "Value");

			propTable.AddRow("Pinnable", RuntimeInfo.IsPinnable(t));
			propTable.AddRow("Boxed", RuntimeInfo.IsBoxed(t));
			propTable.AddRow("Nil", RuntimeInfo.IsNil(t));

			propTable.Write();
		}

		public static void DumpLayout<T>(ref T t)
		{
			var layoutTable = new ConsoleTable("Offset", "Size", "Type", "Name");


			var mt = t.GetMetaType();
			var f  = mt.Fields.Where(x => !x.IsStatic);
			int s  = Mem.SizeOf(t, SizeOfOptions.Auto);
			var p  = Mem.AddressOf(ref t);


			//sb.AppendLine($"{mt.Name} ({s}) @ {p}:\n");


			foreach (var metaField in f) {
				layoutTable.AddRow(
					$"0x{metaField.Offset:X}", metaField.Size, metaField.FieldType.Name, metaField.Name);
			}


			layoutTable.Write();
		}
	}
}