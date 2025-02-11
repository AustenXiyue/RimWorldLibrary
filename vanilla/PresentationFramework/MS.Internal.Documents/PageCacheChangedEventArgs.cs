using System;
using System.Collections.Generic;

namespace MS.Internal.Documents;

internal class PageCacheChangedEventArgs : EventArgs
{
	private readonly List<PageCacheChange> _changes;

	public List<PageCacheChange> Changes => _changes;

	public PageCacheChangedEventArgs(List<PageCacheChange> changes)
	{
		_changes = changes;
	}
}
