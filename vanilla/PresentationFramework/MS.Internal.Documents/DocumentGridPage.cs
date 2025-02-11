using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;

namespace MS.Internal.Documents;

internal class DocumentGridPage : FrameworkElement, IDisposable
{
	private bool hasAddedChildren;

	private DocumentPaginator _paginator;

	private DocumentPageView _documentPageView;

	private ContentControl _documentContainer;

	private bool _showPageBorders;

	private bool _loaded;

	private bool _disposed;

	public DocumentPage DocumentPage
	{
		get
		{
			CheckDisposed();
			return _documentPageView.DocumentPage;
		}
	}

	public int PageNumber
	{
		get
		{
			CheckDisposed();
			return _documentPageView.PageNumber;
		}
		set
		{
			CheckDisposed();
			if (_documentPageView.PageNumber != value)
			{
				_documentPageView.PageNumber = value;
			}
		}
	}

	public DocumentPageView DocumentPageView
	{
		get
		{
			CheckDisposed();
			return _documentPageView;
		}
	}

	public bool ShowPageBorders
	{
		get
		{
			CheckDisposed();
			return _showPageBorders;
		}
		set
		{
			CheckDisposed();
			if (_showPageBorders != value)
			{
				_showPageBorders = value;
				InvalidateMeasure();
			}
		}
	}

	public bool IsPageLoaded
	{
		get
		{
			CheckDisposed();
			return _loaded;
		}
	}

	protected override int VisualChildrenCount
	{
		get
		{
			if (!_disposed && hasAddedChildren)
			{
				return 1;
			}
			return 0;
		}
	}

	public event EventHandler PageLoaded;

	public DocumentGridPage(DocumentPaginator paginator)
	{
		_paginator = paginator;
		_paginator.GetPageCompleted += OnGetPageCompleted;
		Init();
	}

	protected override Visual GetVisualChild(int index)
	{
		CheckDisposed();
		if (VisualChildrenCount != 0)
		{
			if (index == 0)
			{
				return _documentContainer;
			}
			throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
		}
		throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
	}

	protected sealed override Size MeasureOverride(Size availableSize)
	{
		CheckDisposed();
		if (!hasAddedChildren)
		{
			AddVisualChild(_documentContainer);
			hasAddedChildren = true;
		}
		if (ShowPageBorders)
		{
			ComponentResourceKey resourceKey = new ComponentResourceKey(typeof(FrameworkElement), "DocumentGridPageContainerWithBorder");
			_documentContainer.SetCurrentValue(FrameworkElement.StyleProperty, TryFindResource(resourceKey));
		}
		else
		{
			_documentContainer.SetCurrentValue(FrameworkElement.StyleProperty, TryFindResource(typeof(ContentControl)));
		}
		_documentContainer.Measure(availableSize);
		if (DocumentPage.Size != Size.Empty && DocumentPage.Size.Width != 0.0)
		{
			_documentPageView.SetPageZoom(availableSize.Width / DocumentPage.Size.Width);
		}
		return availableSize;
	}

	protected sealed override Size ArrangeOverride(Size arrangeSize)
	{
		CheckDisposed();
		_documentContainer.Arrange(new Rect(new Point(0.0, 0.0), arrangeSize));
		base.ArrangeOverride(arrangeSize);
		return arrangeSize;
	}

	private void Init()
	{
		_documentPageView = new DocumentPageView();
		_documentPageView.ClipToBounds = true;
		_documentPageView.StretchDirection = StretchDirection.Both;
		_documentPageView.PageNumber = int.MaxValue;
		_documentContainer = new ContentControl();
		_documentContainer.Content = _documentPageView;
		_loaded = false;
	}

	private void OnGetPageCompleted(object sender, GetPageCompletedEventArgs e)
	{
		if (!_disposed && e != null && !e.Cancelled && e.Error == null && e.PageNumber != int.MaxValue && e.PageNumber == PageNumber && e.DocumentPage != null && e.DocumentPage != DocumentPage.Missing)
		{
			_loaded = true;
			if (this.PageLoaded != null)
			{
				this.PageLoaded(this, EventArgs.Empty);
			}
		}
	}

	protected void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			if (_paginator != null)
			{
				_paginator.GetPageCompleted -= OnGetPageCompleted;
				_paginator = null;
			}
			((IDisposable)_documentPageView)?.Dispose();
		}
	}

	private void CheckDisposed()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(typeof(DocumentPageView).ToString());
		}
	}

	void IDisposable.Dispose()
	{
		GC.SuppressFinalize(this);
		Dispose();
	}
}
