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
[AttributeUsage(AttributeTargets.Field)]
public class ImportUnmanagedAttribute : ImportAttribute
{

	public string ModuleName { get; set; }

	public ImportUnmanagedAttribute(string moduleName, ImportType importType, string value = null)
		: this(moduleName, null, importType, value) { }

	public ImportUnmanagedAttribute(string moduleName, string name, ImportType importType, string value = null)
		: base(name, ImportManageType.Unmanaged)
	{
		ModuleName = moduleName;
		ImportType       = importType;
		Value      = value;
	}

}

public enum ImportType
{

	/// <summary>
	/// <see cref="RuntimeResource.GetSignature"/>
	/// </summary>
	Signature,

	/// <summary>
	/// <see cref="RuntimeResource.GetOffset"/>
	/// </summary>
	Offset,

	/// <summary>
	/// <see cref="RuntimeResource.GetSymbol"/>
	/// </summary>
	Symbol,

	/// <summary>
	/// <see cref="RuntimeResource.GetExport"/>
	/// </summary>
	Export

}