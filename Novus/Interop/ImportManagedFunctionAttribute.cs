using System;

namespace Novus.Interop
{
	/// <summary>
	///     Describes an imported managed function.
	///     The <see cref="ImportAttribute.Name" /> is the name of the managed function which is enclosed by
	///     <see cref="ImportManagedFunctionAttribute.Type" />.
	/// </summary>
	/// <remarks>For use with <seealso cref="Resource.LoadImports" /></remarks>
	[AttributeUsage(AttributeTargets.Field)]
	public class ImportManagedFunctionAttribute : ImportAttribute
	{
		public ImportManagedFunctionAttribute(Type type, string name) : base(name, ImportType.Managed)
		{
			Type = type;

		}

		public Type Type { get; set; }
	}
}