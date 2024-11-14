using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

// ReSharper disable AssignNullToNotNullAttribute
// ReSharper disable PossibleNullReferenceException

namespace Novus.Imports.Dynamic;
//https://github.com/bytecode77/bytecode-api/tree/master/BytecodeApi/IO/Interop

/// <summary>
/// Represents a native DLL file with functions that can be dynamically generated and called.
/// </summary>
public sealed class DynamicLibrary
{

	/// <summary>
	/// Gets the name of the DLL that is supplied in the constructor of <see cref="DynamicLibrary" />.
	/// </summary>
	public string DllName { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DynamicLibrary" /> class.
	/// </summary>
	/// <param name="dllName">A <see cref="string" /> specifying the path of the DLL file to use.</param>
	public DynamicLibrary(string dllName)
	{
		DllName = dllName;
	}

	/// <summary>
	/// Returns a new <see cref="DynamicLibraryFunction" /> object that can be used to call the function. The function does not return a value.
	/// </summary>
	/// <param name="name">The name of the entry point in the DLL.</param>
	/// <param name="callingConvention">The function's calling convention.</param>
	/// <param name="charSet">The method's native character set.</param>
	/// <param name="parameterTypes">The types of the method's parameters.</param>
	/// <returns>
	/// A new <see cref="DynamicLibraryFunction" /> object that can be used to call the function.
	/// </returns>
	public DynamicLibraryFunction GetFunction(string name, CallingConvention callingConvention, CharSet charSet,
	                                          params Type[] parameterTypes)
		=> new(this, CreateFunctionMethod(name, callingConvention, charSet, typeof(void), parameterTypes));

	/// <summary>
	/// Returns a new <see cref="DynamicLibraryFunction{T}" /> object that can be used to call the function. The function returns a value.
	/// </summary>
	/// <typeparam name="TReturn">The function's return type.</typeparam>
	/// <param name="name">The name of the entry point in the DLL.</param>
	/// <param name="callingConvention">The function's calling convention.</param>
	/// <param name="charSet">The method's native character set.</param>
	/// <param name="parameterTypes">The types of the method's parameters.</param>
	/// <returns>
	/// A new <see cref="DynamicLibraryFunction{T}" /> object that can be used to call the function.
	/// </returns>
	public DynamicLibraryFunction<TReturn> GetFunction<TReturn>(string name, CallingConvention callingConvention,
	                                                            CharSet charSet, params Type[] parameterTypes)
		=> new(this, CreateFunctionMethod(name, callingConvention, charSet, typeof(TReturn), parameterTypes));

	private MI CreateFunctionMethod(string name, CallingConvention callingConvention, CharSet charSet, Type returnType,
	                                Type[] parameterTypes)
	{
		string assemblyName = Path.GetFileNameWithoutExtension(DllName);

		var typeBuilder = AssemblyBuilder
			.DefineDynamicAssembly(new AssemblyName(assemblyName + nameof(DynamicLibrary)), AssemblyBuilderAccess.Run)
			.DefineDynamicModule(assemblyName + "Module")
			.DefineType(assemblyName          + "Imports", TypeAttributes.Class | TypeAttributes.Public);

		typeBuilder
			.DefinePInvokeMethod(name, DllName, name, MethodAttributes.Static | MethodAttributes.Public,
			                     CallingConventions.Standard, returnType, parameterTypes, callingConvention, charSet)
			.SetCustomAttribute(new CustomAttributeBuilder(
				                    typeof(DllImportAttribute).GetConstructor(new[] { typeof(string) }),
				                    new[] { DllName }));

		return typeBuilder.CreateType().GetMethod(name, BindingFlags.Static | BindingFlags.Public);
	}

	/// <summary>
	/// Returns the filename of the DLL of this <see cref="DynamicLibrary" />.
	/// </summary>
	/// <returns>
	/// The filename of the DLL of this <see cref="DynamicLibrary" />.
	/// </returns>
	public override string ToString()
	{
		return Path.GetFileName(DllName);
	}

}