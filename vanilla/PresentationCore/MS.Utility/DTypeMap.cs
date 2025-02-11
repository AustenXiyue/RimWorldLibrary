using System.Collections;
using System.Windows;
using MS.Internal.PresentationCore;

namespace MS.Utility;

[FriendAccessAllowed]
internal class DTypeMap
{
	private int _entryCount;

	private object[] _entries;

	private Hashtable _overFlow;

	private ItemStructList<DependencyObjectType> _activeDTypes;

	public object this[DependencyObjectType dType]
	{
		get
		{
			if (dType.Id < _entryCount)
			{
				return _entries[dType.Id];
			}
			if (_overFlow != null)
			{
				return _overFlow[dType];
			}
			return null;
		}
		set
		{
			if (dType.Id < _entryCount)
			{
				_entries[dType.Id] = value;
			}
			else
			{
				if (_overFlow == null)
				{
					_overFlow = new Hashtable();
				}
				_overFlow[dType] = value;
			}
			_activeDTypes.Add(dType);
		}
	}

	public ItemStructList<DependencyObjectType> ActiveDTypes => _activeDTypes;

	public DTypeMap(int entryCount)
	{
		_entryCount = entryCount;
		_entries = new object[_entryCount];
		_activeDTypes = new ItemStructList<DependencyObjectType>(128);
	}

	public void Clear()
	{
		for (int i = 0; i < _entryCount; i++)
		{
			_entries[i] = null;
		}
		for (int j = 0; j < _activeDTypes.Count; j++)
		{
			_activeDTypes.List[j] = null;
		}
		if (_overFlow != null)
		{
			_overFlow.Clear();
		}
	}
}
