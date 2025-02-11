using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal.PtsHost;

namespace MS.Internal.Documents;

internal class FlowDocumentPaginator : DynamicDocumentPaginator, IServiceProvider, IFlowDocumentFormatter
{
	private abstract class AsyncRequest
	{
		internal readonly object UserState;

		protected readonly FlowDocumentPaginator Paginator;

		internal AsyncRequest(object userState, FlowDocumentPaginator paginator)
		{
			UserState = userState;
			Paginator = paginator;
		}

		internal abstract void Cancel();

		internal abstract bool Process();
	}

	private class GetPageAsyncRequest : AsyncRequest
	{
		internal readonly int PageNumber;

		internal GetPageAsyncRequest(int pageNumber, object userState, FlowDocumentPaginator paginator)
			: base(userState, paginator)
		{
			PageNumber = pageNumber;
		}

		internal override void Cancel()
		{
			Paginator.OnGetPageCompleted(new GetPageCompletedEventArgs(null, PageNumber, null, cancelled: true, UserState));
		}

		internal override bool Process()
		{
			if (!Paginator._brt.HasPageBreakRecord(PageNumber))
			{
				return false;
			}
			DocumentPage page = Paginator.FormatPage(PageNumber);
			Paginator.OnGetPageCompleted(new GetPageCompletedEventArgs(page, PageNumber, null, cancelled: false, UserState));
			return true;
		}
	}

	private class GetPageNumberAsyncRequest : AsyncRequest
	{
		internal readonly TextPointer TextPointer;

		internal GetPageNumberAsyncRequest(TextPointer textPointer, object userState, FlowDocumentPaginator paginator)
			: base(userState, paginator)
		{
			TextPointer = textPointer;
		}

		internal override void Cancel()
		{
			Paginator.OnGetPageNumberCompleted(new GetPageNumberCompletedEventArgs(TextPointer, -1, null, cancelled: true, UserState));
		}

		internal override bool Process()
		{
			int pageNumber = 0;
			if (!Paginator._brt.GetPageNumberForContentPosition(TextPointer, ref pageNumber))
			{
				return false;
			}
			Paginator.OnGetPageNumberCompleted(new GetPageNumberCompletedEventArgs(TextPointer, pageNumber, null, cancelled: false, UserState));
			return true;
		}
	}

	private class CustomDispatcherObject : DispatcherObject
	{
	}

	private readonly FlowDocument _document;

	private readonly CustomDispatcherObject _dispatcherObject;

	private readonly BreakRecordTable _brt;

	private Size _pageSize;

	private bool _backgroundPagination;

	private const int _paginationTimeout = 30;

	private static Size _defaultPageSize = new Size(816.0, 1056.0);

	private List<AsyncRequest> _asyncRequests = new List<AsyncRequest>(0);

	private DispatcherOperation _backgroundPaginationOperation;

	public override bool IsPageCountValid
	{
		get
		{
			_dispatcherObject.VerifyAccess();
			return _brt.IsClean;
		}
	}

	public override int PageCount
	{
		get
		{
			_dispatcherObject.VerifyAccess();
			return _brt.Count;
		}
	}

	public override Size PageSize
	{
		get
		{
			return _pageSize;
		}
		set
		{
			_dispatcherObject.VerifyAccess();
			Size pageSize = value;
			if (double.IsNaN(pageSize.Width))
			{
				pageSize.Width = _defaultPageSize.Width;
			}
			if (double.IsNaN(pageSize.Height))
			{
				pageSize.Height = _defaultPageSize.Height;
			}
			Size size = ComputePageSize();
			_pageSize = pageSize;
			Size size2 = ComputePageSize();
			if (!DoubleUtil.AreClose(size, size2))
			{
				if (_document.StructuralCache.IsFormattingInProgress)
				{
					_document.StructuralCache.OnInvalidOperationDetected();
					throw new InvalidOperationException(SR.FlowDocumentInvalidContnetChange);
				}
				InvalidateBRT();
			}
		}
	}

	public override bool IsBackgroundPaginationEnabled
	{
		get
		{
			return _backgroundPagination;
		}
		set
		{
			_dispatcherObject.VerifyAccess();
			if (value != _backgroundPagination)
			{
				_backgroundPagination = value;
				InitiateNextAsyncOperation();
			}
			if (!_backgroundPagination)
			{
				CancelAllAsyncOperations();
			}
		}
	}

	public override IDocumentPaginatorSource Source => _document;

