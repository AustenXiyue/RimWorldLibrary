namespace System.Windows.Ink;

internal class ExtendedPropertiesChangedEventArgs : EventArgs
{
	private ExtendedProperty _oldProperty;

	private ExtendedProperty _newProperty;

	internal ExtendedProperty OldProperty => _oldProperty;

	internal ExtendedProperty NewProperty => _newProperty;

	internal ExtendedPropertiesChangedEventArgs(ExtendedProperty oldProperty, ExtendedProperty newProperty)
	{
		if (oldProperty == null && newProperty == null)
		{
			throw new ArgumentNullException("oldProperty");
		}
		_oldProperty = oldProperty;
		_newProperty = newProperty;
	}
}
