namespace Test.TestTypes
{
	internal class MyClass3
	{
		public int a;

		public float f { get; set; }

		public MyClass3(int a, float f)
		{
			this.a = a;
			this.f = f;
		}

		public override string ToString()
		{
			return $"{nameof(a)}: {a}, {nameof(f)}: {f}";
		}
	}
}