	bool IFlowDocumentFormatter.IsLayoutDataValid => !_document.StructuralCache.IsContentChangeInProgress;

	internal event BreakRecordTableInvalidatedEventHandler BreakRecordTableInvalidated;

	internal FlowDocumentPaginator(FlowDocument document)
	{
		_pageSize = _defaultPageSize;
		_document = document;
		_brt = new BreakRecordTable(this);
		_dispatcherObject = new CustomDispatcherObject();
		_backgroundPagination = true;
		InitiateNextAsyncOperation();
	}

	public override void GetPageAsync(int pageNumber, object userState)
	{
		if (pageNumber < 0)
		{
			throw new ArgumentOutOfRangeException("pageNumber", SR.IDPNegativePageNumber);
		}
		if (_document.StructuralCache.IsFormattingInProgress)
		{
			throw new InvalidOperationException(SR.FlowDocumentFormattingReentrancy);
		}
		if (_document.StructuralCache.IsContentChangeInProgress)
		{
			throw new InvalidOperationException(SR.TextContainerChangingReentrancyInvalid);
		}
		DocumentPage documentPage = null;
		if (!_backgroundPagination)
		{
			documentPage = GetPage(pageNumber);
		}
		else
		{
			if (_brt.IsClean && !_brt.HasPageBreakRecord(pageNumber))
			{
				documentPage = DocumentPage.Missing;
			}
			if (_brt.HasPageBreakRecord(pageNumber))
			{
				documentPage = GetPage(pageNumber);
			}
			if (documentPage == null)
			{
				_asyncRequests.Add(new GetPageAsyncRequest(pageNumber, userState, this));
				InitiateNextAsyncOperation();
			}
		}
		if (documentPage != null)
		{
			OnGetPageCompleted(new GetPageCompletedEventArgs(documentPage, pageNumber, null, cancelled: false, userState));
		}
	}

	public override DocumentPage GetPage(int pageNumber)
	{
		_dispatcherObject.VerifyAccess();
		if (pageNumber < 0)
		{
			throw new ArgumentOutOfRangeException("pageNumber", SR.IDPNegativePageNumber);
		}
		if (_document.StructuralCache.IsFormattingInProgress)
		{
			throw new InvalidOperationException(SR.FlowDocumentFormattingReentrancy);
		}
		if (_document.StructuralCache.IsContentChangeInProgress)
		{
			throw new InvalidOperationException(SR.TextContainerChangingReentrancyInvalid);
		}
		DocumentPage documentPage;
		using (_document.Dispatcher.DisableProcessing())
		{
			_document.StructuralCache.IsFormattingInProgress = true;
			try
			{
				if (_brt.IsClean && !_brt.HasPageBreakRecord(pageNumber))
				{
					documentPage = DocumentPage.Missing;
				}
				else
				{
					documentPage = _brt.GetCachedDocumentPage(pageNumber);
					if (documentPage == null)
					{
						documentPage = (_brt.HasPageBreakRecord(pageNumber) ? FormatPage(pageNumber) : FormatPagesTill(pageNumber));
					}
				}
			}
			finally
			{
				_document.StructuralCache.IsFormattingInProgress = false;
			}
		}
		return documentPage;
	}

	public override void GetPageNumberAsync(ContentPosition contentPosition, object userState)
	{
		if (contentPosition == null)
		{
			throw new ArgumentNullException("contentPosition");
		}
		if (contentPosition == ContentPosition.Missing)
		{
			throw new ArgumentException(SR.IDPInvalidContentPosition, "contentPosition");
		}
		if (!(contentPosition is TextPointer textPointer))
		{
			throw new ArgumentException(SR.IDPInvalidContentPosition, "contentPosition");
		}
		if (textPointer.TextContainer != _document.StructuralCache.TextContainer)
		{
			throw new ArgumentException(SR.IDPInvalidContentPosition, "contentPosition");
		}
		int pageNumber = 0;
		if (!_backgroundPagination)
		{
			pageNumber = GetPageNumber(contentPosition);
			OnGetPageNumberCompleted(new GetPageNumberCompletedEventArgs(contentPosition, pageNumber, null, cancelled: false, userState));
		}
		else if (_brt.GetPageNumberForContentPosition(textPointer, ref pageNumber))
		{
			OnGetPageNumberCompleted(new GetPageNumberCompletedEventArgs(contentPosition, pageNumber, null, cancelled: false, userState));
		}
		else
		{
			_asyncRequests.Add(new GetPageNumberAsyncRequest(textPointer, userState, this));
			InitiateNextAsyncOperation();
		}
	}

