using System;
using System.Windows;

namespace MS.Internal.Documents;

internal class RowInfo
{
	private Size _rowSize;

	private double _verticalOffset;

	private int _firstPage;

	private int _pageCount;

	public Size RowSize => _rowSize;

	public double VerticalOffset
	{
		get
		{
			return _verticalOffset;
		}
		set
		{
			_verticalOffset = value;
		}
	}

	public int FirstPage
	{
		get
		{
			return _firstPage;
		}
		set
		{
			_firstPage = value;
		}
	}

	public int PageCount => _pageCount;

	public RowInfo()
	{
		_rowSize = new Size(0.0, 0.0);
	}

	public void AddPage(Size pageSize)
	{
		_pageCount++;
		_rowSize.Width += pageSize.Width;
		_rowSize.Height = Math.Max(pageSize.Height, _rowSize.Height);
	}

	public void ClearPages()
	{
		_pageCount = 0;
		_rowSize.Width = 0.0;
		_rowSize.Height = 0.0;
	}
}
