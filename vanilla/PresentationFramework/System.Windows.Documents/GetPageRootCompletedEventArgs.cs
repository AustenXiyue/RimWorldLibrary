using System.ComponentModel;

namespace System.Windows.Documents;

/// <summary> Provides data for the <see cref="E:System.Windows.Documents.PageContent.GetPageRootCompleted" /> event. </summary>
public sealed class GetPageRootCompletedEventArgs : AsyncCompletedEventArgs
{
	private FixedPage _page;

	/// <summary> Gets the <see cref="T:System.Windows.Documents.FixedPage" /> content asynchronously requested by <see cref="M:System.Windows.Documents.PageContent.GetPageRootAsync(System.Boolean)" />. </summary>
	/// <returns>The root element of the visual tree for the <see cref="T:System.Windows.Documents.PageContent" /> requested by <see cref="M:System.Windows.Documents.PageContent.GetPageRootAsync(System.Boolean)" />.</returns>
	public FixedPage Result
	{
		get
		{
			RaiseExceptionIfNecessary();
			return _page;
		}
	}

	internal GetPageRootCompletedEventArgs(FixedPage page, Exception error, bool cancelled, object userToken)
		: base(error, cancelled, userToken)
	{
		_page = page;
	}
}
