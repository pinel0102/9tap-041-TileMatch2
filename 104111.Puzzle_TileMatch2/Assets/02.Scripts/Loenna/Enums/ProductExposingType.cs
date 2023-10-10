[System.Flags]
public enum ProductExposingType : int
{
	None = 0,
	Home = 1,
	Store = 2,
	StoreAndHome = Home | Store,
	Custom = 4
}
