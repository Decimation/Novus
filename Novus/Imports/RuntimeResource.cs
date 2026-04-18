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
using System.Runtime.ExceptionServices;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using Novus.Imports.Factory;
using Novus.OS;
using Novus.Runtime;

// ReSharper disable UnusedMember.Local
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable UnusedMember.Global
#pragma warning disable CA1873
#pragma warning disable IDE0059


namespace Novus.Imports;

[Flags]
public enum RuntimeResourceOptions
{

	None                  = 0,
	ThrowOnImportError    = 1 << 0,
	UnloadAllOnDispose    = 1 << 1,
	WriteDefaultOnUnload  = 1 << 2,
	NullOnImportError     = 1 << 3,
	GenerateOnImportError = 1 << 4,

}

/// <summary>
/// Represents a runtime component which contains data and resources.
/// </summary>
/// <seealso cref="ER"/>
[DAM(DAMT.All)]
[SupportedOSPlatform(RuntimeInformationExtensions.OS_WIN)]
public sealed class RuntimeResource : IDisposable
{

	public const string RSRC_MGR_NAME = "EmbeddedResources";

	private static readonly ILogger s_logger = Global.LoggerFactoryInt.CreateLogger(nameof(RuntimeResource));

	public Pointer<byte> Address => Module.BaseAddress;

	public ProcessModule Module { get; }

	public string ModuleName { get; }

	public Lazy<SigScanner> Scanner { get; }

	public Lazy<SymbolReader> Symbols { get; }

	public bool LoadedModule { get; private init; }

	public RuntimeResourceOptions Options { get; } = RuntimeResourceOptions.UnloadAllOnDispose
	                                                 | RuntimeResourceOptions.NullOnImportError;

	private readonly List<Type> m_loadedTypes = [];

	private readonly Dictionary<Assembly, ResourceManager> m_managers = new()
	{
		{ Global.Assembly, ER.ResourceManager }
	};

	/// <summary>
	/// Creates a <see cref="RuntimeResource"/> from an already-loaded module.
	/// </summary>
	public RuntimeResource(string moduleName, string pdb = null)
		: this(Process.GetCurrentProcess(), moduleName, pdb) { }

	public RuntimeResource(Process p, string moduleName, string pdb = null)
	{
		ModuleName = moduleName;

		Module = p.FindModule(moduleName);

		if (Module == null) {
			throw new NullReferenceException($"{moduleName} not found");
		}

		Scanner = new Lazy<SigScanner>(() => new SigScanner(Module));

		Symbols      = new Lazy<SymbolReader>(() => new SymbolReader(pdb ?? Module.FileName), LazyThreadSafetyMode.ExecutionAndPublication);
		LoadedModule = false;
	}

	/// <summary>
	/// Loads a module and creates a <see cref="RuntimeResource"/> from it.
	/// </summary>
	public static RuntimeResource LoadModule(string moduleFile, out nint h)
	{
		var f = new FileInfo(moduleFile);

		s_logger.LogDebug("Loading {Mod}", f.Name);

		//var l = Native.LoadLibrary(f.FullName);
		h = NativeLibrary.Load(f.FullName);

		var r = new RuntimeResource(f.Name)
		{
			LoadedModule = true
		};

		return r;
	}

