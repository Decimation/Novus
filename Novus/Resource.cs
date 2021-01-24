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



		private static object GetImportValue(ImportAttribute attribute, FieldInfo field)
		{
			object fieldValue = null;

			switch (attribute) {
				case ImportUnmanagedComponentAttribute unmanagedAttr:
				{
					Guard.Assert(unmanagedAttr.ManageType == ManageType.Unmanaged);


					// Get value

					string resValue = (string) EmbeddedResources.ResourceManager
						.GetObject(unmanagedAttr.Name);

					Guard.AssertNotNull(resValue);

					// Get resource

					string mod           = unmanagedAttr.ModuleName;
					var    unmanagedType = unmanagedAttr.UnmanagedType;

					Resource resource;

					// NOTE: Unique case for CLR
					if (mod == Global.CLR_MODULE && unmanagedAttr is ImportClrComponentAttribute) {
						resource = Global.Clr;
					}
					else {
						resource = new Resource(mod);
					}

					// Find address

					var addr = unmanagedType switch
					{
						UnmanagedType.Signature => resource.Scanner.FindSignature(resValue),
						UnmanagedType.Offset    => resource.Address + (Int32.Parse(resValue, NumberStyles.HexNumber)),
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

			return fieldValue;
		}

		/// <summary>
		/// Loads imported values for members annotated with <see cref="ImportAttribute"/>.
		/// </summary>
		/// <param name="t">Enclosing type</param>
		public static void LoadImports(Type t)
		{
			if (Loaded.Contains(t)) {
				return;
			}

			Debug.WriteLine($"[debug] Loading {t.Name}");

			var annotatedTuples = t.GetAnnotated<ImportAttribute>();

			foreach (var (attribute, member) in annotatedTuples) {
				var    field      = (FieldInfo) member;

				var fieldValue = GetImportValue(attribute, field);

				Debug.WriteLine($"[debug] Loading ({attribute.Name}): {fieldValue} -> {member.Name}");

				// Set value

				field.SetValue(null, fieldValue);
			}

			Loaded.Add(t);
		}
	}
}