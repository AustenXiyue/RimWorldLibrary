using System.ComponentModel;
using System.Windows.Markup;

namespace System.Windows;

/// <summary>Represents an event setter in a style. Event setters invoke the specified event handlers in response to events.</summary>
public class EventSetter : SetterBase
{
	private RoutedEvent _event;

	private Delegate _handler;

	private bool _handledEventsToo;

	/// <summary>Gets or sets the particular routed event that this <see cref="T:System.Windows.EventSetter" /> responds to.</summary>
	/// <returns>The identifier field of the routed event.</returns>
	/// <exception cref="T:System.InvalidOperationException">Attempted to set this property on a sealed <see cref="T:System.Windows.EventSetter" /> .</exception>
	public RoutedEvent Event
	{
		get
		{
			return _event;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			CheckSealed();
			_event = value;
		}
	}

	/// <summary>Gets or sets the reference to a handler for a routed event in the setter. </summary>
	/// <returns>Reference to the handler that is attached by this <see cref="T:System.Windows.EventSetter" />.</returns>
	[TypeConverter(typeof(EventSetterHandlerConverter))]
	public Delegate Handler
	{
		get
		{
			return _handler;
		}
		set
		{
			if ((object)value == null)
			{
				throw new ArgumentNullException("value");
			}
			CheckSealed();
			_handler = value;
		}
	}

	/// <summary>Gets or sets a value that determines whether the handler assigned to the setter should still be invoked, even if the event is marked handled in its event data. </summary>
	/// <returns>true if the handler should still be invoked; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool HandledEventsToo
	{
		get
		{
			return _handledEventsToo;
		}
		set
		{
			CheckSealed();
			_handledEventsToo = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.EventSetter" /> class. </summary>
	public EventSetter()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.EventSetter" /> class, using the provided event and handler parameters. </summary>
	/// <param name="routedEvent">The particular routed event that the <see cref="T:System.Windows.EventSetter" /> responds to.</param>
	public EventSetter(RoutedEvent routedEvent, Delegate handler)
	{
		if (routedEvent == null)
		{
			throw new ArgumentNullException("routedEvent");
		}
		if ((object)handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		_event = routedEvent;
		_handler = handler;
	}

	internal override void Seal()
	{
		if (_event == null)
		{
			throw new ArgumentException(SR.Format(SR.NullPropertyIllegal, "EventSetter.Event"));
		}
		if ((object)_handler == null)
		{
			throw new ArgumentException(SR.Format(SR.NullPropertyIllegal, "EventSetter.Handler"));
		}
		if (_handler.GetType() != _event.HandlerType)
		{
			throw new ArgumentException(SR.HandlerTypeIllegal);
		}
		base.Seal();
	}
}
