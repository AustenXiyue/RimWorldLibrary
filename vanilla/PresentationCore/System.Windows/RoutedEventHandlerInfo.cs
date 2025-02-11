namespace System.Windows;

/// <summary>Provides special handling information to inform event listeners whether specific handlers should be invoked.</summary>
public struct RoutedEventHandlerInfo
{
	private Delegate _handler;

	private bool _handledEventsToo;

	/// <summary>Gets the event handler.</summary>
	/// <returns>The event handler.</returns>
	public Delegate Handler => _handler;

	/// <summary>Gets a value that indicates whether the event handler is invoked when the routed event is marked handled.</summary>
	/// <returns>true if the event handler is invoked when the routed event is marked handled; otherwise, false.</returns>
	public bool InvokeHandledEventsToo => _handledEventsToo;

	internal RoutedEventHandlerInfo(Delegate handler, bool handledEventsToo)
	{
		_handler = handler;
		_handledEventsToo = handledEventsToo;
	}

	internal void InvokeHandler(object target, RoutedEventArgs routedEventArgs)
	{
		if (!routedEventArgs.Handled || _handledEventsToo)
		{
			if (_handler is RoutedEventHandler)
			{
				((RoutedEventHandler)_handler)(target, routedEventArgs);
			}
			else
			{
				routedEventArgs.InvokeHandler(_handler, target);
			}
		}
	}

	/// <summary>Determines whether the specified object is equivalent to the current <see cref="T:System.Windows.RoutedEventHandlerInfo" />.</summary>
	/// <returns>true if the specified object is equivalent to the current <see cref="T:System.Windows.RoutedEventHandlerInfo" />; otherwise, false.</returns>
	/// <param name="obj">The object to compare to the current <see cref="T:System.Windows.RoutedEventHandlerInfo" />.</param>
	public override bool Equals(object obj)
	{
		if (obj == null || !(obj is RoutedEventHandlerInfo))
		{
			return false;
		}
		return Equals((RoutedEventHandlerInfo)obj);
	}

	/// <summary>Determines whether the specified <see cref="T:System.Windows.RoutedEventHandlerInfo" /> is equivalent to the current <see cref="T:System.Windows.RoutedEventHandlerInfo" />.</summary>
	/// <returns>true if the specified <see cref="T:System.Windows.RoutedEventHandlerInfo" /> is equivalent to the current <see cref="T:System.Windows.RoutedEventHandlerInfo" />; otherwise, false.</returns>
	/// <param name="handlerInfo">The <see cref="T:System.Windows.RoutedEventHandlerInfo" /> to compare to the current <see cref="T:System.Windows.RoutedEventHandlerInfo" />.</param>
	public bool Equals(RoutedEventHandlerInfo handlerInfo)
	{
		if (_handler == handlerInfo._handler)
		{
			return _handledEventsToo == handlerInfo._handledEventsToo;
		}
		return false;
	}

	/// <summary>Returns a hash code for the current <see cref="T:System.Windows.RoutedEventHandlerInfo" />.</summary>
	/// <returns>A hash code for the current <see cref="T:System.Windows.RoutedEventHandlerInfo" />.</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Determines whether the specified objects are equivalent.</summary>
	/// <returns>true if the objects are equivalent; otherwise, false.</returns>
	/// <param name="handlerInfo1">The first object to compare.</param>
	/// <param name="handlerInfo2">The second object to compare.</param>
	public static bool operator ==(RoutedEventHandlerInfo handlerInfo1, RoutedEventHandlerInfo handlerInfo2)
	{
		return handlerInfo1.Equals(handlerInfo2);
	}

	/// <summary>Determines whether the specified objects are not equivalent.</summary>
	/// <returns>true if the objects are not equivalent; otherwise, false.</returns>
	/// <param name="handlerInfo1">The first object to compare.</param>
	/// <param name="handlerInfo2">The second object to compare.</param>
	public static bool operator !=(RoutedEventHandlerInfo handlerInfo1, RoutedEventHandlerInfo handlerInfo2)
	{
		return !handlerInfo1.Equals(handlerInfo2);
	}
}
