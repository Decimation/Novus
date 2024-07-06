using System;
using System.Reflection;
using Novus.Memory;
using Novus.Runtime.Meta.Base;
using Novus.Runtime.VM;
using Novus.Runtime.VM.IL;

// ReSharper disable UnusedMember.Global

// ReSharper disable InconsistentNaming

namespace Novus.Runtime.Meta;

/// <summary>
///     <list type="bullet">
///         <item><description>CLR structure: <see cref="CorILMethod"/>, <see cref="CorILMethodFat"/>, <see cref="CorILMethodTiny"/></description></item>
///         <item><description>Reflection structure: <see cref="MethodBody"/></description></item>
///     </list>
/// </summary>
public class MetaIL : BaseClrStructure<CorILMethod>
{
	/*
	 * https://github.com/Decimation/NeoCore/blob/master/NeoCore/CoreClr/Meta/MetaIL.cs
	 */

	public MetaIL(Pointer<CorILMethod> ptr) : base(ptr) { }

	public bool IsFat => Value.Reference.Fat.IsFat;

	public bool IsTiny => Value.Reference.Tiny.IsTiny;

	public CorILMethodFlags Flags => IsFat ? Value.Reference.Fat.Flags : throw new NotImplementedException();

	public int CodeSize => IsFat ? Value.Reference.Fat.CodeSize : Value.Reference.Tiny.CodeSize;

	public Pointer<byte> Code => IsFat ? Value.Reference.Fat.Code : Value.Reference.Tiny.Code;

	/// <summary>
	/// Equals <see cref="MethodBody.GetILAsByteArray"/>
	/// </summary>
	public byte[] GetCodeIL()
		=> IsFat ? Value.Reference.Fat.GetCodeIL() : Value.Reference.Tiny.GetCodeIL();

	/// <summary>
	/// Equals <see cref="MethodBody.MaxStackSize"/>
	/// </summary>
	public int MaxStackSize => IsFat ? Value.Reference.Fat.MaxStackSize : Value.Reference.Tiny.MaxStackSize;

	/// <summary>
	/// Equals <see cref="MethodBody.LocalSignatureMetadataToken"/>
	/// </summary>
	public int LocalVarSigToken =>
		IsFat ? Value.Reference.Fat.LocalVarSigToken : Value.Reference.Tiny.LocalVarSigToken;
}