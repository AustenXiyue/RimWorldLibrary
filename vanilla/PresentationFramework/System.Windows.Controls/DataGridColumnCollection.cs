using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows.Threading;
using MS.Internal;

namespace System.Windows.Controls;

internal class DataGridColumnCollection : ObservableCollection<DataGridColumn>
{
	private DataGrid _dataGridOwner;

	private bool _isUpdatingDisplayIndex;

	private List<int> _displayIndexMap;

	private bool _displayIndexMapInitialized;

	private bool _isClearingDisplayIndex;

	private bool _columnWidthsComputationPending;

	private Dictionary<DataGridColumn, DataGridLength> _originalWidthsForResize;

	private double? _averageColumnWidth;

	private List<RealizedColumnsBlock> _realizedColumnsBlockListForNonVirtualizedRows;

	private List<RealizedColumnsBlock> _realizedColumnsBlockListForVirtualizedRows;

	private bool _hasVisibleStarColumns;

	internal List<int> DisplayIndexMap
	{
		get
		{
			if (!DisplayIndexMapInitialized)
			{
				InitializeDisplayIndexMap();
			}
			return _displayIndexMap;
		}
		private set
		{
			_displayIndexMap = value;
		}
	}

	private bool IsUpdatingDisplayIndex
	{
		get
		{
			return _isUpdatingDisplayIndex;
		}
		set
		{
			_isUpdatingDisplayIndex = value;
		}
	}

	private DataGrid DataGridOwner => _dataGridOwner;

	internal bool DisplayIndexMapInitialized => _displayIndexMapInitialized;

	internal bool HasVisibleStarColumns
	{
		get
		{
			return _hasVisibleStarColumns;
		}
		private set
		{
			if (_hasVisibleStarColumns != value)
			{
				_hasVisibleStarColumns = value;
				DataGridOwner.OnHasVisibleStarColumnsChanged();
			}
		}
	}

	internal double AverageColumnWidth
	{
		get
		{
			if (!_averageColumnWidth.HasValue)
			{
				_averageColumnWidth = ComputeAverageColumnWidth();
			}
			return _averageColumnWidth.Value;
		}
	}

	internal bool ColumnWidthsComputationPending => _columnWidthsComputationPending;

	internal bool RebuildRealizedColumnsBlockListForNonVirtualizedRows { get; set; }

	internal List<RealizedColumnsBlock> RealizedColumnsBlockListForNonVirtualizedRows
	{
		get
		{
			return _realizedColumnsBlockListForNonVirtualizedRows;
		}
		set
		{
			_realizedColumnsBlockListForNonVirtualizedRows = value;
			DataGrid dataGridOwner = DataGridOwner;
			dataGridOwner.NotifyPropertyChanged(dataGridOwner, "RealizedColumnsBlockListForNonVirtualizedRows", default(DependencyPropertyChangedEventArgs), DataGridNotificationTarget.CellsPresenter | DataGridNotificationTarget.ColumnHeadersPresenter);
		}
	}

	internal List<RealizedColumnsBlock> RealizedColumnsDisplayIndexBlockListForNonVirtualizedRows { get; set; }

	internal bool RebuildRealizedColumnsBlockListForVirtualizedRows { get; set; }

	internal List<RealizedColumnsBlock> RealizedColumnsBlockListForVirtualizedRows
	{
		get
		{
			return _realizedColumnsBlockListForVirtualizedRows;
		}
		set
		{
			_realizedColumnsBlockListForVirtualizedRows = value;
			DataGrid dataGridOwner = DataGridOwner;
			dataGridOwner.NotifyPropertyChanged(dataGridOwner, "RealizedColumnsBlockListForVirtualizedRows", default(DependencyPropertyChangedEventArgs), DataGridNotificationTarget.CellsPresenter | DataGridNotificationTarget.ColumnHeadersPresenter);
		}
	}

	internal List<RealizedColumnsBlock> RealizedColumnsDisplayIndexBlockListForVirtualizedRows { get; set; }

