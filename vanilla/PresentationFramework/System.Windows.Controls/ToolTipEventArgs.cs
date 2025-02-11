namespace System.Windows.Controls;

/// <summary>Provides event information for events that occur when a tooltip opens or closes.</summary>
public sealed class ToolTipEventArgs : RoutedEventArgs
{
	internal ToolTipEventArgs(bool opening)
	{
		if (opening)
		{
			base.RoutedEvent = ToolTipService.ToolTipOpeningEvent;
		}
		else
		{
			base.RoutedEvent = ToolTipService.ToolTipClosingEvent;
		}
	}

	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((ToolTipEventHandler)genericHandler)(genericTarget, this);
	}
}
