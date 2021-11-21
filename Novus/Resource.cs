using JetBrains.Annotations;
using Novus.Imports;
using Novus.Memory;
using Novus.Properties;
using Novus.Utilities;
using Novus.Win32;
using Kantan.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using static Kantan.Diagnostics.LogCategories;

// ReSharper disable UnusedMember.Local

// ReSharper disable SuggestVarOrType_DeconstructionDeclarations

#pragma warning disable IDE0059

// ReSharper disable LoopCanBeConvertedToQuery

// ReSharper disable UnusedMember.Global

namespace Novus;

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

	public Lazy<SymbolLoader> Symbols { get; }

	public bool LoadedModule { get; private init; }


	/// <summary>
	/// Creates a <see cref="Resource"/> from an already-loaded module.
	/// </summary>
	public Resource(string moduleName, string pdb = null) : this(Process.GetCurrentProcess(), moduleName, pdb) { }

	public Resource(Process p, string moduleName, string pdb = null)
	{
		ModuleName = moduleName;

		var module = ProcessHelper.FindModule(p, moduleName);

		Guard.AssertNotNull(module);

		Module       = module;
		Scanner      = new Lazy<SigScanner>(() => new SigScanner(Module));
		Address      = Module.BaseAddress;
		Symbols      = new Lazy<SymbolLoader>(() => pdb is not null ? new SymbolLoader(pdb) : null);
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
			Type type = m_loadedTypes[i];
			Unload(type);
		}
	}

	public void Unload(Type t)
	{
		var annotatedTuples = t.GetAnnotated<ImportAttribute>();

		foreach (var (_, member) in annotatedTuples) {
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

		Debug.WriteLine($"Loading type {t.Name}", C_DEBUG);

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
		if (attr is ImportUnmanagedAttribute { Value: { } } unmanaged) {
			return unmanaged.Value;
		}

		foreach (var manager in m_managers) {
			var value = manager.GetObject(attr.Name);

			if (value != null) {
				//Debug.WriteLine($"{manager.BaseName}:: {value}", C_DEBUG);
				return value;
			}
		}

		return null;
	}

	private object GetImportValue(ImportAttribute attribute, FieldInfo field)
	{
		object fieldValue = null;

		string name = attribute.Name ?? field.Name;

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

				//string mod           = unmanagedAttr.ModuleName;
				var unmanagedType = unmanagedAttr.UnmanagedType;

				// Find address

				var addr = unmanagedType switch
				{
					UnmanagedImportType.Signature => FindSignature(resValue),
					UnmanagedImportType.Offset    => GetOffset((Int32.Parse(resValue, NumberStyles.HexNumber))),
					UnmanagedImportType.Symbol => (Pointer<byte>) Module.BaseAddress +
					                              (Symbols.Value?.GetSymbol(name)?.Offset
					                               ?? throw new InvalidOperationException()),

					_ => null
				};

				//Guard.Assert(!addr.IsNull, $"Could not find value for {resValue}!");

				if (addr.IsNull) {
					// throw new ImportException($"Could not find import value for {unmanagedAttr.Name}");

					Trace.WriteLine($"Could not find import value for {unmanagedAttr.Name}!", C_ERROR);

					/*unsafe {
						addr = (IntPtr) ((delegate* managed<void>) &ErrorFunction);
					}*/
				}


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

	private static void ErrorFunction()
	{
		throw new InvalidOperationException();
	}

	public static Pointer<byte> FindFunction(Process p, string m, string s)
	{

		var resource = new Resource(p, m, s);

		return resource.FindExportOrSignature(s);
	}

	public static Pointer<byte> FindFunction(string m, string s) => FindFunction(Process.GetCurrentProcess(), m, s);


	public Pointer<byte> FindExportOrSignature(string signature)
	{
		var p = FindExport(signature);

		if (p.IsNull) {
			p = FindSignature(signature);
		}

		return p;
	}

	public Pointer<byte> FindExport(string signature) => NativeLibrary.GetExport(Module.BaseAddress, signature);

	public Pointer<byte> FindSignature(string signature) => Scanner.Value.FindSignature(signature);

	public Pointer<byte> GetOffset(long ofs) => Address + (ofs);

	public void Dispose()
	{
		UnloadAll();
		Symbols.Value?.Dispose();

		if (LoadedModule) {
			//Native.FreeLibrary(Module.BaseAddress);
			NativeLibrary.Free(Module.BaseAddress);
		}

		GC.SuppressFinalize(this);
	}

	public override string ToString()
	{
		return $"{Module.ModuleName} ({Scanner.Value.Address})";
	}
}