namespace System.Windows;

/// <summary>Provides an abstract base class for events associated with the <see cref="T:System.Windows.DataObject" /> class.</summary>
public abstract class DataObjectEventArgs : RoutedEventArgs
{
	private bool _isDragDrop;

	private bool _commandCancelled;

	/// <summary>Gets a value indicating whether the associated event is part of a drag-and-drop operation.</summary>
	/// <returns>true if the associated event is part of a drag-and-drop operation; otherwise, false.</returns>
	public bool IsDragDrop => _isDragDrop;

	/// <summary>Gets a value indicating whether the associated command or operation has been canceled.</summary>
	/// <returns>true if the command has been canceled; otherwise, false.</returns>
	public bool CommandCancelled => _commandCancelled;

	internal DataObjectEventArgs(RoutedEvent routedEvent, bool isDragDrop)
	{
		if (routedEvent != DataObject.CopyingEvent && routedEvent != DataObject.PastingEvent && routedEvent != DataObject.SettingDataEvent)
		{
			throw new ArgumentOutOfRangeException("routedEvent");
		}
		base.RoutedEvent = routedEvent;
		_isDragDrop = isDragDrop;
		_commandCancelled = false;
	}

	/// <summary>Cancels the associated command or operation.</summary>
	public void CancelCommand()
	{
		_commandCancelled = true;
	}
}
