namespace System.Windows;

internal class RoutedEventHandlerInfoList
{
	internal RoutedEventHandlerInfo[] Handlers;

	internal RoutedEventHandlerInfoList Next;

	internal bool Contains(RoutedEventHandlerInfoList handlers)
	{
		for (RoutedEventHandlerInfoList routedEventHandlerInfoList = this; routedEventHandlerInfoList != null; routedEventHandlerInfoList = routedEventHandlerInfoList.Next)
		{
			if (routedEventHandlerInfoList == handlers)
			{
				return true;
			}
		}
		return false;
	}
}
