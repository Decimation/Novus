using System;
using JetBrains.Annotations;

namespace Novus.Imports.Attributes;

/// <summary>
/// Denotes a structure in native unmanaged memory for interop.
/// </summary>
[MIU(ImplicitUseTargetFlags.WithMembers)]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class NativeStructureAttribute : Attribute { }