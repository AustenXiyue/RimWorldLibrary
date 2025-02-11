using MS.Internal.WindowsBase;

namespace System.ComponentModel;

/// <summary>Provides information for the <see cref="E:System.ComponentModel.ICollectionView.CurrentChanging" /> event.</summary>
public class CurrentChangingEventArgs : EventArgs
{
	private bool _cancel;

	private bool _isCancelable;

	/// <summary>Gets a value that indicates whether the event is cancelable.</summary>
	/// <returns>true if the event is cancelable, otherwise, false. The default value is true.</returns>
	public bool IsCancelable => _isCancelable;

	/// <summary>Gets or sets a value that indicates whether to cancel the event.</summary>
	/// <returns>true if the event is to be canceled; otherwise, false. The default value is false.</returns>
	/// <exception cref="T:System.InvalidOperationException">If the value of <see cref="P:System.ComponentModel.CurrentChangingEventArgs.IsCancelable" /> is false.</exception>
	public bool Cancel
	{
		get
		{
			return _cancel;
		}
		set
		{
			if (IsCancelable)
			{
				_cancel = value;
			}
			else if (value)
			{
				throw new InvalidOperationException(SR.CurrentChangingCannotBeCanceled);
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.CurrentChangingEventArgs" /> class.</summary>
	public CurrentChangingEventArgs()
	{
		Initialize(isCancelable: true);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.CurrentChangingEventArgs" /> class with the specified <paramref name="isCancelable" /> value.</summary>
	/// <param name="isCancelable">A value that indicates whether the event is cancelable.</param>
	public CurrentChangingEventArgs(bool isCancelable)
	{
		Initialize(isCancelable);
	}

	private void Initialize(bool isCancelable)
	{
		_isCancelable = isCancelable;
	}
}
