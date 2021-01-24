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
		public ImportAttribute(string name, ManageType manageType)
		{
			Name       = name;
			ManageType = manageType;
		}

		public string Name { get; set; }

		public ManageType ManageType { get; set; }
	}

	public enum ManageType
	{
		Unmanaged,
		Managed
	}
}