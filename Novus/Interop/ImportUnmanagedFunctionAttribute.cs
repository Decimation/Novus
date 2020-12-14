using System;
using Novus.Properties;

namespace Novus.Interop
{
	/// <summary>
	///     Describes an imported unmanaged function.
	///     The <see cref="ImportAttribute.Name" /> is the name (key) with which to look up in <see cref="EmbeddedResources" />
	///     for the signature to scan using <see cref="Resource.Scanner" />.
	/// </summary>
	/// <remarks>For use with <seealso cref="Resource.LoadImports" /></remarks>
	[AttributeUsage(AttributeTargets.Field)]
	public class ImportUnmanagedFunctionAttribute : ImportAttribute
	{
		// todo: only works for Clr


		public ImportUnmanagedFunctionAttribute(string name) : base(name, ImportType.Unmanaged) { }
	}
}