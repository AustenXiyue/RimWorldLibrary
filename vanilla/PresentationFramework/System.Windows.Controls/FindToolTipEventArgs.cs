namespace System.Windows.Controls;

internal sealed class FindToolTipEventArgs : RoutedEventArgs
{
	private DependencyObject _targetElement;

	private ToolTipService.TriggerAction _triggerAction;

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

	internal ToolTipService.TriggerAction TriggerAction => _triggerAction;

	internal FindToolTipEventArgs(ToolTipService.TriggerAction triggerAction)
	{
		base.RoutedEvent = ToolTipService.FindToolTipEvent;
		_triggerAction = triggerAction;
	}

	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((FindToolTipEventHandler)genericHandler)(genericTarget, this);
	}
}
