using System;

namespace MS.Internal.Data;

internal class BindingValueChangedEventArgs : EventArgs
{
	private object _oldValue;

	private object _newValue;

	public object OldValue => _oldValue;

	public object NewValue => _newValue;

	internal BindingValueChangedEventArgs(object oldValue, object newValue)
	{
		_oldValue = oldValue;
		_newValue = newValue;
	}
}
