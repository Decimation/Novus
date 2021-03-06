﻿using JetBrains.Annotations;
using Novus.Imports;
using Novus.Memory;
using Novus.Properties;
using Novus.Utilities;
using Novus.Win32;
using SimpleCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using static SimpleCore.Diagnostics.LogCategories;

#pragma warning disable IDE0059

// ReSharper disable LoopCanBeConvertedToQuery

// ReSharper disable UnusedMember.Global

namespace Novus
{
	/// <summary>
	/// Represents a runtime component which contains data and resources.
	/// </summary>
	/// <seealso cref="EmbeddedResources"/>
	public class Resource : IDisposable
	{
		public Pointer<byte> Address { get; }

		public ProcessModule Module { get; }

		public string ModuleName { get; }

		public Lazy<SigScanner> Scanner { get; }

		[CanBeNull]
		public SymbolLoader Symbols { get; }

		public bool LoadedModule { get; private set; }


		/// <summary>
		/// Creates a <see cref="Resource"/> from an already-loaded module.
		/// </summary>
		public Resource(string moduleName, string pdb = null)
		{
			ModuleName = moduleName;

			var module = Mem.FindModule(moduleName);

			Guard.AssertNotNull(module);

			Module = module;

			Scanner = new Lazy<SigScanner>(()=> new SigScanner(Module));

			Address = Module.BaseAddress;

			Symbols = pdb is not null ? new SymbolLoader(pdb) : null;

			LoadedModule = false;
		}

		/// <summary>
		/// Loads a module and creates a <see cref="Resource"/> from it.
		/// </summary>
		public static Resource LoadModule(string moduleFile)
		{
			var f = new FileInfo(moduleFile);

			Debug.WriteLine($"Loading {f.Name}");

			//var l = Native.LoadLibrary(f.FullName);
			var l = NativeLibrary.Load(f.FullName);


			var r = new Resource(f.Name)
			{
				LoadedModule = true
			};

			return r;
		}

		public override string ToString()
		{
			return $"{Module.ModuleName} ({Scanner.Value.Address})";
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
		 *
		 *
		 * https://github.com/Decimation/Novus/tree/10a2fde6d3df0e359c13f6808bf169723ffd414d/Novus/Native
		 */

		#region Import

		public void UnloadAll()
		{
			for (int i = m_loadedTypes.Count - 1; i >= 0; i--) {
				var type = m_loadedTypes[i];
				Unload(type);

			}
		}

		public void Unload(Type t)
		{
			var annotatedTuples = t.GetAnnotated<ImportAttribute>();

			foreach (var (attr, member) in annotatedTuples) {
				var field = (FieldInfo) member;

				field.SetValue(null, null);
			}

			Trace.WriteLine($"Unloaded type {t.Name}", C_INFO);
			m_loadedTypes.Remove(t);
		}

		/// <summary>
		/// Loads imported values for members annotated with <see cref="ImportAttribute"/>.
		/// </summary>
		/// <param name="t">Enclosing type</param>
		public void LoadImports(Type t)
		{
			if (m_loadedTypes.Contains(t)) {
				return;
			}

			var mgr = GetManager(t.Assembly);

			if (!m_managers.Contains(mgr)) {
				m_managers.Add(mgr);
			}

			Debug.WriteLine($"Loading {t.Name}", C_DEBUG);

			var annotatedTuples = t.GetAnnotated<ImportAttribute>();

			foreach (var (attribute, member) in annotatedTuples) {
				var field = (FieldInfo) member;

				var fieldValue = GetImportValue(attribute, field);


				// Set value

				field.SetValue(null, fieldValue);

				Debug.WriteLine($"Loaded {member.Name} ({attribute.Name}) with {fieldValue}", C_DEBUG);
			}

			m_loadedTypes.Add(t);

			Trace.WriteLine($"Loaded type {t.Name}", C_INFO);

		}

		private readonly List<Type> m_loadedTypes = new();

		private readonly List<ResourceManager> m_managers = new()
		{
			EmbeddedResources.ResourceManager,
		};

		private static ResourceManager GetManager(Assembly assembly)
		{
			string name = null;

			foreach (string v in assembly.GetManifestResourceNames()) {
				string value = assembly.GetName().Name;

				if (v.Contains(value!) || v.Contains("EmbeddedResources")) {
					name = v;
					break;
				}
			}

			if (name == null) {
				return null;
			}

			name = name[..name.LastIndexOf('.')];

			var resourceManager = new ResourceManager(name, assembly);

			return resourceManager;
		}

		private object GetObject(ImportAttribute attr)
		{
			if (attr is ImportUnmanagedAttribute {Value: { }} unmanaged) {
				return unmanaged.Value;
			}

			foreach (var manager in m_managers) {
				var value = manager.GetObject(attr.Name);

				if (value != null) {
					Debug.WriteLine($"{manager.BaseName}:: {value}", C_DEBUG);
					return value;
				}
			}

			return null;
		}

		private object GetImportValue(ImportAttribute attribute, FieldInfo field)
		{
			object fieldValue = null;

			var name = attribute.Name ?? field.Name;

			switch (attribute) {
				case ImportUnmanagedAttribute unmanagedAttr:
				{
					/*
					 * Name is the name of the resource file key
					 */

					Guard.Assert(unmanagedAttr.ManageType == ImportManageType.Unmanaged);

					/*
					 * Get value
					 *
					 * If value is specified, use it; otherwise, look in resources
					 */

					var resValue = (string) GetObject(attribute);

					Guard.AssertNotNull(resValue);

					/*
					 * Get resource
					 */

					string mod           = unmanagedAttr.ModuleName;
					var    unmanagedType = unmanagedAttr.UnmanagedType;


					// Find address

					var addr = unmanagedType switch
					{
						UnmanagedImportType.Signature => FindSignature(resValue),
						UnmanagedImportType.Offset    => GetOffset((Int32.Parse(resValue, NumberStyles.HexNumber))),
						UnmanagedImportType.Symbol => ((Pointer<byte>) Module.BaseAddress) +
						                              Symbols.GetSymbol(name).Offset,

						_ => null
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

				case ImportManagedAttribute managedAttr:
				{
					/*
					 * Name is the name of the member
					 */

					Guard.Assert(managedAttr.ManageType == ImportManageType.Managed);

					var fn = managedAttr.Type.GetAnyMethod(name);

					var ptr = fn.MethodHandle.GetFunctionPointer();

					fieldValue = ptr;

					break;
				}
			}

			return fieldValue;
		}

		#endregion Import

		public Pointer<byte> FindSignature(string signature)
		{
			return Scanner.Value.FindSignature(signature);
		}

		public Pointer<byte> GetOffset(long ofs)
		{
			return Address + (ofs);
		}

		public void Dispose()
		{
			UnloadAll();
			Symbols?.Dispose();

			if (LoadedModule) {
				//Native.FreeLibrary(Module.BaseAddress);
				NativeLibrary.Free(Module.BaseAddress);
			}

			GC.SuppressFinalize(this);
		}
	}
}