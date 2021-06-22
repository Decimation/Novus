// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Local

using System;

namespace Novus.Win32.Wrappers
{
	/*
	 * https://github.com/migueldeicaza/NStack
	 */

	// TODO: WIP

	public unsafe struct ustring
	{
		private sbyte* m_value;

		public ustring(sbyte* p)
		{
			m_value = p;
		}

		public string Value => new string(m_value);

		public char this[int i] => (char) m_value[i];

		public static implicit operator ustring(sbyte* p) => new ustring(p);

		public override string ToString()
		{
			return Value;
		}
	}
}