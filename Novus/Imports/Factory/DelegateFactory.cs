// Deci Novus DelegateFactory.cs
// $File.CreatedYear-$File.CreatedMonth-9 @ 2:34

#nullable enable
using Novus;

namespace Novus.Imports.Factory;

using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

/// <summary>
/// <a href="https://github.com/cetio/Cassowary">Original source</a>
/// </summary>
public sealed class DelegateFactory
{

	private const char TYPENAME_DELIM = ' ';

	/// <summary>
	///     Creates a new delegate type.
	/// </summary>
	/// <param name="name">Name of the delegate type.</param>
	/// <param name="returnType">Return type of the delegate.</param>
	/// <param name="parameterTypes">Parameter types of the delegate.</param>
	/// <returns>The newly created delegate type.</returns>
	[MImp(MImplO.AggressiveOptimization | MImplO.AggressiveInlining)]
	public static Type MakeNewDelegateType(string name, Type returnType, params Type[] parameterTypes)
	{
		TypeBuilder typeBuilder = TypeFactory.DefineDelegateType(name, returnType, parameterTypes);
		return typeBuilder.CreateTypeInfo().AsType();
	}

	/// <summary>
	///     Creates a new delegate type with the specified return type and parameter types.
	/// </summary>
	/// <param name="returnType">The return type of the delegate.</param>
	/// <param name="parameterTypes">The parameter types of the delegate.</param>
	/// <returns>The created delegate type.</returns>
	[MImp(MImplO.AggressiveOptimization | MImplO.AggressiveInlining)]
	public static Type MakeNewDelegateType(Type returnType, params Type[] parameterTypes)
	{
		if (returnType == typeof(void))
			return MakeNewDelegateType(parameterTypes);

		string typeNames = String.Join(TYPENAME_DELIM, parameterTypes.Select(x => x.Name).Append(returnType.Name));

		return MakeNewDelegateType(
			$"{TypeFactory.NovusCassowary}Multicast{(returnType != typeof(void) ? TYPENAME_DELIM : String.Empty)}{typeNames}",
			returnType,
			parameterTypes);
	}

	/// <summary>
	///     Creates a new delegate type with the specified parameter types.
	/// </summary>
	/// <param name="parameterTypes">The parameter types of the delegate.</param>
	/// <returns>The created delegate type.</returns>
	[MImp(MImplO.AggressiveOptimization | MImplO.AggressiveInlining)]
	public static Type MakeNewDelegateType(params Type[] parameterTypes)
	{
		string typeNames = String.Join(TYPENAME_DELIM, parameterTypes.Select(x => x.Name));

		return MakeNewDelegateType(
			$"{TypeFactory.NovusCassowary}Multicast{(typeNames.Length > 0 ? TYPENAME_DELIM : String.Empty)}{typeNames}",
			typeof(void), parameterTypes);
	}

	/// <summary>
	///     Creates a delegate to a constructor of the specified type.
	/// </summary>
	/// <param name="type">The type containing the constructor.</param>
	/// <param name="binder">An optional binder.</param>
	/// <param name="modifiers">Optional parameter modifiers.</param>
	/// <param name="argumentTypes">The types of constructor arguments.</param>
	/// <returns>A delegate to the constructor.</returns>
	[MImp(MImplO.AggressiveInlining)]
	public static Delegate MakeConstructorDelegate(Type type, Binder? binder, ParameterModifier[]? modifiers,
	                                               params Type[] argumentTypes)
	{
		ConstructorInfo ctorInfo = type.GetConstructor(BindingFlags.NonPublic |
		                                               BindingFlags.Public    |
		                                               BindingFlags.Instance,
		                                               binder, argumentTypes, modifiers)
		                           ?? throw new TypeLoadException(
			                           $"{type.Name} does not define any .ctor({String.Join(", ", argumentTypes.Select(x => x.Name))})");

		var         dynamicMethod = new DynamicMethod(String.Empty, type, argumentTypes);
		ILGenerator il            = dynamicMethod.GetILGenerator();

		for (int i = 0; i < argumentTypes.Length; ++i) {
			switch (i) {
				case 0:
					il.Emit(OpCodes.Ldarg_0);
					break;

				case 1:
					il.Emit(OpCodes.Ldarg_1);
					break;

				case 2:
					il.Emit(OpCodes.Ldarg_2);
					break;

				case 3:
					il.Emit(OpCodes.Ldarg_3);
					break;

				default:
					il.Emit(OpCodes.Ldarg, i);
					break;
			}
		}

		il.Emit(OpCodes.Newobj, ctorInfo);
		il.Emit(OpCodes.Ret);

		return dynamicMethod.CreateDelegate(
			MakeNewDelegateType(type, argumentTypes)
		);
	}

