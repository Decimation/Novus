using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable UnusedMember.Global

namespace Novus.Utilities
{
	public static class MathOperations
	{
		/// <summary>
		/// SI
		/// </summary>
		public const double MAGNITUDE = 1000D;

		/// <summary>
		/// ISO/IEC 80000
		/// </summary>
		public const double MAGNITUDE2 = 1024D;

		/// <summary>
		/// Convert the given bytes to <see cref="MetricUnit"/>
		/// </summary>
		/// <param name="bytes">Value in bytes to be converted</param>
		/// <param name="type">Unit to convert to</param>
		/// <returns>Converted bytes</returns>
		public static double ConvertToUnit(double bytes, MetricUnit type)
		{
			// var rg  = new[] { "k","M","G","T","P","E","Z","Y"};
			// var pow = rg.ToList().IndexOf(type) +1;

			int pow = (int) type;
			return bytes / Math.Pow(MAGNITUDE, pow);
		}
	}
}
