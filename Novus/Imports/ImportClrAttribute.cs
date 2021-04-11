using System;

namespace Novus.Imports
{
	/// <summary>
	///     Describes an imported unmanaged CLR function. Shortcut for <see cref="ImportUnmanagedAttribute" /> for
	///     <see cref="Global.CLR_MODULE" />
	/// </summary>
	/// <remarks>For use with <seealso cref="Resource.LoadImports" /></remarks>
	[AttributeUsage(AttributeTargets.Field)]
	public class ImportClrAttribute : ImportUnmanagedAttribute
	{
		public ImportClrAttribute(string name = null, UnmanagedType unmanagedType = UnmanagedType.Signature)
			: base(Global.CLR_MODULE, name, unmanagedType) { }
	}
}