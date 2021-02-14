using System;

namespace Novus.Imports
{
	/// <summary>
	///     Describes an imported unmanaged function.
	///     The <see cref="ImportAttribute.Name" /> is the name (key) with which to look up in <see cref="Resource" />
	///     for the signature to scan using <see cref="Resource.Scanner" />.
	/// </summary>
	/// <remarks>For use with <seealso cref="Resource.LoadImports" /></remarks>
	[AttributeUsage(AttributeTargets.Field)]
	public class ImportUnmanagedComponentAttribute : ImportAttribute
	{
		public string ModuleName { get; set; }

		public UnmanagedType UnmanagedType { get; set; }

		public ImportUnmanagedComponentAttribute(string moduleName, string name, UnmanagedType unmanagedType) 
			: base(name, ManageType.Unmanaged)
		{
			ModuleName  = moduleName;
			UnmanagedType = unmanagedType;
		}
	}

	public enum UnmanagedType
	{
		Signature,
		Offset,
	}
}