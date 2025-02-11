using MS.Internal.PresentationCore;

namespace System.Windows.Documents;

/// <summary>Provides an abstract base class that supports automatic background pagination and tracking content positions across repaginations in addition to the methods and properties of its own base class.</summary>
public abstract class DynamicDocumentPaginator : DocumentPaginator
{
	/// <summary>Gets or sets a value indicating whether pagination is performed automatically in the background in response to certain events, such as a change in page size.</summary>
	/// <returns>true if background pagination is enabled; otherwise, false. The default is true.</returns>
	public virtual bool IsBackgroundPaginationEnabled
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	/// <summary>Occurs when <see cref="Overload:System.Windows.Documents.DynamicDocumentPaginator.GetPageNumberAsync" /> has completed.</summary>
	public event GetPageNumberCompletedEventHandler GetPageNumberCompleted;

	/// <summary>Occurs when all document content has been paginated.</summary>
	public event EventHandler PaginationCompleted;

	/// <summary>Occurs when one or more content pages have been paginated.</summary>
	public event PaginationProgressEventHandler PaginationProgress;

	/// <summary>When overridden in a derived class, returns the zero-based page number of the specified <see cref="T:System.Windows.Documents.ContentPosition" />.</summary>
	/// <returns>An <see cref="T:System.Int32" /> representing zero-based page number where the specified <paramref name="contentPosition" /> appears.</returns>
	/// <param name="contentPosition">The content position whose page number is needed.</param>
	public abstract int GetPageNumber(ContentPosition contentPosition);

	/// <summary>Asynchronously, returns (through the This method raises the <see cref="E:System.Windows.Documents.DynamicDocumentPaginator.GetPageNumberCompleted" /> event) the zero-based page number of the specified <see cref="T:System.Windows.Documents.ContentPosition" />.</summary>
	/// <param name="contentPosition">The content position whose page number is needed.</param>
	public virtual void GetPageNumberAsync(ContentPosition contentPosition)
	{
		GetPageNumberAsync(contentPosition, null);
	}

	/// <summary>Asynchronously, returns (through the This method raises the <see cref="E:System.Windows.Documents.DynamicDocumentPaginator.GetPageNumberCompleted" /> event) the zero-based page number of the specified <see cref="T:System.Windows.Documents.ContentPosition" />.</summary>
	/// <param name="contentPosition">The content position element to return the page number of.</param>
	/// <param name="userState">A unique identifier for the asynchronous task.</param>
	public virtual void GetPageNumberAsync(ContentPosition contentPosition, object userState)
	{
		if (contentPosition == null)
		{
			throw new ArgumentNullException("contentPosition");
		}
		if (contentPosition == ContentPosition.Missing)
		{
			throw new ArgumentException(SR.PaginatorMissingContentPosition, "contentPosition");
		}
		int pageNumber = GetPageNumber(contentPosition);
		OnGetPageNumberCompleted(new GetPageNumberCompletedEventArgs(contentPosition, pageNumber, null, cancelled: false, userState));
	}

	/// <summary>When overridden in a derived class, gets the position of the specified page in the document's content.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.ContentPosition" /> representing the position of <paramref name="page" />. </returns>
	/// <param name="page">The page whose position is needed.</param>
	public abstract ContentPosition GetPagePosition(DocumentPage page);

	/// <summary>When overridden in a derived class, returns a <see cref="T:System.Windows.Documents.ContentPosition" /> for the specified <see cref="T:System.Object" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Documents.ContentPosition" /> of the given object.</returns>
	/// <param name="value">The object to return the <see cref="T:System.Windows.Documents.ContentPosition" /> of.</param>
	public abstract ContentPosition GetObjectPosition(object value);

	/// <summary>Raises the <see cref="E:System.Windows.Documents.DynamicDocumentPaginator.GetPageNumberCompleted" /> event. </summary>
	/// <param name="e">A <see cref="T:System.Windows.Documents.GetPageNumberCompletedEventArgs" /> that contains the event data. </param>
	protected virtual void OnGetPageNumberCompleted(GetPageNumberCompletedEventArgs e)
	{
		if (this.GetPageNumberCompleted != null)
		{
			this.GetPageNumberCompleted(this, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Documents.DynamicDocumentPaginator.PaginationProgress" /> event. </summary>
	/// <param name="e">A <see cref="T:System.Windows.Documents.PaginationProgressEventArgs" /> that contains the event data. </param>
	protected virtual void OnPaginationProgress(PaginationProgressEventArgs e)
	{
		if (this.PaginationProgress != null)
		{
			this.PaginationProgress(this, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Documents.DynamicDocumentPaginator.PaginationCompleted" /> event. </summary>
	/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data. </param>
	protected virtual void OnPaginationCompleted(EventArgs e)
	{
		if (this.PaginationCompleted != null)
		{
			this.PaginationCompleted(this, e);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.DynamicDocumentPaginator" /> class. </summary>
	protected DynamicDocumentPaginator()
	{
	}
}
