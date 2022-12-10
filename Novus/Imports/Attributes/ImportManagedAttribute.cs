using System;
using JetBrains.Annotations;

namespace Novus.Imports.Attributes;

/// <summary>
///     Describes an imported managed function.
///     The <see cref="ImportAttribute.Name" /> is the name of the managed function which is enclosed by
///     <see cref="Type" />.
/// </summary>
/// <remarks>For use with <seealso cref="RuntimeResource.LoadImports" /></remarks>
[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Field)]
public class ImportManagedAttribute : ImportAttribute
{
    public ImportManagedAttribute(Type type, string name = null) : base(name, ImportManageType.Managed)
    {
        Type = type;

    }

    public Type Type { get; set; }
}