using MS.Win32;

namespace System.Windows.Input;

internal class TextServicesCompartmentEventSink : MS.Win32.UnsafeNativeMethods.ITfCompartmentEventSink
{
	private InputMethod _inputmethod;

	internal TextServicesCompartmentEventSink(InputMethod inputmethod)
	{
		_inputmethod = inputmethod;
	}

	public void OnChange(ref Guid rguid)
	{
		_inputmethod.OnChange(ref rguid);
	}
}
