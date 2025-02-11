namespace System.Windows;

/// <summary>Contains arguments for the <see cref="T:System.Windows.DataObject" />.<see cref="E:System.Windows.DataObject.SettingData" /> event.</summary>
public sealed class DataObjectSettingDataEventArgs : DataObjectEventArgs
{
	private IDataObject _dataObject;

	private string _format;

	/// <summary>Gets the <see cref="T:System.Windows.DataObject" /> associated with the <see cref="E:System.Windows.DataObject.SettingData" /> event.</summary>
	/// <returns>The <see cref="T:System.Windows.DataObject" /> associated with the <see cref="E:System.Windows.DataObject.SettingData" /> event.</returns>
	public IDataObject DataObject => _dataObject;

	/// <summary>Gets a string specifying the new data format that is being added to the accompanying data object.</summary>
	/// <returns>A string specifying the new data format that is being added to the accompanying data object.</returns>
	public string Format => _format;

	/// <summary>Initializes a new instance of <see cref="T:System.Windows.DataObjectSettingDataEventArgs" />.</summary>
	/// <param name="dataObject">The <see cref="T:System.Windows.DataObject" /> to which a new data format is being added.</param>
	/// <param name="format">A string specifying the new data format that is being added to the accompanying data object. See the <see cref="T:System.Windows.DataFormats" /> class for a set of predefined data formats.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="dataObject" /> or <paramref name="format" /> is null.</exception>
	public DataObjectSettingDataEventArgs(IDataObject dataObject, string format)
		: base(System.Windows.DataObject.SettingDataEvent, isDragDrop: false)
	{
		if (dataObject == null)
		{
			throw new ArgumentNullException("dataObject");
		}
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		_dataObject = dataObject;
		_format = format;
	}

	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((DataObjectSettingDataEventHandler)genericHandler)(genericTarget, this);
	}
}
