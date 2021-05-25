using System;
using JetBrains.Annotations;

// ReSharper disable UnusedMember.Global

namespace Novus.Imports
{
	/// <summary>
	///     Describes an imported unmanaged function.
	///     The <see cref="ImportAttribute.Name" /> is the name (key) with which to look up in <see cref="Resource" />
	///     for the signature to scan using <see cref="Resource.Scanner" />.
	/// </summary>
	/// <remarks>For use with <seealso cref="Resource.Load" /></remarks>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Field)]
	public class ImportUnmanagedAttribute : ImportAttribute
	{
		public string ModuleName { get; set; }

		public UnmanagedImportType UnmanagedType { get; set; }

		[CanBeNull]
		public string Value { get; set; }

		public ImportUnmanagedAttribute(string moduleName, UnmanagedImportType unmanagedType, string value = null)
			: this(moduleName, null, unmanagedType, value) { }

		public ImportUnmanagedAttribute(string moduleName, string name, UnmanagedImportType unmanagedType,
		                                string value = null) : base(name, ImportManageType.Unmanaged)
		{
			ModuleName    = moduleName;
			UnmanagedType = unmanagedType;
			Value         = value;
		}
	}

	public enum UnmanagedImportType
	{
		Signature,
		Offset,
		Symbol
	}
}