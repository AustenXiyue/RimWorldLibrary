using System;
using System.ComponentModel;

namespace MS.Internal.Data;

internal class ValueChangedEventArgs : EventArgs
{
	private PropertyDescriptor _pd;

	internal PropertyDescriptor PropertyDescriptor => _pd;

	internal ValueChangedEventArgs(PropertyDescriptor pd)
	{
		_pd = pd;
	}
}
