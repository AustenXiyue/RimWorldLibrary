namespace System.Windows.Documents;

/// <summary>Provides data for the <see cref="E:System.Windows.Documents.DocumentPaginator.PagesChanged" /> event. </summary>
public class PagesChangedEventArgs : EventArgs
{
	private readonly int _start;

	private readonly int _count;

	/// <summary> Gets the page number of the first page that changed. </summary>
	/// <returns>The page number (zero-based) of first page that changed.</returns>
	public int Start => _start;

	/// <summary> Gets the number of continuous pages that changed. </summary>
	/// <returns>The number of continuous pages that changed.</returns>
	public int Count => _count;

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Documents.PagesChangedEventArgs" /> class. </summary>
	/// <param name="start">The page number (zero-based) of first page that changed.</param>
	/// <param name="count">The number of continuous pages that changed.</param>
	public PagesChangedEventArgs(int start, int count)
	{
		_start = start;
		_count = count;
	}
}
