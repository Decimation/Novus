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
using Kantan.Model;
using Kantan.Utilities;

// ReSharper disable ConvertIfStatementToConditionalTernaryExpression

// ReSharper disable UnusedMember.Global

// ReSharper disable CognitiveComplexity

namespace Novus.Runtime;

// TODO: UPDATE

/// <summary>
///     Utilities for inspecting and analyzing objects, data, etc.
/// </summary>
/// 
/// <seealso cref="Mem" />
/// <seealso cref="RuntimeProperties" />
/// <seealso cref="Utilities.ReflectionHelper" />
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

	public enum DumpOptions
	{
		Layout,
		Info,
		Sizes,
	}

	/*public static string Dump<T>(ref T value, DumpOptions options, InspectorOptions options2)
	{
		switch (options) {

			case DumpOptions.Info:
				//

				var addrTable = new ConsoleTable("Addresses", "");

				var addr = Mem.AddressOf(ref value);
				addrTable.AddRow("Address", addr);

				if (Mem.TryGetAddressOfHeap(value, out var heap)) {
					addrTable.AddRow("Address (heap)", heap);
				}

				addrTable.Write(ConsoleTableFormat.Minimal);

				//

				var infoTable = new ConsoleTable("Sizes", "");

				infoTable.AddRow("Intrinsic", Mem.SizeOf<T>());
				infoTable.AddRow("Auto", Mem.SizeOf(value, SizeOfOptions.Auto));

				infoTable.Write(ConsoleTableFormat.Minimal);

				//

				var propTable = new ConsoleTable("Runtime info", "");

				propTable.AddRow("Pinnable", RuntimeProperties.IsPinnable(value));
				propTable.AddRow("Blittable", RuntimeProperties.IsBlittable(value));
				propTable.AddRow("Boxed", RuntimeProperties.IsBoxed(value));
				propTable.AddRow("Nil", RuntimeProperties.IsDefault(value));
				propTable.AddRow("Uninitialized", RuntimeProperties.IsNull(value));

				propTable.AddRow("In GC heap", GCHeap.IsHeapPointer(Mem.AddressOfData(ref value)));

				return propTable.ToMinimalString();
			case DumpOptions.Sizes:
				var layoutTable = new ConsoleTable("Size Type", "Value");

				var options1 = Enum.GetValues<SizeOfOptions>().ToList();

				options1.Remove(SizeOfOptions.Heap);

				foreach (var option in options1) {
					var sizeOf = Mem.SizeOf(value, option);

					if (sizeOf == Native.INVALID) {
						continue;
					}

					layoutTable.AddRow(Enum.GetName(option), sizeOf);
				}

				var mt = value.GetMetaType();

				if (!mt.RuntimeType.IsValueType) {
					layoutTable.AddRow(nameof(SizeOfOptions.Heap), Mem.SizeOf(value, SizeOfOptions.Heap));
				}

				return layoutTable.ToString();
			case DumpOptions.Layout:
				var layoutTable1 = new ConsoleTable();

				var flags = EnumHelper.GetSetFlags(options2);

				if (options2 == InspectorOptions.All) {
					flags.Remove(InspectorOptions.All);

				}

				layoutTable1.AddColumn(flags.Select(Enum.GetName));

				// Rewrite options

				options2 = default;

				foreach (var flag in flags) {
					options2 |= flag;
				}

				var mt1    = value.GetMetaType();
				var fields = mt1.Fields.Where(x => !x.IsStatic);

				int s = Mem.SizeOf(value, SizeOfOptions.Auto);
				var p = Mem.AddressOf(ref value);

				foreach (var metaField in fields) {

					var rowValues = new List<object>();

					if (options2.HasFlag(InspectorOptions.Offset)) {
						rowValues.Add($"{metaField.Offset:X}");
					}

					if (options2.HasFlag(InspectorOptions.Size)) {
						rowValues.Add(metaField.Size);
					}

					if (options2.HasFlag(InspectorOptions.Type)) {
						rowValues.Add(metaField.FieldType.Name);
					}

					if (options2.HasFlag(InspectorOptions.Name)) {
						rowValues.Add(metaField.Name);
					}

					if (options2.HasFlag(InspectorOptions.Address)) {
						var addr1 = Mem.AddressOfData(ref value) + metaField.Offset;
						rowValues.Add(addr1.ToString());
					}

					if (options2.HasFlag(InspectorOptions.Value)) {

						object fieldVal;

						if (!mt1.RuntimeType.IsConstructedGenericType) {
							fieldVal = metaField.Info.GetValue(value);
						}
						else {
							fieldVal = "?";
						}

						rowValues.Add($"{fieldVal}");
					}

					layoutTable1.AddRow(rowValues.ToArray());
				}

				if (value is string str) {
					for (int i = 1; i < str.Length; i++) {
						char c          = str[i];
						var  rowValues  = new List<object>();
						int  offsetBase = (i * sizeof(char));

						if (options2.HasFlag(InspectorOptions.Offset)) {
							rowValues.Add($"{offsetBase + RuntimeProperties.StringOverhead - sizeof(char):X}");
						}

						if (options2.HasFlag(InspectorOptions.Size)) {
							rowValues.Add(sizeof(char));
						}

						if (options2.HasFlag(InspectorOptions.Type)) {
							rowValues.Add(nameof(Char));
						}

						if (options2.HasFlag(InspectorOptions.Name)) {
							rowValues.Add($"Char #{i + 1}");
						}

						if (options2.HasFlag(InspectorOptions.Address)) {

							if (Mem.TryGetAddressOfHeap(value, OffsetOptions.StringData, out var addr2)) {
								addr2 += offsetBase;
								rowValues.Add(addr2.ToString());

							}

						}

						if (options2.HasFlag(InspectorOptions.Value)) {
							rowValues.Add($"{c}");
						}

						layoutTable1.AddRow(rowValues.ToArray());
					}
				}

				return layoutTable1.ToString();
			default:
				throw new ArgumentOutOfRangeException(nameof(options));
		}

	}*/
}