namespace System.Windows;

/// <summary>Provides data for the <see cref="E:System.Windows.FrameworkElement.RequestBringIntoView" /> routed event.</summary>
public class RequestBringIntoViewEventArgs : RoutedEventArgs
{
	private DependencyObject _target;

	private Rect _rcTarget;

	/// <summary>Gets the object that should be made visible in response to the event.</summary>
	/// <returns>The object that called <see cref="M:System.Windows.FrameworkElement.BringIntoView" />.</returns>
	public DependencyObject TargetObject => _target;

	/// <summary>Gets the rectangular region in the object's coordinate space which should be made visible.</summary>
	/// <returns>The requested rectangular space.</returns>
	public Rect TargetRect => _rcTarget;

	internal RequestBringIntoViewEventArgs(DependencyObject target, Rect targetRect)
	{
		_target = target;
		_rcTarget = targetRect;
	}

	/// <summary>Invokes event handlers in a type-specific way, which can increase event system efficiency.</summary>
	/// <param name="genericHandler">The generic handler to call in a type-specific way.</param>
	/// <param name="genericTarget">The target to call the handler on.</param>
	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((RequestBringIntoViewEventHandler)genericHandler)(genericTarget, this);
	}
}
