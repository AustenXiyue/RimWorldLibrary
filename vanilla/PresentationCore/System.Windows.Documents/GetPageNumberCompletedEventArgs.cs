using System.ComponentModel;

namespace System.Windows.Documents;

/// <summary>Provides data for the <see cref="E:System.Windows.Documents.DynamicDocumentPaginator.GetPageNumberCompleted" /> event.</summary>
public class GetPageNumberCompletedEventArgs : AsyncCompletedEventArgs
{
	private readonly ContentPosition _contentPosition;

	private readonly int _pageNumber;

	/// <summary>Gets the <see cref="T:System.Windows.Documents.ContentPosition" /> passed to <see cref="M:System.Windows.Documents.DynamicDocumentPaginator.GetPageNumberAsync(System.Windows.Documents.ContentPosition)" />.</summary>
	/// <returns>The content position passed to <see cref="M:System.Windows.Documents.DynamicDocumentPaginator.GetPageNumberAsync(System.Windows.Documents.ContentPosition)" />.</returns>
	public ContentPosition ContentPosition
	{
		get
		{
			RaiseExceptionIfNecessary();
			return _contentPosition;
		}
	}

	/// <summary>Gets the page number for the <see cref="T:System.Windows.Documents.ContentPosition" /> passed to <see cref="M:System.Windows.Documents.DynamicDocumentPaginator.GetPageNumberAsync(System.Windows.Documents.ContentPosition)" />.</summary>
	/// <returns>The page number (zero-based) for the <see cref="T:System.Windows.Documents.ContentPosition" /> passed to <see cref="M:System.Windows.Documents.DynamicDocumentPaginator.GetPageNumberAsync(System.Windows.Documents.ContentPosition)" />.</returns>
	public int PageNumber
	{
		get
		{
			RaiseExceptionIfNecessary();
			return _pageNumber;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.GetPageNumberCompletedEventArgs" /> class.</summary>
	/// <param name="contentPosition">The <paramref name="contentPosition" /> parameter passed to <see cref="M:System.Windows.Documents.DynamicDocumentPaginator.GetPageNumberAsync(System.Windows.Documents.ContentPosition)" />.</param>
	/// <param name="pageNumber">The page number where the <paramref name="contentPosition" /> appears.</param>
	/// <param name="error">The exception that occurred during the asynchronous operation; or NULL if there were no errors.</param>
	/// <param name="cancelled">true if the asynchronous operation was canceled; otherwise, false.</param>
	/// <param name="userState">The unique <paramref name="userState" /> parameter passed to <see cref="M:System.Windows.Documents.DynamicDocumentPaginator.GetPageNumberAsync(System.Windows.Documents.ContentPosition)" />.</param>
	public GetPageNumberCompletedEventArgs(ContentPosition contentPosition, int pageNumber, Exception error, bool cancelled, object userState)
		: base(error, cancelled, userState)
	{
		_contentPosition = contentPosition;
		_pageNumber = pageNumber;
	}
}
