using System;
using JetBrains.Annotations;

namespace Novus.Interop
{
	/// <summary>
	///     Describes an imported member.
	/// </summary>
	/// <remarks>For use with <seealso cref="Resource.LoadImports" /></remarks>
	[AttributeUsage(AttributeTargets.Field)]
	[MeansImplicitUse(ImplicitUseTargetFlags.WithMembers | ImplicitUseTargetFlags.WithInheritors)]
	public class ImportAttribute : Attribute
	{
		public ImportAttribute(string name, ImportType importType)
		{
			Name       = name;
			ImportType = importType;
		}

		public string Name { get; set; }

		public ImportType ImportType { get; set; }
	}

	public enum ImportType
	{
		Unmanaged,
		Managed
	}
}