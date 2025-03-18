using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Novus.Imports.Attributes;

[MIU(ImplicitUseTargetFlags.WithInheritors)]
[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public class ImportInitializerAttribute : Attribute
{

	public ImportInitializerAttribute(string name)
	{ }

	public ImportInitializerAttribute() : this(null) { }

}