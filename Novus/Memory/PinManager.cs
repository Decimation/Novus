// Author: Deci | Project: Novus | Name: PinManager.cs
// Date: 2026/04/11 @ 12:04:37

#region

using System.Diagnostics;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Novus.Runtime;

#endregion

// ReSharper disable ClassCannotBeInstantiated

namespace Novus.Memory;

public static class PinManager
{

	static PinManager()
	{
		s_pinImpl = RuntimeFeature.IsDynamicCodeSupported
			            ? CreatePinImpl()
			            : static (_, _) => throw new NotSupportedException("Dynamic code is not supported");
	}

	private static readonly Action<object, Action<object>> s_pinImpl;

	private static readonly Dictionary<object, ManualResetEvent> s_pinResetEvents = [];

	[NN]
	private static Action<object, Action<object>> CreatePinImpl()
	{

		var method = new DynamicMethod("InvokeWhilePinnedImpl", typeof(void),
		                               [typeof(object), typeof(Action<object>)],
		                               typeof(ObjectUtility).Module);

		ILGenerator il = method.GetILGenerator();

		// create a pinned local variable of type object
		// this wouldn't be valid in C#, but the runtime doesn't complain about the IL
		LocalBuilder local = il.DeclareLocal(typeof(object), true);

		// store first argument obj in the pinned local variable
		il.Emit(OpCodes.Ldarg_0);
		il.Emit(OpCodes.Stloc_0);

		// invoke the delegate
		il.Emit(OpCodes.Ldarg_1);
		il.Emit(OpCodes.Ldarg_0);
		il.EmitCall(OpCodes.Callvirt, typeof(Action<object>).GetMethod("Invoke")!, null);

		il.Emit(OpCodes.Ret);

		return (Action<object, Action<object>>) method.CreateDelegate(typeof(Action<object, Action<object>>));
	}

	/// <summary>
	///     <paramref name="obj" /> will be *temporarily* pinned while action is being invoked
	/// </summary>
	public static void InvokeWhilePinned(object obj, Action<object> action)
		=> s_pinImpl(obj, action);

	public static bool IsPinned(object obj)
		=> s_pinResetEvents.ContainsKey(obj);

	public static unsafe bool Pin(object obj, [CBN] object s = null)
	{
		var value = new ManualResetEvent(false);

		if (s_pinResetEvents.TryAdd(obj, value)) {
			var b = ThreadPool.QueueUserWorkItem(_ =>
			{
				fixed (byte* p = &obj.AsObjectProxy().Data) {
					value.WaitOne();
				}
			}, s);

			if (b) {
				Debug.WriteLine($"Pinned obj: {obj.GetHashCode()}");

			}

			return b;
		}

		return false;
	}

	public static bool Unpin(object obj)
	{

		if (s_pinResetEvents.TryGetValue(obj, out var p)) {
			var o = p.Set();

			if (o) {
				Debug.WriteLine($"Unpinned obj: {obj.GetHashCode()}");
				o = s_pinResetEvents.Remove(obj);
			}

			return o;
		}

		return false;
	}

}