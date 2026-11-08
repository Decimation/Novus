using System;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

// ReSharper disable AssignNullToNotNullAttribute
// ReSharper disable PossibleNullReferenceException

namespace Novus.Imports.Dynamic;



/// <summary>
/// Represents a native DLL file with functions that can be dynamically generated and called.
/// </summary>
/// <remarks><a href="https://github.com/bytecode77/bytecode-api/tree/master/BytecodeApi/IO/Interop">Source</a></remarks>
public sealed class DynamicLibrary
{

	private readonly TypeBuilder       m_typeBuilder;
	private readonly List<DynamicLibraryFunction> m_funcs;

	/// <summary>
	/// Gets the name of the DLL that is supplied in the constructor of <see cref="DynamicLibrary" />.
	/// </summary>
	public string DllName { get; }

	public string AssemblyName { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DynamicLibrary" /> class.
	/// </summary>
	/// <param name="dllName">A <see cref="string" /> specifying the path of the DLL file to use.</param>
	public DynamicLibrary(string dllName)
	{
		DllName      = dllName;
		AssemblyName = Path.GetFileNameWithoutExtension(DllName);

		m_typeBuilder = AssemblyBuilder
		                .DefineDynamicAssembly(new AssemblyName(AssemblyName + nameof(DynamicLibrary)), AssemblyBuilderAccess.Run)
		                .DefineDynamicModule(AssemblyName + "Module")
		                .DefineType(AssemblyName          + "Imports", TypeAttributes.Class | TypeAttributes.Public);
		m_funcs = [];
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
	public DynamicLibraryFunction GetDynamicFunction(string name, CallingConvention callingConvention, CharSet charSet, params Type[] parameterTypes)
		=> GetDynamicFunction(name, callingConvention, charSet, typeof(void), parameterTypes);

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
	public DynamicLibraryFunction GetDynamicFunction<TReturn>(string name, CallingConvention callingConvention, CharSet charSet, params Type[] parameterTypes)
		=> GetDynamicFunction(name, callingConvention, charSet, typeof(TReturn), parameterTypes);

	public DynamicLibraryFunction GetDynamicFunction(string name, CallingConvention callingConvention, CharSet charSet, Type returnType, params Type[] parameterTypes)
	{
		var exist = m_funcs.Find(x => x.Method.Name == name
		                              && x.Method.GetParameters().Select(static p => p.ParameterType).SequenceEqual(parameterTypes)
		                              && x.Method.ReturnType == returnType);

		if (exist == default) {
			exist = CreateFunctionMethod(name, callingConvention, charSet, returnType, parameterTypes);
			m_funcs.Add(exist);
		}

		return exist;
	}

	private DynamicLibraryFunction CreateFunctionMethod(string name, CallingConvention callingConvention, CharSet charSet, Type returnType, Type[] parameterTypes)
	{
		m_typeBuilder.DefinePInvokeMethod(name, DllName, name, MethodAttributes.Static | MethodAttributes.Public,
		                                  CallingConventions.Standard, returnType, parameterTypes, callingConvention, charSet)
		             .SetCustomAttribute(new CustomAttributeBuilder(typeof(DllImportAttribute).GetConstructor([typeof(string)]), [DllName]));

		var pinvType   = m_typeBuilder.CreateType();
		var pinvMethod = pinvType.GetMethod(name, BindingFlags.Static | BindingFlags.Public);

		return new DynamicLibraryFunction(pinvType, pinvMethod);
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