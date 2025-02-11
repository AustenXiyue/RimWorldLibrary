namespace System.Windows.Input;

internal class TouchesCapturedWithinProperty : ReverseInheritProperty
{
	internal TouchesCapturedWithinProperty()
		: base(UIElement.AreAnyTouchesCapturedWithinPropertyKey, CoreFlags.TouchesCapturedWithinCache, CoreFlags.TouchesCapturedWithinChanged)
	{
	}

	internal override void FireNotifications(UIElement uie, ContentElement ce, UIElement3D uie3D, bool oldValue)
	{
	}
}
