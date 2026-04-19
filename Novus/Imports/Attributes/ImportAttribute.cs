using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

// ReSharper disable UnusedMember.Global

namespace Novus.Imports.Attributes;

/// <summary>
///     Describes an imported member. <br />
/// <see cref="RuntimeResource"/>
/// </summary>
/// <remarks>For use with <seealso cref="RuntimeResource.LoadImports" /></remarks>
[AttributeUsage(AttributeTargets.Field)]
[MIU(ImplicitUseTargetFlags.WithMembers | ImplicitUseTargetFlags.WithInheritors)]
public abstract class ImportAttribute : Attribute
{
	protected ImportAttribute(string name, ImportManageType manageType)
	{
		Name       = name;
		ManageType = manageType;
	}

	protected ImportAttribute(ImportManageType manageType) : this(null, manageType) { }

	[MN]
	[CanBeNull]
	public string Name { get; set; }

	public ImportManageType ManageType { get; }

	public bool AbsoluteMatch { get; set; }

	public ImportType Type { get; set; }

	/// <summary>
	/// Import value (<see cref="Type"/>):
	/// <list type="bullet">
	/// <item><see cref="ImportType.Signature"/>: <c>X1 X2 Xn...</c> format where <c>X</c> is an unsigned byte value. <br />
	/// <c>?</c> indicates wildcard</item>
	/// <item><see cref="ImportType.Offset"/>: hexadecimal offset value</item>
	/// <item><see cref="ImportType.Symbol"/>: Symbol name</item>
	/// <item><see cref="ImportType.Export"/>: Export name</item>
	/// </list>
	/// </summary>
	[CBN]
	public string Value { get; set; }

}

public enum ImportManageType
{
	Unmanaged,
	Managed
}