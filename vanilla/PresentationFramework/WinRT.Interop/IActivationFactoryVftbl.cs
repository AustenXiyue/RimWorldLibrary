using System.Runtime.InteropServices;

namespace WinRT.Interop;

[Guid("00000035-0000-0000-C000-000000000046")]
internal struct IActivationFactoryVftbl
{
	public delegate int _ActivateInstance(nint pThis, out nint instance);

	public IInspectable.Vftbl IInspectableVftbl;

	public _ActivateInstance ActivateInstance;
}
