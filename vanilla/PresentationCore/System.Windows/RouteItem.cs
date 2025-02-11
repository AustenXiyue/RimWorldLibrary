namespace System.Windows;

internal struct RouteItem
{
	private object _target;

	private RoutedEventHandlerInfo _routedEventHandlerInfo;

	internal object Target => _target;

	internal RouteItem(object target, RoutedEventHandlerInfo routedEventHandlerInfo)
	{
		_target = target;
		_routedEventHandlerInfo = routedEventHandlerInfo;
	}

	internal void InvokeHandler(RoutedEventArgs routedEventArgs)
	{
		_routedEventHandlerInfo.InvokeHandler(_target, routedEventArgs);
	}

	public override bool Equals(object o)
	{
		return Equals((RouteItem)o);
	}

	public bool Equals(RouteItem routeItem)
	{
		if (routeItem._target == _target)
		{
			return routeItem._routedEventHandlerInfo == _routedEventHandlerInfo;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public static bool operator ==(RouteItem routeItem1, RouteItem routeItem2)
	{
		return routeItem1.Equals(routeItem2);
	}

	public static bool operator !=(RouteItem routeItem1, RouteItem routeItem2)
	{
		return !routeItem1.Equals(routeItem2);
	}
}
