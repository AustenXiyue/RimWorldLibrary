using System.Collections;

namespace System.Windows.Controls;

internal class CalendarSelectionChangedEventArgs : SelectionChangedEventArgs
{
	public CalendarSelectionChangedEventArgs(RoutedEvent eventId, IList removedItems, IList addedItems)
		: base(eventId, removedItems, addedItems)
	{
	}

	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		if (genericHandler is EventHandler<SelectionChangedEventArgs> eventHandler)
		{
			eventHandler(genericTarget, this);
		}
		else
		{
			base.InvokeEventHandler(genericHandler, genericTarget);
		}
	}
}
