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
	/// <seealso cref="ReflectionHelper"/>
	/// <seealso cref="Mem"/>
	/// <seealso cref="RuntimeInfo"/>
	/// </summary>
	public static unsafe class Inspector
	{
		/// <summary>
		///     Determines whether the value of <paramref name="value" /> is <c>default</c> or <c>null</c> bytes,
		///     or <paramref name="value" /> is <c>null</c>
		///     <remarks>"Nil" is <c>null</c> or <c>default</c>.</remarks>
		/// </summary>
		public static bool IsNil<T>(T value) => EqualityComparer<T>.Default.Equals(value, default);

		public static bool IsStruct<T>(T value) => value.GetType().IsValueType;

		public static bool IsArray<T>(T value) => value is Array;

		public static bool IsString<T>(T value) => value is string;

		/// <summary>
		/// Determines whether <paramref name="value"/> is boxed.
		/// </summary>
		/// <param name="value">Value to test for</param>
		/// <typeparam name="T">Type of <paramref name="value"/></typeparam>
		/// <returns><c>true</c> if <paramref name="value"/> is boxed; <c>false</c> otherwise</returns>
		public static bool IsBoxed<T>([CanBeNull] T value)
		{
			return (typeof(T).IsInterface || typeof(T) == typeof(object)) && value != null && IsStruct(value);
		}

		public static bool IsPinnable([CanBeNull] object o)
		{
			var b = Functions.Func_IsPinnable(o);

			return b;
		}



		public static void DumpLayout<T>(T t = default)
		{
			var sb = new StringBuilder();
			var mt = t.GetType().AsMetaType();
			var f  = mt.Fields.Where(x=>!x.IsStatic);
			var s  = Mem.SizeOf<T>(t, SizeOfOptions.Auto);

			sb.AppendLine($"{mt.Name} ({s}):\n");

			foreach (var metaField in f) {
				sb.AppendLine($"0x{metaField.Offset:X} | {metaField.Size} | ({metaField.FieldType.Name}) {metaField.Name}");
			}


			Console.WriteLine(sb);
		}
	}
}