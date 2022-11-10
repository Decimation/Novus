using JetBrains.Annotations;
using Novus.Imports;
using Novus.Memory;
using Novus.Properties;
using Novus.Utilities;
using Kantan.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Resources;
using System.Runtime.InteropServices;
using Novus.OS;
using static Kantan.Diagnostics.LogCategories;
using static Novus.Imports.ImportType;

// ReSharper disable UnusedMember.Local

// ReSharper disable SuggestVarOrType_DeconstructionDeclarations

#pragma warning disable IDE0059

// ReSharper disable LoopCanBeConvertedToQuery

// ReSharper disable UnusedMember.Global

namespace Novus.Runtime;

/// <summary>
/// Represents a runtime component which contains data and resources.
/// </summary>
/// <seealso cref="EmbeddedResources"/>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class RuntimeResource : IDisposable
{
	public Pointer<byte> Address => Module.Value.BaseAddress;

	public Lazy<ProcessModule> Module { get; }

	public string ModuleName { get; }

	public Lazy<SigScanner> Scanner { get; }

	public Lazy<SymbolReader> Symbols { get; }

	public bool LoadedModule { get; private init; }

	/// <summary>
	/// Creates a <see cref="RuntimeResource"/> from an already-loaded module.
	/// </summary>
	public RuntimeResource(string moduleName, string pdb = null) :
		this(Process.GetCurrentProcess(), moduleName, pdb) { }

	public RuntimeResource(Process p, string moduleName, string pdb = null)
	{
		ModuleName = moduleName;

		Module       = new(() => p.FindModule(moduleName));
		Scanner      = new Lazy<SigScanner>(() => new SigScanner(Module.Value));
		
		Symbols      = new Lazy<SymbolReader>(() => pdb is not null ? new SymbolReader(pdb) : null);
		LoadedModule = false;
	}

	/// <summary>
	/// Loads a module and creates a <see cref="RuntimeResource"/> from it.
	/// </summary>
	public static RuntimeResource LoadModule(string moduleFile)
	{
		var f = new FileInfo(moduleFile);

		Debug.WriteLine($"Loading {f.Name}");

		//var l = Native.LoadLibrary(f.FullName);
		var l = NativeLibrary.Load(f.FullName);

		var r = new RuntimeResource(f.Name)
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

	public bool IsLoaded(Type t) => m_loadedTypes.Contains(t);

	public void Unload(Type t)
	{
		var annotatedTuples = t.GetAnnotated<ImportAttribute>();

		foreach (var (_, member) in annotatedTuples) {
			var field = (FI) member;

			field.SetValue(null, null);
		}

		Trace.WriteLine($"Unloaded type {t.Name}", C_INFO);
		m_loadedTypes.Remove(t);
	}

	public void LoadAll(Assembly assembly = null)
	{
		assembly ??= Assembly.GetCallingAssembly();

		var types = assembly.GetTypes();

		foreach (var t in types) {
			LoadImports(t);
		}
	}

	/// <summary>
	/// Loads imported values for members annotated with <see cref="ImportAttribute"/>.
	/// </summary>
	/// <param name="t">Enclosing type</param>
	/// <param name="throwOnErr">Passed to <see cref="GetImportValue"/></param>
	public void LoadImports(Type t, bool throwOnErr = true)
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
			var field = (FI) member;

			var fieldValue = GetImportValue(attribute, field, throwOnErr);

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

	private static ResourceManager GetManager(Assembly assembly, string rsrcName = "EmbeddedResources")
	{
		string name = null;

		foreach (string v in assembly.GetManifestResourceNames()) {
			string value = assembly.GetName().Name;

			if (v.Contains(value!) || v.Contains(rsrcName)) {
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

	private object GetImportValue(ImportAttribute attribute, FI field, bool throwOnErr)
	{
		object fieldValue = null;

		string name = attribute.Name ?? field.Name;

		switch (attribute) {
			case ImportUnmanagedAttribute unmanagedAttr:
			{
				/*
				 * Name is the name of the resource file key
				 */

				Require.Assert(unmanagedAttr.ManageType == ImportManageType.Unmanaged);

				/*
				 * Get value
				 *
				 * If value is specified, use it; otherwise, look in resources
				 */

				var resValue = (string) GetObject(attribute);

				Require.NotNull(resValue);

				/*
				 * Get resource
				 */

				//string mod           = unmanagedAttr.ModuleName;
				var unmanagedType = unmanagedAttr.Type;

				// Find address

				var addr = FindImport(resValue, unmanagedType);

				//Require.Assert(!addr.IsNull, $"Could not find value for {resValue}!");

				if (addr.IsNull) {
					// throw new ImportException($"Could not find import value for {unmanagedAttr.Name}");

					Trace.WriteLine($"Could not find import value for {unmanagedAttr.Name}!", C_ERROR);

					if (throwOnErr) {
						unsafe {
							//todo
							addr = (IntPtr) ((delegate* managed<void>) &ErrorFunction);

							/*var dyn = new DynamicMethod("Err", typeof(void), Type.EmptyTypes);
							var fnPtr=dyn.MethodHandle.GetFunctionPointer();
							addr = fnPtr;*/
						}
					}

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

				Require.Assert(managedAttr.ManageType == ImportManageType.Managed);

				var fn = managedAttr.Type.GetAnyMethod(name);

				var ptr = fn.MethodHandle.GetFunctionPointer();

				fieldValue = ptr;

				break;
			}
		}

		return fieldValue;
	}

	public Pointer GetSymbol(string name)
	{
		return (Pointer<byte>) Module.Value.BaseAddress +
		       (Symbols.Value?.GetSymbol(name)?.Offset
		        ?? throw new InvalidOperationException());
	}

	#endregion Import

	private static void ErrorFunction()
	{
		throw new InvalidOperationException();
	}

	public static Pointer<byte> FindImport(Process p, string m, string s, ImportType x)
	{
		using var resource = new RuntimeResource(p, m, s);

		return resource.FindImport(s, x);
	}

	public static Pointer<byte> FindImport(string m, string s, ImportType x)
		=> FindImport(Process.GetCurrentProcess(), m, s, x);

	public Pointer<byte> FindImport(string s, ImportType x)
	{
		return x switch
		{
			Signature => GetSignature(s),
			Export    => GetExport(s),
			Offset    => GetOffset(s),
			Symbol    => GetSymbol(s),
			_         => throw new ArgumentOutOfRangeException(nameof(x), x, null)
		};
	}

	private Pointer GetOffset([NN] string s)
	{
		return Address + (long.TryParse(s, NumberStyles.HexNumber, null, out long l) ? l : long.Parse(s));
	}

	public Pointer<byte> GetExport(string name) => NativeLibrary.GetExport(Module.Value.BaseAddress, name);

	public Pointer<byte> GetSignature(string signature) => Scanner.Value.FindSignature(signature);

	public void Dispose()
	{
		UnloadAll();
		Symbols.Value?.Dispose();

		if (LoadedModule) {
			//Native.FreeLibrary(Module.BaseAddress);
			NativeLibrary.Free(Module.Value.BaseAddress);
		}

		GC.SuppressFinalize(this);
	}

	public override string ToString()
	{
		return $"{Module.Value.ModuleName} ({Scanner.Value.Address})";
	}
}