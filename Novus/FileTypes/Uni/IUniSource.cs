// Deci Novus IUniSource.cs
// $File.CreatedYear-$File.CreatedMonth-9 @ 2:39

namespace Novus.FileTypes.Uni;

public interface IUniSource<TOut>
{

	static IUniSource()
	{
		// var del = DelegateFactory.MakeDelegate(typeof(IUni<T, TOut>).GetAnyMethod(nameof(IsType)), null);
		// UniSource.Register.Add(del);
	}

	// public TOut Val { get; }

	public static abstract bool IsType(object o, out TOut t2);

}