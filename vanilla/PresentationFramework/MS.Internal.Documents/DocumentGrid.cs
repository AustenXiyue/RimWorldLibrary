using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Utility;

namespace MS.Internal.Documents;

internal class DocumentGrid : FrameworkElement, IDocumentScrollInfo, IScrollInfo
{
	private delegate void BringPageIntoViewCallback(MakeVisibleData data, int pageNumber);

	private enum VisualTreeModificationState
	{
		BeforeExisting,
		DuringExisting,
		AfterExisting
	}

	private enum ViewMode
	{
		SetColumns,
		FitColumns,
		PageWidth,
		PageHeight,
		Thumbnails,
		Zoom,
		SetHorizontalOffset,
		SetVerticalOffset
	}

	private class DocumentLayout
	{
		private ViewMode _viewMode;

		private int _columns;

		private double _offset;

		public ViewMode ViewMode
		{
			get
			{
				return _viewMode;
			}
			set
			{
				_viewMode = value;
			}
		}

		public int Columns
		{
			get
			{
				return _columns;
			}
			set
			{
				_columns = value;
			}
		}

		public double Offset => _offset;

		public DocumentLayout(int columns, ViewMode viewMode)
			: this(columns, 0.0, viewMode)
		{
		}

		public DocumentLayout(double offset, ViewMode viewMode)
			: this(1, offset, viewMode)
		{
		}

		public DocumentLayout(int columns, double offset, ViewMode viewMode)
		{
			_columns = columns;
			_offset = offset;
			_viewMode = viewMode;
		}
	}

	private struct MakeVisibleData
	{
		private Visual _visual;

		private ContentPosition _contentPosition;

		private Rect _rect;

		public Visual Visual => _visual;

		public ContentPosition ContentPosition => _contentPosition;

		public Rect Rect => _rect;

		public MakeVisibleData(Visual visual, ContentPosition contentPosition, Rect rect)
		{
			_visual = visual;
			_contentPosition = contentPosition;
			_rect = rect;
		}
	}

	private PageCache _pageCache;

	private RowCache _rowCache;

	private ReadOnlyCollection<DocumentPageView> _pageViews;

	private bool _canHorizontallyScroll;

	private bool _canVerticallyScroll;

	private double _verticalOffset;

	private double _horizontalOffset;

	private double _viewportHeight;

	private double _viewportWidth;

	private int _firstVisibleRow;

	private int _visibleRowCount;

	private int _firstVisiblePageNumber;

	private int _lastVisiblePageNumber;

	private ScrollViewer _scrollOwner;

	private DocumentViewer _documentViewerOwner;

	private bool _showPageBorders = true;

	private bool _lockViewModes;

	private int _maxPagesAcross = 1;

	private Size _previousConstraint;

	private DocumentLayout _documentLayout = new DocumentLayout(1, ViewMode.SetColumns);

	private int _documentLayoutsPending;

	private RowInfo _savedPivotRow;

	private double _lastRowChangeExtentWidth;

	private double _lastRowChangeVerticalOffset;

	private ITextContainer _textContainer;

	private RubberbandSelector _rubberBandSelector;

	private bool _isLayoutRequested;

	private bool _pageJumpAfterLayout;

	private int _pageJumpAfterLayoutPageNumber;

	private bool _firstRowLayout = true;

	private bool _scrollChangedEventAttached;

	private Border _documentGridBackground;

	private const int _backgroundVisualIndex = 0;

	private const int _firstPageVisualIndex = 1;

	private readonly Size _defaultConstraint = new Size(250.0, 250.0);

	private VisualCollection _childrenCollection;

	private int _makeVisiblePageNeeded = -1;

	private DispatcherOperation _makeVisibleDispatcher;

	private DispatcherOperation _setScaleOperation;

	private const double _verticalLineScrollAmount = 16.0;

	private const double _horizontalLineScrollAmount = 16.0;

	public bool CanHorizontallyScroll
	{
		get
		{
			return _canHorizontallyScroll;
		}
		set
		{
			_canHorizontallyScroll = value;
		}
	}

	public bool CanVerticallyScroll
	{
		get
		{
			return _canVerticallyScroll;
		}
		set
		{
			_canVerticallyScroll = value;
		}
	}

	public double ExtentWidth => _rowCache.ExtentWidth;

	public double ExtentHeight => _rowCache.ExtentHeight;

	public double ViewportWidth => _viewportWidth;

	public double ViewportHeight => _viewportHeight;

	public double HorizontalOffset => Math.Max(Math.Min(_horizontalOffset, ExtentWidth - ViewportWidth), 0.0);

	public double VerticalOffset => Math.Max(Math.Min(_verticalOffset, ExtentHeight - ViewportHeight), 0.0);

