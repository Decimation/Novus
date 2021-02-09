using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
		[Flags]
		public enum InspectorOptions
		{
			// todo


			Offset  = 0,
			Size    = 1,
			Type    = 1 << 1,
			Name    = 1 << 2,
			Address = 1 << 3,
			Value   = 1 << 4,

			All = Offset | Size | Type | Name | Address | Value
		}


		public static void DumpInfo<T>([NotNull] ref T value)
		{
			/*
			 *
			 */

			var addrTable = new ConsoleTable("-", "Address");

			var addr = Mem.AddressOf(ref value);
			addrTable.AddRow("Address", addr);

			if (Mem.TryGetAddressOfHeap(value, out var heap)) {
				addrTable.AddRow("Address (heap)", heap);
			}

			addrTable.Write();

			/*
			 *
			 */

			var propTable = new ConsoleTable("-", "Value");

			propTable.AddRow("Pinnable", RuntimeInfo.IsPinnable(value));
			propTable.AddRow("Boxed", RuntimeInfo.IsBoxed(value));
			propTable.AddRow("Nil", RuntimeInfo.IsNil(value));

			propTable.Write();
		}

		public const InspectorOptions DEFAULT = InspectorOptions.All;

		public static void DumpLayout<T>(ref T value, InspectorOptions options = DEFAULT)
		{
			var layoutTable = new ConsoleTable();

			var flags = Enums.GetSetFlags(options);

			flags.RemoveAt(flags.Count - 1);

			layoutTable.AddColumn(flags.Select(Enum.GetName));

			Debug.WriteLine($"{flags.QuickJoin()}");


			var mt     = value.GetMetaType();
			var fields = mt.Fields.Where(x => !x.IsStatic);

			int s = Mem.SizeOf(value, SizeOfOptions.Auto);
			var p = Mem.AddressOf(ref value);


			foreach (var metaField in fields) {

				var rowValues = new List<object>();


				if (options.HasFlag(InspectorOptions.Offset)) {
					rowValues.Add($"0x{metaField.Offset:X}");
				}

				if (options.HasFlag(InspectorOptions.Size)) {
					rowValues.Add(metaField.Size);
				}

				if (options.HasFlag(InspectorOptions.Type)) {
					rowValues.Add(metaField.FieldType.Name);
				}

				if (options.HasFlag(InspectorOptions.Name)) {
					rowValues.Add(metaField.Name);
				}

				if (options.HasFlag(InspectorOptions.Address)) {
					var addr = Mem.AddressOfFields(ref value) + metaField.Offset;
					rowValues.Add($"0x{addr}");
				}

				if (options.HasFlag(InspectorOptions.Value)) {
					var fieldVal = metaField.Info.GetValue(value);
					rowValues.Add($"{fieldVal}");
				}

				layoutTable.AddRow(rowValues.ToArray());
			}


			layoutTable.Write();
		}
	}
}