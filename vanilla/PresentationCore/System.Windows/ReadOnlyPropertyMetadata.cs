namespace System.Windows;

internal class ReadOnlyPropertyMetadata : PropertyMetadata
{
	private GetReadOnlyValueCallback _getValueCallback;

	internal override GetReadOnlyValueCallback GetReadOnlyValueCallback => _getValueCallback;

	public ReadOnlyPropertyMetadata(object defaultValue, GetReadOnlyValueCallback getValueCallback, PropertyChangedCallback propertyChangedCallback)
		: base(defaultValue, propertyChangedCallback)
	{
		_getValueCallback = getValueCallback;
	}
}
