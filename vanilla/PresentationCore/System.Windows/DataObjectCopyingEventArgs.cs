namespace System.Windows;

/// <summary>Arguments for the <see cref="T:System.Windows.DataObject" />.<see cref="E:System.Windows.DataObject.Copying" /> event.</summary>
public sealed class DataObjectCopyingEventArgs : DataObjectEventArgs
{
	private IDataObject _dataObject;

	/// <summary>Gets the data object associated with the <see cref="E:System.Windows.DataObject.Copying" /> event.</summary>
	/// <returns>The data object associated with the <see cref="E:System.Windows.DataObject.Copying" /> event.</returns>
	public IDataObject DataObject => _dataObject;

	/// <summary>Initializes a new instance of <see cref="T:System.Windows.DataObjectCopyingEventArgs" />.</summary>
	/// <param name="dataObject">A <see cref="T:System.Windows.DataObject" /> containing data ready to be copied.</param>
	/// <param name="isDragDrop">A Boolean value indicating whether the copy is part of a drag-and-drop operation. true to indicate that the copy is part of a drag-and-drop operation; otherwise, false. If this parameter is set to true, the <see cref="E:System.Windows.DataObject.Copying" /> event fires when dragging is initiated.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="dataObject" /> is null.</exception>
	public DataObjectCopyingEventArgs(IDataObject dataObject, bool isDragDrop)
		: base(System.Windows.DataObject.CopyingEvent, isDragDrop)
	{
		if (dataObject == null)
		{
			throw new ArgumentNullException("dataObject");
		}
		_dataObject = dataObject;
	}

	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((DataObjectCopyingEventHandler)genericHandler)(genericTarget, this);
	}
}
