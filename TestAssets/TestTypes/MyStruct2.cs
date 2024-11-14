namespace Test.TestTypes
{
	public struct MyStruct2
	{
		public int a;

		public float f { get; set; }

		public override string ToString()
		{
			return $"{nameof(a)}: {a}, {nameof(f)}: {f}";
		}
	}
}