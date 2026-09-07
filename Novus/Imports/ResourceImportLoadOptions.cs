// Author: Deci | Project: Novus | Name: ResourceImportLoadOptions.cs
// Date: 2026/08/30 @ 23:08:16

// ReSharper disable UnusedMember.Global
namespace Novus.Imports;

/// <summary>
/// Options for <see cref="RuntimeResource"/>
/// </summary>
[Flags]
public enum ResourceImportLoadOptions
{

	None                  = 0,
	ThrowOnImportError    = 1 << 0,
	UnloadAllOnDispose    = 1 << 1,
	WriteDefaultOnUnload  = 1 << 2,
	NullOnImportError     = 1 << 3,
	GenerateOnImportError = 1 << 4,

}