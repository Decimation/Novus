using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Global
#nullable enable
namespace Novus.Utilities
{
	public static class StreamExtensions
	{
		public static string[] ReadAllLines(this StreamReader stream)
		{
			var list = new List<string>();

			while (!stream.EndOfStream) {
				string? line = stream.ReadLine();

				if (line != null) {
					list.Add(line);
				}
			}

			return list.ToArray();
		}
	}
}