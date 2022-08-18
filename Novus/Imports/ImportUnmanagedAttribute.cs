using System;
using JetBrains.Annotations;
using Novus.Runtime;

// ReSharper disable UnusedMember.Global

namespace Novus.Imports;

/// <summary>
///     Describes an imported unmanaged function.
///     The <see cref="ImportAttribute.Name" /> is the name (key) with which to look up in <see cref="RuntimeResource" />
///     for the signature to scan using <see cref="RuntimeResource.Scanner" />.
/// </summary>
/// <remarks>For use with <seealso cref="RuntimeResource.LoadImports" /></remarks>
[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Field)]
public class ImportUnmanagedAttribute : ImportAttribute
{
	public string ModuleName { get; set; }

	public ImportType Type { get; set; }

	[CanBeNull]
	public string Value { get; set; }

	public ImportUnmanagedAttribute(string moduleName, ImportType type, string value = null)
		: this(moduleName, null, type, value) { }

	public ImportUnmanagedAttribute(string moduleName, string name, ImportType type,
	                                string value = null) : base(name, ImportManageType.Unmanaged)
	{
		ModuleName    = moduleName;
		Type = type;
		Value         = value;
	}
}

public enum ImportType
{
	Signature,
	Offset,
	Symbol,
	Export
}