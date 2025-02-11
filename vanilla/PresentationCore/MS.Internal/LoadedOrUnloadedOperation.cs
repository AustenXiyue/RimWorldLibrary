using System.Windows;
using System.Windows.Threading;
using MS.Internal.PresentationCore;

namespace MS.Internal;

[FriendAccessAllowed]
internal class LoadedOrUnloadedOperation
{
	private DispatcherOperationCallback _callback;

	private DependencyObject _target;

	private bool _cancelled;

	internal LoadedOrUnloadedOperation(DispatcherOperationCallback callback, DependencyObject target)
	{
		_callback = callback;
		_target = target;
	}

	internal void DoWork()
	{
		if (!_cancelled)
		{
			_callback(_target);
		}
	}

	internal void Cancel()
	{
		_cancelled = true;
	}
}
