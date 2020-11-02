using System.Collections.Generic;

namespace Novus
{
	public static class Inspector
	{
		/// <summary>
		///     Determines whether the value of <paramref name="value" /> is <c>default</c> or <c>null</c> bytes,
		///     or <paramref name="value" /> is <c>null</c>
		///     <remarks>"Nil" is <c>null</c> or <c>default</c>.</remarks>
		/// </summary>
		public static bool IsNil<T>(T value)
		{
			return EqualityComparer<T>.Default.Equals(value, default);
		}
	}
}
