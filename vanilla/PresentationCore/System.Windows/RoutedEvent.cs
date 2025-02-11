using System.ComponentModel;
using System.Globalization;
using System.Windows.Markup;

namespace System.Windows;

/// <summary>Represents and identifies a routed event and declares its characteristics.</summary>
[TypeConverter("System.Windows.Markup.RoutedEventConverter, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
[ValueSerializer("System.Windows.Markup.RoutedEventValueSerializer, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
public sealed class RoutedEvent
{
	private string _name;

	private RoutingStrategy _routingStrategy;

	private Type _handlerType;

	private Type _ownerType;

	private int _globalIndex;

	/// <summary>Gets the identifying name of the routed event. </summary>
	/// <returns>The name of the routed event.</returns>
	public string Name => _name;

	/// <summary>Gets the routing strategy of the routed event. </summary>
	/// <returns>One of the enumeration values. The default is the enumeration default, <see cref="F:System.Windows.RoutingStrategy.Bubble" />.</returns>
	public RoutingStrategy RoutingStrategy => _routingStrategy;

	/// <summary>Gets the handler type of the routed event. </summary>
	/// <returns>The handler type of the routed event.</returns>
	public Type HandlerType => _handlerType;

	/// <summary>Gets the registered owner type of the routed event. </summary>
	/// <returns>The owner type of the routed event.</returns>
	public Type OwnerType => _ownerType;

	internal int GlobalIndex => _globalIndex;

	/// <summary>Associates another owner type with the routed event represented by a <see cref="T:System.Windows.RoutedEvent" /> instance, and enables routing of the event and its handling.  </summary>
	/// <returns>The identifier field for the event. This return value should be used to set a public static read-only field that will store the identifier for the representation of the routed event on the owning type. This field is typically defined with public access, because user code must reference the field in order to attach any instance handlers for the routed event when using the <see cref="M:System.Windows.UIElement.AddHandler(System.Windows.RoutedEvent,System.Delegate,System.Boolean)" /> utility method.</returns>
	/// <param name="ownerType">The type where the routed event is added.</param>
	public RoutedEvent AddOwner(Type ownerType)
	{
		GlobalEventManager.AddOwner(this, ownerType);
		return this;
	}

	internal bool IsLegalHandler(Delegate handler)
	{
		Type type = handler.GetType();
		if (!(type == HandlerType))
		{
			return type == typeof(RoutedEventHandler);
		}
		return true;
	}

	/// <summary>Returns the string representation of this <see cref="T:System.Windows.RoutedEvent" />.</summary>
	/// <returns>A string representation for this object, which is identical to the value returned by <see cref="P:System.Windows.RoutedEvent.Name" />.</returns>
	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", _ownerType.Name, _name);
	}

	internal RoutedEvent(string name, RoutingStrategy routingStrategy, Type handlerType, Type ownerType)
	{
		_name = name;
		_routingStrategy = routingStrategy;
		_handlerType = handlerType;
		_ownerType = ownerType;
		_globalIndex = GlobalEventManager.GetNextAvailableGlobalIndex(this);
	}
}
