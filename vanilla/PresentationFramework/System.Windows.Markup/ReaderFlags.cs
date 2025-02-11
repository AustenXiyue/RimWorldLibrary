namespace System.Windows.Markup;

internal enum ReaderFlags : ushort
{
	Unknown = 0,
	DependencyObject = 4096,
	ClrObject = 8192,
	PropertyComplexClr = 12288,
	PropertyComplexDP = 16384,
	PropertyArray = 20480,
	PropertyIList = 24576,
	PropertyIDictionary = 28672,
	PropertyIAddChild = 32768,
	RealizeDeferContent = 36864,
	ConstructorParams = 40960,
	ContextTypeMask = 61440,
	StyleObject = 256,
	FrameworkTemplateObject = 512,
	TableTemplateObject = 1024,
	SingletonConstructorParam = 2048,
	NeedToAddToTree = 1,
	AddedToTree = 2,
	InjectedElement = 4,
	CollectionHolder = 8,
	IDictionary = 16,
	IList = 32,
	ArrayExt = 64,
	IAddChild = 128
}
