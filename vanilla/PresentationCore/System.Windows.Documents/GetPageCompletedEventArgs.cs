using System.ComponentModel;

namespace System.Windows.Documents;

/// <summary>Provides data for the <see cref="E:System.Windows.Documents.DocumentPaginator.GetPageCompleted" /> event. </summary>
public class GetPageCompletedEventArgs : AsyncCompletedEventArgs
{
	private readonly DocumentPage _page;

	private readonly int _pageNumber;

	/// <summary>Gets the <see cref="T:System.Windows.Documents.DocumentPage" /> for the page specified in the call to <see cref="M:System.Windows.Documents.DocumentPaginator.GetPageAsync(System.Int32,System.Object)" />. </summary>
	/// <returns>The document page for the page specified in the call to <see cref="M:System.Windows.Documents.DocumentPaginator.GetPageAsync(System.Int32,System.Object)" />.</returns>
	public DocumentPage DocumentPage
	{
		get
		{
			RaiseExceptionIfNecessary();
			return _page;
		}
	}

	/// <summary>Gets the page number passed to <see cref="M:System.Windows.Documents.DocumentPaginator.GetPageAsync(System.Int32,System.Object)" />. </summary>
	/// <returns>The page number passed to <see cref="M:System.Windows.Documents.DocumentPaginator.GetPageAsync(System.Int32,System.Object)" />.</returns>
	public int PageNumber
	{
		get
		{
			RaiseExceptionIfNecessary();
			return _pageNumber;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.GetPageCompletedEventArgs" /> class. </summary>
	/// <param name="page">The <see cref="T:System.Windows.Documents.DocumentPage" /> for the requested <paramref name="pageNumber" />.</param>
	/// <param name="pageNumber">The <paramref name="pageNumber" /> parameter passed to <see cref="M:System.Windows.Documents.DocumentPaginator.GetPageAsync(System.Int32,System.Object)" />.</param>
	/// <param name="error">The exception that occurred during the asynchronous operation; or NULL if there were no errors.</param>
	/// <param name="cancelled">true if the asynchronous operation was canceled; otherwise, false.</param>
	/// <param name="userState">The unique <paramref name="userState" /> parameter passed to <see cref="M:System.Windows.Documents.DocumentPaginator.GetPageAsync(System.Int32,System.Object)" />.</param>
	public GetPageCompletedEventArgs(DocumentPage page, int pageNumber, Exception error, bool cancelled, object userState)
		: base(error, cancelled, userState)
	{
		_page = page;
		_pageNumber = pageNumber;
	}
}