	public DynamicDocumentPaginator Content
	{
		get
		{
			return _pageCache.Content;
		}
		set
		{
			if (value != _pageCache.Content)
			{
				_textContainer = null;
				if (_pageCache.Content != null)
				{
					_pageCache.Content.GetPageNumberCompleted -= OnGetPageNumberCompleted;
				}
				if (ScrollOwner != null)
				{
					ScrollOwner.ScrollChanged -= OnScrollChanged;
					_scrollChangedEventAttached = false;
				}
				_pageCache.Content = value;
				if (_pageCache.Content != null)
				{
					_pageCache.Content.GetPageNumberCompleted += OnGetPageNumberCompleted;
				}
				ResetVisualTree(pruneOnly: false);
				ResetPageViewCollection();
				_firstVisiblePageNumber = 0;
				_lastVisiblePageNumber = 0;
				EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXPS, EventTrace.Event.WClientDRXPageVisible, _firstVisiblePageNumber, _lastVisiblePageNumber);
				_lastRowChangeExtentWidth = 0.0;
				_lastRowChangeVerticalOffset = 0.0;
				if (_documentLayout.ViewMode == ViewMode.Thumbnails)
				{
					_documentLayout.ViewMode = ViewMode.SetColumns;
				}
				QueueUpdateDocumentLayout(_documentLayout);
				InvalidateMeasure();
				InvalidateDocumentScrollInfo();
			}
		}
	}

	public int PageCount => _pageCache.PageCount;

	public int FirstVisiblePageNumber => _firstVisiblePageNumber;

	public double Scale => _rowCache.Scale;

	public int MaxPagesAcross => _maxPagesAcross;

	public double VerticalPageSpacing
	{
		get
		{
			return _rowCache.VerticalPageSpacing;
		}
		set
		{
			if (!Helper.IsDoubleValid(value))
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_rowCache.VerticalPageSpacing = value;
		}
	}

	public double HorizontalPageSpacing
	{
		get
		{
			return _rowCache.HorizontalPageSpacing;
		}
		set
		{
			if (!Helper.IsDoubleValid(value))
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_rowCache.HorizontalPageSpacing = value;
		}
	}

	public bool ShowPageBorders
	{
		get
		{
			return _showPageBorders;
		}
		set
		{
			if (_showPageBorders == value)
			{
				return;
			}
			_showPageBorders = value;
			int count = _childrenCollection.Count;
			for (int i = 0; i < count; i++)
			{
				if (_childrenCollection[i] is DocumentGridPage documentGridPage)
				{
					documentGridPage.ShowPageBorders = _showPageBorders;
				}
			}
		}
	}

	public bool LockViewModes
	{
		get
		{
			return _lockViewModes;
		}
		set
		{
			_lockViewModes = value;
		}
	}

	public ITextContainer TextContainer
	{
		get
		{
			if (_textContainer == null && Content != null && Content is IServiceProvider serviceProvider)
			{
				_textContainer = (ITextContainer)serviceProvider.GetService(typeof(ITextContainer));
			}
			return _textContainer;
		}
	}

	public ITextView TextView
	{
		get
		{
			if (TextEditor != null)
			{
				return TextEditor.TextView;
			}
			return null;
		}
	}

	public ReadOnlyCollection<DocumentPageView> PageViews => _pageViews;

	public ScrollViewer ScrollOwner
	{
		get
		{
			return _scrollOwner;
		}
		set
		{
			_scrollOwner = value;
			InvalidateDocumentScrollInfo();
		}
	}

	public DocumentViewer DocumentViewerOwner
	{
		get
		{
			return _documentViewerOwner;
		}
		set
		{
			_documentViewerOwner = value;
		}
	}

	protected override int VisualChildrenCount => _childrenCollection.Count;

	private bool IsViewportNonzero
	{
		get
		{
			if (ViewportWidth != 0.0)
			{
				return ViewportHeight != 0.0;
			}
			return false;
		}
	}

	private TextEditor TextEditor
	{
		get
		{
			if (DocumentViewerOwner != null)
			{
				return DocumentViewerOwner.TextEditor;
			}
			return null;
		}
	}

	private double MouseWheelVerticalScrollAmount => 16.0 * (double)SystemParameters.WheelScrollLines;

	private bool CanMouseWheelVerticallyScroll
	{
		get
		{
			if (_canVerticallyScroll)
			{
				return SystemParameters.WheelScrollLines > 0;
			}
			return false;
		}
	}

	private double MouseWheelHorizontalScrollAmount => 16.0 * (double)SystemParameters.WheelScrollLines;

	private bool CanMouseWheelHorizontallyScroll
	{
		get
		{
			if (_canHorizontallyScroll)
			{
				return SystemParameters.WheelScrollLines > 0;
			}
			return false;
		}
	}

	private double CurrentMinimumScale
	{
		get
		{
			if (_documentLayout.ViewMode != ViewMode.Thumbnails)
			{
				return DocumentViewerConstants.MinimumScale;
			}
			return DocumentViewerConstants.MinimumThumbnailsScale;
		}
	}

	static DocumentGrid()
	{
		EventManager.RegisterClassHandler(typeof(DocumentGrid), FrameworkElement.RequestBringIntoViewEvent, new RequestBringIntoViewEventHandler(OnRequestBringIntoView));
		DocumentGridContextMenu.RegisterClassHandler();
	}

	public DocumentGrid()
	{
		Initialize();
	}

	internal DocumentPage GetDocumentPageFromPoint(Point point)
	{
		return GetDocumentPageViewFromPoint(point)?.DocumentPage;
	}

	public void LineUp()
	{
		if (_canVerticallyScroll)
		{
			SetVerticalOffsetInternal(VerticalOffset - 16.0);
		}
	}

	public void LineDown()
	{
		if (_canVerticallyScroll)
		{
			SetVerticalOffsetInternal(VerticalOffset + 16.0);
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXPS, EventTrace.Event.WClientDRXLineDown);
		}
	}

	public void LineLeft()
	{
		if (_canHorizontallyScroll)
		{
			SetHorizontalOffsetInternal(HorizontalOffset - 16.0);
		}
	}

	public void LineRight()
	{
		if (_canHorizontallyScroll)
		{
			SetHorizontalOffsetInternal(HorizontalOffset + 16.0);
		}
	}

	public void PageUp()
	{
		SetVerticalOffsetInternal(VerticalOffset - ViewportHeight);
	}

	public void PageDown()
	{
		SetVerticalOffsetInternal(VerticalOffset + ViewportHeight);
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXPS, EventTrace.Event.WClientDRXPageDown, (int)VerticalOffset);
	}

	public void PageLeft()
	{
		SetHorizontalOffsetInternal(HorizontalOffset - ViewportWidth);
	}

	public void PageRight()
	{
		SetHorizontalOffsetInternal(HorizontalOffset + ViewportWidth);
	}

	public void MouseWheelUp()
	{
		if (CanMouseWheelVerticallyScroll)
		{
			SetVerticalOffsetInternal(VerticalOffset - MouseWheelVerticalScrollAmount);
		}
		else
		{
			PageUp();
		}
	}

	public void MouseWheelDown()
	{
		if (CanMouseWheelVerticallyScroll)
		{
			SetVerticalOffsetInternal(VerticalOffset + MouseWheelVerticalScrollAmount);
		}
		else
		{
			PageDown();
		}
	}

	public void MouseWheelLeft()
	{
		if (CanMouseWheelHorizontallyScroll)
		{
			SetHorizontalOffsetInternal(HorizontalOffset - MouseWheelHorizontalScrollAmount);
		}
		else
		{
			PageLeft();
		}
	}

	public void MouseWheelRight()
	{
		if (CanMouseWheelHorizontallyScroll)
		{
			SetHorizontalOffsetInternal(HorizontalOffset + MouseWheelHorizontalScrollAmount);
		}
		else
		{
			PageRight();
		}
	}

	public Rect MakeVisible(Visual v, Rect r)
	{
		if (Content != null && v != null)
		{
			ContentPosition objectPosition = Content.GetObjectPosition(v);
			MakeContentPositionVisibleAsync(new MakeVisibleData(v, objectPosition, r));
		}
		return r;
	}

	public Rect MakeVisible(object o, Rect r, int pageNumber)
	{
		ContentPosition objectPosition = Content.GetObjectPosition(o);
		MakeVisibleAsync(new MakeVisibleData(o as Visual, objectPosition, r), pageNumber);
		return r;
	}

	public void MakeSelectionVisible()
	{
		if (TextEditor != null && TextEditor.Selection != null)
		{
			ContentPosition contentPosition = TextEditor.Selection.Start.CreatePointer(LogicalDirection.Forward) as ContentPosition;
			MakeContentPositionVisibleAsync(new MakeVisibleData(null, contentPosition, Rect.Empty));
		}
	}

	public void MakePageVisible(int pageNumber)
	{
		if (Math.Abs(pageNumber - _firstVisiblePageNumber) > 1)
		{
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXPS, EventTrace.Event.WClientDRXPageJump, _firstVisiblePageNumber, pageNumber);
		}
		if (pageNumber < 0)
		{
			SetVerticalOffsetInternal(0.0);
			SetHorizontalOffsetInternal(0.0);
		}
		else if (pageNumber >= _pageCache.PageCount || _rowCache.RowCount == 0)
		{
			if (_pageCache.IsPaginationCompleted && _rowCache.HasValidLayout)
			{
				SetVerticalOffsetInternal(ExtentHeight);
				SetHorizontalOffsetInternal(ExtentWidth);
			}
			else
			{
				_pageJumpAfterLayout = true;
				_pageJumpAfterLayoutPageNumber = pageNumber;
			}
		}
		else
		{
			RowInfo rowForPageNumber = _rowCache.GetRowForPageNumber(pageNumber);
			SetVerticalOffsetInternal(rowForPageNumber.VerticalOffset);
			double horizontalOffsetForPage = GetHorizontalOffsetForPage(rowForPageNumber, pageNumber);
			SetHorizontalOffsetInternal(horizontalOffsetForPage);
		}
	}

	public void ScrollToNextRow()
	{
		int num = _firstVisibleRow + 1;
		if (num < _rowCache.RowCount)
		{
			RowInfo row = _rowCache.GetRow(num);
			SetVerticalOffsetInternal(row.VerticalOffset);
		}
	}

	public void ScrollToPreviousRow()
	{
		int num = _firstVisibleRow - 1;
		if (num >= 0 && num < _rowCache.RowCount)
		{
			RowInfo row = _rowCache.GetRow(num);
			SetVerticalOffsetInternal(row.VerticalOffset);
		}
	}

	public void ScrollToHome()
	{
		SetVerticalOffsetInternal(0.0);
	}

	public void ScrollToEnd()
	{
		SetVerticalOffsetInternal(ExtentHeight);
	}

	public void SetScale(double scale)
	{
		if (!DoubleUtil.AreClose(scale, Scale))
		{
			if (scale <= 0.0)
			{
				throw new ArgumentOutOfRangeException("scale");
			}
			if (!Helper.IsDoubleValid(scale))
			{
				throw new ArgumentOutOfRangeException("scale");
			}
			QueueSetScale(scale);
		}
	}

	public void SetColumns(int columns)
	{
		if (columns < 1)
		{
			throw new ArgumentOutOfRangeException("columns");
		}
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXPS, EventTrace.Event.WClientDRXLayoutBegin);
		QueueUpdateDocumentLayout(new DocumentLayout(columns, ViewMode.SetColumns));
	}

	public void FitColumns(int columns)
	{
		if (columns < 1)
		{
			throw new ArgumentOutOfRangeException("columns");
		}
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXPS, EventTrace.Event.WClientDRXLayoutBegin);
		QueueUpdateDocumentLayout(new DocumentLayout(columns, ViewMode.FitColumns));
	}

	public void FitToPageWidth()
	{
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXPS, EventTrace.Event.WClientDRXLayoutBegin);
		QueueUpdateDocumentLayout(new DocumentLayout(1, ViewMode.PageWidth));
	}

	public void FitToPageHeight()
	{
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXPS, EventTrace.Event.WClientDRXLayoutBegin);
		QueueUpdateDocumentLayout(new DocumentLayout(1, ViewMode.PageHeight));
	}

	public void ViewThumbnails()
	{
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXPS, EventTrace.Event.WClientDRXLayoutBegin);
		QueueUpdateDocumentLayout(new DocumentLayout(1, ViewMode.Thumbnails));
	}

	public void SetHorizontalOffset(double offset)
	{
		if (!DoubleUtil.AreClose(_horizontalOffset, offset))
		{
			if (double.IsNaN(offset))
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			if (_documentLayoutsPending == 0)
			{
				SetHorizontalOffsetInternal(offset);
			}
			else
			{
				QueueUpdateDocumentLayout(new DocumentLayout(offset, ViewMode.SetHorizontalOffset));
			}
		}
	}

	public void SetVerticalOffset(double offset)
	{
		if (!DoubleUtil.AreClose(_verticalOffset, offset))
		{
			if (double.IsNaN(offset))
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			if (_documentLayoutsPending == 0)
			{
				SetVerticalOffsetInternal(offset);
			}
			else
			{
				QueueUpdateDocumentLayout(new DocumentLayout(offset, ViewMode.SetVerticalOffset));
			}
		}
	}

	protected override Visual GetVisualChild(int index)
	{
		if (_childrenCollection == null || index < 0 || index >= _childrenCollection.Count)
		{
			throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
		}
		return _childrenCollection[index];
	}

	protected override Size MeasureOverride(Size constraint)
	{
		if (double.IsInfinity(constraint.Width) || double.IsInfinity(constraint.Height))
		{
			constraint = _defaultConstraint;
		}
		RecalculateVisualPages(VerticalOffset, constraint);
		int count = _childrenCollection.Count;
		for (int i = 0; i < count; i++)
		{
			if (i == 0)
			{
				((_childrenCollection[i] as Border) ?? throw new InvalidOperationException(SR.DocumentGridVisualTreeContainsNonBorderAsFirstElement)).Measure(constraint);
				continue;
			}
			if (!(_childrenCollection[i] is DocumentGridPage documentGridPage))
			{
				throw new InvalidOperationException(SR.DocumentGridVisualTreeContainsNonDocumentGridPage);
			}
			Size pageSize = _pageCache.GetPageSize(documentGridPage.PageNumber);
			pageSize.Width *= Scale;
			pageSize.Height *= Scale;
			if (documentGridPage.IsMeasureValid)
			{
				continue;
			}
			documentGridPage.Measure(pageSize);
			Size pageSize2 = _pageCache.GetPageSize(documentGridPage.PageNumber);
			if (pageSize2 != Size.Empty)
			{
				pageSize2.Width *= Scale;
				pageSize2.Height *= Scale;
				if (pageSize2.Width != pageSize.Width || pageSize2.Height != pageSize.Height)
				{
					documentGridPage.Measure(pageSize2);
				}
			}
		}
		return constraint;
	}

	protected override Size ArrangeOverride(Size arrangeSize)
	{
		if (_viewportHeight != arrangeSize.Height || _viewportWidth != arrangeSize.Width)
		{
			_viewportWidth = arrangeSize.Width;
			_viewportHeight = arrangeSize.Height;
			if (LockViewModes && IsViewLoaded() && _firstVisiblePageNumber < _pageCache.PageCount && _rowCache.HasValidLayout)
			{
				ApplyViewParameters(_rowCache.GetRowForPageNumber(_firstVisiblePageNumber));
				MeasureOverride(arrangeSize);
			}
			UpdateTextView();
		}
		if (IsViewportNonzero && ExecutePendingLayoutRequests())
		{
			MeasureOverride(arrangeSize);
		}
		if (_previousConstraint != arrangeSize)
		{
			_previousConstraint = arrangeSize;
			InvalidateDocumentScrollInfo();
		}
		if (_childrenCollection.Count == 0)
		{
			return arrangeSize;
		}
		(_childrenCollection[0] as UIElement).Arrange(new Rect(new Point(0.0, 0.0), arrangeSize));
		int num = 1;
		for (int i = _firstVisibleRow; i < _firstVisibleRow + _visibleRowCount; i++)
		{
			CalculateRowOffsets(i, out var xOffset, out var yOffset);
			RowInfo row = _rowCache.GetRow(i);
			for (int j = row.FirstPage; j < row.FirstPage + row.PageCount; j++)
			{
				if (num > _childrenCollection.Count - 1)
				{
					throw new InvalidOperationException(SR.DocumentGridVisualTreeOutOfSync);
				}
				Size pageSize = _pageCache.GetPageSize(j);
				pageSize.Width *= Scale;
				pageSize.Height *= Scale;
				if (!(_childrenCollection[num] is UIElement uIElement))
				{
					throw new InvalidOperationException(SR.DocumentGridVisualTreeContainsNonUIElement);
				}
				Point location = ((!_pageCache.IsContentRightToLeft) ? new Point(xOffset, yOffset) : new Point(Math.Max(ViewportWidth, ExtentWidth) - (xOffset + pageSize.Width), yOffset));
				uIElement.Arrange(new Rect(location, pageSize));
				xOffset += pageSize.Width + HorizontalPageSpacing;
				num++;
			}
		}
		AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
		if (adornerLayer != null && adornerLayer.GetAdorners(this) != null)
		{
			adornerLayer.Update(this);
		}
		return arrangeSize;
	}

	protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
	{
		bool flag = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
		if (flag && _rubberBandSelector == null)
		{
			if (Content is IServiceProvider serviceProvider)
			{
				_rubberBandSelector = serviceProvider.GetService(typeof(RubberbandSelector)) as RubberbandSelector;
				if (_rubberBandSelector != null)
				{
					DocumentViewerOwner.Focus();
					ITextRange selection = TextEditor.Selection;
					selection.Select(selection.Start, selection.Start);
					DocumentViewerOwner.IsSelectionEnabled = false;
					_rubberBandSelector.AttachRubberbandSelector(this);
				}
			}
		}
		else if (!flag && _rubberBandSelector != null)
		{
			if (_rubberBandSelector != null)
			{
				_rubberBandSelector.DetachRubberbandSelector();
				_rubberBandSelector = null;
			}
			DocumentViewerOwner.IsSelectionEnabled = true;
		}
	}

	protected internal override void OnVisualParentChanged(DependencyObject oldParent)
	{
		base.OnVisualParentChanged(oldParent);
		if (VisualTreeHelper.GetParent(this) != null)
		{
			ResetVisualTree(pruneOnly: false);
		}
	}

	private void RecalculateVisualPages(double offset, Size constraint)
	{
		if (_rowCache.RowCount == 0)
		{
			ResetVisualTree(pruneOnly: false);
			ResetPageViewCollection();
			_firstVisibleRow = 0;
			_visibleRowCount = 0;
			_firstVisiblePageNumber = 0;
			_lastVisiblePageNumber = 0;
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXPS, EventTrace.Event.WClientDRXPageVisible, _firstVisiblePageNumber, _lastVisiblePageNumber);
			return;
		}
		int startRowIndex = 0;
		int rowCount = 0;
		_rowCache.GetVisibleRowIndices(offset, offset + constraint.Height, out startRowIndex, out rowCount);
		if (rowCount == 0)
		{
			ResetVisualTree(pruneOnly: false);
			ResetPageViewCollection();
			_firstVisibleRow = 0;
			_visibleRowCount = 0;
			_firstVisiblePageNumber = 0;
			_lastVisiblePageNumber = 0;
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXPS, EventTrace.Event.WClientDRXPageVisible, _firstVisiblePageNumber, _lastVisiblePageNumber);
			return;
		}
		int num = -1;
		int num2 = -1;
		if (_childrenCollection.Count > 1)
		{
			num = ((_childrenCollection[1] is DocumentGridPage documentGridPage) ? documentGridPage.PageNumber : (-1));
			num2 = ((_childrenCollection[_childrenCollection.Count - 1] is DocumentGridPage documentGridPage2) ? documentGridPage2.PageNumber : (-1));
		}
		RowInfo row = _rowCache.GetRow(startRowIndex);
		_firstVisiblePageNumber = row.FirstPage;
		RowInfo row2 = _rowCache.GetRow(startRowIndex + rowCount - 1);
		_lastVisiblePageNumber = row2.FirstPage + row2.PageCount - 1;
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXPS, EventTrace.Event.WClientDRXPageVisible, _firstVisiblePageNumber, _lastVisiblePageNumber);
		_firstVisibleRow = startRowIndex;
		_visibleRowCount = rowCount;
		if (_firstVisiblePageNumber == num && _lastVisiblePageNumber == num2)
		{
			return;
		}
		ArrayList arrayList = new ArrayList();
		for (int i = _firstVisibleRow; i < _firstVisibleRow + _visibleRowCount; i++)
		{
			RowInfo row3 = _rowCache.GetRow(i);
			for (int j = row3.FirstPage; j < row3.FirstPage + row3.PageCount; j++)
			{
				if (j < num || j > num2 || _childrenCollection.Count <= 1)
				{
					DocumentGridPage documentGridPage3 = new DocumentGridPage(Content);
					documentGridPage3.ShowPageBorders = ShowPageBorders;
					documentGridPage3.PageNumber = j;
					documentGridPage3.PageLoaded += OnPageLoaded;
					arrayList.Add(documentGridPage3);
				}
				else
				{
					arrayList.Add(_childrenCollection[1 + j - Math.Max(0, num)]);
				}
			}
		}
		ResetVisualTree(pruneOnly: true);
		Collection<DocumentPageView> collection = new Collection<DocumentPageView>();
		VisualTreeModificationState visualTreeModificationState = VisualTreeModificationState.BeforeExisting;
		int num3 = 1;
		for (int k = 0; k < arrayList.Count; k++)
		{
			Visual visual = (Visual)arrayList[k];
			switch (visualTreeModificationState)
			{
			case VisualTreeModificationState.BeforeExisting:
				if (num3 < _childrenCollection.Count && _childrenCollection[num3] == visual)
				{
					visualTreeModificationState = VisualTreeModificationState.DuringExisting;
				}
				else
				{
					_childrenCollection.Insert(num3, visual);
				}
				num3++;
				break;
			case VisualTreeModificationState.DuringExisting:
				if (num3 >= _childrenCollection.Count || _childrenCollection[num3] != visual)
				{
					visualTreeModificationState = VisualTreeModificationState.AfterExisting;
					_childrenCollection.Add(visual);
				}
				num3++;
				break;
			case VisualTreeModificationState.AfterExisting:
				_childrenCollection.Add(visual);
				break;
			}
			collection.Add(((DocumentGridPage)arrayList[k]).DocumentPageView);
		}
		_pageViews = new ReadOnlyCollection<DocumentPageView>(collection);
		InvalidatePageViews();
		InvalidateDocumentScrollInfo();
	}

	private void OnPageLoaded(object sender, EventArgs args)
	{
		DocumentGridPage documentGridPage = sender as DocumentGridPage;
		Invariant.Assert(documentGridPage != null, "Invalid sender for OnPageLoaded event.");
		documentGridPage.PageLoaded -= OnPageLoaded;
		if (_makeVisiblePageNeeded == documentGridPage.PageNumber)
		{
			_makeVisiblePageNeeded = -1;
			_makeVisibleDispatcher.Priority = DispatcherPriority.Background;
		}
		if (EventTrace.IsEnabled(EventTrace.Keyword.KeywordXPS, EventTrace.Level.Info))
		{
			EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientDRXPageLoaded, EventTrace.Keyword.KeywordXPS, EventTrace.Level.Info, documentGridPage.PageNumber);
		}
	}

	private void CalculateRowOffsets(int row, out double xOffset, out double yOffset)
	{
		xOffset = 0.0;
		yOffset = 0.0;
		RowInfo row2 = _rowCache.GetRow(row);
		double num = Math.Max(ViewportWidth, ExtentWidth);
		if (row == _rowCache.RowCount - 1 && !_pageCache.DynamicPageSizes)
		{
			xOffset = (num - ExtentWidth) / 2.0 + HorizontalPageSpacing / 2.0 - HorizontalOffset;
		}
		else
		{
			xOffset = (num - row2.RowSize.Width) / 2.0 + HorizontalPageSpacing / 2.0 - HorizontalOffset;
		}
		if (ExtentHeight > ViewportHeight)
		{
			yOffset = row2.VerticalOffset + VerticalPageSpacing / 2.0 - VerticalOffset;
		}
		else
		{
			yOffset = row2.VerticalOffset + (ViewportHeight - ExtentHeight) / 2.0 + VerticalPageSpacing / 2.0;
		}
	}

	private void ResetVisualTree(bool pruneOnly)
	{
		for (int num = _childrenCollection.Count - 1; num >= 1; num--)
		{
			if (_childrenCollection[num] is DocumentGridPage documentGridPage && (!pruneOnly || _rowCache.RowCount == 0 || documentGridPage.PageNumber < _firstVisiblePageNumber || documentGridPage.PageNumber > _lastVisiblePageNumber))
			{
				_childrenCollection.Remove(documentGridPage);
				documentGridPage.PageLoaded -= OnPageLoaded;
				((IDisposable)documentGridPage).Dispose();
			}
		}
		if (_documentGridBackground == null)
		{
			_documentGridBackground = new Border();
			_documentGridBackground.Background = Brushes.Transparent;
			_childrenCollection.Add(_documentGridBackground);
		}
	}

	private void ResetPageViewCollection()
	{
		_pageViews = null;
		InvalidatePageViews();
	}

	private void OnGetPageNumberCompleted(object sender, GetPageNumberCompletedEventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		if (e.UserState is MakeVisibleData)
		{
			MakeVisibleData data = (MakeVisibleData)e.UserState;
			MakeVisibleAsync(data, e.PageNumber);
		}
	}

	private void MakeVisibleAsync(MakeVisibleData data, int pageNumber)
	{
		base.Dispatcher.BeginInvoke(DispatcherPriority.Background, new BringPageIntoViewCallback(BringPageIntoViewDelegate), data, pageNumber);
	}

	private void BringPageIntoViewDelegate(MakeVisibleData data, int pageNumber)
	{
		if (!_rowCache.HasValidLayout || (data.Visual is FixedPage && data.Visual.VisualContentBounds == data.Rect) || pageNumber < _firstVisiblePageNumber || pageNumber > _lastVisiblePageNumber)
		{
			MakePageVisible(pageNumber);
		}
		if (IsPageLoaded(pageNumber))
		{
			MakeVisibleImpl(data);
			return;
		}
		_makeVisiblePageNeeded = pageNumber;
		_makeVisibleDispatcher = base.Dispatcher.BeginInvoke(DispatcherPriority.Inactive, (DispatcherOperationCallback)delegate(object arg)
		{
			MakeVisibleImpl((MakeVisibleData)arg);
			return (object)null;
		}, data);
	}

	private void MakeVisibleImpl(MakeVisibleData data)
	{
		if (data.Visual != null)
		{
			if (IsAncestorOf(data.Visual))
			{
				GeneralTransform generalTransform = data.Visual.TransformToAncestor(this);
				Rect rect = ((data.Rect != Rect.Empty) ? data.Rect : data.Visual.VisualContentBounds);
				Rect r = generalTransform.TransformBounds(rect);
				MakeRectVisible(r, alwaysCenter: false);
			}
		}
		else if (data.ContentPosition != null)
		{
			ITextPointer textPointer = data.ContentPosition as ITextPointer;
			if (TextViewContains(textPointer))
			{
				MakeRectVisible(TextView.GetRectangleFromTextPosition(textPointer), alwaysCenter: false);
			}
		}
		else
		{
			Invariant.Assert(condition: false, "Invalid object brought into view.");
		}
	}

	private void MakeRectVisible(Rect r, bool alwaysCenter)
	{
		if (r != Rect.Empty)
		{
			Rect rect = new Rect(HorizontalOffset + r.X, VerticalOffset + r.Y, r.Width, r.Height);
			Rect rect2 = new Rect(HorizontalOffset, VerticalOffset, ViewportWidth, ViewportHeight);
			if (alwaysCenter || !rect.IntersectsWith(rect2))
			{
				SetHorizontalOffsetInternal(rect.X - ViewportWidth / 2.0);
				SetVerticalOffsetInternal(rect.Y - ViewportHeight / 2.0);
			}
		}
	}

	private void MakeIPVisible(Rect r)
	{
		if (r != Rect.Empty && TextEditor != null && !new Rect(HorizontalOffset, VerticalOffset, ViewportWidth, ViewportHeight).Contains(r))
		{
			if (r.X < HorizontalOffset)
			{
				SetHorizontalOffsetInternal(HorizontalOffset - (HorizontalOffset - r.X));
			}
			else if (r.X > HorizontalOffset + ViewportWidth)
			{
				SetHorizontalOffsetInternal(HorizontalOffset + (r.X - (HorizontalOffset + ViewportWidth)));
			}
			if (r.Y < VerticalOffset)
			{
				SetVerticalOffsetInternal(VerticalOffset - (VerticalOffset - r.Y));
			}
			else if (r.Y + r.Height > VerticalOffset + ViewportHeight)
			{
				SetVerticalOffsetInternal(VerticalOffset + (r.Y + r.Height - (VerticalOffset + ViewportHeight)));
			}
		}
	}

	private void MakeContentPositionVisibleAsync(MakeVisibleData data)
	{
		if (data.ContentPosition != null && data.ContentPosition != ContentPosition.Missing)
		{
			Content.GetPageNumberAsync(data.ContentPosition, data);
		}
	}

	private void QueueSetScale(double scale)
	{
		if (_setScaleOperation != null && _setScaleOperation.Status == DispatcherOperationStatus.Pending)
		{
			_setScaleOperation.Abort();
		}
		_setScaleOperation = Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(SetScaleDelegate), scale);
	}

	private object SetScaleDelegate(object scale)
	{
		if (!(scale is double scale2))
		{
			return null;
		}
		_documentLayout.ViewMode = ViewMode.Zoom;
		ITextPointer visibleSelection = GetVisibleSelection();
		if (visibleSelection != null)
		{
			int pageNumberForVisibleSelection = GetPageNumberForVisibleSelection(visibleSelection);
			UpdateLayoutScale(scale2);
			MakePageVisible(pageNumberForVisibleSelection);
			base.LayoutUpdated += OnZoomLayoutUpdated;
		}
		else
		{
			UpdateLayoutScale(scale2);
		}
		return null;
	}

	private void UpdateLayoutScale(double scale)
	{
		if (!DoubleUtil.AreClose(scale, Scale))
		{
			double extentHeight = ExtentHeight;
			double extentWidth = ExtentWidth;
			_rowCache.Scale = scale;
			double num = ((extentHeight == 0.0) ? 1.0 : (ExtentHeight / extentHeight));
			double num2 = ((extentWidth == 0.0) ? 1.0 : (ExtentWidth / extentWidth));
			SetVerticalOffsetInternal(_verticalOffset * num);
			SetHorizontalOffsetInternal(_horizontalOffset * num2);
			InvalidateMeasure();
			InvalidateChildMeasure();
			InvalidateDocumentScrollInfo();
		}
	}

	private void QueueUpdateDocumentLayout(DocumentLayout layout)
	{
		_documentLayoutsPending++;
		Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(UpdateDocumentLayoutDelegate), layout);
	}

	private object UpdateDocumentLayoutDelegate(object layout)
	{
		if (layout is DocumentLayout)
		{
			UpdateDocumentLayout((DocumentLayout)layout);
		}
		_documentLayoutsPending--;
		return null;
	}

	private void UpdateDocumentLayout(DocumentLayout layout)
	{
		if (layout.ViewMode == ViewMode.SetHorizontalOffset)
		{
			SetHorizontalOffsetInternal(layout.Offset);
			return;
		}
		if (layout.ViewMode == ViewMode.SetVerticalOffset)
		{
			SetVerticalOffsetInternal(layout.Offset);
			return;
		}
		_documentLayout = layout;
		_maxPagesAcross = _documentLayout.Columns;
		if (IsViewportNonzero)
		{
			if (_documentLayout.ViewMode == ViewMode.Thumbnails)
			{
				int maxPagesAcross = (_documentLayout.Columns = CalculateThumbnailColumns());
				_maxPagesAcross = maxPagesAcross;
			}
			int activeFocusPage = GetActiveFocusPage();
			_rowCache.RecalcRows(activeFocusPage, _documentLayout.Columns);
			_isLayoutRequested = false;
		}
		else
		{
			_isLayoutRequested = true;
		}
	}

	private bool ExecutePendingLayoutRequests()
	{
		if (_isLayoutRequested)
		{
			UpdateDocumentLayout(_documentLayout);
			return true;
		}
		return false;
	}

	private void SetHorizontalOffsetInternal(double offset)
	{
		if (!DoubleUtil.AreClose(_horizontalOffset, offset))
		{
			if (double.IsNaN(offset))
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			_horizontalOffset = offset;
			InvalidateMeasure();
			InvalidateDocumentScrollInfo();
			UpdateTextView();
		}
	}

	private void SetVerticalOffsetInternal(double offset)
	{
		if (!DoubleUtil.AreClose(_verticalOffset, offset))
		{
			if (double.IsNaN(offset))
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			_verticalOffset = offset;
			InvalidateMeasure();
			InvalidateDocumentScrollInfo();
			UpdateTextView();
		}
	}

	private void UpdateTextView()
	{
		if (TextView is MultiPageTextView multiPageTextView)
		{
			multiPageTextView.OnPageLayoutChanged();
		}
	}

	private int CalculateThumbnailColumns()
	{
		if (!IsViewportNonzero)
		{
			return 1;
		}
		if (_pageCache.PageCount == 0)
		{
			return 1;
		}
		Size pageSize = _pageCache.GetPageSize(0);
		double num = ViewportWidth / ViewportHeight;
		int val = (int)Math.Floor(ViewportWidth / (CurrentMinimumScale * pageSize.Width + HorizontalPageSpacing));
		val = Math.Min(val, _pageCache.PageCount);
		val = Math.Min(val, DocumentViewerConstants.MaximumMaxPagesAcross);
		int result = 1;
		double num2 = double.MaxValue;
		for (int i = 1; i <= val; i++)
		{
			int num3 = (int)Math.Floor((double)(_pageCache.PageCount / i));
			double num4 = pageSize.Width * (double)i;
			double num5 = pageSize.Height * (double)num3;
			double num6 = Math.Abs(num4 / num5 - num);
			if (num6 < num2)
			{
				num2 = num6;
				result = i;
			}
		}
		return result;
	}

	private void InvalidateChildMeasure()
	{
		int count = _childrenCollection.Count;
		for (int i = 0; i < count; i++)
		{
			if (_childrenCollection[i] is UIElement uIElement)
			{
				uIElement.InvalidateMeasure();
			}
		}
	}

	private bool RowIsClean(RowInfo row)
	{
		bool result = true;
		for (int i = row.FirstPage; i < row.FirstPage + row.PageCount; i++)
		{
			if (_pageCache.IsPageDirty(i))
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private void EnsureFit(RowInfo pivotRow)
	{
		double num = CalculateScaleFactor(pivotRow);
		double num2 = num * _rowCache.Scale;
		if (!(num2 < CurrentMinimumScale) && !(num2 > DocumentViewerConstants.MaximumScale) && !DoubleUtil.AreClose(1.0, num))
		{
			ApplyViewParameters(pivotRow);
			SetVerticalOffsetInternal(pivotRow.VerticalOffset);
		}
	}

	private void ApplyViewParameters(RowInfo pivotRow)
	{
		if (_pageCache.DynamicPageSizes)
		{
			_maxPagesAcross = pivotRow.PageCount;
		}
		double val = CalculateScaleFactor(pivotRow) * _rowCache.Scale;
		val = Math.Max(val, CurrentMinimumScale);
		val = Math.Min(val, DocumentViewerConstants.MaximumScale);
		UpdateLayoutScale(val);
	}

	private double CalculateScaleFactor(RowInfo pivotRow)
	{
		double num = ((!_pageCache.DynamicPageSizes) ? (ExtentWidth - (double)MaxPagesAcross * HorizontalPageSpacing) : (pivotRow.RowSize.Width - (double)pivotRow.PageCount * HorizontalPageSpacing));
		double num2 = pivotRow.RowSize.Height - VerticalPageSpacing;
		if (num <= 0.0 || num2 <= 0.0)
		{
			return 1.0;
		}
		double num3 = ((!_pageCache.DynamicPageSizes) ? (ViewportWidth - (double)MaxPagesAcross * HorizontalPageSpacing) : (ViewportWidth - (double)pivotRow.PageCount * HorizontalPageSpacing));
		double num4 = ViewportHeight - VerticalPageSpacing;
		if (num3 <= 0.0 || num4 <= 0.0)
		{
			return 1.0;
		}
		double result = 1.0;
		switch (_documentLayout.ViewMode)
		{
		case ViewMode.FitColumns:
			result = Math.Min(num3 / num, num4 / num2);
			break;
		case ViewMode.PageWidth:
			result = num3 / num;
			break;
		case ViewMode.PageHeight:
			result = num4 / num2;
			break;
		case ViewMode.Thumbnails:
		{
			double num5 = ExtentHeight - VerticalPageSpacing * (double)_rowCache.RowCount;
			double num6 = ViewportHeight - VerticalPageSpacing * (double)_rowCache.RowCount;
			result = ((!(num6 <= 0.0)) ? Math.Min(num3 / num, num6 / num5) : 1.0);
			break;
		}
		default:
			throw new InvalidOperationException(SR.DocumentGridInvalidViewMode);
		case ViewMode.SetColumns:
		case ViewMode.Zoom:
			break;
		}
		return result;
	}

	private void Initialize()
	{
		_pageCache = new PageCache();
		_childrenCollection = new VisualCollection(this);
		_rowCache = new RowCache();
		_rowCache.PageCache = _pageCache;
		_rowCache.RowCacheChanged += OnRowCacheChanged;
		_rowCache.RowLayoutCompleted += OnRowLayoutCompleted;
	}

	private void InvalidateDocumentScrollInfo()
	{
		if (ScrollOwner != null)
		{
			ScrollOwner.InvalidateScrollInfo();
		}
		if (DocumentViewerOwner != null)
		{
			DocumentViewerOwner.InvalidateDocumentScrollInfo();
		}
	}

	private void InvalidatePageViews()
	{
		Invariant.Assert(DocumentViewerOwner != null, "DocumentViewerOwner cannot be null.");
		if (DocumentViewerOwner != null)
		{
			DocumentViewerOwner.InvalidatePageViewsInternal();
			DocumentViewerOwner.ApplyTemplate();
		}
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXPS, EventTrace.Event.WClientDRXInvalidateView);
	}

	private ITextPointer GetVisibleSelection()
	{
		ITextPointer result = null;
		if (HasSelection())
		{
			ITextPointer start = TextEditor.Selection.Start;
			if (TextViewContains(start))
			{
				result = start;
			}
		}
		return result;
	}

	private bool HasSelection()
	{
		if (TextEditor != null)
		{
			return TextEditor.Selection != null;
		}
		return false;
	}

	private int GetPageNumberForVisibleSelection(ITextPointer selection)
	{
		Invariant.Assert(TextViewContains(selection));
		foreach (DocumentPageView pageView in _pageViews)
		{
			if (((IServiceProvider)pageView).GetService(typeof(ITextView)) is DocumentPageTextView { IsValid: not false } documentPageTextView && documentPageTextView.Contains(selection))
			{
				return pageView.PageNumber;
			}
		}
		Invariant.Assert(condition: false, "Selection was in TextView, but not found in any visible page!");
		return 0;
	}

	private Point GetActiveFocusPoint()
	{
		ITextPointer visibleSelection = GetVisibleSelection();
		if (visibleSelection != null && visibleSelection.HasValidLayout)
		{
			Rect rectangleFromTextPosition = TextView.GetRectangleFromTextPosition(visibleSelection);
			if (rectangleFromTextPosition != Rect.Empty)
			{
				return new Point(rectangleFromTextPosition.Left, rectangleFromTextPosition.Top);
			}
		}
		return new Point(0.0, 0.0);
	}

	private int GetActiveFocusPage()
	{
		return GetDocumentPageViewFromPoint(GetActiveFocusPoint())?.PageNumber ?? _firstVisiblePageNumber;
	}

	private DocumentPageView GetDocumentPageViewFromPoint(Point point)
	{
		DependencyObject dependencyObject = VisualTreeHelper.HitTest(this, point)?.VisualHit;
		DocumentPageView documentPageView = null;
		while (dependencyObject != null)
		{
			if (dependencyObject is DocumentPageView result)
			{
				return result;
			}
			dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
		}
		return null;
	}

	private bool TextViewContains(ITextPointer tp)
	{
		if (TextView != null && TextView.IsValid)
		{
			return TextView.Contains(tp);
		}
		return false;
	}

	private double GetHorizontalOffsetForPage(RowInfo row, int pageNumber)
	{
		if (row == null)
		{
			throw new ArgumentNullException("row");
		}
		if (pageNumber < row.FirstPage || pageNumber > row.FirstPage + row.PageCount)
		{
			throw new ArgumentOutOfRangeException("pageNumber");
		}
		double num = (_pageCache.DynamicPageSizes ? Math.Max(0.0, (ExtentWidth - row.RowSize.Width) / 2.0) : 0.0);
		for (int i = row.FirstPage; i < pageNumber; i++)
		{
			num += _pageCache.GetPageSize(i).Width * Scale + HorizontalPageSpacing;
		}
		return num;
	}

	private bool RowCacheChangeIsVisible(RowCacheChange change)
	{
		int firstVisibleRow = _firstVisibleRow;
		int num = _firstVisibleRow + _visibleRowCount;
		int start = change.Start;
		int num2 = change.Start + change.Count;
		if ((start >= firstVisibleRow && start <= num) || (num2 >= firstVisibleRow && num2 <= num) || (start < firstVisibleRow && num2 > num))
		{
			return true;
		}
		return false;
	}

	private bool IsPageLoaded(int pageNumber)
	{
		return GetDocumentGridPageForPageNumber(pageNumber)?.IsPageLoaded ?? false;
	}

	private bool IsViewLoaded()
	{
		bool result = true;
		for (int i = 1; i < _childrenCollection.Count; i++)
		{
			if (_childrenCollection[i] is DocumentGridPage { IsPageLoaded: false })
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private DocumentGridPage GetDocumentGridPageForPageNumber(int pageNumber)
	{
		for (int i = 1; i < _childrenCollection.Count; i++)
		{
			if (_childrenCollection[i] is DocumentGridPage documentGridPage && documentGridPage.PageNumber == pageNumber)
			{
				return documentGridPage;
			}
		}
		return null;
	}

	private static void OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs args)
	{
		DocumentGrid documentGrid = sender as DocumentGrid;
		DocumentGrid documentGrid2 = args.TargetObject as DocumentGrid;
		if (documentGrid != null && documentGrid2 != null && documentGrid == documentGrid2)
		{
			args.Handled = true;
			documentGrid2.MakeIPVisible(args.TargetRect);
		}
		else
		{
			args.Handled = false;
		}
	}

	private void OnScrollChanged(object sender, EventArgs args)
	{
		if (ScrollOwner != null)
		{
			_scrollChangedEventAttached = false;
			ScrollOwner.ScrollChanged -= OnScrollChanged;
		}
		if (_rowCache.HasValidLayout)
		{
			EnsureFit(_rowCache.GetRowForPageNumber(FirstVisiblePageNumber));
		}
	}

	private void OnZoomLayoutUpdated(object sender, EventArgs args)
	{
		base.LayoutUpdated -= OnZoomLayoutUpdated;
		ITextPointer visibleSelection = GetVisibleSelection();
		if (visibleSelection != null)
		{
			MakeRectVisible(TextView.GetRectangleFromTextPosition(visibleSelection), alwaysCenter: true);
		}
	}

	private void OnRowCacheChanged(object source, RowCacheChangedEventArgs args)
	{
		if (_savedPivotRow != null && RowIsClean(_savedPivotRow))
		{
			if (_documentLayout.ViewMode != ViewMode.Zoom && _documentLayout.ViewMode != 0)
			{
				if (_savedPivotRow.FirstPage < _rowCache.RowCount)
				{
					RowInfo rowForPageNumber = _rowCache.GetRowForPageNumber(_savedPivotRow.FirstPage);
					if (rowForPageNumber.RowSize.Width != _savedPivotRow.RowSize.Width || rowForPageNumber.RowSize.Height != _savedPivotRow.RowSize.Height)
					{
						ApplyViewParameters(rowForPageNumber);
					}
					_savedPivotRow = null;
				}
			}
			else
			{
				_savedPivotRow = null;
			}
		}
		if (_pageCache.DynamicPageSizes && _lastRowChangeVerticalOffset != VerticalOffset && _lastRowChangeExtentWidth < ExtentWidth)
		{
			if (_lastRowChangeExtentWidth != 0.0)
			{
				SetHorizontalOffsetInternal(HorizontalOffset + (ExtentWidth - _lastRowChangeExtentWidth) / 2.0);
			}
			_lastRowChangeExtentWidth = ExtentWidth;
		}
		_lastRowChangeVerticalOffset = VerticalOffset;
		for (int i = 0; i < args.Changes.Count; i++)
		{
			RowCacheChange change = args.Changes[i];
			if (RowCacheChangeIsVisible(change))
			{
				InvalidateMeasure();
				InvalidateChildMeasure();
			}
		}
		InvalidateDocumentScrollInfo();
	}

	private void OnRowLayoutCompleted(object source, RowLayoutCompletedEventArgs args)
	{
		if (args != null)
		{
			if (args.PivotRowIndex >= _rowCache.RowCount)
			{
				throw new ArgumentOutOfRangeException("args");
			}
			RowInfo row = _rowCache.GetRow(args.PivotRowIndex);
			if (!RowIsClean(row) && _documentLayout.ViewMode != ViewMode.Zoom)
			{
				_savedPivotRow = row;
			}
			else
			{
				_savedPivotRow = null;
			}
			ApplyViewParameters(row);
			if (!_firstRowLayout && !_pageJumpAfterLayout)
			{
				MakePageVisible(row.FirstPage);
			}
			else if (_pageJumpAfterLayout)
			{
				MakePageVisible(_pageJumpAfterLayoutPageNumber);
				_pageJumpAfterLayout = false;
			}
			_firstRowLayout = false;
			if (!_scrollChangedEventAttached && ScrollOwner != null && _documentLayout.ViewMode != ViewMode.Zoom && _documentLayout.ViewMode != 0)
			{
				_scrollChangedEventAttached = true;
				ScrollOwner.ScrollChanged += OnScrollChanged;
			}
		}
	}
}
