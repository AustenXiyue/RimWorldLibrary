using System.Windows.Input;

namespace System.Windows;

internal class MouseOverProperty : ReverseInheritProperty
{
	internal MouseOverProperty()
		: base(UIElement.IsMouseOverPropertyKey, CoreFlags.IsMouseOverCache, CoreFlags.IsMouseOverChanged)
	{
	}

	internal override void FireNotifications(UIElement uie, ContentElement ce, UIElement3D uie3D, bool oldValue)
	{
		bool flag = false;
		if (uie != null)
		{
			flag = (!oldValue && uie.IsMouseOver) || (oldValue && !uie.IsMouseOver);
		}
		else if (ce != null)
		{
			flag = (!oldValue && ce.IsMouseOver) || (oldValue && !ce.IsMouseOver);
		}
		else if (uie3D != null)
		{
			flag = (!oldValue && uie3D.IsMouseOver) || (oldValue && !uie3D.IsMouseOver);
		}
		if (flag)
		{
			MouseEventArgs mouseEventArgs = new MouseEventArgs(Mouse.PrimaryDevice, Environment.TickCount, Mouse.PrimaryDevice.StylusDevice);
			mouseEventArgs.RoutedEvent = (oldValue ? Mouse.MouseLeaveEvent : Mouse.MouseEnterEvent);
			if (uie != null)
			{
				uie.RaiseEvent(mouseEventArgs);
			}
			else if (ce != null)
			{
				ce.RaiseEvent(mouseEventArgs);
			}
			else
			{
				uie3D?.RaiseEvent(mouseEventArgs);
			}
		}
	}
}
