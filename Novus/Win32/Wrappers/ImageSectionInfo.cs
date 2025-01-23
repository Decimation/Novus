using Novus.Win32.Structures.DbgHelp;

namespace Novus.Win32.Wrappers;

/// <summary>
/// Wraps an <see cref="ImageSectionHeader"/>
/// </summary>
public class ImageSectionInfo
{
	public string Name { get; }

	public int Number { get; }

	public nint Address { get; }

	public int Size { get; }

	public ImageSectionCharacteristics Characteristics { get; }

	internal ImageSectionInfo(ImageSectionHeader struc, int number, nint address)
	{
		Number          = number;
		Name            = struc.Name;
		Address         = address;
		Size            = (int) struc.VirtualSize;
		Characteristics = struc.Characteristics;
	}

	public override string ToString()
	{
		return
			$"Number: {Number} | Name: {Name} | Address: {Address.ToInt64():X} | Size: {Size} | "
			+ $"Characteristics: {Characteristics}";
	}
}