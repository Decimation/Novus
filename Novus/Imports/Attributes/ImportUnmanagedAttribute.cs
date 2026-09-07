using System;
using JetBrains.Annotations;

// ReSharper disable UnusedMember.Global

namespace Novus.Imports.Attributes;

/// <summary>
///     Describes an imported unmanaged function.
///     The <see cref="ImportAttribute.Name" /> is the name (key) with which to look up in <see cref="RuntimeResource" />
///     for the signature to scan using <see cref="RuntimeResource.Scanner" />.
/// </summary>
/// <remarks>For use with <seealso cref="RuntimeResource.LoadImports" /></remarks>
[MIU]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ImportUnmanagedAttribute : ImportAttribute
{

	public string ModuleName { get; set; }

	public ImportUnmanagedAttribute(string moduleName, ImportType importType, string value = null)
		: this(moduleName, null, importType, value) { }

	public ImportUnmanagedAttribute(string moduleName, string name, ImportType importType, string value = null)
		: base(name, ImportManageType.Unmanaged)
	{
		ModuleName = moduleName;
		ImportType = importType;
		Value      = value;
	}

}