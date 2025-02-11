using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Documents;

/// <summary>Provides collection support for a collection of document pages. </summary>
public sealed class PageContentCollection : IEnumerable<PageContent>, IEnumerable
{
	private FixedDocument _logicalParent;

	private List<PageContent> _internalList;

	/// <summary>Gets the <see cref="T:System.Windows.Documents.PageContent" /> element at the specified index within the collection. </summary>
	/// <returns>The page content element at the specified index within the collection. </returns>
	/// <param name="pageIndex">The zero-based index of the page to get. </param>
	public PageContent this[int pageIndex] => InternalList[pageIndex];

	/// <summary>Gets the number of elements contained in the page collection.</summary>
	/// <returns>The number of elements in the collection.</returns>
	public int Count => InternalList.Count;

	private IList<PageContent> InternalList => _internalList;

	internal PageContentCollection(FixedDocument logicalParent)
	{
		_logicalParent = logicalParent;
		_internalList = new List<PageContent>();
	}

	/// <summary>Adds a new page to the page collection.</summary>
	/// <returns>The zero-based index within the collection where the page was added.</returns>
	/// <param name="newPageContent">The new page to add to the collection. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="newPageContent" /> was passed as null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The page passed as <paramref name="newPageContent" /> already existed in the collection.</exception>
	public int Add(PageContent newPageContent)
	{
		if (newPageContent == null)
		{
			throw new ArgumentNullException("newPageContent");
		}
		_logicalParent.AddLogicalChild(newPageContent);
		InternalList.Add(newPageContent);
		int num = InternalList.Count - 1;
		_logicalParent.OnPageContentAppended(num);
		return num;
	}

	/// <summary>Returns an enumerator for iterating through the page collection. </summary>
	/// <returns>An enumerator that can be used to iterate through the collection.</returns>
	public IEnumerator<PageContent> GetEnumerator()
	{
		return InternalList.GetEnumerator();
	}

	/// <summary>This member supports the Microsoft .NET Framework infrastructure and is not intended to be used directly from your code.  Use the type-safe <see cref="M:System.Windows.Documents.PageContentCollection.GetEnumerator" /> method instead. </summary>
	/// <returns>The enumerator.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<PageContent>)this).GetEnumerator();
	}

	internal int IndexOf(PageContent pc)
	{
		return InternalList.IndexOf(pc);
	}
}
