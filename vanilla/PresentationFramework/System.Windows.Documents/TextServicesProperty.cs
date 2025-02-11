using MS.Win32;

namespace System.Windows.Documents;

internal class TextServicesProperty
{
	private TextServicesPropertyRanges _propertyRanges;

	private readonly TextStore _textstore;

	internal TextServicesProperty(TextStore textstore)
	{
		_textstore = textstore;
	}

	internal void OnEndEdit(MS.Win32.UnsafeNativeMethods.ITfContext context, int ecReadOnly, MS.Win32.UnsafeNativeMethods.ITfEditRecord editRecord)
	{
		if (_propertyRanges == null)
		{
			_propertyRanges = new TextServicesDisplayAttributePropertyRanges(_textstore);
		}
		_propertyRanges.OnEndEdit(context, ecReadOnly, editRecord);
	}

	internal void OnLayoutUpdated()
	{
		if (_propertyRanges is TextServicesDisplayAttributePropertyRanges textServicesDisplayAttributePropertyRanges)
		{
			textServicesDisplayAttributePropertyRanges.OnLayoutUpdated();
		}
	}
}
