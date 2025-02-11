namespace System.Windows;

internal class ReadOnlyFrameworkPropertyMetadata : FrameworkPropertyMetadata
{
	private GetReadOnlyValueCallback _getValueCallback;

	internal override GetReadOnlyValueCallback GetReadOnlyValueCallback => _getValueCallback;

	public ReadOnlyFrameworkPropertyMetadata(object defaultValue, GetReadOnlyValueCallback getValueCallback)
		: base(defaultValue)
	{
		_getValueCallback = getValueCallback;
	}
}
