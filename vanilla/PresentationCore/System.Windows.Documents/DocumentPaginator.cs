using System.ComponentModel;
using MS.Internal.PresentationCore;

namespace System.Windows.Documents;

/// <summary>Provides an abstract base class that supports creation of multiple-page elements from a single document.</summary>
public abstract class DocumentPaginator
{
	/// <summary>When overridden in a derived class, gets a value indicating whether <see cref="P:System.Windows.Documents.DocumentPaginator.PageCount" /> is the total number of pages. </summary>
	/// <returns>true if pagination is complete and <see cref="P:System.Windows.Documents.DocumentPaginator.PageCount" /> is the total number of pages; otherwise, false, if pagination is in process and <see cref="P:System.Windows.Documents.DocumentPaginator.PageCount" /> is the number of pages currently formatted (not the total).This value may revert to false, after being true, if <see cref="P:System.Windows.Documents.DocumentPaginator.PageSize" /> or content changes; because those events would force a repagination.</returns>
	public abstract bool IsPageCountValid { get; }

	/// <summary>When overridden in a derived class, gets a count of the number of pages currently formatted</summary>
	/// <returns>A count of the number of pages that have been formatted.</returns>
	public abstract int PageCount { get; }

	/// <summary>When overridden in a derived class, gets or sets the suggested width and height of each page.</summary>
	/// <returns>A <see cref="T:System.Windows.Size" /> representing the width and height of each page.</returns>
	public abstract Size PageSize { get; set; }

	/// <summary>When overridden in a derived class, returns the element being paginated.</summary>
	/// <returns>An <see cref="T:System.Windows.Documents.IDocumentPaginatorSource" /> representing the element being paginated.</returns>
	public abstract IDocumentPaginatorSource Source { get; }

	/// <summary>Occurs when <see cref="Overload:System.Windows.Documents.DocumentPaginator.GetPageAsync" /> has completed.</summary>
	public event GetPageCompletedEventHandler GetPageCompleted;

	/// <summary>Occurs when a <see cref="Overload:System.Windows.Documents.DocumentPaginator.ComputePageCountAsync" /> operation has finished. </summary>
	public event AsyncCompletedEventHandler ComputePageCountCompleted;

	/// <summary>Occurs when the document content is changed.</summary>
	public event PagesChangedEventHandler PagesChanged;

	/// <summary>When overridden in a derived class, gets the <see cref="T:System.Windows.Documents.DocumentPage" /> for the specified page number.</summary>
	/// <returns>The <see cref="T:System.Windows.Documents.DocumentPage" /> for the specified <paramref name="pageNumber" />, or <see cref="F:System.Windows.Documents.DocumentPage.Missing" /> if the page does not exist.</returns>
	/// <param name="pageNumber">The zero-based page number of the document page that is needed.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pageNumber" /> is negative.</exception>
	public abstract DocumentPage GetPage(int pageNumber);

	/// <summary>Asynchronously returns (through the <see cref="E:System.Windows.Documents.DocumentPaginator.GetPageCompleted" /> event) the <see cref="T:System.Windows.Documents.DocumentPage" /> for the specified page number.</summary>
	/// <param name="pageNumber">The zero-based page number of the document page that is needed.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pageNumber" /> is negative.</exception>
	public virtual void GetPageAsync(int pageNumber)
	{
		GetPageAsync(pageNumber, null);
	}

	/// <summary>Asynchronously returns (through the <see cref="E:System.Windows.Documents.DocumentPaginator.GetPageCompleted" /> event) the <see cref="T:System.Windows.Documents.DocumentPage" /> for the specified page number and assigns the specified ID to the asynchronous task.</summary>
	/// <param name="pageNumber">The zero-based page number of the <see cref="T:System.Windows.Documents.DocumentPage" /> to get.</param>
	/// <param name="userState">A unique identifier for the asynchronous task.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pageNumber" /> is negative.</exception>
	public virtual void GetPageAsync(int pageNumber, object userState)
	{
		if (pageNumber < 0)
		{
			throw new ArgumentOutOfRangeException("pageNumber", SR.PaginatorNegativePageNumber);
		}
		DocumentPage page = GetPage(pageNumber);
		OnGetPageCompleted(new GetPageCompletedEventArgs(page, pageNumber, null, cancelled: false, userState));
	}

	/// <summary>Forces a pagination of the content, updates <see cref="P:System.Windows.Documents.DocumentPaginator.PageCount" /> with the new total, and sets <see cref="P:System.Windows.Documents.DocumentPaginator.IsPageCountValid" /> to true.</summary>
	public virtual void ComputePageCount()
	{
		GetPage(int.MaxValue);
	}

	/// <summary>Asynchronously, forces a pagination of the content, updates <see cref="P:System.Windows.Documents.DocumentPaginator.PageCount" /> with the new total, and sets <see cref="P:System.Windows.Documents.DocumentPaginator.IsPageCountValid" /> to true.</summary>
	public virtual void ComputePageCountAsync()
	{
		ComputePageCountAsync(null);
	}

	/// <summary>Asynchronously, forces a pagination of the content, updates <see cref="P:System.Windows.Documents.DocumentPaginator.PageCount" /> with the new total, sets <see cref="P:System.Windows.Documents.DocumentPaginator.IsPageCountValid" /> to true. </summary>
	/// <param name="userState">A unique identifier for the asynchronous task.</param>
	public virtual void ComputePageCountAsync(object userState)
	{
		ComputePageCount();
		OnComputePageCountCompleted(new AsyncCompletedEventArgs(null, cancelled: false, userState));
	}

	/// <summary>Cancels a previous <see cref="Overload:System.Windows.Documents.DocumentPaginator.GetPageAsync" /> or <see cref="Overload:System.Windows.Documents.DynamicDocumentPaginator.GetPageNumberAsync" /> operation.</summary>
	/// <param name="userState">The original <paramref name="userState" /> passed to <see cref="Overload:System.Windows.Documents.DocumentPaginator.GetPageAsync" />, <see cref="Overload:System.Windows.Documents.DynamicDocumentPaginator.GetPageNumberAsync" />, or <see cref="Overload:System.Windows.Documents.DocumentPaginator.ComputePageCountAsync" /> that identifies the asynchronous task to cancel.</param>
	public virtual void CancelAsync(object userState)
	{
	}

	/// <summary>Raises the <see cref="E:System.Windows.Documents.DocumentPaginator.GetPageCompleted" /> event. </summary>
	/// <param name="e">A <see cref="T:System.Windows.Documents.GetPageCompletedEventArgs" /> that contains the event data. </param>
	protected virtual void OnGetPageCompleted(GetPageCompletedEventArgs e)
	{
		if (this.GetPageCompleted != null)
		{
			this.GetPageCompleted(this, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Documents.DocumentPaginator.ComputePageCountCompleted" /> event. </summary>
	/// <param name="e">An <see cref="T:System.ComponentModel.AsyncCompletedEventArgs" /> that contains the event data. </param>
	protected virtual void OnComputePageCountCompleted(AsyncCompletedEventArgs e)
	{
		if (this.ComputePageCountCompleted != null)
		{
			this.ComputePageCountCompleted(this, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Documents.DocumentPaginator.PagesChanged" /> event. </summary>
	/// <param name="e">A <see cref="T:System.Windows.Documents.PagesChangedEventArgs" /> that contains the event data. </param>
	protected virtual void OnPagesChanged(PagesChangedEventArgs e)
	{
		if (this.PagesChanged != null)
		{
			this.PagesChanged(this, e);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.DocumentPaginator" /> class. </summary>
	protected DocumentPaginator()
	{
	}
}
