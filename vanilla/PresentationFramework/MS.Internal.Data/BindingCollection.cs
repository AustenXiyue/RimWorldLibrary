using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;

namespace MS.Internal.Data;

internal class BindingCollection : Collection<BindingBase>
{
	private BindingBase _owner;

	private BindingCollectionChangedCallback _collectionChangedCallback;

	internal BindingCollection(BindingBase owner, BindingCollectionChangedCallback callback)
	{
		Invariant.Assert(owner != null && callback != null);
		_owner = owner;
		_collectionChangedCallback = callback;
	}

	private BindingCollection()
	{
	}

	protected override void ClearItems()
	{
		_owner.CheckSealed();
		base.ClearItems();
		OnBindingCollectionChanged();
	}

	protected override void RemoveItem(int index)
	{
		_owner.CheckSealed();
		base.RemoveItem(index);
		OnBindingCollectionChanged();
	}

	protected override void InsertItem(int index, BindingBase item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		ValidateItem(item);
		_owner.CheckSealed();
		base.InsertItem(index, item);
		OnBindingCollectionChanged();
	}

	protected override void SetItem(int index, BindingBase item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		ValidateItem(item);
		_owner.CheckSealed();
		base.SetItem(index, item);
		OnBindingCollectionChanged();
	}

	private void ValidateItem(BindingBase binding)
	{
		if (!(binding is Binding))
		{
			throw new NotSupportedException(SR.Format(SR.BindingCollectionContainsNonBinding, binding.GetType().Name));
		}
	}

	private void OnBindingCollectionChanged()
	{
		if (_collectionChangedCallback != null)
		{
			_collectionChangedCallback();
		}
	}
}
