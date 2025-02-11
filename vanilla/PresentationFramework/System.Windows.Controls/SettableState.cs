namespace System.Windows.Controls;

internal struct SettableState<T>
{
	internal T _value;

	internal bool _isSet;

	internal bool _wasSet;

	internal SettableState(T value)
	{
		_value = value;
		_isSet = (_wasSet = false);
	}
}