	internal int FirstVisibleDisplayIndex
	{
		get
		{
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				if (ColumnFromDisplayIndex(i).IsVisible)
				{
					return i;
				}
			}
			return -1;
		}
	}

	internal int LastVisibleDisplayIndex
	{
		get
		{
			for (int num = base.Count - 1; num >= 0; num--)
			{
				if (ColumnFromDisplayIndex(num).IsVisible)
				{
					return num;
				}
			}
			return -1;
		}
	}

	internal bool RefreshAutoWidthColumns { get; set; }

	internal DataGridColumnCollection(DataGrid dataGridOwner)
	{
		DisplayIndexMap = new List<int>(5);
		_dataGridOwner = dataGridOwner;
		RealizedColumnsBlockListForNonVirtualizedRows = null;
		RealizedColumnsDisplayIndexBlockListForNonVirtualizedRows = null;
		RebuildRealizedColumnsBlockListForNonVirtualizedRows = true;
		RealizedColumnsBlockListForVirtualizedRows = null;
		RealizedColumnsDisplayIndexBlockListForVirtualizedRows = null;
		RebuildRealizedColumnsBlockListForVirtualizedRows = true;
	}

	protected override void InsertItem(int index, DataGridColumn item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item", SR.DataGrid_NullColumn);
		}
		if (item.DataGridOwner != null)
		{
			throw new ArgumentException(SR.Format(SR.DataGrid_InvalidColumnReuse, item.Header), "item");
		}
		if (DisplayIndexMapInitialized)
		{
			ValidateDisplayIndex(item, item.DisplayIndex, isAdding: true);
		}
		base.InsertItem(index, item);
		item.CoerceValue(DataGridColumn.IsFrozenProperty);
	}

	protected override void SetItem(int index, DataGridColumn item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item", SR.DataGrid_NullColumn);
		}
		if (index >= base.Count || index < 0)
		{
			throw new ArgumentOutOfRangeException("index", SR.Format(SR.DataGrid_ColumnIndexOutOfRange, item.Header));
		}
		if (item.DataGridOwner != null && base[index] != item)
		{
			throw new ArgumentException(SR.Format(SR.DataGrid_InvalidColumnReuse, item.Header), "item");
		}
		if (DisplayIndexMapInitialized)
		{
			ValidateDisplayIndex(item, item.DisplayIndex);
		}
		base.SetItem(index, item);
		item.CoerceValue(DataGridColumn.IsFrozenProperty);
	}

	protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
	{
		switch (e.Action)
		{
		case NotifyCollectionChangedAction.Add:
			if (DisplayIndexMapInitialized)
			{
				UpdateDisplayIndexForNewColumns(e.NewItems, e.NewStartingIndex);
			}
			InvalidateHasVisibleStarColumns();
			break;
		case NotifyCollectionChangedAction.Move:
			if (DisplayIndexMapInitialized)
			{
				UpdateDisplayIndexForMovedColumn(e.OldStartingIndex, e.NewStartingIndex);
			}
			break;
		case NotifyCollectionChangedAction.Remove:
			if (DisplayIndexMapInitialized)
			{
				UpdateDisplayIndexForRemovedColumns(e.OldItems, e.OldStartingIndex);
			}
			ClearDisplayIndex(e.OldItems, e.NewItems);
			InvalidateHasVisibleStarColumns();
			break;
		case NotifyCollectionChangedAction.Replace:
			if (DisplayIndexMapInitialized)
			{
				UpdateDisplayIndexForReplacedColumn(e.OldItems, e.NewItems);
			}
			ClearDisplayIndex(e.OldItems, e.NewItems);
			InvalidateHasVisibleStarColumns();
			break;
		case NotifyCollectionChangedAction.Reset:
			if (DisplayIndexMapInitialized)
			{
				DisplayIndexMap.Clear();
				DataGridOwner.UpdateColumnsOnVirtualizedCellInfoCollections(NotifyCollectionChangedAction.Reset, -1, null, -1);
			}
			HasVisibleStarColumns = false;
			break;
		}
		InvalidateAverageColumnWidth();
		base.OnCollectionChanged(e);
	}

	protected override void ClearItems()
	{
		ClearDisplayIndex(this, null);
		DataGridOwner.UpdateDataGridReference(this, clear: true);
		base.ClearItems();
	}

	internal void NotifyPropertyChanged(DependencyObject d, string propertyName, DependencyPropertyChangedEventArgs e, DataGridNotificationTarget target)
	{
		if (DataGridHelper.ShouldNotifyColumnCollection(target))
		{
			if (e.Property == DataGridColumn.DisplayIndexProperty)
			{
				OnColumnDisplayIndexChanged((DataGridColumn)d, (int)e.OldValue, (int)e.NewValue);
				if (((DataGridColumn)d).IsVisible)
				{
					InvalidateColumnRealization(invalidateForNonVirtualizedRows: true);
				}
			}
			else if (e.Property == DataGridColumn.WidthProperty)
			{
				if (((DataGridColumn)d).IsVisible)
				{
					InvalidateColumnRealization(invalidateForNonVirtualizedRows: false);
				}
			}
			else if (e.Property == DataGrid.FrozenColumnCountProperty)
			{
				InvalidateColumnRealization(invalidateForNonVirtualizedRows: false);
				OnDataGridFrozenColumnCountChanged((int)e.OldValue, (int)e.NewValue);
			}
			else if (e.Property == DataGridColumn.VisibilityProperty)
			{
				InvalidateAverageColumnWidth();
				InvalidateHasVisibleStarColumns();
				InvalidateColumnWidthsComputation();
				InvalidateColumnRealization(invalidateForNonVirtualizedRows: true);
			}
			else if (e.Property == DataGrid.EnableColumnVirtualizationProperty)
			{
				InvalidateColumnRealization(invalidateForNonVirtualizedRows: true);
			}
			else if (e.Property == DataGrid.CellsPanelHorizontalOffsetProperty)
			{
				OnCellsPanelHorizontalOffsetChanged(e);
			}
			else if (e.Property == DataGrid.HorizontalScrollOffsetProperty || string.Compare(propertyName, "ViewportWidth", StringComparison.Ordinal) == 0)
			{
				InvalidateColumnRealization(invalidateForNonVirtualizedRows: false);
			}
		}
		if (DataGridHelper.ShouldNotifyColumns(target))
		{
			int count = base.Count;
			for (int i = 0; i < count; i++)
			{
				base[i].NotifyPropertyChanged(d, e, DataGridNotificationTarget.Columns);
			}
		}
	}

	internal DataGridColumn ColumnFromDisplayIndex(int displayIndex)
	{
		return base[DisplayIndexMap[displayIndex]];
	}

	private int CoerceDefaultDisplayIndex(DataGridColumn column)
	{
		return CoerceDefaultDisplayIndex(column, IndexOf(column));
	}

	private int CoerceDefaultDisplayIndex(DataGridColumn column, int newDisplayIndex)
	{
		if (DataGridHelper.IsDefaultValue(column, DataGridColumn.DisplayIndexProperty))
		{
			bool isUpdatingDisplayIndex = IsUpdatingDisplayIndex;
			try
			{
				IsUpdatingDisplayIndex = true;
				column.DisplayIndex = newDisplayIndex;
				return newDisplayIndex;
			}
			finally
			{
				IsUpdatingDisplayIndex = isUpdatingDisplayIndex;
			}
		}
		return column.DisplayIndex;
	}

	private void OnColumnDisplayIndexChanged(DataGridColumn column, int oldDisplayIndex, int newDisplayIndex)
	{
		int num = oldDisplayIndex;
		if (!_displayIndexMapInitialized)
		{
			InitializeDisplayIndexMap(column, oldDisplayIndex, out oldDisplayIndex);
		}
		if (_isClearingDisplayIndex)
		{
			return;
		}
		newDisplayIndex = CoerceDefaultDisplayIndex(column);
		if (newDisplayIndex != oldDisplayIndex)
		{
			if (num != -1)
			{
				DataGridOwner.OnColumnDisplayIndexChanged(new DataGridColumnEventArgs(column));
			}
			UpdateDisplayIndexForChangedColumn(oldDisplayIndex, newDisplayIndex);
		}
	}

	private void UpdateDisplayIndexForChangedColumn(int oldDisplayIndex, int newDisplayIndex)
	{
		if (IsUpdatingDisplayIndex)
		{
			return;
		}
		try
		{
			IsUpdatingDisplayIndex = true;
			int item = DisplayIndexMap[oldDisplayIndex];
			DisplayIndexMap.RemoveAt(oldDisplayIndex);
			DisplayIndexMap.Insert(newDisplayIndex, item);
			if (newDisplayIndex < oldDisplayIndex)
			{
				for (int i = newDisplayIndex + 1; i <= oldDisplayIndex; i++)
				{
					ColumnFromDisplayIndex(i).DisplayIndex++;
				}
			}
			else
			{
				for (int j = oldDisplayIndex; j < newDisplayIndex; j++)
				{
					ColumnFromDisplayIndex(j).DisplayIndex--;
				}
			}
			DataGridOwner.UpdateColumnsOnVirtualizedCellInfoCollections(NotifyCollectionChangedAction.Move, oldDisplayIndex, null, newDisplayIndex);
		}
		finally
		{
			IsUpdatingDisplayIndex = false;
		}
	}

	private void UpdateDisplayIndexForMovedColumn(int oldColumnIndex, int newColumnIndex)
	{
		int newDisplayIndex = RemoveFromDisplayIndexMap(oldColumnIndex);
		InsertInDisplayIndexMap(newDisplayIndex, newColumnIndex);
		DataGridOwner.UpdateColumnsOnVirtualizedCellInfoCollections(NotifyCollectionChangedAction.Move, oldColumnIndex, null, newColumnIndex);
	}

	private void UpdateDisplayIndexForNewColumns(IList newColumns, int startingIndex)
	{
		try
		{
			IsUpdatingDisplayIndex = true;
			DataGridColumn column = (DataGridColumn)newColumns[0];
			int num = CoerceDefaultDisplayIndex(column, startingIndex);
			InsertInDisplayIndexMap(num, startingIndex);
			for (int i = 0; i < DisplayIndexMap.Count; i++)
			{
				if (i > num)
				{
					ColumnFromDisplayIndex(i).DisplayIndex++;
				}
			}
			DataGridOwner.UpdateColumnsOnVirtualizedCellInfoCollections(NotifyCollectionChangedAction.Add, -1, null, num);
		}
		finally
		{
			IsUpdatingDisplayIndex = false;
		}
	}

	internal void InitializeDisplayIndexMap()
	{
		int resultDisplayIndex = -1;
		InitializeDisplayIndexMap(null, -1, out resultDisplayIndex);
	}

	private void InitializeDisplayIndexMap(DataGridColumn changingColumn, int oldDisplayIndex, out int resultDisplayIndex)
	{
		resultDisplayIndex = oldDisplayIndex;
		if (_displayIndexMapInitialized)
		{
			return;
		}
		_displayIndexMapInitialized = true;
		int count = base.Count;
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		if (changingColumn != null && oldDisplayIndex >= count)
		{
			throw new ArgumentOutOfRangeException("displayIndex", oldDisplayIndex, SR.Format(SR.DataGrid_ColumnDisplayIndexOutOfRange, changingColumn.Header));
		}
		for (int i = 0; i < count; i++)
		{
			DataGridColumn dataGridColumn = base[i];
			int num = dataGridColumn.DisplayIndex;
			ValidateDisplayIndex(dataGridColumn, num);
			if (dataGridColumn == changingColumn)
			{
				num = oldDisplayIndex;
			}
			if (num >= 0)
			{
				if (dictionary.ContainsKey(num))
				{
					throw new ArgumentException(SR.DataGrid_DuplicateDisplayIndex);
				}
				dictionary.Add(num, i);
			}
		}
		int j = 0;
		for (int k = 0; k < count; k++)
		{
			DataGridColumn dataGridColumn2 = base[k];
			_ = dataGridColumn2.DisplayIndex;
			bool flag = DataGridHelper.IsDefaultValue(dataGridColumn2, DataGridColumn.DisplayIndexProperty);
			if (dataGridColumn2 == changingColumn && oldDisplayIndex == -1)
			{
				flag = true;
			}
			if (flag)
			{
				for (; dictionary.ContainsKey(j); j++)
				{
				}
				CoerceDefaultDisplayIndex(dataGridColumn2, j);
				dictionary.Add(j, k);
				if (dataGridColumn2 == changingColumn)
				{
					resultDisplayIndex = j;
				}
				j++;
			}
		}
		for (int l = 0; l < count; l++)
		{
			DisplayIndexMap.Add(dictionary[l]);
		}
	}

	private void UpdateDisplayIndexForRemovedColumns(IList oldColumns, int startingIndex)
	{
		try
		{
			IsUpdatingDisplayIndex = true;
			int num = RemoveFromDisplayIndexMap(startingIndex);
			for (int i = 0; i < DisplayIndexMap.Count; i++)
			{
				if (i >= num)
				{
					ColumnFromDisplayIndex(i).DisplayIndex--;
				}
			}
			DataGridOwner.UpdateColumnsOnVirtualizedCellInfoCollections(NotifyCollectionChangedAction.Remove, num, (DataGridColumn)oldColumns[0], -1);
		}
		finally
		{
			IsUpdatingDisplayIndex = false;
		}
	}

	private void UpdateDisplayIndexForReplacedColumn(IList oldColumns, IList newColumns)
	{
		if (oldColumns == null || oldColumns.Count <= 0 || newColumns == null || newColumns.Count <= 0)
		{
			return;
		}
		DataGridColumn dataGridColumn = (DataGridColumn)oldColumns[0];
		DataGridColumn dataGridColumn2 = (DataGridColumn)newColumns[0];
		if (dataGridColumn != null && dataGridColumn2 != null)
		{
			int num = CoerceDefaultDisplayIndex(dataGridColumn2);
			if (dataGridColumn.DisplayIndex != num)
			{
				UpdateDisplayIndexForChangedColumn(dataGridColumn.DisplayIndex, num);
			}
			DataGridOwner.UpdateColumnsOnVirtualizedCellInfoCollections(NotifyCollectionChangedAction.Replace, num, dataGridColumn, num);
		}
	}

	private void ClearDisplayIndex(IList oldColumns, IList newColumns)
	{
		if (oldColumns == null)
		{
			return;
		}
		try
		{
			_isClearingDisplayIndex = true;
			int count = oldColumns.Count;
			for (int i = 0; i < count; i++)
			{
				DataGridColumn dataGridColumn = (DataGridColumn)oldColumns[i];
				if (newColumns == null || !newColumns.Contains(dataGridColumn))
				{
					dataGridColumn.ClearValue(DataGridColumn.DisplayIndexProperty);
				}
			}
		}
		finally
		{
			_isClearingDisplayIndex = false;
		}
	}

	private bool IsDisplayIndexValid(DataGridColumn column, int displayIndex, bool isAdding)
	{
		if (displayIndex == -1 && DataGridHelper.IsDefaultValue(column, DataGridColumn.DisplayIndexProperty))
		{
			return true;
		}
		if (displayIndex >= 0)
		{
			if (!isAdding)
			{
				return displayIndex < base.Count;
			}
			return displayIndex <= base.Count;
		}
		return false;
	}

	private void InsertInDisplayIndexMap(int newDisplayIndex, int columnIndex)
	{
		DisplayIndexMap.Insert(newDisplayIndex, columnIndex);
		for (int i = 0; i < DisplayIndexMap.Count; i++)
		{
			if (DisplayIndexMap[i] >= columnIndex && i != newDisplayIndex)
			{
				DisplayIndexMap[i]++;
			}
		}
	}

	private int RemoveFromDisplayIndexMap(int columnIndex)
	{
		int num = DisplayIndexMap.IndexOf(columnIndex);
		DisplayIndexMap.RemoveAt(num);
		for (int i = 0; i < DisplayIndexMap.Count; i++)
		{
			if (DisplayIndexMap[i] >= columnIndex)
			{
				DisplayIndexMap[i]--;
			}
		}
		return num;
	}

	internal void ValidateDisplayIndex(DataGridColumn column, int displayIndex)
	{
		ValidateDisplayIndex(column, displayIndex, isAdding: false);
	}

	internal void ValidateDisplayIndex(DataGridColumn column, int displayIndex, bool isAdding)
	{
		if (!IsDisplayIndexValid(column, displayIndex, isAdding))
		{
			throw new ArgumentOutOfRangeException("displayIndex", displayIndex, SR.Format(SR.DataGrid_ColumnDisplayIndexOutOfRange, column.Header));
		}
	}

	[Conditional("DEBUG")]
	private void Debug_VerifyDisplayIndexMap()
	{
		for (int i = 0; i < DisplayIndexMap.Count; i++)
		{
		}
	}

	private void OnDataGridFrozenColumnCountChanged(int oldFrozenCount, int newFrozenCount)
	{
		if (newFrozenCount > oldFrozenCount)
		{
			int num = Math.Min(newFrozenCount, base.Count);
			for (int i = oldFrozenCount; i < num; i++)
			{
				ColumnFromDisplayIndex(i).IsFrozen = true;
			}
		}
		else
		{
			int num2 = Math.Min(oldFrozenCount, base.Count);
			for (int j = newFrozenCount; j < num2; j++)
			{
				ColumnFromDisplayIndex(j).IsFrozen = false;
			}
		}
	}

	private bool HasVisibleStarColumnsInternal(DataGridColumn ignoredColumn, out double perStarWidth)
	{
		bool result = false;
		perStarWidth = 0.0;
		using (IEnumerator<DataGridColumn> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				DataGridColumn current = enumerator.Current;
				if (current == ignoredColumn || !current.IsVisible)
				{
					continue;
				}
				DataGridLength width = current.Width;
				if (width.IsStar)
				{
					result = true;
					if (!DoubleUtil.AreClose(width.Value, 0.0) && !DoubleUtil.AreClose(width.DesiredValue, 0.0))
					{
						perStarWidth = width.DesiredValue / width.Value;
						break;
					}
				}
			}
		}
		return result;
	}

	private bool HasVisibleStarColumnsInternal(out double perStarWidth)
	{
		return HasVisibleStarColumnsInternal(null, out perStarWidth);
	}

	private bool HasVisibleStarColumnsInternal(DataGridColumn ignoredColumn)
	{
		double perStarWidth;
		return HasVisibleStarColumnsInternal(ignoredColumn, out perStarWidth);
	}

	internal void InvalidateHasVisibleStarColumns()
	{
		HasVisibleStarColumns = HasVisibleStarColumnsInternal(null);
	}

	private void RecomputeStarColumnWidths()
	{
		double viewportWidthForColumns = DataGridOwner.GetViewportWidthForColumns();
		double num = 0.0;
		using (IEnumerator<DataGridColumn> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				DataGridColumn current = enumerator.Current;
				DataGridLength width = current.Width;
				if (current.IsVisible && !width.IsStar)
				{
					num += width.DisplayValue;
				}
			}
		}
		if (!double.IsNaN(num))
		{
			ComputeStarColumnWidths(viewportWidthForColumns - num);
		}
	}

	private double ComputeStarColumnWidths(double availableStarSpace)
	{
		List<DataGridColumn> list = new List<DataGridColumn>();
		List<DataGridColumn> list2 = new List<DataGridColumn>();
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		using (IEnumerator<DataGridColumn> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				DataGridColumn current = enumerator.Current;
				DataGridLength width = current.Width;
				if (current.IsVisible && width.IsStar)
				{
					list.Add(current);
					num += width.Value;
					num2 += current.MinWidth;
					num3 += current.MaxWidth;
				}
			}
		}
		if (DoubleUtil.LessThan(availableStarSpace, num2))
		{
			availableStarSpace = num2;
		}
		if (DoubleUtil.GreaterThan(availableStarSpace, num3))
		{
			availableStarSpace = num3;
		}
		while (list.Count > 0)
		{
			double num5 = availableStarSpace / num;
			int i = 0;
			for (int num6 = list.Count; i < num6; i++)
			{
				DataGridColumn dataGridColumn = list[i];
				DataGridLength width2 = dataGridColumn.Width;
				double minWidth = dataGridColumn.MinWidth;
				double value = availableStarSpace * width2.Value / num;
				if (DoubleUtil.GreaterThan(minWidth, value))
				{
					availableStarSpace = Math.Max(0.0, availableStarSpace - minWidth);
					num -= width2.Value;
					list.RemoveAt(i);
					i--;
					num6--;
					list2.Add(dataGridColumn);
				}
			}
			bool flag = false;
			int j = 0;
			for (int count = list.Count; j < count; j++)
			{
				DataGridColumn dataGridColumn2 = list[j];
				DataGridLength width3 = dataGridColumn2.Width;
				double maxWidth = dataGridColumn2.MaxWidth;
				double value2 = availableStarSpace * width3.Value / num;
				if (DoubleUtil.LessThan(maxWidth, value2))
				{
					flag = true;
					list.RemoveAt(j);
					availableStarSpace -= maxWidth;
					num4 += maxWidth;
					num -= width3.Value;
					dataGridColumn2.UpdateWidthForStarColumn(maxWidth, num5 * width3.Value, width3.Value);
					break;
				}
			}
			if (flag)
			{
				int k = 0;
				for (int count2 = list2.Count; k < count2; k++)
				{
					DataGridColumn dataGridColumn3 = list2[k];
					list.Add(dataGridColumn3);
					availableStarSpace += dataGridColumn3.MinWidth;
					num += dataGridColumn3.Width.Value;
				}
				list2.Clear();
				continue;
			}
			int l = 0;
			for (int count3 = list2.Count; l < count3; l++)
			{
				DataGridColumn dataGridColumn4 = list2[l];
				DataGridLength width4 = dataGridColumn4.Width;
				double minWidth2 = dataGridColumn4.MinWidth;
				dataGridColumn4.UpdateWidthForStarColumn(minWidth2, width4.Value * num5, width4.Value);
				num4 += minWidth2;
			}
			list2.Clear();
			int m = 0;
			for (int count4 = list.Count; m < count4; m++)
			{
				DataGridColumn dataGridColumn5 = list[m];
				DataGridLength width5 = dataGridColumn5.Width;
				double num7 = availableStarSpace * width5.Value / num;
				dataGridColumn5.UpdateWidthForStarColumn(num7, width5.Value * num5, width5.Value);
				num4 += num7;
			}
			list.Clear();
		}
		return num4;
	}

	private void OnCellsPanelHorizontalOffsetChanged(DependencyPropertyChangedEventArgs e)
	{
		InvalidateColumnRealization(invalidateForNonVirtualizedRows: false);
		double viewportWidthForColumns = DataGridOwner.GetViewportWidthForColumns();
		RedistributeColumnWidthsOnAvailableSpaceChange((double)e.OldValue - (double)e.NewValue, viewportWidthForColumns);
	}

	internal void InvalidateAverageColumnWidth()
	{
		_averageColumnWidth = null;
		((DataGridOwner == null) ? null : (DataGridOwner.InternalItemsHost as VirtualizingStackPanel))?.ResetMaximumDesiredSize();
	}

	private double ComputeAverageColumnWidth()
	{
		double num = 0.0;
		int num2 = 0;
		using (IEnumerator<DataGridColumn> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				DataGridColumn current = enumerator.Current;
				DataGridLength width = current.Width;
				if (current.IsVisible && !double.IsNaN(width.DisplayValue))
				{
					num += width.DisplayValue;
					num2++;
				}
			}
		}
		if (num2 != 0)
		{
			return num / (double)num2;
		}
		return 0.0;
	}

	internal void InvalidateColumnWidthsComputation()
	{
		if (!_columnWidthsComputationPending)
		{
			DataGridOwner.Dispatcher.BeginInvoke(new DispatcherOperationCallback(ComputeColumnWidths), DispatcherPriority.Render, this);
			_columnWidthsComputationPending = true;
		}
	}

	private object ComputeColumnWidths(object arg)
	{
		ComputeColumnWidths();
		DataGridOwner.NotifyPropertyChanged(DataGridOwner, "DelayedColumnWidthComputation", default(DependencyPropertyChangedEventArgs), DataGridNotificationTarget.CellsPresenter | DataGridNotificationTarget.ColumnHeadersPresenter);
		return null;
	}

	private void ComputeColumnWidths()
	{
		if (HasVisibleStarColumns)
		{
			InitializeColumnDisplayValues();
			DistributeSpaceAmongColumns(DataGridOwner.GetViewportWidthForColumns());
		}
		else
		{
			ExpandAllColumnWidthsToDesiredValue();
		}
		if (RefreshAutoWidthColumns)
		{
			using (IEnumerator<DataGridColumn> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					DataGridColumn current = enumerator.Current;
					if (current.Width.IsAuto)
					{
						current.Width = DataGridLength.Auto;
					}
				}
			}
			RefreshAutoWidthColumns = false;
		}
		_columnWidthsComputationPending = false;
	}

	private void InitializeColumnDisplayValues()
	{
		using IEnumerator<DataGridColumn> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			DataGridColumn current = enumerator.Current;
			if (!current.IsVisible)
			{
				continue;
			}
			DataGridLength width = current.Width;
			if (!width.IsStar)
			{
				double minWidth = current.MinWidth;
				double num = DataGridHelper.CoerceToMinMax(double.IsNaN(width.DesiredValue) ? minWidth : width.DesiredValue, minWidth, current.MaxWidth);
				if (!DoubleUtil.AreClose(width.DisplayValue, num))
				{
					current.SetWidthInternal(new DataGridLength(width.Value, width.UnitType, width.DesiredValue, num));
				}
			}
		}
	}

	internal void RedistributeColumnWidthsOnMinWidthChangeOfColumn(DataGridColumn changedColumn, double oldMinWidth)
	{
		if (ColumnWidthsComputationPending)
		{
			return;
		}
		DataGridLength width = changedColumn.Width;
		double minWidth = changedColumn.MinWidth;
		if (DoubleUtil.GreaterThan(minWidth, width.DisplayValue))
		{
			if (HasVisibleStarColumns)
			{
				TakeAwayWidthFromColumns(changedColumn, minWidth - width.DisplayValue, widthAlreadyUtilized: false);
			}
			changedColumn.SetWidthInternal(new DataGridLength(width.Value, width.UnitType, width.DesiredValue, minWidth));
		}
		else
		{
			if (!DoubleUtil.LessThan(minWidth, oldMinWidth))
			{
				return;
			}
			if (width.IsStar)
			{
				if (DoubleUtil.AreClose(width.DisplayValue, oldMinWidth))
				{
					GiveAwayWidthToColumns(changedColumn, oldMinWidth - minWidth, recomputeStars: true);
				}
			}
			else if (DoubleUtil.GreaterThan(oldMinWidth, width.DesiredValue))
			{
				double num = Math.Max(width.DesiredValue, minWidth);
				if (HasVisibleStarColumns)
				{
					GiveAwayWidthToColumns(changedColumn, oldMinWidth - num);
				}
				changedColumn.SetWidthInternal(new DataGridLength(width.Value, width.UnitType, width.DesiredValue, num));
			}
		}
	}

	internal void RedistributeColumnWidthsOnMaxWidthChangeOfColumn(DataGridColumn changedColumn, double oldMaxWidth)
	{
		if (ColumnWidthsComputationPending)
		{
			return;
		}
		DataGridLength width = changedColumn.Width;
		double maxWidth = changedColumn.MaxWidth;
		if (DoubleUtil.LessThan(maxWidth, width.DisplayValue))
		{
			if (HasVisibleStarColumns)
			{
				GiveAwayWidthToColumns(changedColumn, width.DisplayValue - maxWidth);
			}
			changedColumn.SetWidthInternal(new DataGridLength(width.Value, width.UnitType, width.DesiredValue, maxWidth));
		}
		else
		{
			if (!DoubleUtil.GreaterThan(maxWidth, oldMaxWidth))
			{
				return;
			}
			if (width.IsStar)
			{
				RecomputeStarColumnWidths();
			}
			else if (DoubleUtil.LessThan(oldMaxWidth, width.DesiredValue))
			{
				double num = Math.Min(width.DesiredValue, maxWidth);
				if (HasVisibleStarColumns)
				{
					double takeAwayWidth = TakeAwayWidthFromUnusedSpace(spaceAlreadyUtilized: false, num - oldMaxWidth);
					takeAwayWidth = TakeAwayWidthFromStarColumns(changedColumn, takeAwayWidth);
					num -= takeAwayWidth;
				}
				changedColumn.SetWidthInternal(new DataGridLength(width.Value, width.UnitType, width.DesiredValue, num));
			}
		}
	}

	internal void RedistributeColumnWidthsOnWidthChangeOfColumn(DataGridColumn changedColumn, DataGridLength oldWidth)
	{
		if (ColumnWidthsComputationPending)
		{
			return;
		}
		DataGridLength width = changedColumn.Width;
		bool hasVisibleStarColumns = HasVisibleStarColumns;
		if (oldWidth.IsStar && !width.IsStar && !hasVisibleStarColumns)
		{
			ExpandAllColumnWidthsToDesiredValue();
		}
		else if (width.IsStar && !oldWidth.IsStar)
		{
			if (!HasVisibleStarColumnsInternal(changedColumn))
			{
				ComputeColumnWidths();
				return;
			}
			double minWidth = changedColumn.MinWidth;
			double num = GiveAwayWidthToNonStarColumns(null, oldWidth.DisplayValue - minWidth);
			changedColumn.SetWidthInternal(new DataGridLength(width.Value, width.UnitType, width.DesiredValue, minWidth + num));
			RecomputeStarColumnWidths();
		}
		else if (width.IsStar && oldWidth.IsStar)
		{
			RecomputeStarColumnWidths();
		}
		else if (hasVisibleStarColumns)
		{
			RedistributeColumnWidthsOnNonStarWidthChange(changedColumn, oldWidth);
		}
	}

	internal void RedistributeColumnWidthsOnAvailableSpaceChange(double availableSpaceChange, double newTotalAvailableSpace)
	{
		if (!ColumnWidthsComputationPending && HasVisibleStarColumns)
		{
			if (DoubleUtil.GreaterThanZero(availableSpaceChange))
			{
				GiveAwayWidthToColumns(null, availableSpaceChange);
			}
			else if (DoubleUtil.LessThan(availableSpaceChange, 0.0))
			{
				TakeAwayWidthFromColumns(null, Math.Abs(availableSpaceChange), widthAlreadyUtilized: false, newTotalAvailableSpace);
			}
		}
	}

	private void ExpandAllColumnWidthsToDesiredValue()
	{
		using IEnumerator<DataGridColumn> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			DataGridColumn current = enumerator.Current;
			if (current.IsVisible)
			{
				DataGridLength width = current.Width;
				double maxWidth = current.MaxWidth;
				if (DoubleUtil.GreaterThan(width.DesiredValue, width.DisplayValue) && !DoubleUtil.AreClose(width.DisplayValue, maxWidth))
				{
					current.SetWidthInternal(new DataGridLength(width.Value, width.UnitType, width.DesiredValue, Math.Min(width.DesiredValue, maxWidth)));
				}
			}
		}
	}

	private void RedistributeColumnWidthsOnNonStarWidthChange(DataGridColumn changedColumn, DataGridLength oldWidth)
	{
		DataGridLength width = changedColumn.Width;
		if (DoubleUtil.GreaterThan(width.DesiredValue, oldWidth.DisplayValue))
		{
			double num = TakeAwayWidthFromColumns(changedColumn, width.DesiredValue - oldWidth.DisplayValue, changedColumn != null);
			if (DoubleUtil.GreaterThanZero(num))
			{
				changedColumn.SetWidthInternal(new DataGridLength(width.Value, width.UnitType, width.DesiredValue, Math.Max(width.DisplayValue - num, changedColumn.MinWidth)));
			}
		}
		else if (DoubleUtil.LessThan(width.DesiredValue, oldWidth.DisplayValue))
		{
			double num2 = DataGridHelper.CoerceToMinMax(width.DesiredValue, changedColumn.MinWidth, changedColumn.MaxWidth);
			GiveAwayWidthToColumns(changedColumn, oldWidth.DisplayValue - num2);
		}
	}

	private void DistributeSpaceAmongColumns(double availableSpace)
	{
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		using (IEnumerator<DataGridColumn> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				DataGridColumn current = enumerator.Current;
				if (current.IsVisible)
				{
					num += current.MinWidth;
					num2 += current.MaxWidth;
					if (current.Width.IsStar)
					{
						num3 += current.MinWidth;
					}
				}
			}
		}
		if (DoubleUtil.LessThan(availableSpace, num))
		{
			availableSpace = num;
		}
		if (DoubleUtil.GreaterThan(availableSpace, num2))
		{
			availableSpace = num2;
		}
		double num4 = DistributeSpaceAmongNonStarColumns(availableSpace - num3);
		ComputeStarColumnWidths(num3 + num4);
	}

	private double DistributeSpaceAmongNonStarColumns(double availableSpace)
	{
		double num = 0.0;
		using (IEnumerator<DataGridColumn> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				DataGridColumn current = enumerator.Current;
				DataGridLength width = current.Width;
				if (current.IsVisible && !width.IsStar)
				{
					num += width.DisplayValue;
				}
			}
		}
		if (DoubleUtil.LessThan(availableSpace, num))
		{
			double takeAwayWidth = num - availableSpace;
			TakeAwayWidthFromNonStarColumns(null, takeAwayWidth);
		}
		return Math.Max(availableSpace - num, 0.0);
	}

	internal void OnColumnResizeStarted()
	{
		_originalWidthsForResize = new Dictionary<DataGridColumn, DataGridLength>();
		using IEnumerator<DataGridColumn> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			DataGridColumn current = enumerator.Current;
			_originalWidthsForResize[current] = current.Width;
		}
	}

	internal void OnColumnResizeCompleted(bool cancel)
	{
		if (cancel && _originalWidthsForResize != null)
		{
			using IEnumerator<DataGridColumn> enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				DataGridColumn current = enumerator.Current;
				if (_originalWidthsForResize.ContainsKey(current))
				{
					current.Width = _originalWidthsForResize[current];
				}
			}
		}
		_originalWidthsForResize = null;
	}

	internal void RecomputeColumnWidthsOnColumnResize(DataGridColumn resizingColumn, double horizontalChange, bool retainAuto)
	{
		DataGridLength width = resizingColumn.Width;
		double value = width.DisplayValue + horizontalChange;
		if (DoubleUtil.LessThan(value, resizingColumn.MinWidth))
		{
			horizontalChange = resizingColumn.MinWidth - width.DisplayValue;
		}
		else if (DoubleUtil.GreaterThan(value, resizingColumn.MaxWidth))
		{
			horizontalChange = resizingColumn.MaxWidth - width.DisplayValue;
		}
		int displayIndex = resizingColumn.DisplayIndex;
		if (DoubleUtil.GreaterThanZero(horizontalChange))
		{
			RecomputeColumnWidthsOnColumnPositiveResize(horizontalChange, displayIndex, retainAuto);
		}
		else if (DoubleUtil.LessThan(horizontalChange, 0.0))
		{
			RecomputeColumnWidthsOnColumnNegativeResize(0.0 - horizontalChange, displayIndex, retainAuto);
		}
	}

	private void RecomputeColumnWidthsOnColumnPositiveResize(double horizontalChange, int resizingColumnIndex, bool retainAuto)
	{
		double perStarWidth = 0.0;
		if (HasVisibleStarColumnsInternal(out perStarWidth))
		{
			horizontalChange = TakeAwayUnusedSpaceOnColumnPositiveResize(horizontalChange, resizingColumnIndex, retainAuto);
			horizontalChange = RecomputeNonStarColumnWidthsOnColumnPositiveResize(horizontalChange, resizingColumnIndex, retainAuto, onlyShrinkToDesiredWidth: true);
			horizontalChange = RecomputeStarColumnWidthsOnColumnPositiveResize(horizontalChange, resizingColumnIndex, perStarWidth, retainAuto);
			horizontalChange = RecomputeNonStarColumnWidthsOnColumnPositiveResize(horizontalChange, resizingColumnIndex, retainAuto, onlyShrinkToDesiredWidth: false);
		}
		else
		{
			SetResizedColumnWidth(ColumnFromDisplayIndex(resizingColumnIndex), horizontalChange, retainAuto);
		}
	}

	private double RecomputeStarColumnWidthsOnColumnPositiveResize(double horizontalChange, int resizingColumnIndex, double perStarWidth, bool retainAuto)
	{
		while (DoubleUtil.GreaterThanZero(horizontalChange))
		{
			double minPerStarExcessRatio = double.PositiveInfinity;
			double starFactorsForPositiveResize = GetStarFactorsForPositiveResize(resizingColumnIndex + 1, out minPerStarExcessRatio);
			if (!DoubleUtil.GreaterThanZero(starFactorsForPositiveResize))
			{
				break;
			}
			horizontalChange = ReallocateStarValuesForPositiveResize(resizingColumnIndex, horizontalChange, minPerStarExcessRatio, starFactorsForPositiveResize, perStarWidth, retainAuto);
			if (DoubleUtil.IsZero(horizontalChange))
			{
				break;
			}
		}
		return horizontalChange;
	}

	private static bool CanColumnParticipateInResize(DataGridColumn column)
	{
		if (column.IsVisible)
		{
			return column.CanUserResize;
		}
		return false;
	}

	private double GetStarFactorsForPositiveResize(int startIndex, out double minPerStarExcessRatio)
	{
		minPerStarExcessRatio = double.PositiveInfinity;
		double num = 0.0;
		int i = startIndex;
		for (int count = base.Count; i < count; i++)
		{
			DataGridColumn dataGridColumn = ColumnFromDisplayIndex(i);
			if (!CanColumnParticipateInResize(dataGridColumn))
			{
				continue;
			}
			DataGridLength width = dataGridColumn.Width;
			if (width.IsStar && !DoubleUtil.AreClose(width.Value, 0.0) && DoubleUtil.GreaterThan(width.DisplayValue, dataGridColumn.MinWidth))
			{
				num += width.Value;
				double num2 = (width.DisplayValue - dataGridColumn.MinWidth) / width.Value;
				if (DoubleUtil.LessThan(num2, minPerStarExcessRatio))
				{
					minPerStarExcessRatio = num2;
				}
			}
		}
		return num;
	}

	private double ReallocateStarValuesForPositiveResize(int startIndex, double horizontalChange, double perStarExcessRatio, double totalStarFactors, double perStarWidth, bool retainAuto)
	{
		double num = 0.0;
		double num2 = 0.0;
		if (DoubleUtil.LessThan(horizontalChange, perStarExcessRatio * totalStarFactors))
		{
			num = horizontalChange / totalStarFactors;
			num2 = horizontalChange;
			horizontalChange = 0.0;
		}
		else
		{
			num = perStarExcessRatio;
			num2 = num * totalStarFactors;
			horizontalChange -= num2;
		}
		int i = startIndex;
		for (int count = base.Count; i < count; i++)
		{
			DataGridColumn dataGridColumn = ColumnFromDisplayIndex(i);
			DataGridLength width = dataGridColumn.Width;
			if (i == startIndex)
			{
				SetResizedColumnWidth(dataGridColumn, num2, retainAuto);
			}
			else if (dataGridColumn.Width.IsStar && CanColumnParticipateInResize(dataGridColumn) && DoubleUtil.GreaterThan(width.DisplayValue, dataGridColumn.MinWidth))
			{
				double num3 = width.DisplayValue - width.Value * num;
				dataGridColumn.UpdateWidthForStarColumn(Math.Max(num3, dataGridColumn.MinWidth), num3, num3 / perStarWidth);
			}
		}
		return horizontalChange;
	}

	private double RecomputeNonStarColumnWidthsOnColumnPositiveResize(double horizontalChange, int resizingColumnIndex, bool retainAuto, bool onlyShrinkToDesiredWidth)
	{
		if (DoubleUtil.GreaterThanZero(horizontalChange))
		{
			double num = 0.0;
			bool flag = true;
			int num2 = base.Count - 1;
			while (flag && num2 > resizingColumnIndex)
			{
				DataGridColumn dataGridColumn = ColumnFromDisplayIndex(num2);
				if (CanColumnParticipateInResize(dataGridColumn))
				{
					DataGridLength width = dataGridColumn.Width;
					double num3 = (onlyShrinkToDesiredWidth ? (width.DisplayValue - Math.Max(width.DesiredValue, dataGridColumn.MinWidth)) : (width.DisplayValue - dataGridColumn.MinWidth));
					if (!width.IsStar && DoubleUtil.GreaterThanZero(num3))
					{
						if (DoubleUtil.GreaterThanOrClose(num + num3, horizontalChange))
						{
							num3 = horizontalChange - num;
							flag = false;
						}
						dataGridColumn.SetWidthInternal(new DataGridLength(width.Value, width.UnitType, width.DesiredValue, width.DisplayValue - num3));
						num += num3;
					}
				}
				num2--;
			}
			if (DoubleUtil.GreaterThanZero(num))
			{
				SetResizedColumnWidth(ColumnFromDisplayIndex(resizingColumnIndex), num, retainAuto);
				horizontalChange -= num;
			}
		}
		return horizontalChange;
	}

	private void RecomputeColumnWidthsOnColumnNegativeResize(double horizontalChange, int resizingColumnIndex, bool retainAuto)
	{
		double perStarWidth = 0.0;
		if (HasVisibleStarColumnsInternal(out perStarWidth))
		{
			horizontalChange = RecomputeNonStarColumnWidthsOnColumnNegativeResize(horizontalChange, resizingColumnIndex, retainAuto, expandBeyondDesiredWidth: false);
			horizontalChange = RecomputeStarColumnWidthsOnColumnNegativeResize(horizontalChange, resizingColumnIndex, perStarWidth, retainAuto);
			horizontalChange = RecomputeNonStarColumnWidthsOnColumnNegativeResize(horizontalChange, resizingColumnIndex, retainAuto, expandBeyondDesiredWidth: true);
			if (DoubleUtil.GreaterThanZero(horizontalChange))
			{
				DataGridColumn dataGridColumn = ColumnFromDisplayIndex(resizingColumnIndex);
				if (!dataGridColumn.Width.IsStar)
				{
					SetResizedColumnWidth(dataGridColumn, 0.0 - horizontalChange, retainAuto);
				}
			}
		}
		else
		{
			SetResizedColumnWidth(ColumnFromDisplayIndex(resizingColumnIndex), 0.0 - horizontalChange, retainAuto);
		}
	}

	private double RecomputeNonStarColumnWidthsOnColumnNegativeResize(double horizontalChange, int resizingColumnIndex, bool retainAuto, bool expandBeyondDesiredWidth)
	{
		if (DoubleUtil.GreaterThanZero(horizontalChange))
		{
			double num = 0.0;
			bool flag = true;
			int num2 = resizingColumnIndex + 1;
			int count = base.Count;
			while (flag && num2 < count)
			{
				DataGridColumn dataGridColumn = ColumnFromDisplayIndex(num2);
				if (CanColumnParticipateInResize(dataGridColumn))
				{
					DataGridLength width = dataGridColumn.Width;
					double num3 = (expandBeyondDesiredWidth ? dataGridColumn.MaxWidth : Math.Min(width.DesiredValue, dataGridColumn.MaxWidth));
					if (!width.IsStar && DoubleUtil.LessThan(width.DisplayValue, num3))
					{
						double num4 = num3 - width.DisplayValue;
						if (DoubleUtil.GreaterThanOrClose(num + num4, horizontalChange))
						{
							num4 = horizontalChange - num;
							flag = false;
						}
						dataGridColumn.SetWidthInternal(new DataGridLength(width.Value, width.UnitType, width.DesiredValue, width.DisplayValue + num4));
						num += num4;
					}
				}
				num2++;
			}
			if (DoubleUtil.GreaterThanZero(num))
			{
				SetResizedColumnWidth(ColumnFromDisplayIndex(resizingColumnIndex), 0.0 - num, retainAuto);
				horizontalChange -= num;
			}
		}
		return horizontalChange;
	}

	private double RecomputeStarColumnWidthsOnColumnNegativeResize(double horizontalChange, int resizingColumnIndex, double perStarWidth, bool retainAuto)
	{
		while (DoubleUtil.GreaterThanZero(horizontalChange))
		{
			double minPerStarLagRatio = double.PositiveInfinity;
			double starFactorsForNegativeResize = GetStarFactorsForNegativeResize(resizingColumnIndex + 1, out minPerStarLagRatio);
			if (!DoubleUtil.GreaterThanZero(starFactorsForNegativeResize))
			{
				break;
			}
			horizontalChange = ReallocateStarValuesForNegativeResize(resizingColumnIndex, horizontalChange, minPerStarLagRatio, starFactorsForNegativeResize, perStarWidth, retainAuto);
			if (DoubleUtil.AreClose(horizontalChange, 0.0))
			{
				break;
			}
		}
		return horizontalChange;
	}

	private double GetStarFactorsForNegativeResize(int startIndex, out double minPerStarLagRatio)
	{
		minPerStarLagRatio = double.PositiveInfinity;
		double num = 0.0;
		int i = startIndex;
		for (int count = base.Count; i < count; i++)
		{
			DataGridColumn dataGridColumn = ColumnFromDisplayIndex(i);
			if (!CanColumnParticipateInResize(dataGridColumn))
			{
				continue;
			}
			DataGridLength width = dataGridColumn.Width;
			if (width.IsStar && !DoubleUtil.AreClose(width.Value, 0.0) && DoubleUtil.LessThan(width.DisplayValue, dataGridColumn.MaxWidth))
			{
				num += width.Value;
				double num2 = (dataGridColumn.MaxWidth - width.DisplayValue) / width.Value;
				if (DoubleUtil.LessThan(num2, minPerStarLagRatio))
				{
					minPerStarLagRatio = num2;
				}
			}
		}
		return num;
	}

	private double ReallocateStarValuesForNegativeResize(int startIndex, double horizontalChange, double perStarLagRatio, double totalStarFactors, double perStarWidth, bool retainAuto)
	{
		double num = 0.0;
		double num2 = 0.0;
		if (DoubleUtil.LessThan(horizontalChange, perStarLagRatio * totalStarFactors))
		{
			num = horizontalChange / totalStarFactors;
			num2 = horizontalChange;
			horizontalChange = 0.0;
		}
		else
		{
			num = perStarLagRatio;
			num2 = num * totalStarFactors;
			horizontalChange -= num2;
		}
		int i = startIndex;
		for (int count = base.Count; i < count; i++)
		{
			DataGridColumn dataGridColumn = ColumnFromDisplayIndex(i);
			DataGridLength width = dataGridColumn.Width;
			if (i == startIndex)
			{
				SetResizedColumnWidth(dataGridColumn, 0.0 - num2, retainAuto);
			}
			else if (dataGridColumn.Width.IsStar && CanColumnParticipateInResize(dataGridColumn) && DoubleUtil.LessThan(width.DisplayValue, dataGridColumn.MaxWidth))
			{
				double num3 = width.DisplayValue + width.Value * num;
				dataGridColumn.UpdateWidthForStarColumn(Math.Min(num3, dataGridColumn.MaxWidth), num3, num3 / perStarWidth);
			}
		}
		return horizontalChange;
	}

	private static void SetResizedColumnWidth(DataGridColumn column, double widthDelta, bool retainAuto)
	{
		DataGridLength width = column.Width;
		double num = DataGridHelper.CoerceToMinMax(width.DisplayValue + widthDelta, column.MinWidth, column.MaxWidth);
		if (width.IsStar)
		{
			double num2 = width.DesiredValue / width.Value;
			column.UpdateWidthForStarColumn(num, num, num / num2);
		}
		else if (!width.IsAbsolute && retainAuto)
		{
			column.SetWidthInternal(new DataGridLength(width.Value, width.UnitType, width.DesiredValue, num));
		}
		else
		{
			column.SetWidthInternal(new DataGridLength(num, DataGridLengthUnitType.Pixel, num, num));
		}
	}

	private double GiveAwayWidthToColumns(DataGridColumn ignoredColumn, double giveAwayWidth)
	{
		return GiveAwayWidthToColumns(ignoredColumn, giveAwayWidth, recomputeStars: false);
	}

	private double GiveAwayWidthToColumns(DataGridColumn ignoredColumn, double giveAwayWidth, bool recomputeStars)
	{
		double num = giveAwayWidth;
		giveAwayWidth = GiveAwayWidthToScrollViewerExcess(giveAwayWidth, ignoredColumn != null);
		giveAwayWidth = GiveAwayWidthToNonStarColumns(ignoredColumn, giveAwayWidth);
		if (DoubleUtil.GreaterThanZero(giveAwayWidth) || recomputeStars)
		{
			double num2 = 0.0;
			double num3 = 0.0;
			bool flag = false;
			using (IEnumerator<DataGridColumn> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					DataGridColumn current = enumerator.Current;
					DataGridLength width = current.Width;
					if (width.IsStar && current.IsVisible)
					{
						if (current == ignoredColumn)
						{
							flag = true;
						}
						num2 += width.DisplayValue;
						num3 += current.MaxWidth;
					}
				}
			}
			double num4 = num2;
			if (!flag)
			{
				num4 += giveAwayWidth;
			}
			else if (!DoubleUtil.AreClose(num, giveAwayWidth))
			{
				num4 -= num - giveAwayWidth;
			}
			giveAwayWidth = Math.Max(ComputeStarColumnWidths(Math.Min(num4, num3)) - num4, 0.0);
		}
		return giveAwayWidth;
	}

	private double GiveAwayWidthToNonStarColumns(DataGridColumn ignoredColumn, double giveAwayWidth)
	{
		while (DoubleUtil.GreaterThanZero(giveAwayWidth))
		{
			int countOfParticipatingColumns = 0;
			double num = FindMinimumLaggingWidthOfNonStarColumns(ignoredColumn, out countOfParticipatingColumns);
			if (countOfParticipatingColumns == 0)
			{
				break;
			}
			double num2 = num * (double)countOfParticipatingColumns;
			if (DoubleUtil.GreaterThanOrClose(num2, giveAwayWidth))
			{
				num = giveAwayWidth / (double)countOfParticipatingColumns;
				giveAwayWidth = 0.0;
			}
			else
			{
				giveAwayWidth -= num2;
			}
			GiveAwayWidthToEveryNonStarColumn(ignoredColumn, num);
		}
		return giveAwayWidth;
	}

	private double FindMinimumLaggingWidthOfNonStarColumns(DataGridColumn ignoredColumn, out int countOfParticipatingColumns)
	{
		double num = double.PositiveInfinity;
		countOfParticipatingColumns = 0;
		using IEnumerator<DataGridColumn> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			DataGridColumn current = enumerator.Current;
			if (ignoredColumn == current || !current.IsVisible)
			{
				continue;
			}
			DataGridLength width = current.Width;
			if (width.IsStar)
			{
				continue;
			}
			double maxWidth = current.MaxWidth;
			if (DoubleUtil.LessThan(width.DisplayValue, width.DesiredValue) && !DoubleUtil.AreClose(width.DisplayValue, maxWidth))
			{
				countOfParticipatingColumns++;
				double num2 = Math.Min(width.DesiredValue, maxWidth) - width.DisplayValue;
				if (DoubleUtil.LessThan(num2, num))
				{
					num = num2;
				}
			}
		}
		return num;
	}

	private void GiveAwayWidthToEveryNonStarColumn(DataGridColumn ignoredColumn, double perColumnGiveAwayWidth)
	{
		using IEnumerator<DataGridColumn> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			DataGridColumn current = enumerator.Current;
			if (ignoredColumn != current && current.IsVisible)
			{
				DataGridLength width = current.Width;
				if (!width.IsStar && DoubleUtil.LessThan(width.DisplayValue, Math.Min(width.DesiredValue, current.MaxWidth)))
				{
					current.SetWidthInternal(new DataGridLength(width.Value, width.UnitType, width.DesiredValue, width.DisplayValue + perColumnGiveAwayWidth));
				}
			}
		}
	}

	private double GiveAwayWidthToScrollViewerExcess(double giveAwayWidth, bool includedInColumnsWidth)
	{
		double viewportWidthForColumns = DataGridOwner.GetViewportWidthForColumns();
		double num = 0.0;
		using (IEnumerator<DataGridColumn> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				DataGridColumn current = enumerator.Current;
				if (current.IsVisible)
				{
					num += current.Width.DisplayValue;
				}
			}
		}
		if (includedInColumnsWidth)
		{
			if (DoubleUtil.GreaterThan(num, viewportWidthForColumns))
			{
				double val = num - viewportWidthForColumns;
				giveAwayWidth -= Math.Min(val, giveAwayWidth);
			}
		}
		else
		{
			giveAwayWidth = Math.Min(giveAwayWidth, Math.Max(0.0, viewportWidthForColumns - num));
		}
		return giveAwayWidth;
	}

	private double TakeAwayUnusedSpaceOnColumnPositiveResize(double horizontalChange, int resizingColumnIndex, bool retainAuto)
	{
		double num = TakeAwayWidthFromUnusedSpace(spaceAlreadyUtilized: false, horizontalChange);
		if (DoubleUtil.LessThan(num, horizontalChange))
		{
			SetResizedColumnWidth(ColumnFromDisplayIndex(resizingColumnIndex), horizontalChange - num, retainAuto);
		}
		return num;
	}

	private double TakeAwayWidthFromUnusedSpace(bool spaceAlreadyUtilized, double takeAwayWidth, double totalAvailableWidth)
	{
		double num = 0.0;
		using (IEnumerator<DataGridColumn> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				DataGridColumn current = enumerator.Current;
				if (current.IsVisible)
				{
					num += current.Width.DisplayValue;
				}
			}
		}
		if (spaceAlreadyUtilized)
		{
			if (DoubleUtil.GreaterThanOrClose(totalAvailableWidth, num))
			{
				return 0.0;
			}
			return Math.Min(num - totalAvailableWidth, takeAwayWidth);
		}
		double num2 = totalAvailableWidth - num;
		if (DoubleUtil.GreaterThanZero(num2))
		{
			takeAwayWidth = Math.Max(0.0, takeAwayWidth - num2);
		}
		return takeAwayWidth;
	}

	private double TakeAwayWidthFromUnusedSpace(bool spaceAlreadyUtilized, double takeAwayWidth)
	{
		double viewportWidthForColumns = DataGridOwner.GetViewportWidthForColumns();
		if (DoubleUtil.GreaterThanZero(viewportWidthForColumns))
		{
			return TakeAwayWidthFromUnusedSpace(spaceAlreadyUtilized, takeAwayWidth, viewportWidthForColumns);
		}
		return takeAwayWidth;
	}

	private double TakeAwayWidthFromColumns(DataGridColumn ignoredColumn, double takeAwayWidth, bool widthAlreadyUtilized)
	{
		double viewportWidthForColumns = DataGridOwner.GetViewportWidthForColumns();
		return TakeAwayWidthFromColumns(ignoredColumn, takeAwayWidth, widthAlreadyUtilized, viewportWidthForColumns);
	}

	private double TakeAwayWidthFromColumns(DataGridColumn ignoredColumn, double takeAwayWidth, bool widthAlreadyUtilized, double totalAvailableWidth)
	{
		takeAwayWidth = TakeAwayWidthFromUnusedSpace(widthAlreadyUtilized, takeAwayWidth, totalAvailableWidth);
		takeAwayWidth = TakeAwayWidthFromStarColumns(ignoredColumn, takeAwayWidth);
		takeAwayWidth = TakeAwayWidthFromNonStarColumns(ignoredColumn, takeAwayWidth);
		return takeAwayWidth;
	}

	private double TakeAwayWidthFromStarColumns(DataGridColumn ignoredColumn, double takeAwayWidth)
	{
		if (DoubleUtil.GreaterThanZero(takeAwayWidth))
		{
			double num = 0.0;
			double num2 = 0.0;
			using (IEnumerator<DataGridColumn> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					DataGridColumn current = enumerator.Current;
					DataGridLength width = current.Width;
					if (width.IsStar && current.IsVisible)
					{
						if (current == ignoredColumn)
						{
							num += takeAwayWidth;
						}
						num += width.DisplayValue;
						num2 += current.MinWidth;
					}
				}
			}
			double num3 = num - takeAwayWidth;
			takeAwayWidth = Math.Max(ComputeStarColumnWidths(Math.Max(num3, num2)) - num3, 0.0);
		}
		return takeAwayWidth;
	}

	private double TakeAwayWidthFromNonStarColumns(DataGridColumn ignoredColumn, double takeAwayWidth)
	{
		while (DoubleUtil.GreaterThanZero(takeAwayWidth))
		{
			int countOfParticipatingColumns = 0;
			double num = FindMinimumExcessWidthOfNonStarColumns(ignoredColumn, out countOfParticipatingColumns);
			if (countOfParticipatingColumns == 0)
			{
				break;
			}
			double num2 = num * (double)countOfParticipatingColumns;
			if (DoubleUtil.GreaterThanOrClose(num2, takeAwayWidth))
			{
				num = takeAwayWidth / (double)countOfParticipatingColumns;
				takeAwayWidth = 0.0;
			}
			else
			{
				takeAwayWidth -= num2;
			}
			TakeAwayWidthFromEveryNonStarColumn(ignoredColumn, num);
		}
		return takeAwayWidth;
	}

	private double FindMinimumExcessWidthOfNonStarColumns(DataGridColumn ignoredColumn, out int countOfParticipatingColumns)
	{
		double num = double.PositiveInfinity;
		countOfParticipatingColumns = 0;
		using IEnumerator<DataGridColumn> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			DataGridColumn current = enumerator.Current;
			if (ignoredColumn == current || !current.IsVisible)
			{
				continue;
			}
			DataGridLength width = current.Width;
			if (width.IsStar)
			{
				continue;
			}
			double minWidth = current.MinWidth;
			if (DoubleUtil.GreaterThan(width.DisplayValue, minWidth))
			{
				countOfParticipatingColumns++;
				double num2 = width.DisplayValue - minWidth;
				if (DoubleUtil.LessThan(num2, num))
				{
					num = num2;
				}
			}
		}
		return num;
	}

	private void TakeAwayWidthFromEveryNonStarColumn(DataGridColumn ignoredColumn, double perColumnTakeAwayWidth)
	{
		using IEnumerator<DataGridColumn> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			DataGridColumn current = enumerator.Current;
			if (ignoredColumn != current && current.IsVisible)
			{
				DataGridLength width = current.Width;
				if (!width.IsStar && DoubleUtil.GreaterThan(width.DisplayValue, current.MinWidth))
				{
					current.SetWidthInternal(new DataGridLength(width.Value, width.UnitType, width.DesiredValue, width.DisplayValue - perColumnTakeAwayWidth));
				}
			}
		}
	}

	internal void InvalidateColumnRealization(bool invalidateForNonVirtualizedRows)
	{
		RebuildRealizedColumnsBlockListForVirtualizedRows = true;
		if (invalidateForNonVirtualizedRows)
		{
			RebuildRealizedColumnsBlockListForNonVirtualizedRows = true;
		}
	}
}
