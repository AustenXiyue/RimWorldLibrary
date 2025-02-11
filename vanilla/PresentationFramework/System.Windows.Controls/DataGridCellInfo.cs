namespace System.Windows.Controls;

/// <summary>Represents information about a specific cell in a <see cref="T:System.Windows.Controls.DataGrid" />.</summary>
public struct DataGridCellInfo
{
	private ItemsControl.ItemInfo _info;

	private DataGridColumn _column;

	private WeakReference _owner;

	/// <summary>Gets the data item for the row that contains the cell.</summary>
	/// <returns>The data item for the row that contains the cell.</returns>
	public object Item
	{
		get
		{
			if (!(_info != null))
			{
				return null;
			}
			return _info.Item;
		}
	}

	/// <summary>Gets the column that contains the cell.</summary>
	/// <returns>The column that contains the cell.</returns>
	public DataGridColumn Column => _column;

	/// <summary>Gets a value that indicates whether the structure holds valid information.</summary>
	/// <returns>true if the structure has valid information; otherwise, false.</returns>
	public bool IsValid => ArePropertyValuesValid;

	internal bool IsSet
	{
		get
		{
			if (_column == null)
			{
				return _info.Item != DependencyProperty.UnsetValue;
			}
			return true;
		}
	}

	internal ItemsControl.ItemInfo ItemInfo => _info;

	private bool ArePropertyValuesValid
	{
		get
		{
			if (Item != DependencyProperty.UnsetValue)
			{
				return _column != null;
			}
			return false;
		}
	}

	internal static DataGridCellInfo Unset => new DataGridCellInfo(DependencyProperty.UnsetValue);

	private DataGrid Owner
	{
		get
		{
			if (_owner != null)
			{
				return (DataGrid)_owner.Target;
			}
			return null;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DataGridCellInfo" /> structure using the specified data item and column.</summary>
	/// <param name="item">The data item for the row that contains the cell.</param>
	/// <param name="column">The column that contains the cell.</param>
	public DataGridCellInfo(object item, DataGridColumn column)
	{
		if (column == null)
		{
			throw new ArgumentNullException("column");
		}
		_info = new ItemsControl.ItemInfo(item);
		_column = column;
		_owner = null;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DataGridCellInfo" /> structure for the specified cell.</summary>
	/// <param name="cell">The cell for which information is to be generated.</param>
	public DataGridCellInfo(DataGridCell cell)
	{
		if (cell == null)
		{
			throw new ArgumentNullException("cell");
		}
		DataGrid dataGridOwner = cell.DataGridOwner;
		_info = dataGridOwner.NewItemInfo(cell.RowDataItem, cell.RowOwner);
		_column = cell.Column;
		_owner = new WeakReference(dataGridOwner);
	}

	internal DataGridCellInfo(object item, DataGridColumn column, DataGrid owner)
	{
		_info = owner.NewItemInfo(item);
		_column = column;
		_owner = new WeakReference(owner);
	}

	internal DataGridCellInfo(ItemsControl.ItemInfo info, DataGridColumn column, DataGrid owner)
	{
		_info = info;
		_column = column;
		_owner = new WeakReference(owner);
	}

	internal DataGridCellInfo(object item)
	{
		_info = new ItemsControl.ItemInfo(item);
		_column = null;
		_owner = null;
	}

	internal DataGridCellInfo(DataGridCellInfo info)
	{
		_info = info._info.Clone();
		_column = info._column;
		_owner = info._owner;
	}

	private DataGridCellInfo(DataGrid owner, DataGridColumn column, object item)
	{
		_info = owner.NewItemInfo(item);
		_column = column;
		_owner = new WeakReference(owner);
	}

	internal static DataGridCellInfo CreatePossiblyPartialCellInfo(object item, DataGridColumn column, DataGrid owner)
	{
		if (item == null && column == null)
		{
			return Unset;
		}
		return new DataGridCellInfo(owner, column, (item == null) ? DependencyProperty.UnsetValue : item);
	}

	/// <summary>Indicates whether the specified object is equal to the current instance.</summary>
	/// <returns>true if the comparison object represents the same cell; otherwise, false. </returns>
	/// <param name="obj">The object to compare to the current instance.</param>
	public override bool Equals(object obj)
	{
		if (obj is DataGridCellInfo)
		{
			return EqualsImpl((DataGridCellInfo)obj);
		}
		return false;
	}

	/// <summary>Indicates whether two <see cref="T:System.Windows.Controls.DataGridCellInfo" /> instances are equal.</summary>
	/// <returns>true if the two structures represent the same cell; otherwise, false.</returns>
	/// <param name="cell1">The first structure to compare.</param>
	/// <param name="cell2">The second structure to compare.</param>
	public static bool operator ==(DataGridCellInfo cell1, DataGridCellInfo cell2)
	{
		return cell1.EqualsImpl(cell2);
	}

	/// <summary>Indicates whether two <see cref="T:System.Windows.Controls.DataGridCellInfo" /> instances are not equal.</summary>
	/// <returns>true if the two structures do not represent the same cell; otherwise, false.</returns>
	/// <param name="cell1">The first structure to compare.</param>
	/// <param name="cell2">The second structure to compare.</param>
	public static bool operator !=(DataGridCellInfo cell1, DataGridCellInfo cell2)
	{
		return !cell1.EqualsImpl(cell2);
	}

	internal bool EqualsImpl(DataGridCellInfo cell)
	{
		if (cell._column == _column && cell.Owner == Owner)
		{
			return cell._info == _info;
		}
		return false;
	}

	/// <summary>Returns a hash code for the current <see cref="T:System.Windows.Controls.DataGridCellInfo" /> structure.</summary>
	/// <returns>A hash code for the structure.</returns>
	public override int GetHashCode()
	{
		return ((!(_info == null)) ? _info.GetHashCode() : 0) ^ ((_column != null) ? _column.GetHashCode() : 0);
	}

	internal bool IsValidForDataGrid(DataGrid dataGrid)
	{
		DataGrid owner = Owner;
		if (!ArePropertyValuesValid || owner != dataGrid)
		{
			return owner == null;
		}
		return true;
	}
}