	public override int GetPageNumber(ContentPosition contentPosition)
	{
		_dispatcherObject.VerifyAccess();
		if (contentPosition == null)
		{
			throw new ArgumentNullException("contentPosition");
		}
		if (!(contentPosition is TextPointer textPointer))
		{
			throw new ArgumentException(SR.IDPInvalidContentPosition, "contentPosition");
		}
		if (textPointer.TextContainer != _document.StructuralCache.TextContainer)
		{
			throw new ArgumentException(SR.IDPInvalidContentPosition, "contentPosition");
		}
		if (_document.StructuralCache.IsFormattingInProgress)
		{
			throw new InvalidOperationException(SR.FlowDocumentFormattingReentrancy);
		}
		if (_document.StructuralCache.IsContentChangeInProgress)
		{
			throw new InvalidOperationException(SR.TextContainerChangingReentrancyInvalid);
		}
		int pageNumber;
		using (_document.Dispatcher.DisableProcessing())
		{
			_document.StructuralCache.IsFormattingInProgress = true;
			pageNumber = 0;
			try
			{
				while (!_brt.GetPageNumberForContentPosition(textPointer, ref pageNumber))
				{
					if (_brt.IsClean)
					{
						pageNumber = -1;
						break;
					}
					FormatPage(pageNumber);
				}
			}
			finally
			{
				_document.StructuralCache.IsFormattingInProgress = false;
			}
		}
		return pageNumber;
	}

	public override ContentPosition GetPagePosition(DocumentPage page)
	{
		_dispatcherObject.VerifyAccess();
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		if (!(page is FlowDocumentPage { IsDisposed: false } flowDocumentPage))
		{
			return ContentPosition.Missing;
		}
		Point point = new Point(0.0, 0.0);
		if (_document.FlowDirection == FlowDirection.RightToLeft)
		{
			new MatrixTransform(-1.0, 0.0, 0.0, 1.0, flowDocumentPage.Size.Width, 0.0).TryTransform(point, out var result);
			point = result;
		}
		ITextView textView = (ITextView)((IServiceProvider)flowDocumentPage).GetService(typeof(ITextView));
		Invariant.Assert(textView != null, "Cannot access ITextView for FlowDocumentPage.");
		if (textView.TextSegments.Count == 0)
		{
			return ContentPosition.Missing;
		}
		ITextPointer textPointer = textView.GetTextPositionFromPoint(point, snapToText: true);
		if (textPointer == null)
		{
			textPointer = textView.TextSegments[0].Start;
		}
		if (!(textPointer is TextPointer))
		{
			return ContentPosition.Missing;
		}
		return (ContentPosition)textPointer;
	}

	public override ContentPosition GetObjectPosition(object o)
	{
		_dispatcherObject.VerifyAccess();
		if (o == null)
		{
			throw new ArgumentNullException("o");
		}
		return _document.GetObjectPosition(o);
	}

	public override void CancelAsync(object userState)
	{
		if (userState == null)
		{
			CancelAllAsyncOperations();
			return;
		}
		for (int i = 0; i < _asyncRequests.Count; i++)
		{
			AsyncRequest asyncRequest = _asyncRequests[i];
			if (asyncRequest.UserState == userState)
			{
				asyncRequest.Cancel();
				_asyncRequests.RemoveAt(i);
				i--;
			}
		}
	}

