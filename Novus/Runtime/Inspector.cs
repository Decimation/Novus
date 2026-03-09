using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Novus.Memory;
using Novus.Runtime.Meta;
using Novus.Utilities;
using Kantan.Model;
using Kantan.Utilities;

// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
// ReSharper disable UnusedMember.Global
// ReSharper disable CognitiveComplexity

namespace Novus.Runtime;

// TODO: UPDATE/WIP

/// <summary>
///     Utilities for inspecting and analyzing objects, data, etc.
/// </summary>
/// 
/// <seealso cref="Mem" />
/// <seealso cref="RuntimeProperties" />
/// <seealso cref="Utilities.ReflectionHelper" />
[Experimental(Global.DIAG_ID_EXPERIMENTAL)]
public static class Inspector
{
	[Flags]
	public enum InspectorOptions
	{
		None = 0,

		Offset  = 1,
		Size    = 1 << 1,
		Type    = 1 << 2,
		Name    = 1 << 3,
		Address = 1 << 4,
		Value   = 1 << 5,

		All = Offset | Size | Type | Name | Address | Value
	}

}