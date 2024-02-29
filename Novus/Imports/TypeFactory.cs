// Deci Novus TypeFactory.cs
// $File.CreatedYear-$File.CreatedMonth-9 @ 2:35

#region

using System.Reflection;
using System.Reflection.Emit;

#endregion

namespace Novus.Imports;

public static class TypeFactory
{

	#region

	private static readonly Assembly[] _assemblies = AppDomain.CurrentDomain.GetAssemblies();

	private static readonly AssemblyBuilder _assemblyBuilder =
		AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Factories"), AssemblyBuilderAccess.Run);

	private static readonly ModuleBuilder _moduleBuilder = _assemblyBuilder.DefineDynamicModule("Cassowary");

	private static int _count = 0;

	private static readonly Type[] _ctorTypes =
	[
		typeof(object),
		typeof(nint)
	];

	#endregion

	/// <summary>
	///     Defines a delegate type dynamically.
	/// </summary>
	/// <param name="name">The name of the delegate type.</param>
	/// <param name="returnType">The return type of the delegate.</param>
	/// <param name="parameterTypes">The parameter types of the delegate.</param>
	/// <returns>The defined TypeBuilder for the delegate type.</returns>
	[MImpl(MImplO.AggressiveOptimization | MImplO.AggressiveInlining)]
	internal static TypeBuilder DefineDelegateType(string name, Type returnType, Type[] parameterTypes)
	{
		TypeBuilder typeBuilder = _moduleBuilder.DefineType(name + $"%{_count++}%",
		                                                    TypeAttributes.Public |
		                                                    TypeAttributes.Sealed |
		                                                    TypeAttributes.AutoClass,
		                                                    typeof(MulticastDelegate));

		typeBuilder.DefineConstructor(MethodAttributes.FamANDAssem
		                              | MethodAttributes.Family
		                              | MethodAttributes.HideBySig
		                              | MethodAttributes.RTSpecialName,
		                              CallingConventions.Standard,
		                              _ctorTypes)
			.SetImplementationFlags(MethodImplAttributes.CodeTypeMask);

		typeBuilder.DefineMethod("Invoke",
		                         MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Virtual
		                         | MethodAttributes.HideBySig | MethodAttributes.VtableLayoutMask, returnType,
		                         parameterTypes)
			.SetImplementationFlags(MethodImplAttributes.CodeTypeMask);

		return typeBuilder;
	}

	/// <summary>
	///     Defines a type dynamically.
	/// </summary>
	/// <param name="name">The name of the type.</param>
	/// <param name="attr">The attributes of the type.</param>
	/// <param name="parent">The parent type of the type being defined.</param>
	/// <returns>The defined TypeBuilder for the type.</returns>
	[MImpl(MImplO.AggressiveOptimization | MImplO.AggressiveInlining)]
	internal static TypeBuilder DefineType(string name, TypeAttributes attr, Type parent)
	{
		return _moduleBuilder.DefineType(name + $"%{_count++}%", attr, parent);
	}

	/// <summary>
	///     Resolves a type by its name.
	/// </summary>
	/// <param name="typeName">The fully qualified name of the type.</param>
	/// <returns>The resolved Type, or null if not found.</returns>
	[MImpl(MImplO.AggressiveOptimization)]
	internal static Type? ResolveType(string typeName, bool throwOnNull = false)
	{
		Type? type;

		foreach (Assembly assembly in _assemblies) {
			if ((type = assembly.GetType(typeName)) != null)
				return type;
		}

		if (throwOnNull)
			throw new TypeLoadException($"Cannot find Type of name {typeName}");

		return null;
	}

}