	/*
	 * DOC
	 *
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

		if (Options.HasFlag(RuntimeResourceOptions.WriteDefaultOnUnload)) {
			foreach (var (k, member) in annotatedTuples) {
				var field = (FI) member;

				var    o = field.FieldType;
				object val;

				if (o.IsFunctionPointer) {
					val = IntPtr.Zero;
				}
				else {
					val = o.IsValueType ? /*Activator.CreateInstance(o)*/ default : null;
				}

				field.SetValue(null, val);
			}

		}

		s_logger.LogTrace("Unloaded {Name}", t.Name);
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
	public void LoadImports(Type t)
	{
		if (m_loadedTypes.Contains(t)) {
			return;
		}

		var mgr = GetOrAddManager(t.Assembly);

		s_logger.LogDebug("Loading type {Name}", t.Name);

		var iiaTuple = t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
		                .Where(mi => Attribute.IsDefined(mi, typeof(ImportInitializerAttribute)))
		                .Select(mi => (mi.GetCustomAttribute<ImportInitializerAttribute>(), mi));

		foreach ((ImportInitializerAttribute iia, MI mi) in iiaTuple) {
			var res = mi.Invoke(null, null);
			s_logger.LogTrace("Invoking {Iia} -> {Res}", iia, res);
		}

		var annotatedTuples = t.GetAnnotated<ImportAttribute>();
		s_logger.LogInformation("Found {Len} annotated", annotatedTuples.Length);
		LoadType(t, annotatedTuples);
	}

	private void LoadType(Type t, IEnumerable<(ImportAttribute Attribute, MMI Member)> annotatedTuples)
	{
		foreach (var (attribute, member) in annotatedTuples) {
			var field = (FI) member;

			var fieldValue = GetImportValue(attribute, field);

			// Set value

			s_logger.LogTrace("Loading {Mem} ({AttrName}) with {Fv}", member.Name, attribute.Name, fieldValue);
			field.SetValue(null, fieldValue);
		}


		m_loadedTypes.Add(t);
		s_logger.LogTrace("Loaded type {Type}", t.Name);
	}

	[CBN]
	private object GetObject(ImportAttribute attr)
	{
		if (attr is ImportUnmanagedAttribute { Value: not null } unmanaged) {
			return unmanaged.Value;
		}

		foreach (var (asm, manager) in m_managers) {
			var value = manager.GetObject(attr.Name);

			if (value != null) {
				//Debug.WriteLine($"{manager.BaseName}:: {value}", C_DEBUG);
				return value;
			}
		}

		return null;
	}

	private object GetImportValue(ImportAttribute attribute, FI field)
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

					s_logger.LogError("Could not find import value for {Name}", unmanagedAttr.Name);

					if (Options.HasFlag(RuntimeResourceOptions.ThrowOnImportError)) {
						throw new InvalidOperationException($"Could not find import value for {unmanagedAttr.Name}!");
					}

					if (Options.HasFlag(RuntimeResourceOptions.NullOnImportError)) {
						/*var dyn   = new DynamicMethod("Err", typeof(void), Type.EmptyTypes);
						var fnPtr = dyn.MethodHandle.GetFunctionPointer();
						addr = fnPtr;*/

						/*DynamicMethod dyn = MethodFactory.GetOrGenerateThrowingFunction(field.FieldType);
						addr = dyn.MethodHandle.GetFunctionPointer();*/

						addr = Mem.Nullptr;
					}
					else {
						addr = Mem.Nullptr;
					}
				}

				if (field.FieldType == typeof(Pointer<byte>)) {
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

				if (fn == null) {
					Debugger.Break();
				}

				var ptr = fn.MethodHandle.GetFunctionPointer();

				fieldValue = ptr;

				break;
			}
		}

		return fieldValue;
	}

#endregion Import

#region Manager

	[CBN]
	public ResourceManager GetOrAddManager(Assembly assembly, [CBN] string rsrcName = RSRC_MGR_NAME)
	{
		if (m_managers.TryGetValue(assembly, out ResourceManager mgr)) {
			return mgr;
		}

		string name = GetResourceManagerName(assembly, rsrcName);

		if (name == null) {
			return null;
		}

		mgr = new ResourceManager(name, assembly);

		TryAddManager(assembly, mgr);

		return mgr;
	}

	[CBN]
	public static string GetResourceManagerName(Assembly assembly, [CBN] string rsrcName = RSRC_MGR_NAME)
	{
		string name = null;

		foreach (string v in assembly.GetManifestResourceNames()) {
			string value = assembly.GetName().Name;
			rsrcName ??= value;

			if (value != null && (v.Contains(value) || v.Contains(rsrcName))) {
				name = v;
				break;
			}
		}

		if (name == null) {
			return null;
		}

		name = name[..name.LastIndexOf('.')];

		// var resourceManager = new ResourceManager(name, assembly);

		return name;
	}

	public bool TryAddManager(Assembly asm, ResourceManager mgr)
		=> m_managers.TryAdd(asm, mgr);

