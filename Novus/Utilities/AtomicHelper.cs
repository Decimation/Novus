using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Novus.Utilities;

[Experimental(Global.DIAG_ID_EXPERIMENTAL)]
public static class AtomicHelper
{

	private static readonly Dictionary<Type, nint> Cache = new();

	private static nint GetExchangeFunction<T>()
	{
		var method =
			typeof(Interlocked).GetAnyMethod(nameof(Interlocked.Exchange),
			                                 new[] { typeof(T).MakeByRefType(), typeof(T) });

		if (method == null) {
			return IntPtr.Zero;
		}

		var pointer = method.MethodHandle.GetFunctionPointer();
		return pointer;
	}

	[MethodImpl(Global.IMPL_OPTIONS)]
	public static unsafe T Exchange<T>(ref T location1, T location2) /*where T : unmanaged*/
	{
		/*fixed (T* p = &location1) {
			T   value;
			var v = Interlocked.Exchange(ref *(int*)p, Unsafe.Read<int>(&location2));
			value = *(T*)(&v);
			return Unsafe.Read<T>(&value);
		}*/

		var fn = GetCacheExchangeFunction<T>();

		var function = (delegate*<ref T, T, T>) fn;

		return function(ref location1, location2);

	}

	/// <returns><c>(delegate*&lt;ref T, T, T&gt;)</c></returns>
	[MethodImpl(Global.IMPL_OPTIONS)]
	public static nint GetCacheExchangeFunction<T>()
	{
		var type = typeof(T);

		nint ptr;

		if (!Cache.ContainsKey(type)) {
			ptr         = GetExchangeFunction<T>();
			Cache[type] = ptr;
		}
		else {
			ptr = Cache[type];
		}

		return ptr;
	}

}