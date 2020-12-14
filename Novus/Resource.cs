using Novus.Interop;
using Novus.Memory;
using Novus.Properties;
using Novus.Utilities;
using SimpleCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Novus
{
	/// <summary>
	/// Represents a runtime component which contains data and resources.
	/// </summary>
	/// <seealso cref="EmbeddedResources"/>
	public class Resource
	{
		public string Name { get; }

		public ProcessModule Module { get; }

		public SigScanner Scanner { get; }


		public Resource(string name)
		{
			Name = name;

			var module = Mem.FindModule(name);

			Guard.AssertNotNull(module);

			Module = module;

			Scanner = new SigScanner(Module);
		}

		public override string ToString()
		{
			return String.Format("{0} ({1})", Module.ModuleName, Scanner.Address);
		}

		/// <summary>
		/// Loads imported values for members annotated with <see cref="ImportAttribute"/>.
		/// </summary>
		/// <param name="t">Enclosing type</param>
		public void LoadImports(Type t)
		{
			var rg = t.GetAnnotated<ImportAttribute>();

			foreach (var (attribute, member) in rg) {
				var    field      = (FieldInfo) member;
				object fieldValue = null;

				switch (attribute) {
					case ImportUnmanagedFunctionAttribute unmanagedAttr:
					{
						Guard.Assert(unmanagedAttr.ImportType == ImportType.Unmanaged);

						// Get signature

						var sig = (string?) EmbeddedResources.ResourceManager
							.GetObject(unmanagedAttr.Name);
						Guard.AssertNotNull(sig);

						// Find signature address

						var addr = Scanner.FindSignature(sig);
						Guard.Assert(!addr.IsNull);

						fieldValue = (IntPtr) addr;
						break;
					}
					case ImportManagedFunctionAttribute managedAttr:
					{
						Guard.Assert(managedAttr.ImportType == ImportType.Managed);

						var fn = managedAttr.Type.GetAnyMethod(managedAttr.Name);

						var ptr = fn.MethodHandle.GetFunctionPointer();

						fieldValue = ptr;
						break;
					}
				}

				Debug.WriteLine("{0} -> {1} @ {2}", member.Name, attribute.Name, fieldValue);

				// Set value

				field.SetValue(null, fieldValue);
			}
		}
	}
}