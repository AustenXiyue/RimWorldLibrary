using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Threading;

namespace MS.Internal.Documents;

internal class ReaderPageViewer : FlowDocumentPageViewer, IFlowDocumentViewer
{
	private EventHandler _pageNumberChanged;

	private EventHandler _pageCountChanged;

	private EventHandler _printCompleted;

	private EventHandler _printStarted;

	private bool _raisePageNumberChanged;

	private bool _raisePageCountChanged;

	ContentPosition IFlowDocumentViewer.ContentPosition
	{
		get
		{
			return base.ContentPosition;
		}
		set
		{
			if (value != null && base.Document != null)
			{
				base.Dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(base.BringContentPositionIntoView), value);
			}
		}
	}

	ITextSelection IFlowDocumentViewer.TextSelection
	{
		get
		{
			return base.Selection;
		}
		set
		{
			if (value != null && base.Document != null)
			{
				base.Dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(SetTextSelection), value);
			}
		}
	}

	bool IFlowDocumentViewer.CanGoToPreviousPage => CanGoToPreviousPage;

	bool IFlowDocumentViewer.CanGoToNextPage => CanGoToNextPage;

	int IFlowDocumentViewer.PageNumber => MasterPageNumber;

	int IFlowDocumentViewer.PageCount => base.PageCount;

	event EventHandler IFlowDocumentViewer.PageNumberChanged
	{
		add
		{
			_pageNumberChanged = (EventHandler)Delegate.Combine(_pageNumberChanged, value);
		}
		remove
		{
			_pageNumberChanged = (EventHandler)Delegate.Remove(_pageNumberChanged, value);
		}
	}

	event EventHandler IFlowDocumentViewer.PageCountChanged
	{
		add
		{
			_pageCountChanged = (EventHandler)Delegate.Combine(_pageCountChanged, value);
		}
		remove
		{
			_pageCountChanged = (EventHandler)Delegate.Remove(_pageCountChanged, value);
		}
	}

	event EventHandler IFlowDocumentViewer.PrintStarted
	{
		add
		{
			_printStarted = (EventHandler)Delegate.Combine(_printStarted, value);
		}
		remove
		{
			_printStarted = (EventHandler)Delegate.Remove(_printStarted, value);
		}
	}

	event EventHandler IFlowDocumentViewer.PrintCompleted
	{
		add
		{
			_printCompleted = (EventHandler)Delegate.Combine(_printCompleted, value);
		}
		remove
		{
			_printCompleted = (EventHandler)Delegate.Remove(_printCompleted, value);
		}
	}

	protected override void OnPrintCompleted()
	{
		base.OnPrintCompleted();
		if (_printCompleted != null)
		{
			_printCompleted(this, EventArgs.Empty);
		}
	}

	protected override void OnPrintCommand()
	{
		base.OnPrintCommand();
		if (_printStarted != null && base.IsPrinting)
		{
			_printStarted(this, EventArgs.Empty);
		}
	}

	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (e.Property == DocumentViewerBase.PageCountProperty || e.Property == DocumentViewerBase.MasterPageNumberProperty || e.Property == DocumentViewerBase.CanGoToPreviousPageProperty || e.Property == DocumentViewerBase.CanGoToNextPageProperty)
		{
			if (!_raisePageNumberChanged && !_raisePageCountChanged)
			{
				base.Dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(RaisePropertyChangedAsync), null);
			}
			if (e.Property == DocumentViewerBase.PageCountProperty)
			{
				_raisePageCountChanged = true;
				CoerceValue(DocumentViewerBase.CanGoToNextPageProperty);
			}
			else if (e.Property == DocumentViewerBase.MasterPageNumberProperty)
			{
				_raisePageNumberChanged = true;
				CoerceValue(DocumentViewerBase.CanGoToNextPageProperty);
			}
			else
			{
				_raisePageNumberChanged = true;
			}
		}
	}

	private object SetTextSelection(object arg)
	{
		ITextSelection textSelection = arg as ITextSelection;
		FlowDocument flowDocument = base.Document as FlowDocument;
		if (textSelection != null && flowDocument != null && textSelection.AnchorPosition != null && textSelection.AnchorPosition.TextContainer == flowDocument.StructuralCache.TextContainer)
		{
			ITextSelection textSelection2 = flowDocument.StructuralCache.TextContainer.TextSelection;
			if (textSelection2 != null)
			{
				textSelection2.SetCaretToPosition(textSelection.AnchorPosition, textSelection.MovingPosition.LogicalDirection, allowStopAtLineEnd: true, allowStopNearSpace: true);
				textSelection2.ExtendToPosition(textSelection.MovingPosition);
			}
		}
		return null;
	}

	private object RaisePropertyChangedAsync(object arg)
	{
		if (_raisePageCountChanged)
		{
			if (_pageCountChanged != null)
			{
				_pageCountChanged(this, EventArgs.Empty);
			}
			_raisePageCountChanged = false;
		}
		if (_raisePageNumberChanged)
		{
			if (_pageNumberChanged != null)
			{
				_pageNumberChanged(this, EventArgs.Empty);
			}
			_raisePageNumberChanged = false;
		}
		return null;
	}

	void IFlowDocumentViewer.PreviousPage()
	{
		OnPreviousPageCommand();
	}

	void IFlowDocumentViewer.NextPage()
	{
		OnNextPageCommand();
	}

	void IFlowDocumentViewer.FirstPage()
	{
		OnFirstPageCommand();
	}

	void IFlowDocumentViewer.LastPage()
	{
		OnLastPageCommand();
	}

	void IFlowDocumentViewer.Print()
	{
		OnPrintCommand();
	}

	void IFlowDocumentViewer.CancelPrint()
	{
		OnCancelPrintCommand();
	}

	void IFlowDocumentViewer.ShowFindResult(ITextRange findResult)
	{
		if (findResult.Start is ContentPosition)
		{
			BringContentPositionIntoView((ContentPosition)findResult.Start);
		}
	}

	bool IFlowDocumentViewer.CanGoToPage(int pageNumber)
	{
		return CanGoToPage(pageNumber);
	}

	void IFlowDocumentViewer.GoToPage(int pageNumber)
	{
		OnGoToPageCommand(pageNumber);
	}

	void IFlowDocumentViewer.SetDocument(FlowDocument document)
	{
		base.Document = document;
	}
}
