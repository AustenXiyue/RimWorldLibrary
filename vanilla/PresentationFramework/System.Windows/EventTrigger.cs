using System.ComponentModel;
using System.Windows.Markup;

namespace System.Windows;

/// <summary>Represents a trigger that applies a set of actions in response to an event.</summary>
[ContentProperty("Actions")]
public class EventTrigger : TriggerBase, IAddChild
{
	internal class EventTriggerSourceListener
	{
		private EventTrigger _owningTrigger;

		private FrameworkElement _owningTriggerHost;

		internal EventTriggerSourceListener(EventTrigger trigger, FrameworkElement host)
		{
			_owningTrigger = trigger;
			_owningTriggerHost = host;
		}

		internal void Handler(object sender, RoutedEventArgs e)
		{
			TriggerActionCollection actions = _owningTrigger.Actions;
			for (int i = 0; i < actions.Count; i++)
			{
				actions[i].Invoke(_owningTriggerHost);
			}
		}
	}

	private RoutedEvent _routedEvent;

	private string _sourceName;

	private int _childId;

	private TriggerActionCollection _actions;

	internal static readonly UncommonField<TriggerCollection> TriggerCollectionField = new UncommonField<TriggerCollection>(null);

	private RoutedEventHandler _routedEventHandler;

	private FrameworkElement _source;

