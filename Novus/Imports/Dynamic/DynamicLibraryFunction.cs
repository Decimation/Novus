using System;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;

namespace Novus.Imports.Dynamic;

public class DynamicLibraryFunction
{

	public Type Type { get; }

	public MethodInfo Method { get; }

	public DynamicLibraryFunction(Type type, MethodInfo method)
	{
		Type   = type;
		Method = method;
	}

	public T Call<T>(params object[] args)
	{
		return (T) Method.Invoke(null, args);
	}

	public override string ToString()
	{
		return $"{nameof(Type)}: {Type}, {nameof(Method)}: {Method}";
	}

}