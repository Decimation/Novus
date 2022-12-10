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
    protected ImportAttribute(string name, ImportManageType manageType)
    {
        Name = name;
        ManageType = manageType;
    }

    protected ImportAttribute(ImportManageType manageType) : this(null, manageType) { }

    [MN]
    public string Name { get; set; }

    public ImportManageType ManageType { get; set; }
}

public enum ImportManageType
{
    Unmanaged,
    Managed
}