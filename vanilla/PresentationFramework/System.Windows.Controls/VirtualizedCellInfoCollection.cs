using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace System.Windows.Controls;

internal class VirtualizedCellInfoCollection : IList<DataGridCellInfo>, ICollection<DataGridCellInfo>, IEnumerable<DataGridCellInfo>, IEnumerable
{
	private class VirtualizedCellInfoCollectionEnumerator : IEnumerator<DataGridCellInfo>, IEnumerator, IDisposable
	{
		private DataGrid _owner;

		private List<CellRegion> _regions;

		private int _current;

		private int _count;

		private VirtualizedCellInfoCollection _collection;

		public DataGridCellInfo Current
		{
			get
			{
				if (CurrentWithinBounds)
				{
					return _collection.GetCellInfoFromIndex(_owner, _regions, _current);
				}
				return DataGridCellInfo.Unset;
			}
		}

		private bool CurrentWithinBounds
		{
			get
			{
				if (_current >= 0)
				{
					return _current < _count;
				}
				return false;
			}
		}

		object IEnumerator.Current => Current;

		public VirtualizedCellInfoCollectionEnumerator(DataGrid owner, List<CellRegion> regions, VirtualizedCellInfoCollection collection)
		{
			_owner = owner;
			_regions = new List<CellRegion>(regions);
			_current = -1;
			_collection = collection;
			if (_regions != null)
			{
				int count = _regions.Count;
				for (int i = 0; i < count; i++)
				{
					_count += _regions[i].Size;
				}
			}
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		public bool MoveNext()
		{
			if (_current < _count)
			{
				_current++;
			}
			return CurrentWithinBounds;
		}

		public void Reset()
		{
			_current = -1;
		}
	}

	private struct CellRegion
	{
		private int _left;

		private int _top;

		private int _width;

		private int _height;

		public int Left
		{
			get
			{
				return _left;
			}
			set
			{
				_left = value;
			}
		}

		public int Top
		{
			get
			{
				return _top;
			}
			set
			{
				_top = value;
			}
		}

		public int Right
		{
			get
			{
				return _left + _width - 1;
			}
			set
			{
				_width = value - _left + 1;
			}
		}

		public int Bottom
		{
			get
			{
				return _top + _height - 1;
			}
			set
			{
				_height = value - _top + 1;
			}
		}

		public int Width
		{
			get
			{
				return _width;
			}
			set
			{
				_width = value;
			}
		}

		public int Height
		{
			get
			{
				return _height;
			}
			set
			{
				_height = value;
			}
		}

		public bool IsEmpty
		{
			get
			{
				if (_width != 0)
				{
					return _height == 0;
				}
				return true;
			}
		}

		public int Size => _width * _height;

		public static CellRegion Empty => new CellRegion(0, 0, 0, 0);

		public CellRegion(int left, int top, int width, int height)
		{
			_left = left;
			_top = top;
			_width = width;
			_height = height;
		}

		public bool Contains(int x, int y)
		{
			if (IsEmpty)
			{
				return false;
			}
			if (x >= Left && y >= Top && x <= Right)
			{
				return y <= Bottom;
			}
			return false;
		}

		public bool Contains(CellRegion region)
		{
			if (Left <= region.Left && Top <= region.Top && Right >= region.Right)
			{
				return Bottom >= region.Bottom;
			}
			return false;
		}

		public bool Intersects(CellRegion region)
		{
			if (Intersects(Left, Right, region.Left, region.Right))
			{
				return Intersects(Top, Bottom, region.Top, region.Bottom);
			}
			return false;
		}

		private static bool Intersects(int start1, int end1, int start2, int end2)
		{
			if (start1 <= end2)
			{
				return end1 >= start2;
			}
			return false;
		}

		public CellRegion Intersection(CellRegion region)
		{
			if (Intersects(region))
			{
				int num = Math.Max(Left, region.Left);
				int num2 = Math.Max(Top, region.Top);
				int num3 = Math.Min(Right, region.Right);
				int num4 = Math.Min(Bottom, region.Bottom);
				return new CellRegion(num, num2, num3 - num + 1, num4 - num2 + 1);
			}
			return Empty;
		}

