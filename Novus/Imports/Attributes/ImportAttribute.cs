using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

// ReSharper disable UnusedMember.Global

namespace Novus.Imports.Attributes;

/// <summary>
///     Describes an imported member.
/// </summary>
/// <remarks>For use with <seealso cref="RuntimeResource.LoadImports" /></remarks>
[AttributeUsage(AttributeTargets.Field)]
[MeansImplicitUse(ImplicitUseTargetFlags.WithMembers | ImplicitUseTargetFlags.WithInheritors)]
public abstract class ImportAttribute : Attribute
{
	protected ImportAttribute(string name, ImportManageType manageType, int ordinal = ORDINAL_NA)
	{
		Name       = name;
		ManageType = manageType;
		Ordinal    = ordinal;
	}

	protected ImportAttribute(ImportManageType manageType) : this(null, manageType) { }

	[MN]
	public string Name { get; set; }

	public ImportManageType ManageType { get; set; }

	public int Ordinal { get; set; }

	public const int ORDINAL_NA = -1;
}

public enum ImportManageType
{
	Unmanaged,
	Managed
}