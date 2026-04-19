using System;
using JetBrains.Annotations;

namespace Novus.Imports.Attributes;

/// <summary>
///     Describes an imported managed function.
///     The <see cref="ImportAttribute.Name" /> is the name of the managed function which is enclosed by
///     <see cref="Source" />.
/// </summary>
/// <remarks>For use with <seealso cref="RuntimeResource.LoadImports" /></remarks>
[MIU]
[AttributeUsage(AttributeTargets.Field)]
public class ImportManagedAttribute : ImportAttribute
{

	public ImportManagedAttribute(Type source, string name = null) : base(name, ImportManageType.Managed)
	{
		Source = source;
	}

	public Type Source { get; set; }

}