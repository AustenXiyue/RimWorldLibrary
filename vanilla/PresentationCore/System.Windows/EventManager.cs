using System.ComponentModel;
using MS.Internal.PresentationCore;

namespace System.Windows;

/// <summary>Provides event-related utility methods that register routed events for class owners and add class handlers. </summary>
public static class EventManager
{
	/// <summary>Registers a new routed event with the Windows Presentation Foundation (WPF) event system. </summary>
	/// <returns>The identifier for the newly registered routed event. This identifier object can now be stored as a static field in a class and then used as a parameter for methods that attach handlers to the event. The routed event identifier is also used for other event system APIs.</returns>
	/// <param name="name">The name of the routed event. The name must be unique within the owner type and cannot be null or an empty string.</param>
	/// <param name="routingStrategy">The routing strategy of the event as a value of the enumeration.</param>
	/// <param name="handlerType">The type of the event handler. This must be a delegate type and cannot be null.</param>
	/// <param name="ownerType">The owner class type of the routed event. This cannot be null.</param>
	public static RoutedEvent RegisterRoutedEvent(string name, RoutingStrategy routingStrategy, Type handlerType, Type ownerType)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (routingStrategy != 0 && routingStrategy != RoutingStrategy.Bubble && routingStrategy != RoutingStrategy.Direct)
		{
			throw new InvalidEnumArgumentException("routingStrategy", (int)routingStrategy, typeof(RoutingStrategy));
		}
		if (handlerType == null)
		{
			throw new ArgumentNullException("handlerType");
		}
		if (ownerType == null)
		{
			throw new ArgumentNullException("ownerType");
		}
		if (GlobalEventManager.GetRoutedEventFromName(name, ownerType, includeSupers: false) != null)
		{
			throw new ArgumentException(SR.Format(SR.DuplicateEventName, name, ownerType));
		}
		return GlobalEventManager.RegisterRoutedEvent(name, routingStrategy, handlerType, ownerType);
	}

	/// <summary>Registers a class handler for a particular routed event. </summary>
	/// <param name="classType">The type of the class that is declaring class handling.</param>
	/// <param name="routedEvent">The routed event identifier of the event to handle.</param>
	/// <param name="handler">A reference to the class handler implementation.</param>
	public static void RegisterClassHandler(Type classType, RoutedEvent routedEvent, Delegate handler)
	{
		RegisterClassHandler(classType, routedEvent, handler, handledEventsToo: false);
	}

	/// <summary> Registers a class handler for a particular routed event, with the option to handle events where event data is already marked handled.</summary>
	/// <param name="classType">The type of the class that is declaring class handling.</param>
	/// <param name="routedEvent">The routed event identifier of the event to handle.</param>
	/// <param name="handler">A reference to the class handler implementation.</param>
	/// <param name="handledEventsToo">true to invoke this class handler even if arguments of the routed event have been marked as handled; false to retain the default behavior of not invoking the handler on any marked-handled event.</param>
	public static void RegisterClassHandler(Type classType, RoutedEvent routedEvent, Delegate handler, bool handledEventsToo)
	{
		if (classType == null)
		{
			throw new ArgumentNullException("classType");
		}
		if (routedEvent == null)
		{
			throw new ArgumentNullException("routedEvent");
		}
		if ((object)handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		if (!typeof(UIElement).IsAssignableFrom(classType) && !typeof(ContentElement).IsAssignableFrom(classType) && !typeof(UIElement3D).IsAssignableFrom(classType))
		{
			throw new ArgumentException(SR.ClassTypeIllegal);
		}
		if (!routedEvent.IsLegalHandler(handler))
		{
			throw new ArgumentException(SR.HandlerTypeIllegal);
		}
		GlobalEventManager.RegisterClassHandler(classType, routedEvent, handler, handledEventsToo);
	}

	/// <summary>Returns identifiers for routed events that have been registered to the event system. </summary>
	/// <returns>An array of type <see cref="T:System.Windows.RoutedEvent" /> that contains the registered objects.</returns>
	public static RoutedEvent[] GetRoutedEvents()
	{
		return GlobalEventManager.GetRoutedEvents();
	}

	/// <summary>Finds all routed event identifiers for events that are registered with the provided owner type. </summary>
	/// <returns>An array of matching routed event identifiers if any match is found; otherwise, null.</returns>
	/// <param name="ownerType">The type to start the search with. Base classes are included in the search.</param>
	public static RoutedEvent[] GetRoutedEventsForOwner(Type ownerType)
	{
		if (ownerType == null)
		{
			throw new ArgumentNullException("ownerType");
		}
		return GlobalEventManager.GetRoutedEventsForOwner(ownerType);
	}

	[FriendAccessAllowed]
	internal static RoutedEvent GetRoutedEventFromName(string name, Type ownerType)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (ownerType == null)
		{
			throw new ArgumentNullException("ownerType");
		}
		return GlobalEventManager.GetRoutedEventFromName(name, ownerType, includeSupers: true);
	}
}
