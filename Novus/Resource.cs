using Novus.Memory;
using Novus.Properties;
using Novus.Utilities;
using SimpleCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using Novus.Imports;

namespace Novus
{
	/// <summary>
	/// Represents a runtime component which contains data and resources.
	/// </summary>
	/// <seealso cref="EmbeddedResources"/>
	public class Resource
	{
		public Pointer<byte> Address    { get; }
		public ProcessModule Module     { get; }
		public string        ModuleName { get; }
		public SigScanner    Scanner    { get; }


		private static readonly List<Type> LoadedTypes = new();

		public Resource(string moduleName)
		{
			ModuleName = moduleName;

			var module = Mem.FindModule(moduleName);

			Guard.AssertNotNull(module);

			Module = module;

			Scanner = new SigScanner(Module);

			Address = Module.BaseAddress;
		}


		/// <summary>
		/// Loads imported values for members annotated with <see cref="ImportAttribute"/>.
		/// </summary>
		/// <param name="t">Enclosing type</param>
		public static void LoadImports(Type t)
		{
			if (LoadedTypes.Contains(t)) {
				return;
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


		private static ResourceManager GetManager(Assembly assembly)
		{


			string name = null;

			foreach (var v in assembly.GetManifestResourceNames()) {

				if (v.Contains("EmbeddedResources")) {
					name = v;
					break;
				}
			}

			name = name.Substring(0, name.LastIndexOf('.'));


			//"Novus.Properties.EmbeddedResources"

			ResourceManager resourceManager = new ResourceManager(name, assembly);
			

			return resourceManager;
		}

		public static object GetObject(string s)
		{
			/*var asm1            = Assembly.GetCallingAssembly();
			var asm2            = Assembly.GetEntryAssembly();
			
			var resourceManager = GetManager(asm1);
			
			var resValue        = (string)resourceManager.GetObject(s);

			if (resValue == null) {
				resValue=(string)EmbeddedResources.ResourceManager.GetObject(s);
			}

			return (string)resValue;*/

			var resValue = (string)EmbeddedResources.ResourceManager.GetObject(s);

			if (resValue != null) {
				return resValue;
			}

			Console.WriteLine(Assembly.GetCallingAssembly());
			Console.WriteLine(Assembly.GetEntryAssembly());
			Console.WriteLine(Assembly.GetExecutingAssembly());

			var manager = GetManager(Assembly.GetCallingAssembly());

			Console.WriteLine($"using {manager.BaseName}");

			return  manager.GetObject(s);

		}

		private static object GetImportValue(ImportAttribute attribute, FieldInfo field)
		{
			object fieldValue = null;

			switch (attribute) {
				case ImportUnmanagedComponentAttribute unmanagedAttr:
				{
					Guard.Assert(unmanagedAttr.ManageType == ManageType.Unmanaged);

					// Get value

					//Assembly.GetEntryAssembly();

					// var resourceManager = GetManager();
					// var resValue        = (string) resourceManager.GetObject(unmanagedAttr.Name);

					// string resValue = (string) EmbeddedResources.ResourceManager
					// .GetObject(unmanagedAttr.Name);

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
	}
}