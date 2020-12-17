using System;
using Novus.Win32.Structures;

namespace Novus.Win32.Wrappers
{
	/// <summary>
	/// Wraps an <see cref="ImageSectionHeader"/>
	/// </summary>
	public class ImageSectionInfo
	{
		public string Name { get; }

		public int Number { get; }

		public IntPtr Address { get; }

		public int Size { get; }

		public ImageSectionCharacteristics Characteristics { get; }

		internal ImageSectionInfo(ImageSectionHeader struc, int number, IntPtr address)
		{
			Number          = number;
			Name            = struc.Name;
			Address         = address;
			Size            = (int) struc.VirtualSize;
			Characteristics = struc.Characteristics;
		}

		public override string ToString()
		{
			return String.Format("Number: {0} | Name: {1} | Address: {2:X} | Size: {3} | Characteristics: {4}", Number,
				Name,
				Address.ToInt64(), Size, Characteristics);
		}
	}
}