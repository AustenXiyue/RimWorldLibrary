using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using MS.Internal;

namespace System.Windows.Controls;

/// <summary>Represents a collection of <see cref="T:System.Windows.Controls.GridViewColumn" /> objects.</summary>
[Serializable]
public class GridViewColumnCollection : ObservableCollection<GridViewColumn>
{
	[NonSerialized]
	private DependencyObject _owner;

	private bool _inViewMode;

	private List<GridViewColumn> _columns = new List<GridViewColumn>();

	private List<int> _actualIndices = new List<int>();

	private bool _isImmutable;

	[NonSerialized]
	private GridViewColumnCollectionChangedEventArgs _internalEventArg;

	internal List<GridViewColumn> ColumnCollection => _columns;

	internal List<int> IndexList => _actualIndices;

	internal DependencyObject Owner
	{
		get
		{
			return _owner;
		}
		set
		{
			if (value == _owner)
			{
				return;
			}
			if (value == null)
			{
				foreach (GridViewColumn column in _columns)
				{
					InheritanceContextHelper.RemoveContextFromObject(_owner, column);
				}
			}
			else
			{
				foreach (GridViewColumn column2 in _columns)
				{
					InheritanceContextHelper.ProvideContextForObject(value, column2);
				}
			}
			_owner = value;
		}
	}

	internal bool InViewMode
	{
		get
		{
			return _inViewMode;
		}
		set
		{
			_inViewMode = value;
		}
	}

	private bool IsImmutable
	{
		get
		{
			return _isImmutable;
		}
		set
		{
			_isImmutable = value;
		}
	}

	internal event NotifyCollectionChangedEventHandler InternalCollectionChanged
	{
		add
		{
			_internalCollectionChanged += value;
		}
		remove
		{
			_internalCollectionChanged -= value;
		}
	}

	private event NotifyCollectionChangedEventHandler _internalCollectionChanged;

	/// <summary>Removes all of the <see cref="T:System.Windows.Controls.GridViewColumn" /> objects from the <see cref="T:System.Windows.Controls.GridViewColumnCollection" />.</summary>
	protected override void ClearItems()
	{
		VerifyAccess();
		_internalEventArg = ClearPreprocess();
		base.ClearItems();
	}

	/// <summary>Removes a <see cref="T:System.Windows.Controls.GridViewColumn" /> from the <see cref="T:System.Windows.Controls.GridViewColumnCollection" /> at the specified index.</summary>
	/// <param name="index">The position of the <see cref="T:System.Windows.Controls.GridViewColumn" /> to remove.</param>
	protected override void RemoveItem(int index)
	{
		VerifyAccess();
		_internalEventArg = RemoveAtPreprocess(index);
		base.RemoveItem(index);
	}

	/// <summary>Adds a <see cref="T:System.Windows.Controls.GridViewColumn" /> to the collection at the specified index.</summary>
	/// <param name="index">The position to place the new <see cref="T:System.Windows.Controls.GridViewColumn" />.</param>
	/// <param name="column">The <see cref="T:System.Windows.Controls.GridViewColumn" /> to insert.</param>
	protected override void InsertItem(int index, GridViewColumn column)
	{
		VerifyAccess();
		_internalEventArg = InsertPreprocess(index, column);
		base.InsertItem(index, column);
	}

	/// <summary>Replaces the <see cref="T:System.Windows.Controls.GridViewColumn" /> that is at the specified index with another <see cref="T:System.Windows.Controls.GridViewColumn" />.</summary>
	/// <param name="index">The position at which the new <see cref="T:System.Windows.Controls.GridViewColumn" /> replaces the old <see cref="T:System.Windows.Controls.GridViewColumn" />.</param>
	/// <param name="column">The <see cref="T:System.Windows.Controls.GridViewColumn" /> to place at the specified position.</param>
	protected override void SetItem(int index, GridViewColumn column)
	{
		VerifyAccess();
		_internalEventArg = SetPreprocess(index, column);
		if (_internalEventArg != null)
		{
			base.SetItem(index, column);
		}
	}

	/// <summary>Changes the position of a <see cref="T:System.Windows.Controls.GridViewColumn" /> in the collection.</summary>
	/// <param name="oldIndex">The original position of the <see cref="T:System.Windows.Controls.GridViewColumn" />.</param>
	/// <param name="newIndex">The new position of the <see cref="T:System.Windows.Controls.GridViewColumn" />.</param>
	protected override void MoveItem(int oldIndex, int newIndex)
	{
		if (oldIndex != newIndex)
		{
			VerifyAccess();
			_internalEventArg = MovePreprocess(oldIndex, newIndex);
			base.MoveItem(oldIndex, newIndex);
		}
	}

	/// <summary>Raises the <see cref="E:System.Collections.ObjectModel.ObservableCollection`1.CollectionChanged" /> event when the <see cref="T:System.Windows.Controls.GridViewColumnCollection" /> changes.</summary>
	/// <param name="e">The event arguments.</param>
	protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
	{
		OnInternalCollectionChanged();
		base.OnCollectionChanged(e);
	}

	internal void BlockWrite()
	{
		IsImmutable = true;
	}

	internal void UnblockWrite()
	{
		IsImmutable = false;
	}

