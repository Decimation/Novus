namespace UnitTest.TestTypes;

internal interface IInter1<T>
{
	static abstract ref T Ref { get; }
}