using System;
using JetBrains.Annotations;

namespace Novus.Imports.Attributes;

[MeansImplicitUse(ImplicitUseTargetFlags.WithMembers)]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class NativeStructureAttribute : Attribute { }