	private void OnInternalCollectionChanged()
	{
		if (this._internalCollectionChanged != null && _internalEventArg != null)
		{
			this._internalCollectionChanged(this, _internalEventArg);
			_internalEventArg = null;
		}
	}

	private void ColumnPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		GridViewColumn gridViewColumn = sender as GridViewColumn;
		if (this._internalCollectionChanged != null && gridViewColumn != null)
		{
			this._internalCollectionChanged(this, new GridViewColumnCollectionChangedEventArgs(gridViewColumn, e.PropertyName));
		}
	}

	private GridViewColumnCollectionChangedEventArgs MovePreprocess(int oldIndex, int newIndex)
	{
		VerifyIndexInRange(oldIndex, "oldIndex");
		VerifyIndexInRange(newIndex, "newIndex");
		int num = _actualIndices[oldIndex];
		if (oldIndex < newIndex)
		{
			for (int i = oldIndex; i < newIndex; i++)
			{
				_actualIndices[i] = _actualIndices[i + 1];
			}
		}
		else
		{
			for (int num2 = oldIndex; num2 > newIndex; num2--)
			{
				_actualIndices[num2] = _actualIndices[num2 - 1];
			}
		}
		_actualIndices[newIndex] = num;
		return new GridViewColumnCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, _columns[num], newIndex, oldIndex, num);
	}

	private GridViewColumnCollectionChangedEventArgs ClearPreprocess()
	{
		GridViewColumn[] array = new GridViewColumn[base.Count];
		if (base.Count > 0)
		{
			CopyTo(array, 0);
		}
		foreach (GridViewColumn column in _columns)
		{
			column.ResetPrivateData();
			((INotifyPropertyChanged)column).PropertyChanged -= ColumnPropertyChanged;
			InheritanceContextHelper.RemoveContextFromObject(_owner, column);
		}
		_columns.Clear();
		_actualIndices.Clear();
		return new GridViewColumnCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, array);
	}

	private GridViewColumnCollectionChangedEventArgs RemoveAtPreprocess(int index)
	{
		VerifyIndexInRange(index, "index");
		int num = _actualIndices[index];
		GridViewColumn gridViewColumn = _columns[num];
		gridViewColumn.ResetPrivateData();
		((INotifyPropertyChanged)gridViewColumn).PropertyChanged -= ColumnPropertyChanged;
		_columns.RemoveAt(num);
		UpdateIndexList(num, index);
		UpdateActualIndexInColumn(num);
		InheritanceContextHelper.RemoveContextFromObject(_owner, gridViewColumn);
		return new GridViewColumnCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, gridViewColumn, index, num);
	}

	private void UpdateIndexList(int actualIndex, int index)
	{
		for (int i = 0; i < index; i++)
		{
			int num = _actualIndices[i];
			if (num > actualIndex)
			{
				_actualIndices[i] = num - 1;
			}
		}
		for (int j = index + 1; j < _actualIndices.Count; j++)
		{
			int num2 = _actualIndices[j];
			if (num2 < actualIndex)
			{
				_actualIndices[j - 1] = num2;
			}
			else if (num2 > actualIndex)
			{
				_actualIndices[j - 1] = num2 - 1;
			}
		}
		_actualIndices.RemoveAt(_actualIndices.Count - 1);
	}

	private void UpdateActualIndexInColumn(int iStart)
	{
		for (int i = iStart; i < _columns.Count; i++)
		{
			_columns[i].ActualIndex = i;
		}
	}

	private GridViewColumnCollectionChangedEventArgs InsertPreprocess(int index, GridViewColumn column)
	{
		int count = _columns.Count;
		if (index < 0 || index > count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		ValidateColumnForInsert(column);
		_columns.Add(column);
		column.ActualIndex = count;
		_actualIndices.Insert(index, count);
		InheritanceContextHelper.ProvideContextForObject(_owner, column);
		((INotifyPropertyChanged)column).PropertyChanged += ColumnPropertyChanged;
		return new GridViewColumnCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, column, index, count);
	}

	private GridViewColumnCollectionChangedEventArgs SetPreprocess(int index, GridViewColumn newColumn)
	{
		VerifyIndexInRange(index, "index");
		GridViewColumn gridViewColumn = base[index];
		if (gridViewColumn != newColumn)
		{
			int actualIndex = _actualIndices[index];
			RemoveAtPreprocess(index);
			InsertPreprocess(index, newColumn);
			return new GridViewColumnCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newColumn, gridViewColumn, index, actualIndex);
		}
		return null;
	}

	private void VerifyIndexInRange(int index, string indexName)
	{
		if (index < 0 || index >= _actualIndices.Count)
		{
			throw new ArgumentOutOfRangeException(indexName);
		}
	}

	private void ValidateColumnForInsert(GridViewColumn column)
	{
		if (column == null)
		{
			throw new ArgumentNullException("column");
		}
		if (column.ActualIndex >= 0)
		{
			throw new InvalidOperationException(SR.ListView_NotAllowShareColumnToTwoColumnCollection);
		}
	}

	private void VerifyAccess()
	{
		if (IsImmutable)
		{
			throw new InvalidOperationException(SR.ListView_GridViewColumnCollectionIsReadOnly);
		}
		CheckReentrancy();
	}

	/// <summary>Initializes an instance of the <see cref="T:System.Windows.Controls.GridViewColumnCollection" /> class.</summary>
	public GridViewColumnCollection()
	{
	}
}