		public bool Union(CellRegion region)
		{
			if (Contains(region))
			{
				return true;
			}
			if (region.Contains(this))
			{
				Left = region.Left;
				Top = region.Top;
				Width = region.Width;
				Height = region.Height;
				return true;
			}
			bool flag = region.Left == Left && region.Width == Width;
			bool flag2 = region.Top == Top && region.Height == Height;
			if (flag || flag2)
			{
				int num = (flag ? Top : Left);
				int num2 = (flag ? Bottom : Right);
				int num3 = (flag ? region.Top : region.Left);
				int num4 = (flag ? region.Bottom : region.Right);
				bool flag3 = false;
				if (num4 <= num2)
				{
					flag3 = num - num4 <= 1;
				}
				else if (num <= num3)
				{
					flag3 = num3 - num2 <= 1;
				}
				if (flag3)
				{
					int right = Right;
					int bottom = Bottom;
					Left = Math.Min(Left, region.Left);
					Top = Math.Min(Top, region.Top);
					Right = Math.Max(right, region.Right);
					Bottom = Math.Max(bottom, region.Bottom);
					return true;
				}
			}
			return false;
		}

		public bool Remainder(CellRegion region, out List<CellRegion> remainder)
		{
			if (Intersects(region))
			{
				if (region.Contains(this))
				{
					remainder = null;
				}
				else
				{
					remainder = new List<CellRegion>();
					if (Top < region.Top)
					{
						remainder.Add(new CellRegion(Left, Top, Width, region.Top - Top));
					}
					if (Left < region.Left)
					{
						int num = Math.Max(Top, region.Top);
						int num2 = Math.Min(Bottom, region.Bottom);
						remainder.Add(new CellRegion(Left, num, region.Left - Left, num2 - num + 1));
					}
					if (Right > region.Right)
					{
						int num3 = Math.Max(Top, region.Top);
						int num4 = Math.Min(Bottom, region.Bottom);
						remainder.Add(new CellRegion(region.Right + 1, num3, Right - region.Right, num4 - num3 + 1));
					}
					if (Bottom > region.Bottom)
					{
						remainder.Add(new CellRegion(Left, region.Bottom + 1, Width, Bottom - region.Bottom));
					}
				}
				return true;
			}
			remainder = null;
			return false;
		}
	}

	private class RemovedCellInfoCollection : VirtualizedCellInfoCollection
	{
		private DataGridColumn _removedColumn;

		private object _removedItem;

		internal RemovedCellInfoCollection(DataGrid owner, List<CellRegion> regions, DataGridColumn column)
			: base(owner, regions)
		{
			_removedColumn = column;
		}

		internal RemovedCellInfoCollection(DataGrid owner, List<CellRegion> regions, object item)
			: base(owner, regions)
		{
			_removedItem = item;
		}

		protected override DataGridCellInfo CreateCellInfo(ItemsControl.ItemInfo rowInfo, DataGridColumn column, DataGrid owner)
		{
			if (_removedColumn != null)
			{
				return new DataGridCellInfo(rowInfo, _removedColumn, owner);
			}
			return new DataGridCellInfo(_removedItem, column, owner);
		}
	}

	private bool _isReadOnly;

	private DataGrid _owner;

	private List<CellRegion> _regions;

	public DataGridCellInfo this[int index]
	{
		get
		{
			if (index >= 0 && index < Count)
			{
				return GetCellInfoFromIndex(_owner, _regions, index);
			}
			throw new ArgumentOutOfRangeException("index");
		}
		set
		{
			throw new NotSupportedException(SR.VirtualizedCellInfoCollection_DoesNotSupportIndexChanges);
		}
	}

	public int Count
	{
		get
		{
			int num = 0;
			int count = _regions.Count;
			for (int i = 0; i < count; i++)
			{
				num += _regions[i].Size;
			}
			return num;
		}
	}

	public bool IsReadOnly
	{
		get
		{
			return _isReadOnly;
		}
		private set
		{
			_isReadOnly = value;
		}
	}

	protected bool IsEmpty
	{
		get
		{
			int count = _regions.Count;
			for (int i = 0; i < count; i++)
			{
				if (!_regions[i].IsEmpty)
				{
					return false;
				}
			}
			return true;
		}
	}

	protected DataGrid Owner => _owner;

	internal VirtualizedCellInfoCollection(DataGrid owner)
	{
		_owner = owner;
		_regions = new List<CellRegion>();
	}

