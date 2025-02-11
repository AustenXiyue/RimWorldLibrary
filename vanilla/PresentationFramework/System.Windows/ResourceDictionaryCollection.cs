using System.Collections.ObjectModel;

namespace System.Windows;

internal class ResourceDictionaryCollection : ObservableCollection<ResourceDictionary>
{
	private ResourceDictionary _owner;

	internal ResourceDictionaryCollection(ResourceDictionary owner)
	{
		_owner = owner;
	}

	protected override void ClearItems()
	{
		for (int i = 0; i < base.Count; i++)
		{
			_owner.RemoveParentOwners(base[i]);
		}
		base.ClearItems();
	}

	protected override void InsertItem(int index, ResourceDictionary item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		base.InsertItem(index, item);
	}

	protected override void SetItem(int index, ResourceDictionary item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		base.SetItem(index, item);
	}
}
