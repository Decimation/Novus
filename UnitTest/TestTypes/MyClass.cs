namespace UnitTest.TestTypes
{
	internal class MyClass
	{
		public string s;
		public int    a;

		protected bool Equals(MyClass other)
		{
			return s == other.s && a == other.a;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((MyClass) obj);
		}

		public override int GetHashCode()
		{
			unchecked {
				return ((s != null ? s.GetHashCode() : 0) * 397) ^ a;
			}
		}

		public override string ToString()
		{
			return $"{nameof(s)}: {s}, {nameof(a)}: {a}";
		}
	}
}