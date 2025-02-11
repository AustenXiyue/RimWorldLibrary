using MS.Utility;

namespace System.Windows;

internal class ClassHandlersStore
{
	private ItemStructList<ClassHandlers> _eventHandlersList;

	internal ClassHandlersStore(int size)
	{
		_eventHandlersList = new ItemStructList<ClassHandlers>(size);
	}

	internal RoutedEventHandlerInfoList AddToExistingHandlers(int index, Delegate handler, bool handledEventsToo)
	{
		RoutedEventHandlerInfo routedEventHandlerInfo = new RoutedEventHandlerInfo(handler, handledEventsToo);
		RoutedEventHandlerInfoList routedEventHandlerInfoList = _eventHandlersList.List[index].Handlers;
		if (routedEventHandlerInfoList == null || !_eventHandlersList.List[index].HasSelfHandlers)
		{
			routedEventHandlerInfoList = new RoutedEventHandlerInfoList();
			routedEventHandlerInfoList.Handlers = new RoutedEventHandlerInfo[1];
			routedEventHandlerInfoList.Handlers[0] = routedEventHandlerInfo;
			routedEventHandlerInfoList.Next = _eventHandlersList.List[index].Handlers;
			_eventHandlersList.List[index].Handlers = routedEventHandlerInfoList;
			_eventHandlersList.List[index].HasSelfHandlers = true;
		}
		else
		{
			int num = routedEventHandlerInfoList.Handlers.Length;
			RoutedEventHandlerInfo[] array = new RoutedEventHandlerInfo[num + 1];
			Array.Copy(routedEventHandlerInfoList.Handlers, 0, array, 0, num);
			array[num] = routedEventHandlerInfo;
			routedEventHandlerInfoList.Handlers = array;
		}
		return routedEventHandlerInfoList;
	}

	internal RoutedEventHandlerInfoList GetExistingHandlers(int index)
	{
		return _eventHandlersList.List[index].Handlers;
	}

	internal int CreateHandlersLink(RoutedEvent routedEvent, RoutedEventHandlerInfoList handlers)
	{
		ClassHandlers item = default(ClassHandlers);
		item.RoutedEvent = routedEvent;
		item.Handlers = handlers;
		item.HasSelfHandlers = false;
		_eventHandlersList.Add(item);
		return _eventHandlersList.Count - 1;
	}

	internal void UpdateSubClassHandlers(RoutedEvent routedEvent, RoutedEventHandlerInfoList baseClassListeners)
	{
		int handlersIndex = GetHandlersIndex(routedEvent);
		if (handlersIndex == -1)
		{
			return;
		}
		bool hasSelfHandlers = _eventHandlersList.List[handlersIndex].HasSelfHandlers;
		RoutedEventHandlerInfoList routedEventHandlerInfoList = (hasSelfHandlers ? _eventHandlersList.List[handlersIndex].Handlers.Next : _eventHandlersList.List[handlersIndex].Handlers);
		bool flag = false;
		if (routedEventHandlerInfoList != null)
		{
			if (baseClassListeners.Next != null && baseClassListeners.Next.Contains(routedEventHandlerInfoList))
			{
				flag = true;
			}
		}
		else
		{
			flag = true;
		}
		if (flag)
		{
			if (hasSelfHandlers)
			{
				_eventHandlersList.List[handlersIndex].Handlers.Next = baseClassListeners;
			}
			else
			{
				_eventHandlersList.List[handlersIndex].Handlers = baseClassListeners;
			}
		}
	}

	internal int GetHandlersIndex(RoutedEvent routedEvent)
	{
		for (int i = 0; i < _eventHandlersList.Count; i++)
		{
			if (_eventHandlersList.List[i].RoutedEvent == routedEvent)
			{
				return i;
			}
		}
		return -1;
	}
}
