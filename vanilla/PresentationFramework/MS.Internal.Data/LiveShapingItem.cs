using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace MS.Internal.Data;

internal class LiveShapingItem : DependencyObject
{
	[Flags]
	private enum PrivateFlags
	{
		IsSortDirty = 1,
		IsSortPendingClean = 2,
		IsFilterDirty = 4,
		IsGroupDirty = 8,
		FailsFilter = 0x10,
		ForwardChanges = 0x20,
		IsDeleted = 0x40
	}

	private static readonly DependencyProperty StartingIndexProperty = DependencyProperty.Register("StartingIndex", typeof(int), typeof(LiveShapingItem));

	private static readonly DependencyProperty ParentGroupsProperty = DependencyProperty.Register("ParentGroups", typeof(object), typeof(LiveShapingItem));

	private LiveShapingBlock _block;

	private object _item;

	private PrivateFlags _flags;

	internal object Item
	{
		get
		{
			return _item;
		}
		set
		{
			_item = value;
		}
	}

	internal LiveShapingBlock Block
	{
		get
		{
			return _block;
		}
		set
		{
			_block = value;
		}
	}

	private LiveShapingList List => Block.List;

	internal bool IsSortDirty
	{
		get
		{
			return TestFlag(PrivateFlags.IsSortDirty);
		}
		set
		{
			ChangeFlag(PrivateFlags.IsSortDirty, value);
		}
	}

	internal bool IsSortPendingClean
	{
		get
		{
			return TestFlag(PrivateFlags.IsSortPendingClean);
		}
		set
		{
			ChangeFlag(PrivateFlags.IsSortPendingClean, value);
		}
	}

	internal bool IsFilterDirty
	{
		get
		{
			return TestFlag(PrivateFlags.IsFilterDirty);
		}
		set
		{
			ChangeFlag(PrivateFlags.IsFilterDirty, value);
		}
	}

	internal bool IsGroupDirty
	{
		get
		{
			return TestFlag(PrivateFlags.IsGroupDirty);
		}
		set
		{
			ChangeFlag(PrivateFlags.IsGroupDirty, value);
		}
	}

	internal bool FailsFilter
	{
		get
		{
			return TestFlag(PrivateFlags.FailsFilter);
		}
		set
		{
			ChangeFlag(PrivateFlags.FailsFilter, value);
		}
	}

	internal bool ForwardChanges
	{
		get
		{
			return TestFlag(PrivateFlags.ForwardChanges);
		}
		set
		{
			ChangeFlag(PrivateFlags.ForwardChanges, value);
		}
	}

	internal bool IsDeleted
	{
		get
		{
			return TestFlag(PrivateFlags.IsDeleted);
		}
		set
		{
			ChangeFlag(PrivateFlags.IsDeleted, value);
		}
	}

	internal int StartingIndex
	{
		get
		{
			return (int)GetValue(StartingIndexProperty);
		}
		set
		{
			SetValue(StartingIndexProperty, value);
		}
	}

	internal List<CollectionViewGroupInternal> ParentGroups => GetValue(ParentGroupsProperty) as List<CollectionViewGroupInternal>;

	internal CollectionViewGroupInternal ParentGroup => GetValue(ParentGroupsProperty) as CollectionViewGroupInternal;

	internal LiveShapingItem(object item, LiveShapingList list, bool filtered = false, LiveShapingBlock block = null, bool oneTime = false)
	{
		_block = block;
		list.InitializeItem(this, item, filtered, oneTime);
		ForwardChanges = !oneTime;
	}

	internal void FindPosition(out RBFinger<LiveShapingItem> oldFinger, out RBFinger<LiveShapingItem> newFinger, Comparison<LiveShapingItem> comparison)
	{
		Block.FindPosition(this, out oldFinger, out newFinger, comparison);
	}

	internal RBFinger<LiveShapingItem> GetFinger()
	{
		return Block.GetFinger(this);
	}

	internal int GetAndClearStartingIndex()
	{
		int startingIndex = StartingIndex;
		ClearValue(StartingIndexProperty);
		return startingIndex;
	}

	internal void SetBinding(string path, DependencyProperty dp, bool oneTime = false, bool enableXT = false)
	{
		if (enableXT && oneTime)
		{
			enableXT = false;
		}
		if (LookupEntry(dp.GlobalIndex).Found)
		{
			return;
		}
		if (!string.IsNullOrEmpty(path))
		{
			Binding binding;
			if (SystemXmlHelper.IsXmlNode(_item))
			{
				binding = new Binding();
				binding.XPath = path;
			}
			else
			{
				binding = new Binding(path);
			}
			binding.Source = _item;
			if (oneTime)
			{
				binding.Mode = BindingMode.OneTime;
			}
			BindingExpressionBase bindingExpressionBase = binding.CreateBindingExpression(this, dp);
			if (enableXT)
			{
				bindingExpressionBase.TargetWantsCrossThreadNotifications = true;
			}
			SetValue(dp, bindingExpressionBase);
		}
		else if (!oneTime && Item is INotifyPropertyChanged source)
		{
			PropertyChangedEventManager.AddHandler(source, OnPropertyChanged, string.Empty);
		}
	}

	internal object GetValue(string path, DependencyProperty dp)
	{
		if (!string.IsNullOrEmpty(path))
		{
			SetBinding(path, dp);
			return GetValue(dp);
		}
		return Item;
	}

	internal void Clear()
	{
		List.ClearItem(this);
	}

	internal void OnCrossThreadPropertyChange(DependencyProperty dp)
	{
		List.OnItemPropertyChangedCrossThread(this, dp);
	}

	internal void AddParentGroup(CollectionViewGroupInternal group)
	{
		object value = GetValue(ParentGroupsProperty);
		if (value == null)
		{
			SetValue(ParentGroupsProperty, group);
		}
		else if (!(value is List<CollectionViewGroupInternal> list))
		{
			List<CollectionViewGroupInternal> list2 = new List<CollectionViewGroupInternal>(2);
			list2.Add(value as CollectionViewGroupInternal);
			list2.Add(group);
			SetValue(ParentGroupsProperty, list2);
		}
		else
		{
			list.Add(group);
		}
	}

	internal void RemoveParentGroup(CollectionViewGroupInternal group)
	{
		object value = GetValue(ParentGroupsProperty);
		if (!(value is List<CollectionViewGroupInternal> list))
		{
			if (value == group)
			{
				ClearValue(ParentGroupsProperty);
			}
			return;
		}
		list.Remove(group);
		if (list.Count == 1)
		{
			SetValue(ParentGroupsProperty, list[0]);
		}
	}

	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		if (ForwardChanges)
		{
			List.OnItemPropertyChanged(this, e.Property);
		}
	}

	private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		List.OnItemPropertyChanged(this, null);
	}

	private bool TestFlag(PrivateFlags flag)
	{
		return (_flags & flag) != 0;
	}

	private void ChangeFlag(PrivateFlags flag, bool value)
	{
		if (value)
		{
			_flags |= flag;
		}
		else
		{
			_flags &= ~flag;
		}
	}
}
