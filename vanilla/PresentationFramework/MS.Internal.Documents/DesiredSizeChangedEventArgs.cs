using System;
using System.Windows;

namespace MS.Internal.Documents;

internal class DesiredSizeChangedEventArgs : EventArgs
{
	private readonly UIElement _child;

	internal UIElement Child => _child;

	internal DesiredSizeChangedEventArgs(UIElement child)
	{
		_child = child;
	}
}
