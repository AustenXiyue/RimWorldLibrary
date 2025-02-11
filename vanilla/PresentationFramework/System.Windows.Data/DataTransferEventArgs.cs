namespace System.Windows.Data;

/// <summary>Encapsulates arguments for data transfer events.</summary>
public class DataTransferEventArgs : RoutedEventArgs
{
	private DependencyObject _targetObject;

	private DependencyProperty _dp;

	/// <summary>Gets the binding target object of the binding that raised the event.</summary>
	/// <returns>The target object of the binding that raised the event.</returns>
	public DependencyObject TargetObject => _targetObject;

	/// <summary>Gets the specific binding target property that is involved in the data transfer event.</summary>
	/// <returns>The property that changed.</returns>
	public DependencyProperty Property => _dp;

	internal DataTransferEventArgs(DependencyObject targetObject, DependencyProperty dp)
	{
		_targetObject = targetObject;
		_dp = dp;
	}

	/// <summary>Invokes the specified handler in a type-specific way on the specified object.</summary>
	/// <param name="genericHandler">The generic handler to call in a type-specific way.</param>
	/// <param name="genericTarget">The object to invoke the handler on.</param>
	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((EventHandler<DataTransferEventArgs>)genericHandler)(genericTarget, this);
	}
}
