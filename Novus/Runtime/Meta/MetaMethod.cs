﻿using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Novus.Memory;
using Novus.Runtime.Meta.Base;
using Novus.Runtime.VM;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace Novus.Runtime.Meta
{
	/// <summary>
	///     <list type="bullet">
	///         <item><description>CLR structure: <see cref="MethodDesc"/>, <see cref="MethodDescChunk"/></description></item>
	///         <item><description>Reflection structure: <see cref="MethodInfo"/></description></item>
	///     </list>
	/// </summary>
	public unsafe class MetaMethod : EmbeddedClrStructure<MethodDesc>
	{
		public MetaMethod(Pointer<MethodDesc> ptr) : base(ptr) { }

		public MetaMethod(MethodInfo member) : base(member) { }

		public int ChunkIndex => Value.Reference.ChunkIndex;

		public int SlotNumber => Value.Reference.SlotNumber;


		public bool IsRuntimeSupplied =>
			Classification == MethodClassification.FCall ||
			Classification == MethodClassification.Array;

		public bool IsNoMetadata => Classification == MethodClassification.Dynamic;
		public bool HasILHeader  => IsIL && !IsUnboxingStub && RVA > default(long);

		private bool IsUnboxingStub => Code.HasFlag(CodeFlags.IsUnboxingStub);

		public bool IsIL => MethodClassification.IL           == Classification ||
		                    MethodClassification.Instantiated == Classification;

		public bool IsInlined
		{
			get
			{

				// https://github.com/dotnet/coreclr/blob/master/src/jit/inline.def
				// https://github.com/dotnet/coreclr/blob/master/src/jit/inline.cpp
				// https://github.com/dotnet/coreclr/blob/master/src/jit/inline.h
				// https://github.com/dotnet/coreclr/blob/master/src/jit/inlinepolicy.cpp
				// https://github.com/dotnet/coreclr/blob/master/src/jit/inlinepolicy.h
				// https://mattwarren.org/2016/03/09/adventures-in-benchmarking-method-inlining/

				throw new NotImplementedException();
			}
		}

		public MethodClassification Classification => Value.Reference.Classification;

		public MethodProperties Properties => Value.Reference.Properties;

		public CodeFlags Code => Value.Reference.Code;

		public ParamFlags ParameterTypes => Value.Reference.Flags3AndTokenRemainder;

		public MethodAttributes Attributes => Info.Attributes;


		public override MethodInfo Info => (MethodInfo) (EnclosingType.RuntimeType).Module.ResolveMethod(Token);

		public void Prepare() => RuntimeHelpers.PrepareMethod(Info.MethodHandle);

		public long RVA => Value.Reference.RVA;

		public override MetaType EnclosingType => Value.Reference.MethodTable;

		public override int Token => Value.Reference.Token;

		public Pointer<byte> NativeCode
		{
			get => Value.Reference.NativeCode;
			// set => Value.Reference.SetNativeCodeInterlocked(value.ToInt64());
		}

		public Pointer<byte> Function
		{
			get => Info.MethodHandle.GetFunctionPointer();
			//set => NativeCode = value;
		}

		public bool IsPointingToNativeCode => Value.Reference.IsPointingToNativeCode;

		// public MetaIL ILHeader
		// {
		// 	get
		// 	{
		//
		// 		// bool
		// 		const int ALLOW_OVERRIDES_PARAM = 0;
		// 		return new MetaIL(Value.Reference.GetILHeader(ALLOW_OVERRIDES_PARAM));
		// 	}
		// }

		public static implicit operator MetaMethod(Pointer<MethodDesc> ptr) => new(ptr);

		public static implicit operator MetaMethod(MethodInfo t) => new(t);
	}
}