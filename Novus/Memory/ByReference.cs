using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Novus.Memory;

public readonly ref struct ByReference<T>
{
	public ByReference() : this(ref U.NullRef<T>()) { }

	public ByReference(ref T value)
	{
		AsPointer = new(ref value);
	}

	public Pointer<T> AsPointer { get; }

	public ref T Value => ref AsPointer.Reference;
}