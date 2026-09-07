using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

// ReSharper disable UnusedMember.Global

namespace Novus.Imports.Attributes;

/// <summary>
///     Describes an imported member.
/// </summary>
/// <remarks>For use with <seealso cref="RuntimeResource" /></remarks>
[MIU(ImplicitUseTargetFlags.WithMembers | ImplicitUseTargetFlags.WithInheritors)]
[AttributeUsage(AttributeTargets.Field  | AttributeTargets.Property)]
public abstract class ImportAttribute : Attribute
{
	protected ImportAttribute(string name, ImportManageType manageType)
	{
		Name       = name;
		ManageType = manageType;
	}

	protected ImportAttribute(ImportManageType manageType) : this(null, manageType) { }

	/// <summary>
	/// Name of the import, if applicable. If <c>null</c>, the name of the annotated member will be used.
	/// </summary>
	[CBN]
	public string Name { get; set; }

	/// <summary>
	/// Import value (<see cref="ImportType"/>):
	/// <list type="bullet">
	/// <item><see cref="ImportType.Signature"/>: <br />
	/// <c>X1 X2 Xn...</c> format where <c>X</c> is an unsigned byte value;
	/// <c>?</c> indicates wildcard</item>
	/// <item><see cref="ImportType.Offset"/>: hexadecimal offset value</item>
	/// <item><see cref="ImportType.Symbol"/>: Symbol name</item>
	/// <item><see cref="ImportType.Export"/>: Export name</item>
	/// </list>
	/// </summary>
	[CBN]
	public string Value { get; set; }

	public ImportManageType ManageType { get; }

	public bool AbsoluteMatch { get; set; }

	public ImportType ImportType { get; set; }

}

public enum ImportManageType
{
	Unmanaged,
	Managed
}

public enum ImportType
{

	/// <seea cref="RuntimeResource.GetSignature"/>
	Signature,

	/// <seealso cref="RuntimeResource.GetOffset"/>
	Offset,

	/// <seealso cref="RuntimeResource.GetSymbol"/>
	Symbol,

	/// <seealso cref="RuntimeResource.GetExport"/>
	Export

}