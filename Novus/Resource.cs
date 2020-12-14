using Novus.Interop;
using Novus.Memory;
using Novus.Properties;
using Novus.Utilities;
using SimpleCore.Diagnostics;
using System;
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
		public string ModuleName { get; }

		public ProcessModule Module { get; }

		public SigScanner Scanner { get; }

		public Resource(string moduleName)
		{
			ModuleName = moduleName;

			var module = Mem.FindModule(moduleName);

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
		public static void LoadImports(Type t)
		{
			Debug.WriteLine($"Loading {t.Name}");

			var annotatedTuples = t.GetAnnotated<ImportAttribute>();

			foreach (var (attribute, member) in annotatedTuples) {
				var    field      = (FieldInfo) member;
				object fieldValue = null;

				switch (attribute) {
					case ImportUnmanagedFunctionAttribute unmanagedAttr:
					{
						Guard.Assert(unmanagedAttr.ImportType == ImportType.Unmanaged);

						// Get signature

						var sig = (string) EmbeddedResources.ResourceManager
							.GetObject(unmanagedAttr.Name);
						Guard.AssertNotNull(sig);

						// Get resource

						string mod = unmanagedAttr.ModuleName;

						Resource resource;

						// NOTE: Unique case for CLR
						if (mod == Global.CLR_MODULE && unmanagedAttr is ImportClrFunctionAttribute) {
							resource = Global.Clr;
						}
						else {
							resource = new Resource(mod);
						}

						// Find signature address

						var addr = resource.Scanner.FindSignature(sig);
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

				Debug.WriteLine($"{attribute.Name} with {fieldValue} -> {member.Name}");

				// Set value

				field.SetValue(null, fieldValue);
			}
		}
	}
}