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
	protected ImportAttribute(string name, ImportManageType manageType, Type resolver = null)
	{
		Name       = name;
		ManageType = manageType;
		Resolver   = resolver;
	}

	protected ImportAttribute(ImportManageType manageType) : this(null, manageType) { }

	[MN]
	public string Name { get; set; }

	public ImportManageType ManageType { get; set; }

	[CBN]
	public Type Resolver { get; set; }

	public bool AbsoluteMatch { get; set; }
}

public enum ImportManageType
{
	Unmanaged,
	Managed
}