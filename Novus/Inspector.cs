using System;
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
	/// Utilities for inspecting and analyzing data.
	/// </summary>
	/// <seealso cref="ReflectionHelper"/>
	/// <seealso cref="Mem"/>
	/// <seealso cref="RuntimeInfo"/>
	public static unsafe class Inspector
	{
		/// <summary>
		///     Determines whether the value of <paramref name="value" /> is <c>nil</c>.
		///     <remarks><c>Nil</c> is <c>null</c> or <c>default</c>.</remarks>
		/// </summary>
		public static bool IsNil<T>([NotNull] T value)
		{
			return EqualityComparer<T>.Default.Equals(value, default);
		}


		public static bool IsStruct<T>([NotNull] T value)
		{
			return value.GetType().IsValueType;
		}

		public static bool IsArray<T>([NotNull] T value)
		{
			return value is Array;
		}

		public static bool IsString<T>([NotNull] T value)
		{
			return value is string;
		}

		/// <summary>
		/// Determines whether <paramref name="value"/> is boxed.
		/// </summary>
		public static bool IsBoxed<T>([CanBeNull] T value)
		{
			return (typeof(T).IsInterface || typeof(T) == typeof(object)) && value != null && IsStruct(value);
		}

		/// <summary>
		/// Determines whether <paramref name="value"/> is pinnable.
		/// </summary>
		public static bool IsPinnable([CanBeNull] object value)
		{
			bool b = Functions.Func_IsPinnable(value);

			return b;
		}


		public static void DumpInfo<T>([NotNull] T t)
		{
			var sb = new StringBuilder();

			var addr = Mem.AddressOf(ref t);
			sb.AppendFormat("Address: {0}\n", addr);

			if (Mem.TryGetAddressOfHeap(t, out var heap)) {
				sb.AppendFormat("Address (heap): {0}\n", heap);
			}

			sb.AppendFormat("Pinnable: {0}\n", IsPinnable(t));
			sb.AppendFormat("Boxed: {0}\n", IsBoxed(t));
			sb.AppendFormat("Nil: {0}\n", IsNil(t));

			var type = t.GetType().AsMetaType();


			Console.WriteLine(sb);
		}

		public static void DumpLayout<T>(T t)
		{
			var sb = new StringBuilder();
			var mt = t.GetType().AsMetaType();
			var f  = mt.Fields.Where(x => !x.IsStatic);
			var s  = Mem.SizeOf<T>(t, SizeOfOptions.Auto);

			sb.AppendLine($"{mt.Name} ({s}):\n");

			foreach (var metaField in f) {
				sb.AppendLine(
					$"0x{metaField.Offset:X} | {metaField.Size} | ({metaField.FieldType.Name}) {metaField.Name}");
			}


			Console.WriteLine(sb);
		}
	}
}