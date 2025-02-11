namespace System.Windows;

/// <summary>Provides data about a change in value to a dependency property as reported by particular routed events, including the previous and current value of the property that changed. </summary>
/// <typeparam name="T">The type of the dependency property that has changed.</typeparam>
public class RoutedPropertyChangedEventArgs<T> : RoutedEventArgs
{
	private T _oldValue;

	private T _newValue;

	/// <summary>Gets the previous value of the property as reported by a property changed event. </summary>
	/// <returns>The generic value. In a practical implementation of the <see cref="T:System.Windows.RoutedPropertyChangedEventArgs`1" />, the generic type of this property is replaced with the constrained type of the implementation.</returns>
	public T OldValue => _oldValue;

	/// <summary>Gets the new value of a property as reported by a property changed event. </summary>
	/// <returns>The generic value. In a practical implementation of the <see cref="T:System.Windows.RoutedPropertyChangedEventArgs`1" />, the generic type of this property is replaced with the constrained type of the implementation.</returns>
	public T NewValue => _newValue;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.RoutedPropertyChangedEventArgs`1" /> class, with provided old and new values.</summary>
	/// <param name="oldValue">Previous value of the property, prior to the event being raised.</param>
	/// <param name="newValue">Current value of the property at the time of the event.</param>
	public RoutedPropertyChangedEventArgs(T oldValue, T newValue)
	{
		_oldValue = oldValue;
		_newValue = newValue;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.RoutedPropertyChangedEventArgs`1" /> class, with provided old and new values, and an event identifier.</summary>
	/// <param name="oldValue">Previous value of the property, prior to the event being raised.</param>
	/// <param name="newValue">Current value of the property at the time of the event.</param>
	/// <param name="routedEvent">Identifier of the routed event that this arguments class carries information for.</param>
	public RoutedPropertyChangedEventArgs(T oldValue, T newValue, RoutedEvent routedEvent)
		: this(oldValue, newValue)
	{
		base.RoutedEvent = routedEvent;
	}

	/// <summary>Invokes event handlers in a type-specific way, which can increase event system efficiency.</summary>
	/// <param name="genericHandler">The generic handler to call in a type-specific way.</param>
	/// <param name="genericTarget">The target to call the handler on.</param>
	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((RoutedPropertyChangedEventHandler<T>)genericHandler)(genericTarget, this);
	}
}
