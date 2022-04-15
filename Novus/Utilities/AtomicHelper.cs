using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Novus.Utilities;

public static class AtomicHelper
{
	private static readonly Dictionary<Type, IntPtr> Cache = new();

	private static IntPtr GetExchangeFunction<T>()
	{
		var method = typeof(Interlocked).GetAnyMethod("Exchange", new[] { typeof(T).MakeByRefType(), typeof(T) });

		if (method == null) {
			return IntPtr.Zero;
		}

		var pointer = method.MethodHandle.GetFunctionPointer();
		return pointer;
	}

	public static unsafe T Exchange<T>(ref T location1, T location2) /*where T : unmanaged*/
	{
/*fixed (T* p = &location1) {
			T   value;
			var v = Interlocked.Exchange(ref *(int*)p, Unsafe.Read<int>(&location2));
			value = *(T*)(&v);
			return Unsafe.Read<T>(&value);
		}*/

		var type = typeof(T);

		if (!Cache.ContainsKey(type)) {
			Cache[type] = GetExchangeFunction<T>();
		}

		var function = (delegate*<ref T, T, T>) Cache[type];

		return function(ref location1, location2);

	}
}