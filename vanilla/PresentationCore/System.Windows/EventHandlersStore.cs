using MS.Internal.PresentationCore;
using MS.Utility;

namespace System.Windows;

[FriendAccessAllowed]
internal class EventHandlersStore
{
	private FrugalMap _entries;

	internal FrugalObjectList<RoutedEventHandlerInfo> this[RoutedEvent key]
	{
		get
		{
			object obj = _entries[key.GlobalIndex];
			if (obj == DependencyProperty.UnsetValue)
			{
				return null;
			}
			return (FrugalObjectList<RoutedEventHandlerInfo>)obj;
		}
	}

	internal Delegate this[EventPrivateKey key]
	{
		get
		{
			object obj = _entries[key.GlobalIndex];
			if (obj == DependencyProperty.UnsetValue)
			{
				return null;
			}
			return (Delegate)obj;
		}
	}

	internal int Count => _entries.Count;

	public EventHandlersStore()
	{
		_entries = default(FrugalMap);
	}

	public EventHandlersStore(EventHandlersStore source)
	{
		_entries = source._entries;
	}

	public void Add(EventPrivateKey key, Delegate handler)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if ((object)handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		Delegate @delegate = this[key];
		if ((object)@delegate == null)
		{
			_entries[key.GlobalIndex] = handler;
		}
		else
		{
			_entries[key.GlobalIndex] = Delegate.Combine(@delegate, handler);
		}
	}

	public void Remove(EventPrivateKey key, Delegate handler)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if ((object)handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		Delegate @delegate = this[key];
		if ((object)@delegate != null)
		{
			@delegate = Delegate.Remove(@delegate, handler);
			if ((object)@delegate == null)
			{
				_entries[key.GlobalIndex] = DependencyProperty.UnsetValue;
			}
			else
			{
				_entries[key.GlobalIndex] = @delegate;
			}
		}
	}

	public Delegate Get(EventPrivateKey key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		return this[key];
	}

	public void AddRoutedEventHandler(RoutedEvent routedEvent, Delegate handler, bool handledEventsToo)
	{
		if (routedEvent == null)
		{
			throw new ArgumentNullException("routedEvent");
		}
		if ((object)handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		if (!routedEvent.IsLegalHandler(handler))
		{
			throw new ArgumentException(SR.HandlerTypeIllegal);
		}
		RoutedEventHandlerInfo value = new RoutedEventHandlerInfo(handler, handledEventsToo);
		FrugalObjectList<RoutedEventHandlerInfo> frugalObjectList = this[routedEvent];
		if (frugalObjectList == null)
		{
			frugalObjectList = (FrugalObjectList<RoutedEventHandlerInfo>)(_entries[routedEvent.GlobalIndex] = new FrugalObjectList<RoutedEventHandlerInfo>(1));
		}
		frugalObjectList.Add(value);
	}

	public void RemoveRoutedEventHandler(RoutedEvent routedEvent, Delegate handler)
	{
		if (routedEvent == null)
		{
			throw new ArgumentNullException("routedEvent");
		}
		if ((object)handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		if (!routedEvent.IsLegalHandler(handler))
		{
			throw new ArgumentException(SR.HandlerTypeIllegal);
		}
		FrugalObjectList<RoutedEventHandlerInfo> frugalObjectList = this[routedEvent];
		if (frugalObjectList == null || frugalObjectList.Count <= 0)
		{
			return;
		}
		if (frugalObjectList.Count == 1 && frugalObjectList[0].Handler == handler)
		{
			_entries[routedEvent.GlobalIndex] = DependencyProperty.UnsetValue;
			return;
		}
		for (int i = 0; i < frugalObjectList.Count; i++)
		{
			if (frugalObjectList[i].Handler == handler)
			{
				frugalObjectList.RemoveAt(i);
				break;
			}
		}
	}

	public bool Contains(RoutedEvent routedEvent)
	{
		if (routedEvent == null)
		{
			throw new ArgumentNullException("routedEvent");
		}
		FrugalObjectList<RoutedEventHandlerInfo> frugalObjectList = this[routedEvent];
		if (frugalObjectList != null)
		{
			return frugalObjectList.Count != 0;
		}
		return false;
	}

	public RoutedEventHandlerInfo[] GetRoutedEventHandlers(RoutedEvent routedEvent)
	{
		if (routedEvent == null)
		{
			throw new ArgumentNullException("routedEvent");
		}
		return this[routedEvent]?.ToArray();
	}
}
