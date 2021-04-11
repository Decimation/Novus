using System;

namespace Novus.Imports
{
	/// <summary>
	///     Describes an imported managed function.
	///     The <see cref="ImportAttribute.Name" /> is the name of the managed function which is enclosed by
	///     <see cref="ImportManagedAttribute.Type" />.
	/// </summary>
	/// <remarks>For use with <seealso cref="Resource.LoadImports" /></remarks>
	[AttributeUsage(AttributeTargets.Field)]
	public class ImportManagedAttribute : ImportAttribute
	{
		public ImportManagedAttribute(Type type, string name = null) : base(name, ManageType.Managed)
		{
			Type = type;

		}


		public Type Type { get; set; }
	}
}