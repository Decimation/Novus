using System;
using JetBrains.Annotations;

namespace Novus.Imports
{
	[MeansImplicitUse(ImplicitUseTargetFlags.WithMembers)]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public sealed class NativeStructureAttribute : Attribute { }
}