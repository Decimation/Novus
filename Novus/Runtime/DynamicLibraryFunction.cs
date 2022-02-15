using System;
using System.Diagnostics;
using System.Reflection;

namespace Novus.Runtime;
//https://github.com/bytecode77/bytecode-api/tree/master/BytecodeApi/IO/Interop

/// <summary>
/// Represents the function of a native DLL file that returns a value of the specified type.
/// </summary>
/// <typeparam name="T">The function's return type.</typeparam>
public sealed class DynamicLibraryFunction<T>
{
	private readonly DynamicLibraryFunction m_value;

	public DynamicLibrary Library => m_value.Library;

	public string Name => m_value.Name;

	public MethodInfo Method => m_value.Method;

	internal DynamicLibraryFunction(DynamicLibrary library, MethodInfo method)
	{
		m_value = new(library, method);
	}

	public override string ToString()
	{
		return m_value.ToString();
	}

	/// <summary>
	/// Calls the function with the specified parameters.
	/// </summary>
	/// <param name="parameters">A collection of parameters. The number of parameters must match the number of parameter types upon creation.</param>
	public T Call(params object[] parameters)
	{
		return (T) Method.Invoke(null, parameters);
	}
}

/// <summary>
/// Represents the function of a native DLL file that does not return a value.
/// </summary>
public sealed class DynamicLibraryFunction
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public MethodInfo Method { get; }


	/// <summary>
	/// Gets the <see cref="DynamicLibrary" /> that was used to create this <see cref="DynamicLibraryFunction" />.
	/// </summary>
	public DynamicLibrary Library { get; }

	public string Name => Method.Name;

	internal DynamicLibraryFunction(DynamicLibrary library, MethodInfo method)
	{
		Library = library;
		Method  = method;
	}

	/// <summary>
	/// Calls the function with the specified parameters.
	/// </summary>
	/// <param name="parameters">A collection of parameters. The number of parameters must match the number of parameter types upon creation.</param>
	public void Call(params object[] parameters)
	{
		Method.Invoke(null, parameters);
	}

	public override string ToString()
	{
		return Name;
	}
}