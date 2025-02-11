namespace WinRT.Interop;

internal struct IDelegateVftbl
{
	public IUnknownVftbl IUnknownVftbl;

	public nint Invoke;
}
