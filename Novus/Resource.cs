using Novus.Memory;
using Novus.Properties;
using Novus.Utilities;
using SimpleCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using Novus.Imports;
using Novus.Win32;
using Novus.Win32.Wrappers;

// ReSharper disable UnusedMember.Global

namespace Novus
{
	/// <summary>
	/// Represents a runtime component which contains data and resources.
	/// </summary>
	/// <seealso cref="EmbeddedResources"/>
	public class Resource
	{
		public Pointer<byte> Address { get; }

		public ProcessModule Module { get; }

		public string ModuleName { get; }

		public SigScanner Scanner { get; }


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

		/*
		 * Native internal CLR functions
		 *
		 * Originally, IL had to be used to call native functions as the calli opcode was needed.
		 *
		 * Now, we can use C# 9 unmanaged function pointers because they are implemented using
		 * the calli opcode.
		 *
		 * Delegate function pointers are backed by IntPtr.
		 *
		 * https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/function-pointers
		 * https://github.com/dotnet/csharplang/blob/master/proposals/csharp-9.0/function-pointers.md
		 * https://devblogs.microsoft.com/dotnet/improvements-in-native-code-interop-in-net-5-0/
		 *
		 * Normal delegates using the UnmanagedFunctionPointer attribute is also possible, but it's
		 * better to use the new unmanaged function pointers.
		 */


		/// <summary>
		/// Loads imported values for members annotated with <see cref="ImportAttribute"/>.
		/// </summary>
		/// <param name="t">Enclosing type</param>
		public static void LoadImports(Type t)
		{
			if (LoadedTypes.Contains(t)) {
				return;
			}


			var mgr = GetManager(t.Assembly);

			if (!Managers.Contains(mgr)) {
				Managers.Add(mgr);
			}

			Debug.WriteLine($"[debug] Loading {t.Name}");

			var annotatedTuples = t.GetAnnotated<ImportAttribute>();

			foreach (var (attribute, member) in annotatedTuples) {
				var field = (FieldInfo) member;


				var fieldValue = GetImportValue(attribute, field);

				Debug.WriteLine($"[debug] Loading {member.Name} ({attribute.Name})");

				// Set value

				field.SetValue(null, fieldValue);
			}

			LoadedTypes.Add(t);

		}

		private static readonly List<Type> LoadedTypes = new();

		private static readonly List<ResourceManager> Managers = new()
		{
			EmbeddedResources.ResourceManager,
		};

		private static ResourceManager GetManager(Assembly assembly)
		{
			string name = null;

			foreach (var v in assembly.GetManifestResourceNames()) {

				var value = assembly.GetName().Name;

				if (v.Contains(value) || v.Contains("EmbeddedResources")) {
					name = v;
					break;
				}
			}

			if (name == null) {
				return null;
			}

			name = name.Substring(0, name.LastIndexOf('.'));


			var resourceManager = new ResourceManager(name, assembly);


			return resourceManager;
		}


		private static object GetObject(string s)
		{
			foreach (var manager in Managers) {
				var v = manager.GetObject(s);

				if (v != null) {
					Trace.WriteLine($"{manager.BaseName}:: {v}");
					return v;
				}
			}

			return null;

		}

		//todo: add symbol access

		public Pointer<byte> FindSignature(string signature)
		{
			return Scanner.FindSignature(signature);
		}

		public Pointer<byte> GetOffset(long ofs)
		{
			return Address + (ofs);
		}

		private static object GetImportValue(ImportAttribute attribute, FieldInfo field)
		{
			object fieldValue = null;

			switch (attribute) {
				case ImportUnmanagedComponentAttribute unmanagedAttr:
				{
					Guard.Assert(unmanagedAttr.ManageType == ManageType.Unmanaged);

					// Get value


					var resValue = (string) GetObject(unmanagedAttr.Name);


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
						UnmanagedType.Signature => resource.FindSignature(resValue),
						UnmanagedType.Offset    => resource.GetOffset((Int32.Parse(resValue, NumberStyles.HexNumber))),
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
	}
}