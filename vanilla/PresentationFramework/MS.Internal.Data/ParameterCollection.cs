using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;

namespace MS.Internal.Data;

internal class ParameterCollection : Collection<object>, IList, ICollection, IEnumerable
{
	private bool _isReadOnly;

	private ParameterCollectionChanged _parametersChanged;

	bool IList.IsReadOnly => IsReadOnly;

	bool IList.IsFixedSize => IsFixedSize;

	protected virtual bool IsReadOnly
	{
		get
		{
			return _isReadOnly;
		}
		set
		{
			_isReadOnly = value;
		}
	}

	protected bool IsFixedSize => IsReadOnly;

	public ParameterCollection(ParameterCollectionChanged parametersChanged)
	{
		_parametersChanged = parametersChanged;
	}

	protected override void ClearItems()
	{
		CheckReadOnly();
		base.ClearItems();
		OnCollectionChanged();
	}

	protected override void InsertItem(int index, object value)
	{
		CheckReadOnly();
		base.InsertItem(index, value);
		OnCollectionChanged();
	}

	protected override void RemoveItem(int index)
	{
		CheckReadOnly();
		base.RemoveItem(index);
		OnCollectionChanged();
	}

	protected override void SetItem(int index, object value)
	{
		CheckReadOnly();
		base.SetItem(index, value);
		OnCollectionChanged();
	}

	internal void SetReadOnly(bool isReadOnly)
	{
		IsReadOnly = isReadOnly;
	}

	internal void ClearInternal()
	{
		base.ClearItems();
	}

	private void CheckReadOnly()
	{
		if (IsReadOnly)
		{
			throw new InvalidOperationException(SR.ObjectDataProviderParameterCollectionIsNotInUse);
		}
	}

	private void OnCollectionChanged()
	{
		_parametersChanged(this);
	}
}
