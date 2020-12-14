using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Novus.Properties;

namespace Novus.Interop
{
	/// <summary>
	///     Describes an imported unmanaged function.
	///     The <see cref="ImportAttribute.Name" /> is the name (key) with which to look up in <see cref="Resource" />
	///     for the signature to scan using <see cref="Resource.Scanner" />.
	/// </summary>
	/// <remarks>For use with <seealso cref="Resource.LoadImports" /></remarks>
	[AttributeUsage(AttributeTargets.Field)]
	public class ImportUnmanagedFunctionAttribute : ImportAttribute
	{
		

		public string ModuleName { get; set; }

		public ImportUnmanagedFunctionAttribute(string moduleName, string name) : base(name, ImportType.Unmanaged)
		{
			ModuleName = moduleName;
		}
	}
}
