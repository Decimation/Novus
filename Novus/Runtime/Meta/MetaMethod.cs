﻿// Author: Deci | Project: Novus | Name: MetaMethod.cs
// Date: 2020/11/22 @ 17:11:28

using System.Reflection;
using System.Runtime.CompilerServices;
using Kantan.Diagnostics;
using Novus.Memory;
using Novus.Runtime.Meta.Base;
using Novus.Runtime.VM;

// ReSharper disable SuggestBaseTypeForParameter

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace Novus.Runtime.Meta;

/// <summary>
///     <list type="bullet">
///         <item>
///             <description>CLR structure: <see cref="MethodDesc" />, <see cref="MethodDescChunk" /></description>
///         </item>
///         <item>
///             <description>Reflection structure: <see cref="MethodInfo" /></description>
///         </item>
///     </list>
/// </summary>
public unsafe class MetaMethod : EmbeddedClrStructure<MethodDesc>
{

	public MetaMethod(Pointer<MethodDesc> ptr) : base(ptr) { }

	public MetaMethod(MethodInfo member) : base(member) { }

	public int ChunkIndex => Value.Reference.ChunkIndex;

	public int SlotNumber => Value.Reference.SlotNumber;

	public bool IsRuntimeSupplied => Classification is MethodClassification.FCall or MethodClassification.Array;

	public bool IsNoMetadata => Classification == MethodClassification.Dynamic;

	public bool HasILHeader => IsIL && !IsUnboxingStub && RVA > default(long);

	private bool IsUnboxingStub => (Flags3 & (ushort) CodeFlags.IsUnboxingStub) != 0;

	public bool IsIL => Classification is MethodClassification.IL or MethodClassification.Instantiated;


	/// <summary>
	/// https://github.com/dotnet/coreclr/blob/master/src/jit/inline.def
	///	https://github.com/dotnet/coreclr/blob/master/src/jit/inline.cpp
	///	https://github.com/dotnet/coreclr/blob/master/src/jit/inline.h
	///	https://github.com/dotnet/coreclr/blob/master/src/jit/inlinepolicy.cpp
	///	https://github.com/dotnet/coreclr/blob/master/src/jit/inlinepolicy.h
	///	https://mattwarren.org/2016/03/09/adventures-in-benchmarking-method-inlining/
	/// </summary>
	public bool IsInlined => throw new NotImplementedException();

	public MethodClassification Classification => Value.Reference.Classification;

	public MethodProperties Properties => Value.Reference.Properties;

	// public CodeFlags CodeFlags => Value.Reference.CodeFlags;

	public ushort Flags3 => Value.Reference.Flags3AndTokenRemainder;

	public MethodAttributes Attributes => Info.Attributes;

	public override MethodInfo Info => (MethodInfo) (EnclosingType.RuntimeType).Module.ResolveMethod(Token);

	public RuntimeMethodHandle RuntimeMethodHandle => Value.Reference.RuntimeMethodHandle;

	public long RVA => Value.Reference.RVA;

	public override MetaType EnclosingType => Value.Reference.MethodTable;

	public override int Token => Value.Reference.Token;

	public Pointer<byte> NativeCode => Value.Reference.NativeCode;

	// set => Value.Reference.SetNativeCodeInterlocked(value.ToInt64());
	public Pointer<byte> Function => Info.MethodHandle.GetFunctionPointer();

	public Pointer<byte> EntryPoint
	{
		set => Value.Reference.EntryPoint = (void*) value;
	}

	//set => NativeCode = value;
	public bool IsPointingToNativeCode => Value.Reference.IsPointingToNativeCode;

	public MetaIL ILHeader
	{
		get
		{
			Require.Assert(HasILHeader);
			return new MetaIL(Value.Reference.ILHeader);
		}
	}


	public void Reset()
	{
		Value.Reference.Reset();
	}

	public void Prepare()
	{
		RuntimeHelpers.PrepareMethod(Info.MethodHandle);
	}

	public static implicit operator MetaMethod(Pointer<MethodDesc> ptr)
		=> new(ptr);

	public static implicit operator MetaMethod(MethodInfo t)
		=> new(t);

}