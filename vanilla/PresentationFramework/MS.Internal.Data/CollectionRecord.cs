using System;

namespace MS.Internal.Data;

internal class CollectionRecord
{
	public SynchronizationInfo SynchronizationInfo;

	private WeakReference _wrViewTable = ViewManager.NullWeakRef;

	public ViewTable ViewTable
	{
		get
		{
			return (ViewTable)_wrViewTable.Target;
		}
		set
		{
			_wrViewTable = new WeakReference(value);
		}
	}

	public bool IsAlive
	{
		get
		{
			if (!SynchronizationInfo.IsAlive)
			{
				return _wrViewTable.IsAlive;
			}
			return true;
		}
	}
}
