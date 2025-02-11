namespace System.Windows.Documents;

/// <summary> Provides data for the <see cref="E:System.Windows.Documents.DynamicDocumentPaginator.PaginationProgress" /> event. </summary>
public class PaginationProgressEventArgs : EventArgs
{
	private readonly int _start;

	private readonly int _count;

	/// <summary> Gets the page number of the first page that was paginated. </summary>
	/// <returns>The page number (zero-based) of first page that was paginated.</returns>
	public int Start => _start;

	/// <summary> Gets the number of continuous pages that were paginated. </summary>
	/// <returns>The number of continuous pages that were paginated.</returns>
	public int Count => _count;

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Documents.PaginationProgressEventArgs" /> class. </summary>
	/// <param name="start">The page number (zero-based) of first page paginated.</param>
	/// <param name="count">The number of continuous pages paginated.</param>
	public PaginationProgressEventArgs(int start, int count)
	{
		_start = start;
		_count = count;
	}
}
