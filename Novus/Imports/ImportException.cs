using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Novus.Imports;

public sealed class ImportException : Exception
{
	/// <inheritdoc />
	public ImportException() { }

	/// <inheritdoc />
	public ImportException([CBN] string message) : base(message) { }
}