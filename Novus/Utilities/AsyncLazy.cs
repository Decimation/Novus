using System.Runtime.CompilerServices;

namespace Novus.Utilities;

public class AsyncLazy<T> : Lazy<Task<T>>
{

	public AsyncLazy(Func<T> valueFactory, CancellationToken ct = default) :
		base(() => Task.Factory.StartNew(valueFactory, ct)) { }

	public AsyncLazy(Func<Task<T>> taskFactory, CancellationToken ct = default) :
		base(() => Task.Factory.StartNew(taskFactory, ct).Unwrap()) { }

	public AsyncLazy(Func<object, T> valueFactory, CancellationToken ct = default)
		: base(() => Task.Factory.StartNew(valueFactory, ct)) { }

	public AsyncLazy(Func<object, Task<T>> valueFactory, CancellationToken ct = default)
		: base(() => Task.Factory.StartNew(valueFactory, ct).Unwrap()) { }

	/*public AsyncLazy(Func<object, Task<T>> valueFactory, CancellationToken ct = default)
	: base(() => Task.Factory.StartNew(z => valueFactory(z)), ct) { }*/

	public TaskAwaiter<T> GetAwaiter()
	{
		return Value.GetAwaiter();
	}

}