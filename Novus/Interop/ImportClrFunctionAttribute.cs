using System;

namespace Novus.Interop
{
	/// <summary>
	///     Describes an imported unmanaged CLR function. Shortcut for <see cref="ImportUnmanagedFunctionAttribute" /> for
	///     <see cref="Global.CLR_MODULE" />
	/// </summary>
	/// <remarks>For use with <seealso cref="Resource.LoadImports" /></remarks>
	[AttributeUsage(AttributeTargets.Field)]
	public class ImportClrFunctionAttribute : ImportUnmanagedFunctionAttribute
	{
		public ImportClrFunctionAttribute(string name) : base(Global.CLR_MODULE, name) { }
	}
}