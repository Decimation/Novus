using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Novus.CoreClr;
using Novus.Utilities;

namespace Novus
{
	/// <summary>
	/// <seealso cref="ReflectionHelper"/>
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
			var b = ClrFunctions.Func_IsPinnable(o);

			return b;
		}
	}
}