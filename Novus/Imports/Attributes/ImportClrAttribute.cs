using System;
using JetBrains.Annotations;

namespace Novus.Imports.Attributes;

/// <summary>
///     Describes an imported unmanaged CLR function. Shortcut for <see cref="ImportUnmanagedAttribute" /> for
///     <see cref="Global.CLR_MODULE" />
/// </summary>
/// <remarks>For use with <seealso cref="RuntimeResource.LoadImports" /></remarks>
[MIU]
[AttributeUsage(AttributeTargets.Field)]
public sealed class ImportClrAttribute : ImportUnmanagedAttribute
{

	public ImportClrAttribute(string name = null, ImportType unmanagedType = ImportType.Signature, Type resolver = null)
		: base(Global.CLR_MODULE, name, unmanagedType, resolver: resolver) { }

}