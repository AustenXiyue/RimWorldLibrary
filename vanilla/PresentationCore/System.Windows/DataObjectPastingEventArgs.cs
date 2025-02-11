using MS.Internal.PresentationCore;

namespace System.Windows;

/// <summary>Contains arguments for the <see cref="T:System.Windows.DataObject" />.<see cref="E:System.Windows.DataObject.Pasting" /> event.</summary>
public sealed class DataObjectPastingEventArgs : DataObjectEventArgs
{
	private IDataObject _originalDataObject;

	private IDataObject _dataObject;

	private string _formatToApply;

	/// <summary>Gets a copy of the original data object associated with the paste operation.</summary>
	/// <returns>A copy of the original data object associated with the paste operation.</returns>
	public IDataObject SourceDataObject => _originalDataObject;

	/// <summary>Gets or sets a suggested <see cref="T:System.Windows.DataObject" /> to use for the paste operation.</summary>
	/// <returns>The currently suggested <see cref="T:System.Windows.DataObject" /> to use for the paste operation. Getting this value returns the currently suggested <see cref="T:System.Windows.DataObject" /> for the paste operation.Setting this value specifies a new suggested <see cref="T:System.Windows.DataObject" /> to use for the paste operation.</returns>
	/// <exception cref="T:System.ArgumentNullException">Raised when an attempt is made to set this property to null.</exception>
	/// <exception cref="T:System.ArgumentException">Raised when an attempt is made to set this property to a data object that contains no data formats.</exception>
	public IDataObject DataObject
	{
		get
		{
			return _dataObject;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			string[] formats = value.GetFormats(autoConvert: false);
			if (formats == null || formats.Length == 0)
			{
				throw new ArgumentException(SR.DataObject_DataObjectMustHaveAtLeastOneFormat);
			}
			_dataObject = value;
			_formatToApply = formats[0];
		}
	}

	/// <summary>Gets or sets a string specifying the suggested data format to use for the paste operation.</summary>
	/// <returns>A string specifying the suggested data format to use for the paste operation.Getting this value returns the currently suggested data format to use for the paste operation.Setting this value specifies a new suggested data format to use for the paste operation.</returns>
	/// <exception cref="T:System.ArgumentNullException">Raised when an attempt is made to set this property to null.</exception>
	/// <exception cref="T:System.ArgumentException">Raised when an attempt is made to set this property to a data format that is not present in the data object referenced by the <see cref="P:System.Windows.DataObjectPastingEventArgs.DataObject" /> property.</exception>
	public string FormatToApply
	{
		get
		{
			return _formatToApply;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (!_dataObject.GetDataPresent(value))
			{
				throw new ArgumentException(SR.Format(SR.DataObject_DataFormatNotPresentOnDataObject, value));
			}
			_formatToApply = value;
		}
	}

	/// <summary>Initializes a new instance of <see cref="T:System.Windows.DataObjectPastingEventArgs" />.</summary>
	/// <param name="dataObject">A <see cref="T:System.Windows.DataObject" /> containing the data to be pasted.</param>
	/// <param name="isDragDrop">A Boolean value indicating whether the paste is part of a drag-and-drop operation. true to indicate that the paste is part of a drag-and-drop operation; otherwise, false. If this parameter is set to true, a <see cref="E:System.Windows.DataObject.Pasting" /> event is fired on drop.</param>
	/// <param name="formatToApply">A string specifying the preferred data format to use for the paste operation. See the <see cref="T:System.Windows.DataFormats" /> class for a set of predefined data formats.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="dataObject" /> or <paramref name="formatToApply" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">Raised when <paramref name="formatToApply" /> specifies a data format that is not present in the data object specified by <paramref name="dataObject" />.</exception>
	public DataObjectPastingEventArgs(IDataObject dataObject, bool isDragDrop, string formatToApply)
		: base(System.Windows.DataObject.PastingEvent, isDragDrop)
	{
		if (dataObject == null)
		{
			throw new ArgumentNullException("dataObject");
		}
		if (formatToApply == null)
		{
			throw new ArgumentNullException("formatToApply");
		}
		if (formatToApply == string.Empty)
		{
			throw new ArgumentException(SR.DataObject_EmptyFormatNotAllowed);
		}
		if (!dataObject.GetDataPresent(formatToApply))
		{
			throw new ArgumentException(SR.Format(SR.DataObject_DataFormatNotPresentOnDataObject, formatToApply));
		}
		_originalDataObject = dataObject;
		_dataObject = dataObject;
		_formatToApply = formatToApply;
	}

	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((DataObjectPastingEventHandler)genericHandler)(genericTarget, this);
	}
}
