using JetBrains.Annotations;
using Novus.Memory;
using Novus.Properties;
using Novus.Utilities;
using Kantan.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Novus.Imports.Attributes;
using Novus.Win32.Structures.DbgHelp;
using Novus.Win32.Wrappers;
using System.Net;
using System.Runtime.Versioning;

// ReSharper disable UnusedMember.Local

// ReSharper disable SuggestVarOrType_DeconstructionDeclarations

#pragma warning disable IDE0059

// ReSharper disable LoopCanBeConvertedToQuery

// ReSharper disable UnusedMember.Global

namespace Novus.Imports;

/// <summary>
/// Represents a runtime component which contains data and resources.
/// </summary>
/// <seealso cref="ER"/>
[DAM(DAMT.All)]
[SupportedOSPlatform(Global.OS_WIN)]
public sealed class RuntimeResource : IDisposable
{

	public Pointer Address => Module.Value.BaseAddress;

	public Lazy<ProcessModule> Module { get; }

	public string ModuleName { get; }

	public Lazy<SigScanner> Scanner { get; }

	public Lazy<Win32SymbolReader> Symbols { get; }

	public bool LoadedModule { get; private init; }

	public Pointer ModuleAddress => Module.IsValueCreated ? Module.Value.BaseAddress : Mem.Nullptr;

	/// <summary>
	/// Creates a <see cref="RuntimeResource"/> from an already-loaded module.
	/// </summary>
	public RuntimeResource(string moduleName, string pdb = null)
		: this(Process.GetCurrentProcess(), moduleName, pdb) { }

	public RuntimeResource(Process p, string moduleName, string pdb = null)
	{
		ModuleName = moduleName;

		Module = new(() =>
		{
			var mod = p.FindModule(moduleName);

			return mod;
		});

		Scanner = new Lazy<SigScanner>(() =>
		{
			if (!Module.IsValueCreated) {
				Debug.WriteLine($"{Scanner} accessed before {Module} was init");	
			}

			return new SigScanner(Module.Value);
		});

		Symbols      = new Lazy<Win32SymbolReader>(() => File.Exists(pdb) ? new Win32SymbolReader(pdb) : null);
		LoadedModule = false;
	}

	/// <summary>
	/// Loads a module and creates a <see cref="RuntimeResource"/> from it.
	/// </summary>
	public static RuntimeResource LoadModule(string moduleFile, out nint h)
	{
		var f = new FileInfo(moduleFile);

		Debug.WriteLine($"Loading {f.Name}", nameof(LoadModule));

		//var l = Native.LoadLibrary(f.FullName);
		h = NativeLibrary.Load(f.FullName);

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

	public bool IsLoaded(Type t)
		=> m_loadedTypes.Contains(t);

	public void Unload(Type t)
	{
		var annotatedTuples = t.GetAnnotated<ImportAttribute>();

		foreach (var (_, member) in annotatedTuples) {
			var field = (FI) member;

			field.SetValue(null, null);
		}

		Trace.WriteLine($"Unloaded type {t.Name}", LogCategories.C_INFO);
		m_loadedTypes.Remove(t);
	}

	public void LoadAll([CBN] Assembly assembly = null)
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

		m_managers.Add(mgr);

		Debug.WriteLine($"Loading type {t.Name}", nameof(LoadImports));

		var annotatedTuples = t.GetAnnotated<ImportAttribute>();

		foreach (var (attribute, member) in annotatedTuples) {
			var field = (FI) member;

			var fieldValue = GetImportValue(attribute, field, throwOnErr);

			// Set value

			field.SetValue(null, fieldValue);

			Debug.WriteLine($"Loaded {member.Name} ({attribute.Name}) with {fieldValue}", nameof(LoadImports));
		}

		m_loadedTypes.Add(t);

		Trace.WriteLine($"Loaded type {t.Name}", nameof(LoadImports));

	}

	private readonly List<Type> m_loadedTypes = [];

	private readonly HashSet<ResourceManager> m_managers = [ER.ResourceManager];

