using System;
using System.Windows;

namespace MS.Internal;

internal class InheritedPropertyChangedEventArgs : EventArgs
{
	private InheritablePropertyChangeInfo _info;

	internal InheritablePropertyChangeInfo Info => _info;

	internal InheritedPropertyChangedEventArgs(ref InheritablePropertyChangeInfo info)
	{
		_info = info;
	}
}
