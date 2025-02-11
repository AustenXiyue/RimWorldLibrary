using System;
using System.Collections.Generic;
using System.Windows;

namespace MS.Internal.Documents;

internal class RowCache
{
	private List<RowInfo> _rowCache;

	private int _layoutPivotPage;

	private int _layoutColumns;

	private int _pivotRowIndex;

	private PageCache _pageCache;

	private bool _isLayoutRequested;

	private bool _isLayoutCompleted;

	private double _verticalPageSpacing;

	private double _horizontalPageSpacing;

	private double _scale = 1.0;

	private double _extentHeight;

	private double _extentWidth;

	private bool _hasValidLayout;

	private readonly int _defaultRowCacheSize = 32;

	private readonly int _findOffsetPrecision = 2;

	private readonly double _visibleDelta = 0.5;

	public PageCache PageCache
	{
		get
		{
			return _pageCache;
		}
		set
		{
			_rowCache.Clear();
			_isLayoutCompleted = false;
			_isLayoutRequested = false;
			if (_pageCache != null)
			{
				_pageCache.PageCacheChanged -= OnPageCacheChanged;
				_pageCache.PaginationCompleted -= OnPaginationCompleted;
			}
			_pageCache = value;
			if (_pageCache != null)
			{
				_pageCache.PageCacheChanged += OnPageCacheChanged;
				_pageCache.PaginationCompleted += OnPaginationCompleted;
			}
		}
	}

	public int RowCount => _rowCache.Count;

	public double VerticalPageSpacing
	{
		get
		{
			return _verticalPageSpacing;
		}
		set
		{
			if (value < 0.0)
			{
				value = 0.0;
			}
			if (value != _verticalPageSpacing)
			{
				_verticalPageSpacing = value;
				RecalcLayoutForScaleOrSpacing();
			}
		}
	}

	public double HorizontalPageSpacing
	{
		get
		{
			return _horizontalPageSpacing;
		}
		set
		{
			if (value < 0.0)
			{
				value = 0.0;
			}
			if (value != _horizontalPageSpacing)
			{
				_horizontalPageSpacing = value;
				RecalcLayoutForScaleOrSpacing();
			}
		}
	}

	public double Scale
	{
		get
		{
			return _scale;
		}
		set
		{
			if (_scale != value)
			{
				_scale = value;
				RecalcLayoutForScaleOrSpacing();
			}
		}
	}

	public double ExtentHeight => _extentHeight;

	public double ExtentWidth => _extentWidth;

	public bool HasValidLayout => _hasValidLayout;

	private int LastPageInCache
	{
		get
		{
			if (_rowCache.Count == 0)
			{
				return -1;
			}
			RowInfo rowInfo = _rowCache[_rowCache.Count - 1];
			return rowInfo.FirstPage + rowInfo.PageCount - 1;
		}
	}

	public event RowCacheChangedEventHandler RowCacheChanged;

	public event RowLayoutCompletedEventHandler RowLayoutCompleted;

	public RowCache()
	{
		_rowCache = new List<RowInfo>(_defaultRowCacheSize);
	}

