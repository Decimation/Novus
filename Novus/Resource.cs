using Novus.Interop;
using Novus.Memory;
using Novus.Properties;
using Novus.Utilities;
using SimpleCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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

		public Pointer<byte> Address { get; }

		public Resource(string moduleName)
		{
			ModuleName = moduleName;

			var module = Mem.FindModule(moduleName);

			Guard.AssertNotNull(module);

			Module = module;

			Scanner = new SigScanner(Module);

			Address = Module.BaseAddress;
		}


		public override string ToString()
		{
			return $"{Module.ModuleName} ({Scanner.Address})";
		}

		private static List<Type> Loaded { get; } = new();

		/// <summary>
		/// Loads imported values for members annotated with <see cref="ImportAttribute"/>.
		/// </summary>
		/// <param name="t">Enclosing type</param>
		public static void LoadImports(Type t)
		{
			if (Loaded.Contains(t)) {
				return;
			}

			Debug.WriteLine($"[info] Loading {t.Name}");

			var annotatedTuples = t.GetAnnotated<ImportAttribute>();

			foreach (var (attribute, member) in annotatedTuples) {
				var    field      = (FieldInfo) member;
				object fieldValue = null;

				switch (attribute) {
					case ImportUnmanagedComponentAttribute unmanagedAttr:
					{
						Guard.Assert(unmanagedAttr.ManageType == ManageType.Unmanaged);

						// Get signature

						var sig = (string) EmbeddedResources.ResourceManager
							.GetObject(unmanagedAttr.Name);

						Guard.AssertNotNull(sig);

						// Get resource

						string mod = unmanagedAttr.ModuleName;
						var    rt  = unmanagedAttr.UnmanagedType;

						Resource resource;

						// NOTE: Unique case for CLR
						if (mod == Global.CLR_MODULE && unmanagedAttr is ImportClrComponentAttribute) {
							resource = Global.Clr;
						}
						else {
							resource = new Resource(mod);
						}

						// Find address

						var addr = rt switch
						{
							UnmanagedType.Signature => resource.Scanner.FindSignature(sig),
							UnmanagedType.Offset    => resource.Address + (Int32.Parse(sig, NumberStyles.HexNumber)),
							_                       => throw new ArgumentOutOfRangeException()
						};

						Guard.Assert(!addr.IsNull);


						if (field.FieldType == typeof(Pointer<byte>)) {
							fieldValue = addr;
						}
						else {
							fieldValue = (IntPtr) addr;
						}

						break;
					}

					case ImportManagedComponentAttribute managedAttr:
					{
						Guard.Assert(managedAttr.ManageType == ManageType.Managed);

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

			Loaded.Add(t);
		}
	}
}