	internal void InitiateNextAsyncOperation()
	{
		if (_backgroundPagination && _backgroundPaginationOperation == null && (!_brt.IsClean || _asyncRequests.Count > 0))
		{
			_backgroundPaginationOperation = _document.Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(OnBackgroundPagination), this);
		}
	}

	internal void CancelAllAsyncOperations()
	{
		for (int i = 0; i < _asyncRequests.Count; i++)
		{
			_asyncRequests[i].Cancel();
		}
		_asyncRequests.Clear();
	}

	internal void OnPagesChanged(int pageStart, int pageCount)
	{
		OnPagesChanged(new PagesChangedEventArgs(pageStart, pageCount));
	}

	internal void OnPaginationProgress(int pageStart, int pageCount)
	{
		OnPaginationProgress(new PaginationProgressEventArgs(pageStart, pageCount));
	}

	internal void OnPaginationCompleted()
	{
		OnPaginationCompleted(EventArgs.Empty);
	}

	private void InvalidateBRT()
	{
		if (this.BreakRecordTableInvalidated != null)
		{
			this.BreakRecordTableInvalidated(this, EventArgs.Empty);
		}
		_brt.OnInvalidateLayout();
	}

	private void InvalidateBRTLayout(ITextPointer start, ITextPointer end)
	{
		_brt.OnInvalidateLayout(start, end);
	}

	private DocumentPage FormatPagesTill(int pageNumber)
	{
		while (!_brt.HasPageBreakRecord(pageNumber) && !_brt.IsClean)
		{
			FormatPage(_brt.Count);
		}
		if (_brt.IsClean)
		{
			return DocumentPage.Missing;
		}
		return FormatPage(pageNumber);
	}

	private DocumentPage FormatPage(int pageNumber)
	{
		Invariant.Assert(_brt.HasPageBreakRecord(pageNumber), "BreakRecord for specified page number does not exist.");
		PageBreakRecord pageBreakRecord = _brt.GetPageBreakRecord(pageNumber);
		FlowDocumentPage flowDocumentPage = new FlowDocumentPage(_document.StructuralCache);
		Size size = ComputePageSize();
		Thickness pageMargin = _document.ComputePageMargin();
		PageBreakRecord brOut = flowDocumentPage.FormatFinite(size, pageMargin, pageBreakRecord);
		flowDocumentPage.Arrange(size);
		_brt.UpdateEntry(pageNumber, flowDocumentPage, brOut, flowDocumentPage.DependentMax);
		return flowDocumentPage;
	}

	private object OnBackgroundPagination(object arg)
	{
		DateTime now = DateTime.Now;
		_backgroundPaginationOperation = null;
		_dispatcherObject.VerifyAccess();
		if (_document.StructuralCache.IsFormattingInProgress)
		{
			throw new InvalidOperationException(SR.FlowDocumentFormattingReentrancy);
		}
		if (_document.StructuralCache.PtsContext.Disposed)
		{
			return null;
		}
		using (_document.Dispatcher.DisableProcessing())
		{
			_document.StructuralCache.IsFormattingInProgress = true;
			try
			{
				for (int i = 0; i < _asyncRequests.Count; i++)
				{
					if (_asyncRequests[i].Process())
					{
						_asyncRequests.RemoveAt(i);
						i--;
					}
				}
				DateTime now2 = DateTime.Now;
				if (_backgroundPagination && !_brt.IsClean)
				{
					while (!_brt.IsClean)
					{
						FormatPage(_brt.Count);
						if ((DateTime.Now.Ticks - now.Ticks) / 10000 > 30)
						{
							break;
						}
					}
					InitiateNextAsyncOperation();
				}
			}
			finally
			{
				_document.StructuralCache.IsFormattingInProgress = false;
			}
		}
		return null;
	}

	private Size ComputePageSize()
	{
		Size result = new Size(_document.PageWidth, _document.PageHeight);
		if (double.IsNaN(result.Width))
		{
			result.Width = _pageSize.Width;
			double maxPageWidth = _document.MaxPageWidth;
			if (result.Width > maxPageWidth)
			{
				result.Width = maxPageWidth;
			}
			double minPageWidth = _document.MinPageWidth;
			if (result.Width < minPageWidth)
			{
				result.Width = minPageWidth;
			}
		}
		if (double.IsNaN(result.Height))
		{
			result.Height = _pageSize.Height;
			double maxPageWidth = _document.MaxPageHeight;
			if (result.Height > maxPageWidth)
			{
				result.Height = maxPageWidth;
			}
			double minPageWidth = _document.MinPageHeight;
			if (result.Height < minPageWidth)
			{
				result.Height = minPageWidth;
			}
		}
		return result;
	}

	object IServiceProvider.GetService(Type serviceType)
	{
		return ((IServiceProvider)_document).GetService(serviceType);
	}

	void IFlowDocumentFormatter.OnContentInvalidated(bool affectsLayout)
	{
		if (affectsLayout)
		{
			InvalidateBRT();
		}
		else
		{
			_brt.OnInvalidateRender();
		}
	}

	void IFlowDocumentFormatter.OnContentInvalidated(bool affectsLayout, ITextPointer start, ITextPointer end)
	{
		if (affectsLayout)
		{
			InvalidateBRTLayout(start, end);
		}
		else
		{
			_brt.OnInvalidateRender(start, end);
		}
	}

	void IFlowDocumentFormatter.Suspend()
	{
		IsBackgroundPaginationEnabled = false;
		InvalidateBRT();
	}
}
