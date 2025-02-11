using System.Windows.Input;

namespace System.Windows;

internal class StylusOverProperty : ReverseInheritProperty
{
	internal StylusOverProperty()
		: base(UIElement.IsStylusOverPropertyKey, CoreFlags.IsStylusOverCache, CoreFlags.IsStylusOverChanged)
	{
	}

	internal override void FireNotifications(UIElement uie, ContentElement ce, UIElement3D uie3D, bool oldValue)
	{
		if (Stylus.CurrentStylusDevice != null)
		{
			StylusEventArgs stylusEventArgs = new StylusEventArgs(Stylus.CurrentStylusDevice, Environment.TickCount);
			stylusEventArgs.RoutedEvent = (oldValue ? Stylus.StylusLeaveEvent : Stylus.StylusEnterEvent);
			if (uie != null)
			{
				uie.RaiseEvent(stylusEventArgs);
			}
			else if (ce != null)
			{
				ce.RaiseEvent(stylusEventArgs);
			}
			else
			{
				uie3D?.RaiseEvent(stylusEventArgs);
			}
		}
	}
}
