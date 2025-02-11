using System;
using System.Collections;
using System.Windows.Documents;

namespace MS.Internal.Documents;

internal class PageDestroyedWatcher
{
	private Hashtable _table;

	public PageDestroyedWatcher()
	{
		_table = new Hashtable(16);
	}

	public void AddPage(DocumentPage page)
	{
		if (!_table.Contains(page))
		{
			_table.Add(page, false);
			page.PageDestroyed += OnPageDestroyed;
		}
		else
		{
			_table[page] = false;
		}
	}

	public void RemovePage(DocumentPage page)
	{
		if (_table.Contains(page))
		{
			_table.Remove(page);
			page.PageDestroyed -= OnPageDestroyed;
		}
	}

	public bool IsDestroyed(DocumentPage page)
	{
		if (_table.Contains(page))
		{
			return (bool)_table[page];
		}
		return true;
	}

	private void OnPageDestroyed(object sender, EventArgs e)
	{
		DocumentPage documentPage = sender as DocumentPage;
		Invariant.Assert(documentPage != null, "Invalid type in PageDestroyedWatcher");
		if (_table.Contains(documentPage))
		{
			_table[documentPage] = true;
		}
	}
}
