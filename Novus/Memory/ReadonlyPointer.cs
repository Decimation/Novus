global using MImp = System.Runtime.CompilerServices.MethodImplAttribute;
global using MImplO = System.Runtime.CompilerServices.MethodImplOptions;
using System.Runtime.CompilerServices;

namespace Novus.Memory;

// TODO: WIP

public readonly struct ReadonlyPointer<T>
{
	private const MethodImplOptions OPT = MethodImplOptions.AggressiveInlining |
	                                      MethodImplOptions.AggressiveOptimization;

	private readonly Pointer<T> m_value;

	public ReadonlyPointer(Pointer<T> value)
	{
		m_value = value;
	}

	public static implicit operator ReadonlyPointer<T>(Pointer<T> p) => new(p);

	public ref T Reference
	{
		[method: MImp(OPT)] get => ref m_value.Reference;
	}

	public T Value
	{
		[method: MImp(OPT)] get => m_value.Value;
	}

	[MImp(OPT)]
	public ReadonlyPointer<T> Cast() => m_value;
}