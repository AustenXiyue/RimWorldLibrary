using System;
using System.Windows;
using System.Windows.Documents;

namespace MS.Internal.Documents;

internal class FixedDocumentSequencePaginator : DynamicDocumentPaginator, IServiceProvider
{
	private readonly FixedDocumentSequence _document;

	public override bool IsPageCountValid => _document.IsPageCountValid;

	public override int PageCount => _document.PageCount;

	public override Size PageSize
	{
		get
		{
			return _document.PageSize;
		}
		set
		{
			_document.PageSize = value;
		}
	}

	public override IDocumentPaginatorSource Source => _document;

	internal FixedDocumentSequencePaginator(FixedDocumentSequence document)
	{
		_document = document;
	}

	public override DocumentPage GetPage(int pageNumber)
	{
		return _document.GetPage(pageNumber);
	}

	public override void GetPageAsync(int pageNumber, object userState)
	{
		_document.GetPageAsync(pageNumber, userState);
	}

	public override void CancelAsync(object userState)
	{
		_document.CancelAsync(userState);
	}

	public override int GetPageNumber(ContentPosition contentPosition)
	{
		return _document.GetPageNumber(contentPosition);
	}

	public override ContentPosition GetPagePosition(DocumentPage page)
	{
		return _document.GetPagePosition(page);
	}

	public override ContentPosition GetObjectPosition(object o)
	{
		return _document.GetObjectPosition(o);
	}

	internal void NotifyGetPageCompleted(GetPageCompletedEventArgs e)
	{
		OnGetPageCompleted(e);
	}

	internal void NotifyPaginationCompleted(EventArgs e)
	{
		OnPaginationCompleted(e);
	}

	internal void NotifyPaginationProgress(PaginationProgressEventArgs e)
	{
		OnPaginationProgress(e);
	}

	internal void NotifyPagesChanged(PagesChangedEventArgs e)
	{
		OnPagesChanged(e);
	}

	object IServiceProvider.GetService(Type serviceType)
	{
		return ((IServiceProvider)_document).GetService(serviceType);
	}
}
