using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;

namespace MS.Internal.Documents;

internal class ReaderScrollViewer : FlowDocumentScrollViewer, IFlowDocumentViewer
{
	private EventHandler _pageNumberChanged;

	private EventHandler _pageCountChanged;

	private EventHandler _printCompleted;

	private EventHandler _printStarted;

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

	bool IFlowDocumentViewer.CanGoToPreviousPage => false;

	bool IFlowDocumentViewer.CanGoToNextPage => false;

	int IFlowDocumentViewer.PageNumber => (base.Document != null) ? 1 : 0;

	int IFlowDocumentViewer.PageCount => (base.Document != null) ? 1 : 0;

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
		if (e.Property == FlowDocumentScrollViewer.DocumentProperty)
		{
			if (_pageNumberChanged != null)
			{
				_pageNumberChanged(this, EventArgs.Empty);
			}
			if (_pageCountChanged != null)
			{
				_pageCountChanged(this, EventArgs.Empty);
			}
		}
	}

	private bool IsValidTextSelectionForDocument(ITextSelection textSelection, FlowDocument flowDocument)
	{
		if (textSelection.Start != null && textSelection.Start.TextContainer == flowDocument.StructuralCache.TextContainer)
		{
			return true;
		}
		return false;
	}

	private object SetTextSelection(object arg)
	{
		if (arg is ITextSelection textSelection && base.Document != null && IsValidTextSelectionForDocument(textSelection, base.Document))
		{
			ITextSelection textSelection2 = base.Document.StructuralCache.TextContainer.TextSelection;
			if (textSelection2 != null)
			{
				textSelection2.SetCaretToPosition(textSelection.AnchorPosition, textSelection.MovingPosition.LogicalDirection, allowStopAtLineEnd: true, allowStopNearSpace: true);
				textSelection2.ExtendToPosition(textSelection.MovingPosition);
			}
		}
		return null;
	}

	void IFlowDocumentViewer.PreviousPage()
	{
		if (base.ScrollViewer != null)
		{
			base.ScrollViewer.PageUp();
		}
	}

	void IFlowDocumentViewer.NextPage()
	{
		if (base.ScrollViewer != null)
		{
			base.ScrollViewer.PageDown();
		}
	}

	void IFlowDocumentViewer.FirstPage()
	{
		if (base.ScrollViewer != null)
		{
			base.ScrollViewer.ScrollToHome();
		}
	}

	void IFlowDocumentViewer.LastPage()
	{
		if (base.ScrollViewer != null)
		{
			base.ScrollViewer.ScrollToEnd();
		}
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
	}

	bool IFlowDocumentViewer.CanGoToPage(int pageNumber)
	{
		return pageNumber == 1;
	}

	void IFlowDocumentViewer.GoToPage(int pageNumber)
	{
		if (pageNumber == 1 && base.ScrollViewer != null)
		{
			base.ScrollViewer.ScrollToHome();
		}
	}

	void IFlowDocumentViewer.SetDocument(FlowDocument document)
	{
		base.Document = document;
	}
}
