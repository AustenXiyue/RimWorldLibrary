using System.Collections;
using MS.Internal.PresentationCore;
using MS.Utility;

namespace System.Windows;

internal static class GlobalEventManager
{
	private static ArrayList _globalIndexToEventMap = new ArrayList(100);

	private static DTypeMap _dTypedRoutedEventList = new DTypeMap(10);

	private static Hashtable _ownerTypedRoutedEventList = new Hashtable(10);

	private static int _countRoutedEvents = 0;

	private static DTypeMap _dTypedClassListeners = new DTypeMap(100);

	private static DependencyObjectType _dependencyObjectType = DependencyObjectType.FromSystemTypeInternal(typeof(DependencyObject));

	internal static object Synchronized = new object();

	internal static RoutedEvent RegisterRoutedEvent(string name, RoutingStrategy routingStrategy, Type handlerType, Type ownerType)
	{
		lock (Synchronized)
		{
			RoutedEvent routedEvent = new RoutedEvent(name, routingStrategy, handlerType, ownerType);
			_countRoutedEvents++;
			AddOwner(routedEvent, ownerType);
			return routedEvent;
		}
	}

	internal static void RegisterClassHandler(Type classType, RoutedEvent routedEvent, Delegate handler, bool handledEventsToo)
	{
		DependencyObjectType dependencyObjectType = DependencyObjectType.FromSystemTypeInternal(classType);
		GetDTypedClassListeners(dependencyObjectType, routedEvent, out var classListenersLists, out var index);
		lock (Synchronized)
		{
			RoutedEventHandlerInfoList baseClassListeners = classListenersLists.AddToExistingHandlers(index, handler, handledEventsToo);
			ItemStructList<DependencyObjectType> activeDTypes = _dTypedClassListeners.ActiveDTypes;
			for (int i = 0; i < activeDTypes.Count; i++)
			{
				if (activeDTypes.List[i].IsSubclassOf(dependencyObjectType))
				{
					classListenersLists = (ClassHandlersStore)_dTypedClassListeners[activeDTypes.List[i]];
					classListenersLists.UpdateSubClassHandlers(routedEvent, baseClassListeners);
				}
			}
		}
	}

	internal static RoutedEvent[] GetRoutedEvents()
	{
		lock (Synchronized)
		{
			RoutedEvent[] array = new RoutedEvent[_countRoutedEvents];
			ItemStructList<DependencyObjectType> activeDTypes = _dTypedRoutedEventList.ActiveDTypes;
			int num = 0;
			for (int i = 0; i < activeDTypes.Count; i++)
			{
				FrugalObjectList<RoutedEvent> frugalObjectList = (FrugalObjectList<RoutedEvent>)_dTypedRoutedEventList[activeDTypes.List[i]];
				for (int j = 0; j < frugalObjectList.Count; j++)
				{
					RoutedEvent routedEvent = frugalObjectList[j];
					if (Array.IndexOf(array, routedEvent) < 0)
					{
						array[num++] = routedEvent;
					}
				}
			}
			IDictionaryEnumerator enumerator = _ownerTypedRoutedEventList.GetEnumerator();
			while (enumerator.MoveNext())
			{
				FrugalObjectList<RoutedEvent> frugalObjectList2 = (FrugalObjectList<RoutedEvent>)enumerator.Value;
				for (int k = 0; k < frugalObjectList2.Count; k++)
				{
					RoutedEvent routedEvent2 = frugalObjectList2[k];
					if (Array.IndexOf(array, routedEvent2) < 0)
					{
						array[num++] = routedEvent2;
					}
				}
			}
			return array;
		}
	}

	internal static void AddOwner(RoutedEvent routedEvent, Type ownerType)
	{
		if (ownerType == typeof(DependencyObject) || ownerType.IsSubclassOf(typeof(DependencyObject)))
		{
			DependencyObjectType dType = DependencyObjectType.FromSystemTypeInternal(ownerType);
			object obj = _dTypedRoutedEventList[dType];
			FrugalObjectList<RoutedEvent> frugalObjectList;
			if (obj == null)
			{
				frugalObjectList = new FrugalObjectList<RoutedEvent>(1);
				_dTypedRoutedEventList[dType] = frugalObjectList;
			}
			else
			{
				frugalObjectList = (FrugalObjectList<RoutedEvent>)obj;
			}
			if (!frugalObjectList.Contains(routedEvent))
			{
				frugalObjectList.Add(routedEvent);
			}
		}
		else
		{
			object obj2 = _ownerTypedRoutedEventList[ownerType];
			FrugalObjectList<RoutedEvent> frugalObjectList2;
			if (obj2 == null)
			{
				frugalObjectList2 = new FrugalObjectList<RoutedEvent>(1);
				_ownerTypedRoutedEventList[ownerType] = frugalObjectList2;
			}
			else
			{
				frugalObjectList2 = (FrugalObjectList<RoutedEvent>)obj2;
			}
			if (!frugalObjectList2.Contains(routedEvent))
			{
				frugalObjectList2.Add(routedEvent);
			}
		}
	}

