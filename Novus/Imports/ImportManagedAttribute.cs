using System;
using JetBrains.Annotations;

namespace Novus.Imports;

/// <summary>
///     Describes an imported managed function.
///     The <see cref="ImportAttribute.Name" /> is the name of the managed function which is enclosed by
///     <see cref="ImportManagedAttribute.Type" />.
/// </summary>
/// <remarks>For use with <seealso cref="Resource.LoadImports" /></remarks>
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