using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novus.Imports;

using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

public sealed class DelegateFactory
{

	/// <summary>
	/// Creates a new delegate type.
	/// </summary>
	/// <param name="name">Name of the delegate type.</param>
	/// <param name="returnType">Return type of the delegate.</param>
	/// <param name="parameterTypes">Parameter types of the delegate.</param>
	/// <returns>The newly created delegate type.</returns>
	[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
	public static Type MakeNewDelegateType(string name, Type returnType, params Type[] parameterTypes)
	{
		TypeBuilder typeBuilder = TypeFactory.DefineDelegateType(name, returnType, parameterTypes);
		return typeBuilder.CreateTypeInfo().AsType();
	}

	/// <summary>
	/// Creates a new delegate type with the specified return type and parameter types.
	/// </summary>
	/// <param name="returnType">The return type of the delegate.</param>
	/// <param name="parameterTypes">The parameter types of the delegate.</param>
	/// <returns>The created delegate type.</returns>
	[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
	public static Type MakeNewDelegateType(Type returnType, params Type[] parameterTypes)
	{
		if (returnType == typeof(void))
			return MakeNewDelegateType(parameterTypes);

		string typeNames = string.Join("_", parameterTypes.Select(x => x.Name).Append(returnType.Name));

		return MakeNewDelegateType(
			$"CassowaryMulticast{(returnType != typeof(void) ? '_' : string.Empty)}{typeNames}", returnType,
			parameterTypes);
	}

	/// <summary>
	/// Creates a new delegate type with the specified parameter types.
	/// </summary>
	/// <param name="parameterTypes">The parameter types of the delegate.</param>
	/// <returns>The created delegate type.</returns>
	[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
	public static Type MakeNewDelegateType(params Type[] parameterTypes)
	{
		string typeNames = string.Join("_", parameterTypes.Select(x => x.Name));

		return MakeNewDelegateType($"CassowaryMulticast{(typeNames.Length > 0 ? '_' : string.Empty)}{typeNames}",
		                           typeof(void), parameterTypes);
	}

	/// <summary>
	/// Creates a delegate to a constructor of the specified type.
	/// </summary>
	/// <param name="type">The type containing the constructor.</param>
	/// <param name="binder">An optional binder.</param>
	/// <param name="modifiers">Optional parameter modifiers.</param>
	/// <param name="argumentTypes">The types of constructor arguments.</param>
	/// <returns>A delegate to the constructor.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Delegate MakeConstructorDelegate(Type type, Binder? binder, ParameterModifier[]? modifiers,
	                                               params Type[] argumentTypes)
	{
		ConstructorInfo ctorInfo = type.GetConstructor(
			                           BindingFlags.NonPublic |
			                           BindingFlags.Public |
			                           BindingFlags.Instance,
			                           binder,
			                           argumentTypes,
			                           modifiers)
		                           ?? throw new TypeLoadException(
			                           $"{type.Name} does not define any .ctor({string.Join(", ", argumentTypes.Select(x => x.Name))})");

		DynamicMethod dynamicMethod = new DynamicMethod(string.Empty, type, argumentTypes);
		ILGenerator   il            = dynamicMethod.GetILGenerator();

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
	/// Creates a delegate to a constructor of the specified type.
	/// </summary>
	/// <param name="type">The type containing the constructor.</param>
	/// <param name="binder">An optional binder.</param>
	/// <param name="modifiers">Optional parameter modifiers.</param>
	/// <param name="argumentTypes">The types of constructor arguments.</param>
	/// <returns>A delegate to the constructor.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Delegate MakeConstructorDelegate(Type type, ConstructorInfo ctorInfo, params Type[] argumentTypes)
	{
		DynamicMethod dynamicMethod = new DynamicMethod(string.Empty, type, argumentTypes);
		ILGenerator   il            = dynamicMethod.GetILGenerator();

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
	/// Creates a delegate for a constructor of the specified type.
	/// </summary>
	/// <param name="type">The type of the object to construct.</param>
	/// <param name="binder">An optional binder.</param>
	/// <param name="argumentTypes">The types of constructor arguments.</param>
	/// <returns>A delegate for the constructor.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Delegate MakeConstructorDelegate(Type type, Binder? binder, params Type[] argumentTypes)
	{
		return MakeConstructorDelegate(type, binder, null, argumentTypes);
	}

	/// <summary>
	/// Creates a delegate for a constructor of the specified type.
	/// </summary>
	/// <param name="type">The type of the object to construct.</param>
	/// <param name="argumentTypes">The types of constructor arguments.</param>
	/// <returns>A delegate for the constructor.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Delegate MakeConstructorDelegate(Type type, params Type[] argumentTypes)
	{
		return MakeConstructorDelegate(type, null, null, argumentTypes);
	}

	/// <summary>
	/// Creates a delegate for a constructor of the specified type.
	/// </summary>
	/// <param name="argumentTypes">The types of constructor arguments.</param>
	/// <returns>A delegate for the constructor.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Delegate MakeConstructorDelegate<T>(params Type[] argumentTypes)
	{
		return MakeConstructorDelegate(typeof(T), null, null, argumentTypes);
	}

	/// <summary>
	/// Creates a delegate using the provided MethodInfo. Delegates are cached.
	/// </summary>
	/// <param name="methodInfo">The MethodInfo in which to create a new Delegate from.</param>
	/// <param name="instance">The instance that the new Delegate will invoke on.</param>
	/// <param name="throwOnFailure">Whether to throw an exception on failure.</param>
	/// <returns>A delegate representing the provided MethodInfo.</returns>
	public static Delegate MakeDelegate(MethodInfo methodInfo, object? instance, bool throwOnFailure = true)
	{
		Type delegateType = MakeNewDelegateType(methodInfo.ReturnType,
		                                        methodInfo.GetParameters().Select(x => x.ParameterType).ToArray());
		return MakeDelegate(delegateType, methodInfo, instance, throwOnFailure);
	}

	/// <summary>
	/// Creates a delegate using the provided MethodInfo. Delegates are cached.
	/// </summary>
	/// <param name="methodInfo">The MethodInfo in which to create a new Delegate from.</param>
	/// <param name="throwOnFailure">Whether to throw an exception on failure.</param>
	/// <returns>A delegate representing the provided MethodInfo.</returns>
	public static Delegate MakeDelegate(MethodInfo methodInfo, bool throwOnFailure = true)
	{
		return MakeDelegate(methodInfo, null, throwOnFailure);
	}

	/// <summary>
	/// Creates a delegate using the provided delegate type and MethodInfo.
	/// </summary>
	/// <param name="delegateType">The type of delegate to create.</param>
	/// <param name="methodInfo">The MethodInfo in which to create a new Delegate from.</param>
	/// <param name="instance">The instance that the new Delegate will invoke on.</param>
	/// <param name="throwOnFailure">Whether to throw an exception on failure.</param>
	/// <returns>A delegate representing the provided MethodInfo.</returns>
	public static Delegate MakeDelegate(Type delegateType, MethodInfo methodInfo, object? instance,
	                                    bool throwOnFailure = true)
	{
		return Delegate.CreateDelegate(delegateType, instance, methodInfo, throwOnFailure)!;
	}

}