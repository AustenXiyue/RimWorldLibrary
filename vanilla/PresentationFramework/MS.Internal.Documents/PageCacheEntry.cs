using System.Windows;

namespace MS.Internal.Documents;

internal struct PageCacheEntry
{
	public Size PageSize;

	public bool Dirty;
}
