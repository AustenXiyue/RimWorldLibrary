namespace System.Windows.Input;

internal class TouchesOverProperty : ReverseInheritProperty
{
	internal TouchesOverProperty()
		: base(UIElement.AreAnyTouchesOverPropertyKey, CoreFlags.TouchesOverCache, CoreFlags.TouchesOverChanged, CoreFlags.TouchLeaveCache, CoreFlags.TouchEnterCache)
	{
	}

	internal override void FireNotifications(UIElement uie, ContentElement ce, UIElement3D uie3D, bool oldValue)
	{
	}
}
