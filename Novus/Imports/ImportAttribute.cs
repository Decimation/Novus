using System;
using JetBrains.Annotations;
// ReSharper disable UnusedMember.Global

namespace Novus.Imports
{
	/// <summary>
	///     Describes an imported member.
	/// </summary>
	/// <remarks>For use with <seealso cref="Resource.LoadImports" /></remarks>
	[AttributeUsage(AttributeTargets.Field)]
	[MeansImplicitUse(ImplicitUseTargetFlags.WithMembers | ImplicitUseTargetFlags.WithInheritors)]
	public abstract class ImportAttribute : Attribute
	{
		protected ImportAttribute(string name, ManageType manageType)
		{
			Name       = name;
			ManageType = manageType;
		}

		protected ImportAttribute(ManageType manageType) : this(null, manageType) { }

		[CanBeNull]
		public string Name { get; set; }

		public ManageType ManageType { get; set; }
	}

	public enum ManageType
	{
		Unmanaged,
		Managed
	}
}