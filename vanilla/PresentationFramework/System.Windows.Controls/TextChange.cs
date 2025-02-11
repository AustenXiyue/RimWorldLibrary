namespace System.Windows.Controls;

/// <summary>Contains information about the changes that occur in the <see cref="E:System.Windows.Controls.Primitives.TextBoxBase.TextChanged" /> event.</summary>
public class TextChange
{
	private int _offset;

	private int _addedLength;

	private int _removedLength;

	/// <summary>Gets or sets the position at which the change occurred.</summary>
	/// <returns>The position at which the change occurred.</returns>
	public int Offset
	{
		get
		{
			return _offset;
		}
		internal set
		{
			_offset = value;
		}
	}

	/// <summary>Gets or sets the number of symbols that have been added to the control.</summary>
	/// <returns>The number of symbols that have been added to the control.</returns>
	public int AddedLength
	{
		get
		{
			return _addedLength;
		}
		internal set
		{
			_addedLength = value;
		}
	}

	/// <summary>Gets or sets the number of symbols that have been removed from the control.</summary>
	/// <returns>The number of symbols that have been removed from the control.</returns>
	public int RemovedLength
	{
		get
		{
			return _removedLength;
		}
		internal set
		{
			_removedLength = value;
		}
	}

	internal TextChange()
	{
	}
}