	/// <summary>
	///     Creates a delegate to a constructor of the specified type.
	/// </summary>
	/// <param name="type">The type containing the constructor.</param>
	/// <param name="binder">An optional binder.</param>
	/// <param name="modifiers">Optional parameter modifiers.</param>
	/// <param name="argumentTypes">The types of constructor arguments.</param>
	/// <returns>A delegate to the constructor.</returns>
	[MImp(MImplO.AggressiveInlining)]
	public static Delegate MakeConstructorDelegate(Type type, ConstructorInfo ctorInfo, params Type[] argumentTypes)
	{
		var         dynamicMethod = new DynamicMethod(String.Empty, type, argumentTypes);
		ILGenerator il            = dynamicMethod.GetILGenerator();

		for (int i = 0; i < argumentTypes.Length; ++i) {
			switch (i) {
				case 0:
					il.Emit(OpCodes.Ldarg_0);
					break;

				case 1:
					il.Emit(OpCodes.Ldarg_1);
					break;

				case 2:
					il.Emit(OpCodes.Ldarg_2);
					break;

				case 3:
					il.Emit(OpCodes.Ldarg_3);
					break;

				default:
					il.Emit(OpCodes.Ldarg, i);
					break;
			}
		}

		il.Emit(OpCodes.Newobj, ctorInfo);
		il.Emit(OpCodes.Ret);

		return dynamicMethod.CreateDelegate(
			MakeNewDelegateType(type, argumentTypes)
		);
	}

	/// <summary>
	///     Creates a delegate for a constructor of the specified type.
	/// </summary>
	/// <param name="type">The type of the object to construct.</param>
	/// <param name="binder">An optional binder.</param>
	/// <param name="argumentTypes">The types of constructor arguments.</param>
	/// <returns>A delegate for the constructor.</returns>
	[MImp(MImplO.AggressiveInlining)]
	public static Delegate MakeConstructorDelegate(Type type, Binder? binder, params Type[] argumentTypes)
		=> MakeConstructorDelegate(type, binder, null, argumentTypes);

	/// <summary>
	///     Creates a delegate for a constructor of the specified type.
	/// </summary>
	/// <param name="type">The type of the object to construct.</param>
	/// <param name="argumentTypes">The types of constructor arguments.</param>
	/// <returns>A delegate for the constructor.</returns>
	[MImp(MImplO.AggressiveInlining)]
	public static Delegate MakeConstructorDelegate(Type type, params Type[] argumentTypes)
		=> MakeConstructorDelegate(type, null, null, argumentTypes);

	/// <summary>
	///     Creates a delegate for a constructor of the specified type.
	/// </summary>
	/// <param name="argumentTypes">The types of constructor arguments.</param>
	/// <returns>A delegate for the constructor.</returns>
	[MImp(MImplO.AggressiveInlining)]
	public static Delegate MakeConstructorDelegate<T>(params Type[] argumentTypes)
		=> MakeConstructorDelegate(typeof(T), null, null, argumentTypes);

	/// <summary>
	///     Creates a delegate using the provided MethodInfo. Delegates are cached.
	/// </summary>
	/// <param name="methodInfo">The MethodInfo in which to create a new Delegate from.</param>
	/// <param name="instance">The instance that the new Delegate will invoke on.</param>
	/// <param name="throwOnFailure">Whether to throw an exception on failure.</param>
	/// <returns>A delegate representing the provided MethodInfo.</returns>
	public static Delegate MakeDelegate(MI methodInfo, object? instance, bool throwOnFailure = true)
	{
		Type delegateType = MakeNewDelegateType(methodInfo.ReturnType,
		                                        methodInfo.GetParameters().Select(x => x.ParameterType)
			                                        .ToArray());

		return MakeDelegate(delegateType, methodInfo, instance, throwOnFailure);
	}

	/// <summary>
	///     Creates a delegate using the provided MethodInfo. Delegates are cached.
	/// </summary>
	/// <param name="methodInfo">The MethodInfo in which to create a new Delegate from.</param>
	/// <param name="throwOnFailure">Whether to throw an exception on failure.</param>
	/// <returns>A delegate representing the provided MethodInfo.</returns>
	public static Delegate MakeDelegate(MI methodInfo, bool throwOnFailure = true)
		=> MakeDelegate(methodInfo, null, throwOnFailure);

	/// <summary>
	///     Creates a delegate using the provided delegate type and MethodInfo.
	/// </summary>
	/// <param name="delegateType">The type of delegate to create.</param>
	/// <param name="methodInfo">The MethodInfo in which to create a new Delegate from.</param>
	/// <param name="instance">The instance that the new Delegate will invoke on.</param>
	/// <param name="throwOnFailure">Whether to throw an exception on failure.</param>
	/// <returns>A delegate representing the provided MethodInfo.</returns>
	public static Delegate MakeDelegate(Type delegateType, MI methodInfo, object? instance,
	                                    bool throwOnFailure = true)
		=> Delegate.CreateDelegate(delegateType, instance, methodInfo, throwOnFailure)!;

}