	/// <summary>Gets or sets the <see cref="T:System.Windows.RoutedEvent" /> that will activate this trigger.</summary>
	/// <returns>The default value is null.</returns>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="P:System.Windows.EventTrigger.RoutedEvent" /> property cannot be null.</exception>
	public RoutedEvent RoutedEvent
	{
		get
		{
			return _routedEvent;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (base.IsSealed)
			{
				throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "EventTrigger"));
			}
			if (_routedEventHandler != null)
			{
				throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "EventTrigger"));
			}
			_routedEvent = value;
		}
	}

	/// <summary>Gets or sets the name of the object with the event that activates this trigger. This is only used by element triggers or template triggers.</summary>
	/// <returns>The default value is null. If this property value is null, then the element being monitored for the raising of the event is the templated parent or the logical tree root.</returns>
	/// <exception cref="T:System.InvalidOperationException">After an <see cref="T:System.Windows.EventTrigger" /> is in use, it cannot be modified.</exception>
	[DefaultValue(null)]
	public string SourceName
	{
		get
		{
			return _sourceName;
		}
		set
		{
			if (base.IsSealed)
			{
				throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "EventTrigger"));
			}
			_sourceName = value;
		}
	}

	internal int TriggerChildIndex
	{
		get
		{
			return _childId;
		}
		set
		{
			_childId = value;
		}
	}

	/// <summary>Gets the collection of actions to apply when the event occurs.</summary>
	/// <returns>The default is an empty collection.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public TriggerActionCollection Actions
	{
		get
		{
			if (_actions == null)
			{
				_actions = new TriggerActionCollection();
				_actions.Owner = this;
			}
			return _actions;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.EventTrigger" /> class. </summary>
	public EventTrigger()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.EventTrigger" /> class with the specified event.</summary>
	/// <param name="routedEvent">The <see cref="T:System.Windows.RoutedEvent" /> that activates this trigger.</param>
	public EventTrigger(RoutedEvent routedEvent)
	{
		RoutedEvent = routedEvent;
	}

	/// <summary>Adds a child object. </summary>
	/// <param name="value">The child object to add.</param>
	void IAddChild.AddChild(object value)
	{
		AddChild(value);
	}

	/// <summary>Adds the specified object to the <see cref="P:System.Windows.EventTrigger.Actions" /> collection of the current event trigger.</summary>
	/// <param name="value">A <see cref="T:System.Windows.TriggerAction" /> object to add to the <see cref="P:System.Windows.EventTrigger.Actions" /> collection of this trigger.</param>
	protected virtual void AddChild(object value)
	{
		if (!(value is TriggerAction value2))
		{
			throw new ArgumentException(SR.Format(SR.EventTriggerBadAction, value.GetType().Name));
		}
		Actions.Add(value2);
	}

	/// <summary>Adds the text content of a node to the object. </summary>
	/// <param name="text">The text to add to the object.</param>
	void IAddChild.AddText(string text)
	{
		AddText(text);
	}

	/// <summary>This method is not supported and results in an exception.</summary>
	/// <param name="text">This parameter is not used.</param>
	protected virtual void AddText(string text)
	{
		XamlSerializerUtil.ThrowIfNonWhiteSpaceInAddText(text, this);
	}

	internal override void OnInheritanceContextChangedCore(EventArgs args)
	{
		base.OnInheritanceContextChangedCore(args);
		if (_actions == null)
		{
			return;
		}
		for (int i = 0; i < _actions.Count; i++)
		{
			DependencyObject dependencyObject = _actions[i];
			if (dependencyObject != null && dependencyObject.InheritanceContext == this)
			{
				dependencyObject.OnInheritanceContextChanged(args);
			}
		}
	}

	/// <summary>Returns whether serialization processes should serialize the effective value of the <see cref="P:System.Windows.EventTrigger.Actions" /> property on instances of this class.</summary>
	/// <returns>Returns true if the <see cref="P:System.Windows.EventTrigger.Actions" /> property value should be serialized; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeActions()
	{
		if (_actions != null)
		{
			return _actions.Count > 0;
		}
		return false;
	}

	internal sealed override void Seal()
	{
		if (PropertyValues.Count > 0)
		{
			throw new InvalidOperationException(SR.EventTriggerDoNotSetProperties);
		}
		if (base.HasEnterActions || base.HasExitActions)
		{
			throw new InvalidOperationException(SR.EventTriggerDoesNotEnterExit);
		}
		if (_routedEvent != null && _actions != null && _actions.Count > 0)
		{
			_actions.Seal(this);
		}
		base.Seal();
	}

	internal static void ProcessTriggerCollection(FrameworkElement triggersHost)
	{
		TriggerCollection value = TriggerCollectionField.GetValue(triggersHost);
		if (value != null)
		{
			for (int i = 0; i < value.Count; i++)
			{
				ProcessOneTrigger(triggersHost, value[i]);
			}
		}
	}

	internal static void ProcessOneTrigger(FrameworkElement triggersHost, TriggerBase triggerBase)
	{
		if (triggerBase is EventTrigger eventTrigger)
		{
			eventTrigger._source = FrameworkElement.FindNamedFrameworkElement(triggersHost, eventTrigger.SourceName);
			EventTriggerSourceListener @object = new EventTriggerSourceListener(eventTrigger, triggersHost);
			eventTrigger._routedEventHandler = @object.Handler;
			eventTrigger._source.AddHandler(eventTrigger.RoutedEvent, eventTrigger._routedEventHandler, handledEventsToo: false);
			return;
		}
		throw new InvalidOperationException(SR.TriggersSupportsEventTriggersOnly);
	}

	internal static void DisconnectAllTriggers(FrameworkElement triggersHost)
	{
		TriggerCollection value = TriggerCollectionField.GetValue(triggersHost);
		if (value != null)
		{
			for (int i = 0; i < value.Count; i++)
			{
				DisconnectOneTrigger(triggersHost, value[i]);
			}
		}
	}

	internal static void DisconnectOneTrigger(FrameworkElement triggersHost, TriggerBase triggerBase)
	{
		if (triggerBase is EventTrigger eventTrigger)
		{
			eventTrigger._source.RemoveHandler(eventTrigger.RoutedEvent, eventTrigger._routedEventHandler);
			eventTrigger._routedEventHandler = null;
			return;
		}
		throw new InvalidOperationException(SR.TriggersSupportsEventTriggersOnly);
	}
}
