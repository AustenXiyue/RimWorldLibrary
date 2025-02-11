namespace System.Windows;

internal static class EventRouteFactory
{
	private static EventRoute[] _eventRouteStack;

	private static int _stackTop;

	private static readonly object _synchronized = new object();

	internal static EventRoute FetchObject(RoutedEvent routedEvent)
	{
		EventRoute eventRoute = Pop();
		if (eventRoute == null)
		{
			eventRoute = new EventRoute(routedEvent);
		}
		else
		{
			eventRoute.RoutedEvent = routedEvent;
		}
		return eventRoute;
	}

	internal static void RecycleObject(EventRoute eventRoute)
	{
		eventRoute.Clear();
		Push(eventRoute);
	}

	private static void Push(EventRoute eventRoute)
	{
		lock (_synchronized)
		{
			if (_eventRouteStack == null)
			{
				_eventRouteStack = new EventRoute[2];
				_stackTop = 0;
			}
			if (_stackTop < 2)
			{
				_eventRouteStack[_stackTop++] = eventRoute;
			}
		}
	}

	private static EventRoute Pop()
	{
		lock (_synchronized)
		{
			if (_stackTop > 0)
			{
				EventRoute result = _eventRouteStack[--_stackTop];
				_eventRouteStack[_stackTop] = null;
				return result;
			}
		}
		return null;
	}
}
