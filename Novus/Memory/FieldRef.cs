using System;
using System.Linq.Expressions;
using Novus.Runtime.Meta;
using Novus.Utilities;

namespace Novus.Memory
{
	public sealed class FieldRef<T>
	{
		public Pointer<T> Address { get; }

		public ref T Value => ref Address.Reference;

		public FieldRef(Expression<Func<T>> expression, object o = null)
		{
			var a = (MemberExpression) expression.Body;
			var m = a.Member;

			Address = Mem.AddressOfField<T>(m.DeclaringType, m.Name, o);

		}

		public FieldRef(object o, string s) : this(o.GetType(), s, o) { }

		public FieldRef(Type t, string s, object o = null)
		{
			Address = Mem.AddressOfField<T>(t, s, o);
		}

		public override string ToString()
		{
			return $"{typeof(T)}& = {Value}";
		}
	}
}