	public RowInfo GetRow(int index)
	{
		if (index < 0 || index > _rowCache.Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return _rowCache[index];
	}

	public RowInfo GetRowForPageNumber(int pageNumber)
	{
		if (pageNumber < 0 || pageNumber > LastPageInCache)
		{
			throw new ArgumentOutOfRangeException("pageNumber");
		}
		return _rowCache[GetRowIndexForPageNumber(pageNumber)];
	}

	public int GetRowIndexForPageNumber(int pageNumber)
	{
		if (pageNumber < 0 || pageNumber > LastPageInCache)
		{
			throw new ArgumentOutOfRangeException("pageNumber");
		}
		for (int i = 0; i < _rowCache.Count; i++)
		{
			RowInfo rowInfo = _rowCache[i];
			if (pageNumber >= rowInfo.FirstPage && pageNumber < rowInfo.FirstPage + rowInfo.PageCount)
			{
				return i;
			}
		}
		throw new InvalidOperationException(SR.RowCachePageNotFound);
	}

	public int GetRowIndexForVerticalOffset(double offset)
	{
		if (offset < 0.0 || offset > ExtentHeight)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (_rowCache.Count == 0)
		{
			return 0;
		}
		double num = Math.Round(offset, _findOffsetPrecision);
		for (int i = 0; i < _rowCache.Count; i++)
		{
			double num2 = Math.Round(_rowCache[i].VerticalOffset, _findOffsetPrecision);
			double num3 = Math.Round(_rowCache[i].RowSize.Height, _findOffsetPrecision);
			bool flag = false;
			if (DoubleUtil.AreClose(num2, num2 + num3))
			{
				flag = true;
			}
			if (flag && DoubleUtil.AreClose(num, num2))
			{
				return i;
			}
			if (num >= num2 && num < num2 + num3)
			{
				if (WithinVisibleDelta(num2 + num3, num) || i == _rowCache.Count - 1)
				{
					return i;
				}
				return i + 1;
			}
		}
		DoubleUtil.AreClose(offset, ExtentHeight);
		return _rowCache.Count - 1;
	}

	public void GetVisibleRowIndices(double startOffset, double endOffset, out int startRowIndex, out int rowCount)
	{
		startRowIndex = 0;
		rowCount = 0;
		if (endOffset < startOffset)
		{
			throw new ArgumentOutOfRangeException("endOffset");
		}
		if (startOffset < 0.0 || startOffset > ExtentHeight || _rowCache.Count == 0)
		{
			return;
		}
		startRowIndex = GetRowIndexForVerticalOffset(startOffset);
		rowCount = 1;
		startOffset = Math.Round(startOffset, _findOffsetPrecision);
		endOffset = Math.Round(endOffset, _findOffsetPrecision);
		for (int i = startRowIndex + 1; i < _rowCache.Count; i++)
		{
			double num = Math.Round(_rowCache[i].VerticalOffset, _findOffsetPrecision);
			if (!(num >= endOffset) && WithinVisibleDelta(endOffset, num))
			{
				rowCount++;
				continue;
			}
			break;
		}
	}

	public void RecalcLayoutForScaleOrSpacing()
	{
		if (PageCache == null)
		{
			throw new InvalidOperationException(SR.RowCacheRecalcWithNoPageCache);
		}
		_extentWidth = 0.0;
		_extentHeight = 0.0;
		double num = 0.0;
		for (int i = 0; i < _rowCache.Count; i++)
		{
			RowInfo rowInfo = _rowCache[i];
			int pageCount = rowInfo.PageCount;
			rowInfo.ClearPages();
			rowInfo.VerticalOffset = num;
			for (int j = rowInfo.FirstPage; j < rowInfo.FirstPage + pageCount; j++)
			{
				Size scaledPageSize = GetScaledPageSize(j);
				rowInfo.AddPage(scaledPageSize);
			}
			_extentWidth = Math.Max(rowInfo.RowSize.Width, _extentWidth);
			num += rowInfo.RowSize.Height;
			_extentHeight += rowInfo.RowSize.Height;
			_rowCache[i] = rowInfo;
		}
		RowCacheChangedEventArgs e = new RowCacheChangedEventArgs(new List<RowCacheChange>(1)
		{
			new RowCacheChange(0, _rowCache.Count)
		});
		this.RowCacheChanged(this, e);
	}

	public void RecalcRows(int pivotPage, int columns)
	{
		if (PageCache == null)
		{
			throw new InvalidOperationException(SR.RowCacheRecalcWithNoPageCache);
		}
		if (pivotPage < 0 || pivotPage > PageCache.PageCount)
		{
			throw new ArgumentOutOfRangeException("pivotPage");
		}
		if (columns < 1)
		{
			throw new ArgumentOutOfRangeException("columns");
		}
		_layoutColumns = columns;
		_layoutPivotPage = pivotPage;
		_hasValidLayout = false;
		if (PageCache.PageCount < _layoutColumns)
		{
			if (!PageCache.IsPaginationCompleted || PageCache.PageCount == 0)
			{
				_isLayoutRequested = true;
				_isLayoutCompleted = false;
				return;
			}
			_layoutColumns = Math.Min(_layoutColumns, PageCache.PageCount);
			_layoutColumns = Math.Max(1, _layoutColumns);
			_layoutPivotPage = 0;
		}
		_extentHeight = 0.0;
		_extentWidth = 0.0;
		if (PageCache.DynamicPageSizes)
		{
			_pivotRowIndex = RecalcRowsForDynamicPageSizes(_layoutPivotPage, _layoutColumns);
		}
		else
		{
			_pivotRowIndex = RecalcRowsForFixedPageSizes(_layoutPivotPage, _layoutColumns);
		}
		_isLayoutCompleted = true;
		_isLayoutRequested = false;
		_hasValidLayout = true;
		RowLayoutCompletedEventArgs e = new RowLayoutCompletedEventArgs(_pivotRowIndex);
		this.RowLayoutCompleted(this, e);
		RowCacheChangedEventArgs e2 = new RowCacheChangedEventArgs(new List<RowCacheChange>(1)
		{
			new RowCacheChange(0, _rowCache.Count)
		});
		this.RowCacheChanged(this, e2);
	}

	private bool WithinVisibleDelta(double offset1, double offset2)
	{
		return offset1 - offset2 > _visibleDelta;
	}

	private int RecalcRowsForDynamicPageSizes(int pivotPage, int columns)
	{
		if (pivotPage < 0 || pivotPage >= PageCache.PageCount)
		{
			throw new ArgumentOutOfRangeException("pivotPage");
		}
		if (columns < 1)
		{
			throw new ArgumentOutOfRangeException("columns");
		}
		if (pivotPage + columns > PageCache.PageCount)
		{
			pivotPage = Math.Max(0, PageCache.PageCount - columns);
		}
		_rowCache.Clear();
		RowInfo rowInfo = CreateFixedRow(pivotPage, columns);
		double width = rowInfo.RowSize.Width;
		int num = 0;
		List<RowInfo> list = new List<RowInfo>(pivotPage / columns);
		int num2 = pivotPage;
		while (num2 > 0)
		{
			RowInfo rowInfo2 = CreateDynamicRow(num2 - 1, width, createForward: false);
			num2 = rowInfo2.FirstPage;
			list.Add(rowInfo2);
		}
		for (int num3 = list.Count - 1; num3 >= 0; num3--)
		{
			AddRow(list[num3]);
		}
		num = _rowCache.Count;
		AddRow(rowInfo);
		num2 = pivotPage + columns;
		while (num2 < PageCache.PageCount)
		{
			RowInfo rowInfo3 = CreateDynamicRow(num2, width, createForward: true);
			num2 += rowInfo3.PageCount;
			AddRow(rowInfo3);
		}
		return num;
	}

	private RowInfo CreateDynamicRow(int startPage, double rowWidth, bool createForward)
	{
		if (startPage >= PageCache.PageCount)
		{
			throw new ArgumentOutOfRangeException("startPage");
		}
		RowInfo rowInfo = new RowInfo();
		Size scaledPageSize = GetScaledPageSize(startPage);
		rowInfo.AddPage(scaledPageSize);
		do
		{
			if (createForward)
			{
				scaledPageSize = GetScaledPageSize(startPage + rowInfo.PageCount);
				if (startPage + rowInfo.PageCount >= PageCache.PageCount || rowInfo.RowSize.Width + scaledPageSize.Width > rowWidth)
				{
					break;
				}
			}
			else
			{
				scaledPageSize = GetScaledPageSize(startPage - rowInfo.PageCount);
				if (startPage - rowInfo.PageCount < 0 || rowInfo.RowSize.Width + scaledPageSize.Width > rowWidth)
				{
					break;
				}
			}
			rowInfo.AddPage(scaledPageSize);
		}
		while (rowInfo.PageCount != DocumentViewerConstants.MaximumMaxPagesAcross);
		if (!createForward)
		{
			rowInfo.FirstPage = startPage - (rowInfo.PageCount - 1);
		}
		else
		{
			rowInfo.FirstPage = startPage;
		}
		return rowInfo;
	}

	private int RecalcRowsForFixedPageSizes(int startPage, int columns)
	{
		if (startPage < 0 || startPage > PageCache.PageCount)
		{
			throw new ArgumentOutOfRangeException("startPage");
		}
		if (columns < 1)
		{
			throw new ArgumentOutOfRangeException("columns");
		}
		_rowCache.Clear();
		for (int i = 0; i < PageCache.PageCount; i += columns)
		{
			RowInfo newRow = CreateFixedRow(i, columns);
			AddRow(newRow);
		}
		return GetRowIndexForPageNumber(startPage);
	}

	private RowInfo CreateFixedRow(int startPage, int columns)
	{
		if (startPage >= PageCache.PageCount)
		{
			throw new ArgumentOutOfRangeException("startPage");
		}
		if (columns < 1)
		{
			throw new ArgumentOutOfRangeException("columns");
		}
		RowInfo rowInfo = new RowInfo();
		rowInfo.FirstPage = startPage;
		for (int i = startPage; i < startPage + columns && i <= PageCache.PageCount - 1; i++)
		{
			Size scaledPageSize = GetScaledPageSize(i);
			rowInfo.AddPage(scaledPageSize);
		}
		return rowInfo;
	}

	private RowCacheChange AddPageRange(int startPage, int count)
	{
		if (!_isLayoutCompleted)
		{
			throw new InvalidOperationException(SR.RowCacheCannotModifyNonExistentLayout);
		}
		int num = startPage;
		int num2 = startPage + count;
		int num3 = 0;
		int num4 = 0;
		if (startPage > LastPageInCache + 1)
		{
			num = LastPageInCache + 1;
		}
		RowInfo rowInfo = _rowCache[_rowCache.Count - 1];
		Size scaledPageSize = GetScaledPageSize(num);
		RowInfo row = GetRow(_pivotRowIndex);
		bool flag = false;
		while (num < num2 && rowInfo.RowSize.Width + scaledPageSize.Width <= row.RowSize.Width)
		{
			rowInfo.AddPage(scaledPageSize);
			num++;
			scaledPageSize = GetScaledPageSize(num);
			flag = true;
		}
		if (flag)
		{
			num3 = _rowCache.Count - 1;
			UpdateRow(num3, rowInfo);
		}
		else
		{
			num3 = _rowCache.Count;
		}
		while (num < num2)
		{
			RowInfo rowInfo2 = new RowInfo();
			rowInfo2.FirstPage = num;
			do
			{
				scaledPageSize = GetScaledPageSize(num);
				rowInfo2.AddPage(scaledPageSize);
				num++;
			}
			while (rowInfo2.RowSize.Width + scaledPageSize.Width <= row.RowSize.Width && num < num2);
			AddRow(rowInfo2);
			num4++;
		}
		return new RowCacheChange(num3, num4);
	}

	private void AddRow(RowInfo newRow)
	{
		if (_rowCache.Count == 0)
		{
			newRow.VerticalOffset = 0.0;
			_extentWidth = newRow.RowSize.Width;
		}
		else
		{
			RowInfo rowInfo = _rowCache[_rowCache.Count - 1];
			newRow.VerticalOffset = rowInfo.VerticalOffset + rowInfo.RowSize.Height;
			_extentWidth = Math.Max(newRow.RowSize.Width, _extentWidth);
		}
		_extentHeight += newRow.RowSize.Height;
		_rowCache.Add(newRow);
	}

	private RowCacheChange UpdatePageRange(int startPage, int count)
	{
		if (!_isLayoutCompleted)
		{
			throw new InvalidOperationException(SR.RowCacheCannotModifyNonExistentLayout);
		}
		int rowIndexForPageNumber = GetRowIndexForPageNumber(startPage);
		int num = rowIndexForPageNumber;
		int num2 = startPage;
		while (num2 < startPage + count && num < _rowCache.Count)
		{
			RowInfo rowInfo = _rowCache[num];
			RowInfo rowInfo2 = new RowInfo();
			rowInfo2.VerticalOffset = rowInfo.VerticalOffset;
			rowInfo2.FirstPage = rowInfo.FirstPage;
			for (int i = rowInfo.FirstPage; i < rowInfo.FirstPage + rowInfo.PageCount; i++)
			{
				Size scaledPageSize = GetScaledPageSize(i);
				rowInfo2.AddPage(scaledPageSize);
			}
			UpdateRow(num, rowInfo2);
			num2 = rowInfo2.FirstPage + rowInfo2.PageCount;
			num++;
		}
		return new RowCacheChange(rowIndexForPageNumber, num - rowIndexForPageNumber);
	}

	private void UpdateRow(int index, RowInfo newRow)
	{
		if (!_isLayoutCompleted)
		{
			throw new InvalidOperationException(SR.RowCacheCannotModifyNonExistentLayout);
		}
		if (index > _rowCache.Count)
		{
			return;
		}
		RowInfo rowInfo = _rowCache[index];
		_rowCache[index] = newRow;
		if (rowInfo.RowSize.Height != newRow.RowSize.Height)
		{
			double num = newRow.RowSize.Height - rowInfo.RowSize.Height;
			for (int i = index + 1; i < _rowCache.Count; i++)
			{
				RowInfo rowInfo2 = _rowCache[i];
				rowInfo2.VerticalOffset += num;
				_rowCache[i] = rowInfo2;
			}
			_extentHeight += num;
		}
		if (newRow.RowSize.Width > _extentWidth)
		{
			_extentWidth = newRow.RowSize.Width;
		}
		else if (rowInfo.RowSize.Width != newRow.RowSize.Width)
		{
			_extentWidth = 0.0;
			for (int j = 0; j < _rowCache.Count; j++)
			{
				RowInfo rowInfo3 = _rowCache[j];
				_extentWidth = Math.Max(rowInfo3.RowSize.Width, _extentWidth);
			}
		}
	}

	private RowCacheChange TrimPageRange(int startPage)
	{
		int num = GetRowIndexForPageNumber(startPage);
		RowInfo row = GetRow(num);
		if (row.FirstPage < startPage)
		{
			RowInfo rowInfo = new RowInfo();
			rowInfo.VerticalOffset = row.VerticalOffset;
			rowInfo.FirstPage = row.FirstPage;
			for (int i = row.FirstPage; i < startPage; i++)
			{
				Size scaledPageSize = GetScaledPageSize(i);
				rowInfo.AddPage(scaledPageSize);
			}
			UpdateRow(num, rowInfo);
			num++;
		}
		int count = _rowCache.Count - num;
		if (num < _rowCache.Count)
		{
			_rowCache.RemoveRange(num, count);
		}
		_extentHeight = row.VerticalOffset;
		return new RowCacheChange(num, count);
	}

	private Size GetScaledPageSize(int pageNumber)
	{
		Size result = PageCache.GetPageSize(pageNumber);
		if (result.IsEmpty)
		{
			result = new Size(0.0, 0.0);
		}
		result.Width *= Scale;
		result.Height *= Scale;
		result.Width += HorizontalPageSpacing;
		result.Height += VerticalPageSpacing;
		return result;
	}

	private void OnPageCacheChanged(object sender, PageCacheChangedEventArgs args)
	{
		if (_isLayoutCompleted)
		{
			List<RowCacheChange> list = new List<RowCacheChange>(args.Changes.Count);
			for (int i = 0; i < args.Changes.Count; i++)
			{
				PageCacheChange pageCacheChange = args.Changes[i];
				switch (pageCacheChange.Type)
				{
				case PageCacheChangeType.Add:
				case PageCacheChangeType.Update:
				{
					if (pageCacheChange.Start > LastPageInCache)
					{
						RowCacheChange rowCacheChange2 = AddPageRange(pageCacheChange.Start, pageCacheChange.Count);
						if (rowCacheChange2 != null)
						{
							list.Add(rowCacheChange2);
						}
						break;
					}
					if (pageCacheChange.Start + pageCacheChange.Count - 1 <= LastPageInCache)
					{
						RowCacheChange rowCacheChange3 = UpdatePageRange(pageCacheChange.Start, pageCacheChange.Count);
						if (rowCacheChange3 != null)
						{
							list.Add(rowCacheChange3);
						}
						break;
					}
					RowCacheChange rowCacheChange4 = UpdatePageRange(pageCacheChange.Start, LastPageInCache - pageCacheChange.Start);
					if (rowCacheChange4 != null)
					{
						list.Add(rowCacheChange4);
					}
					rowCacheChange4 = AddPageRange(LastPageInCache + 1, pageCacheChange.Count - (LastPageInCache - pageCacheChange.Start));
					if (rowCacheChange4 != null)
					{
						list.Add(rowCacheChange4);
					}
					break;
				}
				case PageCacheChangeType.Remove:
					if (PageCache.PageCount - 1 < LastPageInCache)
					{
						RowCacheChange rowCacheChange = TrimPageRange(PageCache.PageCount);
						if (rowCacheChange != null)
						{
							list.Add(rowCacheChange);
						}
					}
					if (_rowCache.Count <= 1 && (_rowCache.Count == 0 || _rowCache[0].PageCount < _layoutColumns))
					{
						RecalcRows(0, _layoutColumns);
					}
					break;
				default:
					throw new ArgumentOutOfRangeException("args");
				}
			}
			RowCacheChangedEventArgs e = new RowCacheChangedEventArgs(list);
			this.RowCacheChanged(this, e);
		}
		else if (_isLayoutRequested)
		{
			RecalcRows(_layoutPivotPage, _layoutColumns);
		}
	}

	private void OnPaginationCompleted(object sender, EventArgs args)
	{
		if (_isLayoutRequested)
		{
			RecalcRows(_layoutPivotPage, _layoutColumns);
		}
	}
}