	internal static RoutedEvent[] GetRoutedEventsForOwner(Type ownerType)
	{
		if (ownerType == typeof(DependencyObject) || ownerType.IsSubclassOf(typeof(DependencyObject)))
		{
			DependencyObjectType dType = DependencyObjectType.FromSystemTypeInternal(ownerType);
			FrugalObjectList<RoutedEvent> frugalObjectList = (FrugalObjectList<RoutedEvent>)_dTypedRoutedEventList[dType];
			if (frugalObjectList != null)
			{
				return frugalObjectList.ToArray();
			}
		}
		else
		{
			FrugalObjectList<RoutedEvent> frugalObjectList2 = (FrugalObjectList<RoutedEvent>)_ownerTypedRoutedEventList[ownerType];
			if (frugalObjectList2 != null)
			{
				return frugalObjectList2.ToArray();
			}
		}
		return null;
	}

	internal static RoutedEvent GetRoutedEventFromName(string name, Type ownerType, bool includeSupers)
	{
		if (ownerType == typeof(DependencyObject) || ownerType.IsSubclassOf(typeof(DependencyObject)))
		{
			for (DependencyObjectType dependencyObjectType = DependencyObjectType.FromSystemTypeInternal(ownerType); dependencyObjectType != null; dependencyObjectType = (includeSupers ? dependencyObjectType.BaseType : null))
			{
				FrugalObjectList<RoutedEvent> frugalObjectList = (FrugalObjectList<RoutedEvent>)_dTypedRoutedEventList[dependencyObjectType];
				if (frugalObjectList != null)
				{
					for (int i = 0; i < frugalObjectList.Count; i++)
					{
						RoutedEvent routedEvent = frugalObjectList[i];
						if (routedEvent.Name.Equals(name))
						{
							return routedEvent;
						}
					}
				}
			}
		}
		else
		{
			while (ownerType != null)
			{
				FrugalObjectList<RoutedEvent> frugalObjectList2 = (FrugalObjectList<RoutedEvent>)_ownerTypedRoutedEventList[ownerType];
				if (frugalObjectList2 != null)
				{
					for (int j = 0; j < frugalObjectList2.Count; j++)
					{
						RoutedEvent routedEvent2 = frugalObjectList2[j];
						if (routedEvent2.Name.Equals(name))
						{
							return routedEvent2;
						}
					}
				}
				ownerType = (includeSupers ? ownerType.BaseType : null);
			}
		}
		return null;
	}

	internal static RoutedEventHandlerInfoList GetDTypedClassListeners(DependencyObjectType dType, RoutedEvent routedEvent)
	{
		ClassHandlersStore classListenersLists;
		int index;
		return GetDTypedClassListeners(dType, routedEvent, out classListenersLists, out index);
	}

	internal static RoutedEventHandlerInfoList GetDTypedClassListeners(DependencyObjectType dType, RoutedEvent routedEvent, out ClassHandlersStore classListenersLists, out int index)
	{
		classListenersLists = (ClassHandlersStore)_dTypedClassListeners[dType];
		if (classListenersLists != null)
		{
			index = classListenersLists.GetHandlersIndex(routedEvent);
			if (index != -1)
			{
				return classListenersLists.GetExistingHandlers(index);
			}
		}
		lock (Synchronized)
		{
			return GetUpdatedDTypedClassListeners(dType, routedEvent, out classListenersLists, out index);
		}
	}

	private static RoutedEventHandlerInfoList GetUpdatedDTypedClassListeners(DependencyObjectType dType, RoutedEvent routedEvent, out ClassHandlersStore classListenersLists, out int index)
	{
		classListenersLists = (ClassHandlersStore)_dTypedClassListeners[dType];
		if (classListenersLists != null)
		{
			index = classListenersLists.GetHandlersIndex(routedEvent);
			if (index != -1)
			{
				return classListenersLists.GetExistingHandlers(index);
			}
		}
		DependencyObjectType dependencyObjectType = dType;
		ClassHandlersStore classHandlersStore = null;
		RoutedEventHandlerInfoList routedEventHandlerInfoList = null;
		int num = -1;
		while (num == -1 && dependencyObjectType.Id != _dependencyObjectType.Id)
		{
			dependencyObjectType = dependencyObjectType.BaseType;
			classHandlersStore = (ClassHandlersStore)_dTypedClassListeners[dependencyObjectType];
			if (classHandlersStore != null)
			{
				num = classHandlersStore.GetHandlersIndex(routedEvent);
				if (num != -1)
				{
					routedEventHandlerInfoList = classHandlersStore.GetExistingHandlers(num);
				}
			}
		}
		if (classListenersLists == null)
		{
			if (dType.SystemType == typeof(UIElement) || dType.SystemType == typeof(ContentElement))
			{
				classListenersLists = new ClassHandlersStore(80);
			}
			else
			{
				classListenersLists = new ClassHandlersStore(1);
			}
			_dTypedClassListeners[dType] = classListenersLists;
		}
		index = classListenersLists.CreateHandlersLink(routedEvent, routedEventHandlerInfoList);
		return routedEventHandlerInfoList;
	}

	internal static int GetNextAvailableGlobalIndex(object value)
	{
		lock (Synchronized)
		{
			if (_globalIndexToEventMap.Count >= int.MaxValue)
			{
				throw new InvalidOperationException(SR.TooManyRoutedEvents);
			}
			return _globalIndexToEventMap.Add(value);
		}
	}

	internal static object EventFromGlobalIndex(int globalIndex)
	{
		return _globalIndexToEventMap[globalIndex];
	}
}