	private VirtualizedCellInfoCollection(DataGrid owner, List<CellRegion> regions)
	{
		_owner = owner;
		_regions = ((regions != null) ? new List<CellRegion>(regions) : new List<CellRegion>());
		IsReadOnly = true;
	}

	internal static VirtualizedCellInfoCollection MakeEmptyCollection(DataGrid owner)
	{
		return new VirtualizedCellInfoCollection(owner, null);
	}

	public void Add(DataGridCellInfo cell)
	{
		_owner.Dispatcher.VerifyAccess();
		ValidateIsReadOnly();
		if (!IsValidPublicCell(cell))
		{
			throw new ArgumentException(SR.SelectedCellsCollection_InvalidItem, "cell");
		}
		if (Contains(cell))
		{
			throw new ArgumentException(SR.SelectedCellsCollection_DuplicateItem, "cell");
		}
		AddValidatedCell(cell);
	}

	internal void AddValidatedCell(DataGridCellInfo cell)
	{
		ConvertCellInfoToIndexes(cell, out var rowIndex, out var columnIndex);
		AddRegion(rowIndex, columnIndex, 1, 1);
	}

	public void Clear()
	{
		_owner.Dispatcher.VerifyAccess();
		ValidateIsReadOnly();
		if (!IsEmpty)
		{
			VirtualizedCellInfoCollection oldItems = new VirtualizedCellInfoCollection(_owner, _regions);
			_regions.Clear();
			OnRemove(oldItems);
		}
	}

	public bool Contains(DataGridCellInfo cell)
	{
		if (!IsValidPublicCell(cell))
		{
			throw new ArgumentException(SR.SelectedCellsCollection_InvalidItem, "cell");
		}
		if (IsEmpty)
		{
			return false;
		}
		ConvertCellInfoToIndexes(cell, out var rowIndex, out var columnIndex);
		return Contains(rowIndex, columnIndex);
	}

