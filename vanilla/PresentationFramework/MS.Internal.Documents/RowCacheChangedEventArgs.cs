using System;
using System.Collections.Generic;

namespace MS.Internal.Documents;

internal class RowCacheChangedEventArgs : EventArgs
{
	private readonly List<RowCacheChange> _changes;

	public List<RowCacheChange> Changes => _changes;

	public RowCacheChangedEventArgs(List<RowCacheChange> changes)
	{
		_changes = changes;
	}
}
