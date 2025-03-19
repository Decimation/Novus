using System.Collections.Concurrent;
using System.Reflection.Emit;

namespace Novus.Imports.Factory;

public static class MethodFactory
{

	private static readonly ConcurrentDictionary<Type, DynamicMethod> s_throwingFunctions;

	static MethodFactory()
	{
		s_throwingFunctions = new ConcurrentDictionary<Type, DynamicMethod>();
	}

	public static DynamicMethod GetOrGenerateThrowingFunction(Type fieldType)
	{
		//todo
		if (s_throwingFunctions.TryGetValue(fieldType, out DynamicMethod dyn)) {
			goto ret;
		}

		var unmg = fieldType.IsUnmanagedFunctionPointer;

		var fnPtr = fieldType.GetFunctionPointerReturnType();

		// Type modifiedType = field.GetModifiedFieldType();

		var types = fieldType.GetFunctionPointerParameterTypes();

		dyn = new DynamicMethod("__err", fnPtr, types);
		var gen = dyn.GetILGenerator();

		for (int i = 0; i < types.Length; i++) {
			gen.Emit(OpCodes.Ldarg, i);
			gen.Emit(OpCodes.Newobj, fnPtr);
			gen.Emit(OpCodes.Ret);
		}

		s_throwingFunctions.TryAdd(fieldType, dyn);
	ret:
		return dyn;
	}

}