	internal bool Contains(DataGridCell cell)
	{
		if (!IsEmpty)
		{
			object rowDataItem = cell.RowDataItem;
			int displayIndex = cell.Column.DisplayIndex;
			ItemCollection items = _owner.Items;
			int count = items.Count;
			int count2 = _regions.Count;
			for (int i = 0; i < count2; i++)
			{
				CellRegion cellRegion = _regions[i];
				if (cellRegion.Left > displayIndex || displayIndex > cellRegion.Right)
				{
					continue;
				}
				int bottom = cellRegion.Bottom;
				for (int j = cellRegion.Top; j <= bottom; j++)
				{
					if (j < count && items[j] == rowDataItem)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	internal bool Contains(int rowIndex, int columnIndex)
	{
		int count = _regions.Count;
		for (int i = 0; i < count; i++)
		{
			if (_regions[i].Contains(columnIndex, rowIndex))
			{
				return true;
			}
		}
		return false;
	}

	public void CopyTo(DataGridCellInfo[] array, int arrayIndex)
	{
		List<DataGridCellInfo> list = new List<DataGridCellInfo>();
		int count = _regions.Count;
		for (int i = 0; i < count; i++)
		{
			AddRegionToList(_regions[i], list);
		}
		list.CopyTo(array, arrayIndex);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new VirtualizedCellInfoCollectionEnumerator(_owner, _regions, this);
	}

	public IEnumerator<DataGridCellInfo> GetEnumerator()
	{
		return new VirtualizedCellInfoCollectionEnumerator(_owner, _regions, this);
	}

	public int IndexOf(DataGridCellInfo cell)
	{
		ConvertCellInfoToIndexes(cell, out var rowIndex, out var columnIndex);
		int count = _regions.Count;
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			CellRegion cellRegion = _regions[i];
			if (cellRegion.Contains(columnIndex, rowIndex))
			{
				return num + ((rowIndex - cellRegion.Top) * cellRegion.Width + columnIndex - cellRegion.Left);
			}
			num += cellRegion.Size;
		}
		return -1;
	}

	public void Insert(int index, DataGridCellInfo cell)
	{
		throw new NotSupportedException(SR.VirtualizedCellInfoCollection_DoesNotSupportIndexChanges);
	}

	public bool Remove(DataGridCellInfo cell)
	{
		_owner.Dispatcher.VerifyAccess();
		ValidateIsReadOnly();
		if (!IsEmpty)
		{
			ConvertCellInfoToIndexes(cell, out var rowIndex, out var columnIndex);
			if (Contains(rowIndex, columnIndex))
			{
				RemoveRegion(rowIndex, columnIndex, 1, 1);
				return true;
			}
		}
		return false;
	}

	public void RemoveAt(int index)
	{
		throw new NotSupportedException(SR.VirtualizedCellInfoCollection_DoesNotSupportIndexChanges);
	}

	private void OnAdd(VirtualizedCellInfoCollection newItems)
	{
		OnCollectionChanged(NotifyCollectionChangedAction.Add, null, newItems);
	}

	private void OnRemove(VirtualizedCellInfoCollection oldItems)
	{
		OnCollectionChanged(NotifyCollectionChangedAction.Remove, oldItems, null);
	}

	protected virtual void OnCollectionChanged(NotifyCollectionChangedAction action, VirtualizedCellInfoCollection oldItems, VirtualizedCellInfoCollection newItems)
	{
	}

	private bool IsValidCell(DataGridCellInfo cell)
	{
		return cell.IsValidForDataGrid(_owner);
	}

	private bool IsValidPublicCell(DataGridCellInfo cell)
	{
		if (cell.IsValidForDataGrid(_owner))
		{
			return cell.IsValid;
		}
		return false;
	}

	protected void GetBoundingRegion(out int left, out int top, out int right, out int bottom)
	{
		left = int.MaxValue;
		top = int.MaxValue;
		right = 0;
		bottom = 0;
		int count = _regions.Count;
		for (int i = 0; i < count; i++)
		{
			CellRegion cellRegion = _regions[i];
			if (cellRegion.Left < left)
			{
				left = cellRegion.Left;
			}
			if (cellRegion.Top < top)
			{
				top = cellRegion.Top;
			}
			if (cellRegion.Right > right)
			{
				right = cellRegion.Right;
			}
			if (cellRegion.Bottom > bottom)
			{
				bottom = cellRegion.Bottom;
			}
		}
	}

	internal void AddRegion(int rowIndex, int columnIndex, int rowCount, int columnCount)
	{
		AddRegion(rowIndex, columnIndex, rowCount, columnCount, notify: true);
	}

	private void AddRegion(int rowIndex, int columnIndex, int rowCount, int columnCount, bool notify)
	{
		List<CellRegion> list = new List<CellRegion>();
		list.Add(new CellRegion(columnIndex, rowIndex, columnCount, rowCount));
		int count = _regions.Count;
		for (int i = 0; i < count; i++)
		{
			CellRegion region = _regions[i];
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j].Remainder(region, out var remainder))
				{
					list.RemoveAt(j);
					j--;
					if (remainder != null)
					{
						list.AddRange(remainder);
					}
				}
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		VirtualizedCellInfoCollection newItems = new VirtualizedCellInfoCollection(_owner, list);
		for (int k = 0; k < count; k++)
		{
			for (int l = 0; l < list.Count; l++)
			{
				CellRegion value = _regions[k];
				if (value.Union(list[l]))
				{
					_regions[k] = value;
					list.RemoveAt(l);
					l--;
				}
			}
		}
		int count2 = list.Count;
		for (int m = 0; m < count2; m++)
		{
			_regions.Add(list[m]);
		}
		if (notify)
		{
			OnAdd(newItems);
		}
	}

	internal void RemoveRegion(int rowIndex, int columnIndex, int rowCount, int columnCount)
	{
		List<CellRegion> removeList = null;
		RemoveRegion(rowIndex, columnIndex, rowCount, columnCount, ref removeList);
		if (removeList != null && removeList.Count > 0)
		{
			OnRemove(new VirtualizedCellInfoCollection(_owner, removeList));
		}
	}

	private void RemoveRegion(int rowIndex, int columnIndex, int rowCount, int columnCount, ref List<CellRegion> removeList)
	{
		if (IsEmpty)
		{
			return;
		}
		CellRegion region = new CellRegion(columnIndex, rowIndex, columnCount, rowCount);
		for (int i = 0; i < _regions.Count; i++)
		{
			CellRegion cellRegion = _regions[i];
			CellRegion cellRegion2 = cellRegion.Intersection(region);
			if (!cellRegion2.IsEmpty)
			{
				if (removeList == null)
				{
					removeList = new List<CellRegion>();
				}
				removeList.Add(cellRegion2);
				_regions.RemoveAt(i);
				cellRegion.Remainder(cellRegion2, out var remainder);
				if (remainder != null)
				{
					_regions.InsertRange(i, remainder);
					i += remainder.Count;
				}
				i--;
			}
		}
	}

	internal void OnItemsCollectionChanged(NotifyCollectionChangedEventArgs e, List<Tuple<int, int>> ranges)
	{
		if (!IsEmpty)
		{
			switch (e.Action)
			{
			case NotifyCollectionChangedAction.Add:
				OnAddRow(e.NewStartingIndex);
				break;
			case NotifyCollectionChangedAction.Remove:
				OnRemoveRow(e.OldStartingIndex, e.OldItems[0]);
				break;
			case NotifyCollectionChangedAction.Replace:
				OnReplaceRow(e.OldStartingIndex, e.OldItems[0]);
				break;
			case NotifyCollectionChangedAction.Move:
				OnMoveRow(e.OldStartingIndex, e.NewStartingIndex);
				break;
			case NotifyCollectionChangedAction.Reset:
				RestoreOnlyFullRows(ranges);
				break;
			}
		}
	}

	private void OnAddRow(int rowIndex)
	{
		List<CellRegion> removeList = null;
		int count = _owner.Items.Count;
		int count2 = _owner.Columns.Count;
		if (count2 <= 0)
		{
			return;
		}
		RemoveRegion(rowIndex, 0, count - 1 - rowIndex, count2, ref removeList);
		if (removeList != null)
		{
			int count3 = removeList.Count;
			for (int i = 0; i < count3; i++)
			{
				CellRegion cellRegion = removeList[i];
				AddRegion(cellRegion.Top + 1, cellRegion.Left, cellRegion.Height, cellRegion.Width, notify: false);
			}
		}
	}

	private void OnRemoveRow(int rowIndex, object item)
	{
		List<CellRegion> removeList = null;
		List<CellRegion> removeList2 = null;
		int count = _owner.Items.Count;
		int count2 = _owner.Columns.Count;
		if (count2 <= 0)
		{
			return;
		}
		RemoveRegion(rowIndex + 1, 0, count - rowIndex, count2, ref removeList);
		RemoveRegion(rowIndex, 0, 1, count2, ref removeList2);
		if (removeList != null)
		{
			int count3 = removeList.Count;
			for (int i = 0; i < count3; i++)
			{
				CellRegion cellRegion = removeList[i];
				AddRegion(cellRegion.Top - 1, cellRegion.Left, cellRegion.Height, cellRegion.Width, notify: false);
			}
		}
		if (removeList2 != null)
		{
			RemovedCellInfoCollection oldItems = new RemovedCellInfoCollection(_owner, removeList2, item);
			OnRemove(oldItems);
		}
	}

	private void OnReplaceRow(int rowIndex, object item)
	{
		List<CellRegion> removeList = null;
		RemoveRegion(rowIndex, 0, 1, _owner.Columns.Count, ref removeList);
		if (removeList != null)
		{
			RemovedCellInfoCollection oldItems = new RemovedCellInfoCollection(_owner, removeList, item);
			OnRemove(oldItems);
		}
	}

	private void OnMoveRow(int oldIndex, int newIndex)
	{
		List<CellRegion> removeList = null;
		List<CellRegion> removeList2 = null;
		int count = _owner.Items.Count;
		int count2 = _owner.Columns.Count;
		if (count2 <= 0)
		{
			return;
		}
		RemoveRegion(oldIndex + 1, 0, count - oldIndex - 1, count2, ref removeList);
		RemoveRegion(oldIndex, 0, 1, count2, ref removeList2);
		if (removeList != null)
		{
			int count3 = removeList.Count;
			for (int i = 0; i < count3; i++)
			{
				CellRegion cellRegion = removeList[i];
				AddRegion(cellRegion.Top - 1, cellRegion.Left, cellRegion.Height, cellRegion.Width, notify: false);
			}
		}
		removeList = null;
		RemoveRegion(newIndex, 0, count - newIndex, count2, ref removeList);
		if (removeList2 != null)
		{
			int count4 = removeList2.Count;
			for (int j = 0; j < count4; j++)
			{
				CellRegion cellRegion2 = removeList2[j];
				AddRegion(newIndex, cellRegion2.Left, cellRegion2.Height, cellRegion2.Width, notify: false);
			}
		}
		if (removeList != null)
		{
			int count5 = removeList.Count;
			for (int k = 0; k < count5; k++)
			{
				CellRegion cellRegion3 = removeList[k];
				AddRegion(cellRegion3.Top + 1, cellRegion3.Left, cellRegion3.Height, cellRegion3.Width, notify: false);
			}
		}
	}

	internal void OnColumnsChanged(NotifyCollectionChangedAction action, int oldDisplayIndex, DataGridColumn oldColumn, int newDisplayIndex, IList selectedRows)
	{
		if (!IsEmpty)
		{
			switch (action)
			{
			case NotifyCollectionChangedAction.Add:
				OnAddColumn(newDisplayIndex, selectedRows);
				break;
			case NotifyCollectionChangedAction.Remove:
				OnRemoveColumn(oldDisplayIndex, oldColumn);
				break;
			case NotifyCollectionChangedAction.Replace:
				OnReplaceColumn(oldDisplayIndex, oldColumn, selectedRows);
				break;
			case NotifyCollectionChangedAction.Move:
				OnMoveColumn(oldDisplayIndex, newDisplayIndex);
				break;
			case NotifyCollectionChangedAction.Reset:
				_regions.Clear();
				break;
			}
		}
	}

	private void OnAddColumn(int columnIndex, IList selectedRows)
	{
		List<CellRegion> removeList = null;
		int count = _owner.Items.Count;
		int count2 = _owner.Columns.Count;
		if (count <= 0)
		{
			return;
		}
		RemoveRegion(0, columnIndex, count, count2 - 1 - columnIndex, ref removeList);
		if (removeList != null)
		{
			int count3 = removeList.Count;
			for (int i = 0; i < count3; i++)
			{
				CellRegion cellRegion = removeList[i];
				AddRegion(cellRegion.Top, cellRegion.Left + 1, cellRegion.Height, cellRegion.Width, notify: false);
			}
		}
		FillInFullRowRegions(selectedRows, columnIndex, notify: true);
	}

	private void FillInFullRowRegions(IList rows, int columnIndex, bool notify)
	{
		int count = rows.Count;
		for (int i = 0; i < count; i++)
		{
			int num = _owner.Items.IndexOf(rows[i]);
			if (num >= 0)
			{
				AddRegion(num, columnIndex, 1, 1, notify);
			}
		}
	}

	private void OnRemoveColumn(int columnIndex, DataGridColumn oldColumn)
	{
		List<CellRegion> removeList = null;
		List<CellRegion> removeList2 = null;
		int count = _owner.Items.Count;
		int count2 = _owner.Columns.Count;
		if (count <= 0)
		{
			return;
		}
		RemoveRegion(0, columnIndex + 1, count, count2 - columnIndex, ref removeList);
		RemoveRegion(0, columnIndex, count, 1, ref removeList2);
		if (removeList != null)
		{
			int count3 = removeList.Count;
			for (int i = 0; i < count3; i++)
			{
				CellRegion cellRegion = removeList[i];
				AddRegion(cellRegion.Top, cellRegion.Left - 1, cellRegion.Height, cellRegion.Width, notify: false);
			}
		}
		if (removeList2 != null)
		{
			RemovedCellInfoCollection oldItems = new RemovedCellInfoCollection(_owner, removeList2, oldColumn);
			OnRemove(oldItems);
		}
	}

	private void OnReplaceColumn(int columnIndex, DataGridColumn oldColumn, IList selectedRows)
	{
		List<CellRegion> removeList = null;
		RemoveRegion(0, columnIndex, _owner.Items.Count, 1, ref removeList);
		if (removeList != null)
		{
			RemovedCellInfoCollection oldItems = new RemovedCellInfoCollection(_owner, removeList, oldColumn);
			OnRemove(oldItems);
		}
		FillInFullRowRegions(selectedRows, columnIndex, notify: true);
	}

	private void OnMoveColumn(int oldIndex, int newIndex)
	{
		List<CellRegion> removeList = null;
		List<CellRegion> removeList2 = null;
		int count = _owner.Items.Count;
		int count2 = _owner.Columns.Count;
		if (count <= 0)
		{
			return;
		}
		RemoveRegion(0, oldIndex + 1, count, count2 - oldIndex - 1, ref removeList);
		RemoveRegion(0, oldIndex, count, 1, ref removeList2);
		if (removeList != null)
		{
			int count3 = removeList.Count;
			for (int i = 0; i < count3; i++)
			{
				CellRegion cellRegion = removeList[i];
				AddRegion(cellRegion.Top, cellRegion.Left - 1, cellRegion.Height, cellRegion.Width, notify: false);
			}
		}
		removeList = null;
		RemoveRegion(0, newIndex, count, count2 - newIndex, ref removeList);
		if (removeList2 != null)
		{
			int count4 = removeList2.Count;
			for (int j = 0; j < count4; j++)
			{
				CellRegion cellRegion2 = removeList2[j];
				AddRegion(cellRegion2.Top, newIndex, cellRegion2.Height, cellRegion2.Width, notify: false);
			}
		}
		if (removeList != null)
		{
			int count5 = removeList.Count;
			for (int k = 0; k < count5; k++)
			{
				CellRegion cellRegion3 = removeList[k];
				AddRegion(cellRegion3.Top, cellRegion3.Left + 1, cellRegion3.Height, cellRegion3.Width, notify: false);
			}
		}
	}

	internal void Union(VirtualizedCellInfoCollection collection)
	{
		int count = collection._regions.Count;
		for (int i = 0; i < count; i++)
		{
			CellRegion cellRegion = collection._regions[i];
			AddRegion(cellRegion.Top, cellRegion.Left, cellRegion.Height, cellRegion.Width);
		}
	}

	internal static void Xor(VirtualizedCellInfoCollection c1, VirtualizedCellInfoCollection c2)
	{
		VirtualizedCellInfoCollection virtualizedCellInfoCollection = new VirtualizedCellInfoCollection(c2._owner, c2._regions);
		int count = c1._regions.Count;
		for (int i = 0; i < count; i++)
		{
			CellRegion cellRegion = c1._regions[i];
			c2.RemoveRegion(cellRegion.Top, cellRegion.Left, cellRegion.Height, cellRegion.Width);
		}
		count = virtualizedCellInfoCollection._regions.Count;
		for (int j = 0; j < count; j++)
		{
			CellRegion cellRegion2 = virtualizedCellInfoCollection._regions[j];
			c1.RemoveRegion(cellRegion2.Top, cellRegion2.Left, cellRegion2.Height, cellRegion2.Width);
		}
	}

	internal void ClearFullRows(IList rows)
	{
		if (IsEmpty)
		{
			return;
		}
		int count = _owner.Columns.Count;
		if (_regions.Count == 1)
		{
			CellRegion cellRegion = _regions[0];
			if (cellRegion.Width == count && cellRegion.Height == rows.Count)
			{
				Clear();
				return;
			}
		}
		List<CellRegion> removeList = new List<CellRegion>();
		int count2 = rows.Count;
		for (int i = 0; i < count2; i++)
		{
			int num = _owner.Items.IndexOf(rows[i]);
			if (num >= 0)
			{
				RemoveRegion(num, 0, 1, count, ref removeList);
			}
		}
		if (removeList.Count > 0)
		{
			OnRemove(new VirtualizedCellInfoCollection(_owner, removeList));
		}
	}

	internal void RestoreOnlyFullRows(List<Tuple<int, int>> ranges)
	{
		Clear();
		int count = _owner.Columns.Count;
		if (count <= 0)
		{
			return;
		}
		foreach (Tuple<int, int> range in ranges)
		{
			AddRegion(range.Item1, 0, range.Item2, count);
		}
	}

	internal void RemoveAllButOne(DataGridCellInfo cellInfo)
	{
		if (!IsEmpty)
		{
			ConvertCellInfoToIndexes(cellInfo, out var rowIndex, out var columnIndex);
			RemoveAllButRegion(rowIndex, columnIndex, 1, 1);
		}
	}

	internal void RemoveAllButOne()
	{
		if (!IsEmpty)
		{
			CellRegion cellRegion = _regions[0];
			RemoveAllButRegion(cellRegion.Top, cellRegion.Left, 1, 1);
		}
	}

	internal void RemoveAllButOneRow(int rowIndex)
	{
		if (!IsEmpty && rowIndex >= 0)
		{
			RemoveAllButRegion(rowIndex, 0, 1, _owner.Columns.Count);
		}
	}

	private void RemoveAllButRegion(int rowIndex, int columnIndex, int rowCount, int columnCount)
	{
		List<CellRegion> removeList = null;
		RemoveRegion(rowIndex, columnIndex, rowCount, columnCount, ref removeList);
		VirtualizedCellInfoCollection oldItems = new VirtualizedCellInfoCollection(_owner, _regions);
		_regions.Clear();
		_regions.Add(new CellRegion(columnIndex, rowIndex, columnCount, rowCount));
		OnRemove(oldItems);
	}

	internal bool Intersects(int rowIndex)
	{
		CellRegion region = new CellRegion(0, rowIndex, _owner.Columns.Count, 1);
		int count = _regions.Count;
		for (int i = 0; i < count; i++)
		{
			if (_regions[i].Intersects(region))
			{
				return true;
			}
		}
		return false;
	}

	internal bool Intersects(int rowIndex, out List<int> columnIndexRanges)
	{
		CellRegion region = new CellRegion(0, rowIndex, _owner.Columns.Count, 1);
		columnIndexRanges = null;
		int count = _regions.Count;
		for (int i = 0; i < count; i++)
		{
			CellRegion cellRegion = _regions[i];
			if (cellRegion.Intersects(region))
			{
				if (columnIndexRanges == null)
				{
					columnIndexRanges = new List<int>();
				}
				columnIndexRanges.Add(cellRegion.Left);
				columnIndexRanges.Add(cellRegion.Width);
			}
		}
		return columnIndexRanges != null;
	}

	private void ConvertCellInfoToIndexes(DataGridCellInfo cell, out int rowIndex, out int columnIndex)
	{
		columnIndex = cell.Column.DisplayIndex;
		rowIndex = cell.ItemInfo.Index;
		if (rowIndex < 0)
		{
			rowIndex = _owner.Items.IndexOf(cell.Item);
		}
	}

	private static void ConvertIndexToIndexes(List<CellRegion> regions, int index, out int rowIndex, out int columnIndex)
	{
		columnIndex = -1;
		rowIndex = -1;
		int count = regions.Count;
		for (int i = 0; i < count; i++)
		{
			CellRegion cellRegion = regions[i];
			int size = cellRegion.Size;
			if (index < size)
			{
				columnIndex = cellRegion.Left + index % cellRegion.Width;
				rowIndex = cellRegion.Top + index / cellRegion.Width;
				break;
			}
			index -= size;
		}
	}

	private DataGridCellInfo GetCellInfoFromIndex(DataGrid owner, List<CellRegion> regions, int index)
	{
		ConvertIndexToIndexes(regions, index, out var rowIndex, out var columnIndex);
		if (rowIndex >= 0 && columnIndex >= 0 && rowIndex < owner.Items.Count && columnIndex < owner.Columns.Count)
		{
			DataGridColumn column = owner.ColumnFromDisplayIndex(columnIndex);
			ItemsControl.ItemInfo rowInfo = owner.ItemInfoFromIndex(rowIndex);
			return CreateCellInfo(rowInfo, column, owner);
		}
		return DataGridCellInfo.Unset;
	}

	private void ValidateIsReadOnly()
	{
		if (IsReadOnly)
		{
			throw new NotSupportedException(SR.VirtualizedCellInfoCollection_IsReadOnly);
		}
	}

	private void AddRegionToList(CellRegion region, List<DataGridCellInfo> list)
	{
		for (int i = region.Top; i <= region.Bottom; i++)
		{
			ItemsControl.ItemInfo rowInfo = _owner.ItemInfoFromIndex(i);
			for (int j = region.Left; j <= region.Right; j++)
			{
				DataGridColumn column = _owner.ColumnFromDisplayIndex(j);
				DataGridCellInfo item = CreateCellInfo(rowInfo, column, _owner);
				list.Add(item);
			}
		}
	}

	protected virtual DataGridCellInfo CreateCellInfo(ItemsControl.ItemInfo rowInfo, DataGridColumn column, DataGrid owner)
	{
		return new DataGridCellInfo(rowInfo, column, owner);
	}
}
