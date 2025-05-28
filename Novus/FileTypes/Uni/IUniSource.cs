// Deci Novus IUniSource.cs
// $File.CreatedYear-$File.CreatedMonth-9 @ 2:39

namespace Novus.FileTypes.Uni;

public interface IUniSource
{

	static IUniSource()
	{
		// var del = DelegateFactory.MakeDelegate(typeof(IUni<T, TOut>).GetAnyMethod(nameof(IsType)), null);
		// UniSource.Register.Add(del);
	}

	// public TOut Val { get; }

	// public delegate bool IsTypePredicateCallback(object a, out object b);

	// public static abstract bool IsType(object o, out object t2);

	// public static abstract Task<UniSource> Create(object t);
}
