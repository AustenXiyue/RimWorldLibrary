namespace System.Windows.Controls;

/// <summary>Provides data for the context menu event. </summary>
public sealed class ContextMenuEventArgs : RoutedEventArgs
{
	private double _left;

	private double _top;

	private DependencyObject _targetElement;

	/// <summary> Gets the horizontal position of the mouse.  </summary>
	/// <returns>The horizontal position of the mouse.</returns>
	public double CursorLeft => _left;

	/// <summary>Gets the vertical position of the mouse.  </summary>
	/// <returns>The vertical position of the mouse. </returns>
	public double CursorTop => _top;

	internal DependencyObject TargetElement
	{
		get
		{
			return _targetElement;
		}
		set
		{
			_targetElement = value;
		}
	}

	internal ContextMenuEventArgs(object source, bool opening)
		: this(source, opening, -1.0, -1.0)
	{
	}

	internal ContextMenuEventArgs(object source, bool opening, double left, double top)
	{
		_left = left;
		_top = top;
		base.RoutedEvent = (opening ? ContextMenuService.ContextMenuOpeningEvent : ContextMenuService.ContextMenuClosingEvent);
		base.Source = source;
	}

	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((ContextMenuEventHandler)genericHandler)(genericTarget, this);
	}
}
