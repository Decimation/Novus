using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Novus.Memory;
using Novus.Runtime.Meta;
using Novus.Utilities;
using Novus.Win32;
using SimpleCore.Model;
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
			None = 0,

			Offset  = 1,
			Size    = 1 << 1,
			Type    = 1 << 2,
			Name    = 1 << 3,
			Address = 1 << 4,
			Value   = 1 << 5,
			

			All = Offset | Size | Type | Name | Address | Value
		}

		private const InspectorOptions DEFAULT = InspectorOptions.All;


		public static void DumpInfo<T>([NotNull] ref T value)
		{
			/*
			 *
			 */

			var addrTable = new ConsoleTable("Addresses", "");

			var addr = Mem.AddressOf(ref value);
			addrTable.AddRow("Address", addr);

			if (Mem.TryGetAddressOfHeap(value, out var heap)) {
				addrTable.AddRow("Address (heap)", heap);
			}

			addrTable.Write(ConsoleTableFormat.Minimal);

			/*
			 *
			 */

			var infoTable = new ConsoleTable("Sizes", "");

			infoTable.AddRow("Intrinsic", Mem.SizeOf<T>());
			infoTable.AddRow("Auto", Mem.SizeOf(value, SizeOfOptions.Auto));

			infoTable.Write(ConsoleTableFormat.Minimal);

			/*
			 *
			 */

			var propTable = new ConsoleTable("Runtime info", "");

			propTable.AddRow("Pinnable", RuntimeInfo.IsPinnable(value));
			propTable.AddRow("Blittable", RuntimeInfo.IsBlittable(value));
			propTable.AddRow("Boxed", RuntimeInfo.IsBoxed(value));
			propTable.AddRow("Nil", RuntimeInfo.IsNil(value));
			propTable.AddRow("Uninitialized", RuntimeInfo.IsUninitialized(value));

			propTable.Write(ConsoleTableFormat.Minimal);
		}

		public static void DumpSizes<T>(ref T value)
		{
			var layoutTable = new ConsoleTable("Size Type", "Value");

			var options = Enum.GetValues<SizeOfOptions>().ToList();
			options.Remove(SizeOfOptions.Heap);


			foreach (var option in options) {
				var sizeOf = Mem.SizeOf<T>(value, option);

				if (sizeOf == Native.INVALID) {
					continue;
				}

				layoutTable.AddRow(Enum.GetName(option), sizeOf);
			}

			var mt = value.GetMetaType();

			if (!mt.RuntimeType.IsValueType) {
				layoutTable.AddRow(nameof(SizeOfOptions.Heap), Mem.SizeOf<T>(value, SizeOfOptions.Heap));
			}

			layoutTable.Write();
		}

		public static void DumpLayout<T>(ref T value, InspectorOptions options = DEFAULT)
		{
			var layoutTable = new ConsoleTable();

			var flags = Enums.GetSetFlags(options);

			if (options == InspectorOptions.All) {
				flags.Remove(InspectorOptions.All);

			}

			layoutTable.AddColumn(flags.Select(Enum.GetName));


			// Rewrite options

			options = default;

			foreach (var flag in flags) {
				options |= flag;
			}

			var mt     = value.GetMetaType();
			var fields = mt.Fields.Where(x => !x.IsStatic);

			int s = Mem.SizeOf(value, SizeOfOptions.Auto);
			var p = Mem.AddressOf(ref value);


			foreach (var metaField in fields) {

				var rowValues = new List<object>();


				if (options.HasFlag(InspectorOptions.Offset)) {
					rowValues.Add($"{metaField.Offset:X}");
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
					rowValues.Add(addr.ToString(PointerFormatting.FMT_HEX));
				}

				if (options.HasFlag(InspectorOptions.Value)) {

					object fieldVal;

					if (!mt.RuntimeType.IsConstructedGenericType) {
						fieldVal = metaField.Info.GetValue(value);
					}
					else {
						fieldVal = "?";
					}

					rowValues.Add($"{fieldVal}");
				}

				layoutTable.AddRow(rowValues.ToArray());
			}


			if (value is string str) {
				for (int i = 1; i < str.Length; i++) {
					char c          = str[i];
					var  rowValues  = new List<object>();
					int  offsetBase = (i * sizeof(char));

					if (options.HasFlag(InspectorOptions.Offset)) {
						rowValues.Add($"{offsetBase + RuntimeInfo.StringOverhead - sizeof(char):X}");
					}

					if (options.HasFlag(InspectorOptions.Size)) {
						rowValues.Add(sizeof(char));
					}

					if (options.HasFlag(InspectorOptions.Type)) {
						rowValues.Add(nameof(Char));
					}

					if (options.HasFlag(InspectorOptions.Name)) {
						rowValues.Add($"Char #{i + 1}");
					}

					if (options.HasFlag(InspectorOptions.Address)) {

						if (Mem.TryGetAddressOfHeap(value, OffsetOptions.StringData, out var addr)) {
							addr += offsetBase;
							rowValues.Add(addr.ToString(PointerFormatting.FMT_HEX));

						}


					}

					if (options.HasFlag(InspectorOptions.Value)) {
						var fieldVal = c;
						rowValues.Add($"{fieldVal}");
					}

					layoutTable.AddRow(rowValues.ToArray());
				}
			}


			

			layoutTable.Write();
		}


		public static void DumpObject<T>(ref T value) =>
			DumpLayout(ref value, InspectorOptions.Name | InspectorOptions.Value);

		#region Native

		public static void DumpSections(IntPtr hModule)
		{
			var s = Native.GetPESectionInfo(hModule);

			var table = new ConsoleTable("Number", "Name", "Address", "Size", "Characteristics");

			foreach (var info in s) {
				table.AddRow(info.Number, info.Name, $"{info.Address.ToInt64():X}", info.Size, info.Characteristics);
			}

			table.Write();
		}

		#endregion
	}
}