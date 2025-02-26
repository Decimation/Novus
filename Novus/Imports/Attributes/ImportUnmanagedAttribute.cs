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

	public ImportType Type { get; set; }

	/// <summary>
	/// Import value:
	/// 
	/// <list type="bullet">
	/// <item><see cref="ImportType.Signature"/>: <c>X1 X2 Xn...</c> format where <c>X</c> is an unsigned byte value. <c>?</c> indicates wildcard</item>
	/// <item><see cref="ImportType.Offset"/>: hexadecimal offset value</item>
	/// <item><see cref="ImportType.Symbol"/>: Symbol name</item>
	/// <item><see cref="ImportType.Export"/>: Export name</item>
	/// </list>
	/// </summary>
	[CBN]
	public string Value { get; set; }

	public ImportUnmanagedAttribute(string moduleName, ImportType type, string value = null)
		: this(moduleName, null, type, value) { }

	public ImportUnmanagedAttribute(string moduleName, string name, ImportType type,
	                                string value = null, Type resolver = null)
		: base(name, ImportManageType.Unmanaged, resolver)
	{
		ModuleName = moduleName;
		Type       = type;
		Value      = value;
	}
}

public enum ImportType
{
	Signature,
	Offset,
	Symbol,
	Export
}