	private object GetObject(ImportAttribute attr)
	{
		if (attr is ImportUnmanagedAttribute { Value: not null } unmanaged) {
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

				var addr = FindImport(attribute, resValue);

				//Require.Assert(!addr.IsNull, $"Could not find value for {resValue}!");

				if (addr.IsNull) {
					// throw new ImportException($"Could not find import value for {unmanagedAttr.Name}");

					Trace.WriteLine($"Could not find import value for {unmanagedAttr.Name}!", LogCategories.C_ERROR);

					if (throwOnErr) {
						unsafe {
							var  unmg         = field.FieldType.IsUnmanagedFunctionPointer;
							var  fnPtr        = field.FieldType.GetFunctionPointerReturnType();
							Type modifiedType = field.GetModifiedFieldType();

							var types = field.FieldType.GetFunctionPointerParameterTypes();
							var dyn   = new DynamicMethod("__eerr", fnPtr, types);
							var gen   = dyn.GetILGenerator();

							for (int i = 0; i < types.Length; i++) {
								gen.Emit(OpCodes.Ldarg, i);
								gen.Emit(OpCodes.Newobj, fnPtr);
								gen.Emit(OpCodes.Ret);
							}

							//todo
							addr = (nint) dyn.MethodHandle.GetFunctionPointer();

							/*var dyn = new DynamicMethod("Err", typeof(void), Type.EmptyTypes);
							var fnPtr=dyn.MethodHandle.GetFunctionPointer();
							addr = fnPtr;*/
						}
					}

				}

				if (field.FieldType == typeof(Pointer)) {
					fieldValue = addr;
				}
				else {
					fieldValue = (nint) addr;
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

	#endregion Import

	public static ResourceManager GetManager(Assembly assembly, [CBN] string rsrcName = "EmbeddedResources")
	{
		string name = null;

		foreach (string v in assembly.GetManifestResourceNames()) {
			string value = assembly.GetName().Name;
			rsrcName ??= value;

			if (v.Contains(value!) || v.Contains(rsrcName!)) {
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

	private static void ErrorFunction()
	{
		throw new InvalidOperationException();
	}

	#region

	public Pointer FindImport(ImportAttribute a, string s = null)
	{
		var n        = a.Name;
		var x        = default(ImportType);
		var t        = a.Resolver;
		var absolute = a.AbsoluteMatch;

		switch (a) {
			case ImportUnmanagedAttribute ua:
				s ??= ua.Value;
				x =   ua.Type;
				break;
		}

		return x switch
		{
			// currying

			ImportType.Signature => this.GetSignature(s),
			ImportType.Export    => this.GetExport(s),
			ImportType.Offset    => this.GetOffset(s),
			ImportType.Symbol    => this.GetSymbol(s, t, absolute),
			_                    => Mem.Nullptr
		};
	}

	public static Pointer FindImport(Process p, string m, string s, ImportType x, bool absolute = false)
	{
		using var resource = new RuntimeResource(p, m, s);

		return resource.FindImport(new ImportUnmanagedAttribute(m, x, s)
		{
			AbsoluteMatch = absolute
		}, s);
	}

	public static Pointer<byte> FindImport(string m, string s, ImportType x)
		=> FindImport(Process.GetCurrentProcess(), m, s, x);

	#endregion

	#region

	public Pointer GetOffset(string s)
	{
		return Address + (nint.TryParse(s, NumberStyles.HexNumber, null, out var l) ? l : nint.Parse(s));
	}

	public Pointer GetExport(string name)
		=> NativeLibrary.GetExport(Module.Value.BaseAddress, name);

	public Pointer GetSignature(string signature)
		=> Scanner.Value.FindSignature(signature);

	public Pointer GetSymbol(string name, Type t = null, bool absolute = false)
	{
		if (Symbols.Value == null) {
			return Mem.Nullptr;
		}

		var symbols1 = Symbols.Value.GetSymbols(name)
			.Where(s => s.Tag is SymbolTag.Function or SymbolTag.Data);

		Symbol symbol = null;

		Func<Symbol, bool> predicate = absolute
			                               ? s => s.Name == name
			                               : s => s.Name.Contains(name);

		symbol = symbols1.FirstOrDefault(predicate);

		/*if (resolver != null) {
			var m = (IRuntimeResourceImporter) Activator.CreateInstance(resolver);
			symbol = symbols.FirstOrDefault(m.Match);
		}
		else {
			symbol = symbols.FirstOrDefault();
		}*/

		return (Pointer) Module.Value.BaseAddress +
		       (nint) (symbol?.Offset ?? throw new InvalidOperationException());
	}

	#endregion

	public void Dispose()
	{
		UnloadAll();
		Symbols.Value?.Dispose();

		if (LoadedModule) {
			//Native.FreeLibrary(Module.BaseAddress);
			NativeLibrary.Free(Module.Value.BaseAddress);
		}

		// GC.SuppressFinalize(this);
	}

	public override string ToString()
	{
		return $"{Module.Value.ModuleName} ({Scanner.Value.Address})";
	}

}