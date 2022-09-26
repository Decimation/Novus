using System.Runtime.CompilerServices;

namespace Novus.Memory;

// TODO: WIP

public readonly struct ReadonlyPointer<T>
{
	private readonly Pointer<T> m_value;

	public ReadonlyPointer(Pointer<T> value)
	{
		m_value = value;
	}

	public static implicit operator ReadonlyPointer<T>(Pointer<T> p) => new(p);

	public readonly ref T Reference
	{
		[method: MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get { return ref m_value.Reference; }
	}

	public readonly T Value
	{
		[method: MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get { return m_value.Value; }
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public readonly Pointer<T> Cast() => m_value;
}