#endregion


#region

	public Pointer<byte> FindImport(ImportAttribute ia, string value = null)
	{
		var n        = ia.Name;
		var it       = default(ImportType);
		var resolver = ia.Resolver;
		var absolute = ia.AbsoluteMatch;

		switch (ia) {
			case ImportUnmanagedAttribute iua:
				value ??= iua.Value;
				it    =   iua.Type;
				break;
		}

		value ??= n;


		return it switch
		{
			// currying

			ImportType.Signature => GetSignature(value),
			ImportType.Export    => GetExport(value),
			ImportType.Offset    => GetOffset(value),
			ImportType.Symbol    => GetSymbol(value, resolver, absolute),
			_                    => Mem.Nullptr
		};
	}

	public static Pointer<byte> FindImport(Process proc, string moduleName, string value, ImportType it, bool absolute = false)
	{
		using var resource = new RuntimeResource(proc, moduleName, value);

		return resource.FindImport(new ImportUnmanagedAttribute(moduleName, it, value)
		{
			AbsoluteMatch = absolute
		}, value);
	}

	public static Pointer<byte> FindImport(string moduleName, string value, ImportType it)
		=> FindImport(Process.GetCurrentProcess(), moduleName, value, it);

#endregion

#region

	public Pointer<byte> GetOffset(string s)
		=> Address + (IntPtr.TryParse(s, NumberStyles.HexNumber, null, out var l) ? l : Mem.Nullptr);

	public Pointer<byte> GetExport(string name)
		=> NativeLibrary.GetExport(Module.BaseAddress, name);

	public Pointer<byte> GetSignature(string signature)
		=> Scanner.Value.FindSignature(signature);

	public Pointer<byte> GetSymbol(string name, Type t = null, bool absolute = false)
	{
		if (Symbols.Value == null) {
			return Mem.Nullptr;
		}

		var symbols1 = Symbols.Value.GetSymbols(name)
		                      .Where(static s => s.Tag is SymbolTag.Function or SymbolTag.Data);

		Symbol symbol = null;

		Func<Symbol, bool> predicate = absolute ? s => s.Name == name : s => s.Name.Contains(name);

		symbol = symbols1.FirstOrDefault(predicate);

		/*if (resolver != null) {
			var m = (IRuntimeResourceImporter) Activator.CreateInstance(resolver);
			symbol = symbols.FirstOrDefault(m.Match);
		}
		else {
			symbol = symbols.FirstOrDefault();
		}*/

		return (Pointer<byte>) Module.BaseAddress +
		       (nint) (symbol?.Offset ?? throw new InvalidOperationException());
	}

#endregion

	public void Dispose()
	{
		if (Options.HasFlag(RuntimeResourceOptions.UnloadAllOnDispose)) {
			UnloadAll();
		}

		Symbols.Value?.Dispose();

		if (LoadedModule) {
			//Native.FreeLibrary(Module.BaseAddress);
			NativeLibrary.Free(Module.BaseAddress);
		}

		// GC.SuppressFinalize(this);
	}

	public override string ToString()
	{
		return $"[{Module.ModuleName}] "
		       + $"| {nameof(Scanner)}: {(Scanner.IsValueCreated ? Scanner.Value.Address : '-')}"
		       + $"| {nameof(Symbols)} : {(Symbols.IsValueCreated ? Symbols.Value.Image : '-')}";
	}

}