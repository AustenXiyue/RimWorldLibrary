using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;

namespace System.Windows.Controls;

internal class SelectedItemCollection : ObservableCollection<object>
{
	private class Changer : IDisposable
	{
		private SelectedItemCollection _owner;

		public Changer(SelectedItemCollection owner)
		{
			_owner = owner;
		}

		public void Dispose()
		{
			_owner.FinishChange();
		}
	}

	private int _changeCount;

	private Changer _changer;

	private Selector _selector;

	private bool _updatingSelectedItems;

	internal bool IsChanging => _changeCount > 0;

	internal bool IsUpdatingSelectedItems
	{
		get
		{
			if (!_selector.SelectionChange.IsActive)
			{
				return _updatingSelectedItems;
			}
			return true;
		}
	}

	public SelectedItemCollection(Selector selector)
	{
		_selector = selector;
		_changer = new Changer(this);
	}

	protected override void ClearItems()
	{
		if (_updatingSelectedItems)
		{
			foreach (ItemsControl.ItemInfo item in (IEnumerable<ItemsControl.ItemInfo>)_selector._selectedItems)
			{
				_selector.SelectionChange.Unselect(item);
			}
			return;
		}
		using (ChangeSelectedItems())
		{
			base.ClearItems();
		}
	}

	protected override void RemoveItem(int index)
	{
		if (_updatingSelectedItems)
		{
			_selector.SelectionChange.Unselect(_selector.NewItemInfo(base[index]));
			return;
		}
		using (ChangeSelectedItems())
		{
			base.RemoveItem(index);
		}
	}

	protected override void InsertItem(int index, object item)
	{
		if (_updatingSelectedItems)
		{
			if (index == base.Count)
			{
				_selector.SelectionChange.Select(_selector.NewItemInfo(item), assumeInItemsCollection: true);
				return;
			}
			throw new InvalidOperationException(SR.InsertInDeferSelectionActive);
		}
		using (ChangeSelectedItems())
		{
			base.InsertItem(index, item);
		}
	}

	protected override void SetItem(int index, object item)
	{
		if (_updatingSelectedItems)
		{
			throw new InvalidOperationException(SR.SetInDeferSelectionActive);
		}
		using (ChangeSelectedItems())
		{
			base.SetItem(index, item);
		}
	}

	protected override void MoveItem(int oldIndex, int newIndex)
	{
		if (oldIndex != newIndex)
		{
			if (_updatingSelectedItems)
			{
				throw new InvalidOperationException(SR.MoveInDeferSelectionActive);
			}
			using (ChangeSelectedItems())
			{
				base.MoveItem(oldIndex, newIndex);
			}
		}
	}

	private IDisposable ChangeSelectedItems()
	{
		_changeCount++;
		return _changer;
	}

	private void FinishChange()
	{
		if (--_changeCount == 0)
		{
			_selector.FinishSelectedItemsChange();
		}
	}

	internal void BeginUpdateSelectedItems()
	{
		if (_selector.SelectionChange.IsActive || _updatingSelectedItems)
		{
			throw new InvalidOperationException(SR.DeferSelectionActive);
		}
		_updatingSelectedItems = true;
		_selector.SelectionChange.Begin();
	}

	internal void EndUpdateSelectedItems()
	{
		if (!_selector.SelectionChange.IsActive || !_updatingSelectedItems)
		{
			throw new InvalidOperationException(SR.DeferSelectionNotActive);
		}
		_updatingSelectedItems = false;
		_selector.SelectionChange.End();
	}

	internal void Add(ItemsControl.ItemInfo info)
	{
		if (!_selector.SelectionChange.IsActive || !_updatingSelectedItems)
		{
			throw new InvalidOperationException(SR.DeferSelectionNotActive);
		}
		_selector.SelectionChange.Select(info, assumeInItemsCollection: true);
	}

	internal void Remove(ItemsControl.ItemInfo info)
	{
		if (!_selector.SelectionChange.IsActive || !_updatingSelectedItems)
		{
			throw new InvalidOperationException(SR.DeferSelectionNotActive);
		}
		_selector.SelectionChange.Unselect(info);
	}
}
