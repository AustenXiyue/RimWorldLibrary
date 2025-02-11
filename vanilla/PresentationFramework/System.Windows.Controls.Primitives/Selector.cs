using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Data;
using System.Windows.Input;
using MS.Internal;
using MS.Internal.Controls;
using MS.Internal.Data;
using MS.Internal.KnownBoxes;

namespace System.Windows.Controls.Primitives;

/// <summary>Represents a control that allows a user to select items from among its child elements. </summary>
[DefaultEvent("SelectionChanged")]
[DefaultProperty("SelectedIndex")]
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public abstract class Selector : ItemsControl
{
	[Flags]
	private enum CacheBits
	{
		SyncingSelectionAndCurrency = 1,
		CanSelectMultiple = 2,
		IsSynchronizedWithCurrentItem = 4,
		SkipCoerceSelectedItemCheck = 8,
		SelectedValueDrivesSelection = 0x10,
		SelectedValueWaitsForItems = 0x20,
		NewContainersArePending = 0x40
	}

	internal class SelectionChanger
	{
		private Selector _owner;

		private InternalSelectedItemsStorage _toSelect;

		private InternalSelectedItemsStorage _toUnselect;

		private InternalSelectedItemsStorage _toDeferSelect;

		private bool _active;

		internal bool IsActive => _active;

		internal SelectionChanger(Selector s)
		{
			_owner = s;
			_active = false;
			_toSelect = new InternalSelectedItemsStorage(1, MatchUnresolvedEqualityComparer);
			_toUnselect = new InternalSelectedItemsStorage(1, MatchUnresolvedEqualityComparer);
			_toDeferSelect = new InternalSelectedItemsStorage(1, MatchUnresolvedEqualityComparer);
		}

		internal void Begin()
		{
			_active = true;
			_toSelect.Clear();
			_toUnselect.Clear();
		}

		internal void End()
		{
			List<ItemInfo> list = new List<ItemInfo>();
			List<ItemInfo> list2 = new List<ItemInfo>();
			try
			{
				ApplyCanSelectMultiple();
				CreateDeltaSelectionChange(list, list2);
				_owner.UpdatePublicSelectionProperties();
			}
			finally
			{
				Cleanup();
			}
			if (list.Count > 0 || list2.Count > 0)
			{
				if (_owner.IsSynchronizedWithCurrentItemPrivate)
				{
					_owner.SetCurrentToSelected();
				}
				_owner.InvokeSelectionChanged(list, list2);
			}
		}

		private void ApplyCanSelectMultiple()
		{
			if (_owner.CanSelectMultiple)
			{
				return;
			}
			if (_toSelect.Count == 1)
			{
				_toUnselect = new InternalSelectedItemsStorage(_owner._selectedItems);
			}
			else
			{
				if (_owner._selectedItems.Count <= 1 || _owner._selectedItems.Count == _toUnselect.Count + 1)
				{
					return;
				}
				ItemInfo itemInfo = _owner._selectedItems[0];
				_toUnselect.Clear();
				foreach (ItemInfo item in (IEnumerable<ItemInfo>)_owner._selectedItems)
				{
					if (item != itemInfo)
					{
						_toUnselect.Add(item);
					}
				}
			}
		}

		private void CreateDeltaSelectionChange(List<ItemInfo> unselectedItems, List<ItemInfo> selectedItems)
		{
			for (int i = 0; i < _toDeferSelect.Count; i++)
			{
				ItemInfo itemInfo = _toDeferSelect[i];
				if (_owner.Items.Contains(itemInfo.Item))
				{
					_toSelect.Add(itemInfo);
					_toDeferSelect.Remove(itemInfo);
					i--;
				}
			}
			if (_toUnselect.Count <= 0 && _toSelect.Count <= 0)
			{
				return;
			}
			using (_owner._selectedItems.DeferRemove())
			{
				if (_toUnselect.ResolvedCount > 0)
				{
					foreach (ItemInfo item in (IEnumerable<ItemInfo>)_toUnselect)
					{
						if (item.IsResolved)
						{
							_owner.ItemSetIsSelected(item, value: false);
							if (_owner._selectedItems.Remove(item))
							{
								unselectedItems.Add(item);
							}
						}
					}
				}
				if (_toUnselect.UnresolvedCount > 0)
				{
					foreach (ItemInfo item2 in (IEnumerable<ItemInfo>)_toUnselect)
					{
						if (!item2.IsResolved)
						{
							ItemInfo itemInfo2 = _owner._selectedItems.FindMatch(ItemInfo.Key(item2));
							if (itemInfo2 != null)
							{
								_owner.ItemSetIsSelected(itemInfo2, value: false);
								_owner._selectedItems.Remove(itemInfo2);
								unselectedItems.Add(itemInfo2);
							}
						}
					}
				}
			}
			using (_toSelect.DeferRemove())
			{
				if (_toSelect.ResolvedCount > 0)
				{
					List<ItemInfo> list = ((_toSelect.UnresolvedCount > 0) ? new List<ItemInfo>() : null);
					foreach (ItemInfo item3 in (IEnumerable<ItemInfo>)_toSelect)
					{
						if (item3.IsResolved)
						{
							_owner.ItemSetIsSelected(item3, value: true);
							if (!_owner._selectedItems.Contains(item3))
							{
								_owner._selectedItems.Add(item3);
								selectedItems.Add(item3);
							}
							list?.Add(item3);
						}
					}
					if (list != null)
					{
						foreach (ItemInfo item4 in list)
						{
							_toSelect.Remove(item4);
						}
					}
				}
				int num = 0;
				while (_toSelect.UnresolvedCount > 0 && num < _owner.Items.Count)
				{
					ItemInfo itemInfo3 = _owner.NewItemInfo(_owner.Items[num], null, num);
					ItemInfo e = new ItemInfo(itemInfo3.Item, ItemInfo.KeyContainer);
					if (_toSelect.Contains(e) && !_owner._selectedItems.Contains(itemInfo3))
					{
						_owner.ItemSetIsSelected(itemInfo3, value: true);
						_owner._selectedItems.Add(itemInfo3);
						selectedItems.Add(itemInfo3);
						_toSelect.Remove(e);
					}
					num++;
				}
			}
		}

		internal bool Select(ItemInfo info, bool assumeInItemsCollection)
		{
			if (!ItemGetIsSelectable(info))
			{
				return false;
			}
			if (!assumeInItemsCollection && !_owner.Items.Contains(info.Item))
			{
				if (!_toDeferSelect.Contains(info))
				{
					_toDeferSelect.Add(info);
				}
				return false;
			}
			ItemInfo itemInfo = ItemInfo.Key(info);
			if (_toUnselect.Remove(itemInfo))
			{
				return true;
			}
			if (_owner._selectedItems.Contains(info))
			{
				return false;
			}
			if (!itemInfo.IsKey && _toSelect.Contains(itemInfo))
			{
				return false;
			}
			if (!_owner.CanSelectMultiple && _toSelect.Count > 0)
			{
				foreach (ItemInfo item in (IEnumerable<ItemInfo>)_toSelect)
				{
					_owner.ItemSetIsSelected(item, value: false);
				}
				_toSelect.Clear();
			}
			_toSelect.Add(info);
			return true;
		}

		internal bool Unselect(ItemInfo info)
		{
			ItemInfo e = ItemInfo.Key(info);
			_toDeferSelect.Remove(info);
			if (_toSelect.Remove(e))
			{
				return true;
			}
			if (!_owner._selectedItems.Contains(e))
			{
				return false;
			}
			if (_toUnselect.Contains(info))
			{
				return false;
			}
			_toUnselect.Add(info);
			return true;
		}

		internal void Validate()
		{
			Begin();
			End();
		}

		internal void Cancel()
		{
			Cleanup();
		}

		internal void CleanupDeferSelection()
		{
			if (_toDeferSelect.Count > 0)
			{
				_toDeferSelect.Clear();
			}
		}

		internal void Cleanup()
		{
			_active = false;
			if (_toSelect.Count > 0)
			{
				_toSelect.Clear();
			}
			if (_toUnselect.Count > 0)
			{
				_toUnselect.Clear();
			}
		}

		internal void SelectJustThisItem(ItemInfo info, bool assumeInItemsCollection)
		{
			Begin();
			CleanupDeferSelection();
			try
			{
				bool flag = false;
				for (int num = _owner._selectedItems.Count - 1; num >= 0; num--)
				{
					if (info != _owner._selectedItems[num])
					{
						Unselect(_owner._selectedItems[num]);
					}
					else
					{
						flag = true;
					}
				}
				if (!flag && info != null && info.Item != DependencyProperty.UnsetValue)
				{
					Select(info, assumeInItemsCollection);
				}
			}
			finally
			{
				End();
			}
		}
	}

	internal class InternalSelectedItemsStorage : IEnumerable<ItemInfo>, IEnumerable
	{
		private class BatchRemoveHelper : IDisposable
		{
			private InternalSelectedItemsStorage _owner;

			private int _level;

			public bool IsActive => _level > 0;

			public int RemovedCount { get; set; }

			public BatchRemoveHelper(InternalSelectedItemsStorage owner)
			{
				_owner = owner;
			}

			public void Enter()
			{
				_level++;
			}

			public void Leave()
			{
				if (_level > 0 && --_level == 0 && RemovedCount > 0)
				{
					_owner.DoBatchRemove();
					RemovedCount = 0;
				}
			}

			public void Dispose()
			{
				Leave();
			}
		}

		private List<ItemInfo> _list;

		private Dictionary<ItemInfo, ItemInfo> _set;

		private IEqualityComparer<ItemInfo> _equalityComparer;

		private int _resolvedCount;

		private int _unresolvedCount;

		private BatchRemoveHelper _batchRemove;

		public ItemInfo this[int index] => _list[index];

		public int Count => _list.Count;

		public bool RemoveIsDeferred
		{
			get
			{
				if (_batchRemove != null)
				{
					return _batchRemove.IsActive;
				}
				return false;
			}
		}

		public int ResolvedCount => _resolvedCount;

		public int UnresolvedCount => _unresolvedCount;

		public bool UsesItemHashCodes
		{
			get
			{
				return _set != null;
			}
			set
			{
				if (value && _set == null)
				{
					_set = new Dictionary<ItemInfo, ItemInfo>(_list.Count);
					for (int i = 0; i < _list.Count; i++)
					{
						_set.Add(_list[i], _list[i]);
					}
				}
				else if (!value)
				{
					_set = null;
				}
			}
		}

		internal InternalSelectedItemsStorage(int capacity, IEqualityComparer<ItemInfo> equalityComparer)
		{
			_equalityComparer = equalityComparer;
			_list = new List<ItemInfo>(capacity);
			_set = new Dictionary<ItemInfo, ItemInfo>(capacity, equalityComparer);
		}

		internal InternalSelectedItemsStorage(InternalSelectedItemsStorage collection, IEqualityComparer<ItemInfo> equalityComparer = null)
		{
			_equalityComparer = equalityComparer ?? collection._equalityComparer;
			_list = new List<ItemInfo>(collection._list);
			if (collection.UsesItemHashCodes)
			{
				_set = new Dictionary<ItemInfo, ItemInfo>(collection._set, _equalityComparer);
			}
			_resolvedCount = collection._resolvedCount;
			_unresolvedCount = collection._unresolvedCount;
		}

		public void Add(object item, DependencyObject container, int index)
		{
			Add(new ItemInfo(item, container, index));
		}

		public void Add(ItemInfo info)
		{
			if (_set != null)
			{
				_set.Add(info, info);
			}
			_list.Add(info);
			if (info.IsResolved)
			{
				_resolvedCount++;
			}
			else
			{
				_unresolvedCount++;
			}
		}

		public bool Remove(ItemInfo e)
		{
			bool flag = false;
			bool flag2 = false;
			if (_set != null)
			{
				if (_set.TryGetValue(e, out var value))
				{
					flag = true;
					flag2 = value.IsResolved;
					_set.Remove(e);
					if (RemoveIsDeferred)
					{
						value.Container = ItemInfo.RemovedContainer;
						BatchRemoveHelper batchRemove = _batchRemove;
						int removedCount = batchRemove.RemovedCount + 1;
						batchRemove.RemovedCount = removedCount;
					}
					else
					{
						RemoveFromList(e);
					}
				}
			}
			else
			{
				flag = RemoveFromList(e);
			}
			if (flag)
			{
				if (flag2)
				{
					_resolvedCount--;
				}
				else
				{
					_unresolvedCount--;
				}
			}
			return flag;
		}

		private bool RemoveFromList(ItemInfo e)
		{
			bool result = false;
			int num = LastIndexInList(e);
			if (num >= 0)
			{
				_list.RemoveAt(num);
				result = true;
			}
			return result;
		}

		public bool Contains(ItemInfo e)
		{
			if (_set != null)
			{
				return _set.ContainsKey(e);
			}
			return IndexInList(e) >= 0;
		}

		public void Clear()
		{
			_list.Clear();
			if (_set != null)
			{
				_set.Clear();
			}
			_resolvedCount = (_unresolvedCount = 0);
		}

		public IDisposable DeferRemove()
		{
			if (_batchRemove == null)
			{
				_batchRemove = new BatchRemoveHelper(this);
			}
			_batchRemove.Enter();
			return _batchRemove;
		}

		private void DoBatchRemove()
		{
			int num = 0;
			int count = _list.Count;
			for (int i = 0; i < count; i++)
			{
				if (!_list[i].IsRemoved)
				{
					if (num < i)
					{
						_list[num] = _list[i];
					}
					num++;
				}
			}
			_list.RemoveRange(num, count - num);
		}

		IEnumerator<ItemInfo> IEnumerable<ItemInfo>.GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		public ItemInfo FindMatch(ItemInfo info)
		{
			if (_set != null)
			{
				if (!_set.TryGetValue(info, out var value))
				{
					return null;
				}
				return value;
			}
			int num = IndexInList(info);
			return (num < 0) ? null : _list[num];
		}

		private int IndexInList(ItemInfo info)
		{
			return _list.FindIndex((ItemInfo x) => _equalityComparer.Equals(info, x));
		}

		private int LastIndexInList(ItemInfo info)
		{
			return _list.FindLastIndex((ItemInfo x) => _equalityComparer.Equals(info, x));
		}
	}

	private class ItemInfoEqualityComparer : IEqualityComparer<ItemInfo>
	{
		private bool _matchUnresolved;

		public ItemInfoEqualityComparer(bool matchUnresolved)
		{
			_matchUnresolved = matchUnresolved;
		}

		bool IEqualityComparer<ItemInfo>.Equals(ItemInfo x, ItemInfo y)
		{
			if ((object)x == y)
			{
				return true;
			}
			if (!(x == null))
			{
				return x.Equals(y, _matchUnresolved);
			}
			return y == null;
		}

		int IEqualityComparer<ItemInfo>.GetHashCode(ItemInfo x)
		{
			return x.GetHashCode();
		}
	}

	private class ChangeInfo
	{
		public InternalSelectedItemsStorage ToAdd { get; private set; }

		public InternalSelectedItemsStorage ToRemove { get; private set; }

		public ChangeInfo(InternalSelectedItemsStorage toAdd, InternalSelectedItemsStorage toRemove)
		{
			ToAdd = toAdd;
			ToRemove = toRemove;
		}
	}

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.Primitives.Selector.SelectionChanged" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.Primitives.Selector.SelectionChanged" /> routed event.</returns>
	public static readonly RoutedEvent SelectionChangedEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.Primitives.Selector.Selected" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.Primitives.Selector.Selected" /> routed event.</returns>
	public static readonly RoutedEvent SelectedEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.Primitives.Selector.Unselected" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.Primitives.Selector.Unselected" /> routed event.</returns>
	public static readonly RoutedEvent UnselectedEvent;

	internal static readonly DependencyPropertyKey IsSelectionActivePropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.Selector.IsSelectionActive" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.Selector.IsSelectionActive" /> attached property.</returns>
	public static readonly DependencyProperty IsSelectionActiveProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.Selector.IsSelected" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.Selector.IsSelected" /> attached property.</returns>
	public static readonly DependencyProperty IsSelectedProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.Selector.IsSynchronizedWithCurrentItem" /> dependency property  </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.Selector.IsSynchronizedWithCurrentItem" /> dependency property.</returns>
	public static readonly DependencyProperty IsSynchronizedWithCurrentItemProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.Selector.SelectedIndex" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.Selector.SelectedIndex" /> dependency property.</returns>
	public static readonly DependencyProperty SelectedIndexProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.Selector.SelectedItem" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.Selector.SelectedItem" /> dependency property.</returns>
	public static readonly DependencyProperty SelectedItemProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.Selector.SelectedValue" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.Selector.SelectedValue" /> dependency property.</returns>
	public static readonly DependencyProperty SelectedValueProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Controls.Primitives.Selector.SelectedValuePath" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.Selector.SelectedValuePath" /> dependency property.</returns>
	public static readonly DependencyProperty SelectedValuePathProperty;

	private static readonly DependencyPropertyKey SelectedItemsPropertyKey;

	internal static readonly DependencyProperty SelectedItemsImplProperty;

	private static readonly BindingExpressionUncommonField ItemValueBindingExpression;

	internal InternalSelectedItemsStorage _selectedItems = new InternalSelectedItemsStorage(1, MatchExplicitEqualityComparer);

	private Point _lastMousePosition;

	private SelectionChanger _selectionChangeInstance;

	private BitVector32 _cacheValid = new BitVector32(2);

	private EventHandler _focusEnterMainFocusScopeEventHandler;

	private DependencyObject _clearingContainer;

	private static readonly UncommonField<ItemInfo> PendingSelectionByValueField;

	private static readonly ItemInfoEqualityComparer MatchExplicitEqualityComparer;

	private static readonly ItemInfoEqualityComparer MatchUnresolvedEqualityComparer;

	private static readonly UncommonField<ChangeInfo> ChangeInfoField;

	/// <summary>Gets or sets a value that indicates whether a <see cref="T:System.Windows.Controls.Primitives.Selector" /> should keep the <see cref="P:System.Windows.Controls.Primitives.Selector.SelectedItem" /> synchronized with the current item in the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> property.  </summary>
	/// <returns>true if the <see cref="P:System.Windows.Controls.Primitives.Selector.SelectedItem" /> is always synchronized with the current item in the <see cref="T:System.Windows.Controls.ItemCollection" />; false if the <see cref="P:System.Windows.Controls.Primitives.Selector.SelectedItem" /> is never synchronized with the current item; null if the <see cref="P:System.Windows.Controls.Primitives.Selector.SelectedItem" /> is synchronized with the current item only if the <see cref="T:System.Windows.Controls.Primitives.Selector" /> uses a <see cref="T:System.Windows.Data.CollectionView" />.  The default value is null.</returns>
	[Bindable(true)]
	[Category("Behavior")]
	[TypeConverter("System.Windows.NullableBoolConverter, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
	[Localizability(LocalizationCategory.NeverLocalize)]
	public bool? IsSynchronizedWithCurrentItem
	{
		get
		{
			return (bool?)GetValue(IsSynchronizedWithCurrentItemProperty);
		}
		set
		{
			SetValue(IsSynchronizedWithCurrentItemProperty, value);
		}
	}

	/// <summary>Gets or sets the index of the first item in the current selection or returns negative one (-1) if the selection is empty.  </summary>
	/// <returns>The index of first item in the current selection. The default value is negative one (-1).</returns>
	[Bindable(true)]
	[Category("Appearance")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Localizability(LocalizationCategory.NeverLocalize)]
	public int SelectedIndex
	{
		get
		{
			return (int)GetValue(SelectedIndexProperty);
		}
		set
		{
			SetValue(SelectedIndexProperty, value);
		}
	}

	/// <summary>Gets or sets the first item in the current selection or returns null if the selection is empty  </summary>
	/// <returns>The first item in the current selection or null if the selection is empty.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public object SelectedItem
	{
		get
		{
			return GetValue(SelectedItemProperty);
		}
		set
		{
			SetValue(SelectedItemProperty, value);
		}
	}

	/// <summary>Gets or sets the value of the <see cref="P:System.Windows.Controls.Primitives.Selector.SelectedItem" />, obtained by using <see cref="P:System.Windows.Controls.Primitives.Selector.SelectedValuePath" />.  </summary>
	/// <returns>The value of the selected item.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Localizability(LocalizationCategory.NeverLocalize)]
	public object SelectedValue
	{
		get
		{
			return GetValue(SelectedValueProperty);
		}
		set
		{
			SetValue(SelectedValueProperty, value);
		}
	}

	/// <summary>Gets or sets the path that is used to get the <see cref="P:System.Windows.Controls.Primitives.Selector.SelectedValue" /> from the <see cref="P:System.Windows.Controls.Primitives.Selector.SelectedItem" />.  </summary>
	/// <returns>The path used to get the <see cref="P:System.Windows.Controls.Primitives.Selector.SelectedValue" />. The default is an empty string.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	[Localizability(LocalizationCategory.NeverLocalize)]
	public string SelectedValuePath
	{
		get
		{
			return (string)GetValue(SelectedValuePathProperty);
		}
		set
		{
			SetValue(SelectedValuePathProperty, value);
		}
	}

	internal IList SelectedItemsImpl => (IList)GetValue(SelectedItemsImplProperty);

	internal bool CanSelectMultiple
	{
		get
		{
			return _cacheValid[2];
		}
		set
		{
			if (_cacheValid[2] != value)
			{
				_cacheValid[2] = value;
				if (!value && _selectedItems.Count > 1)
				{
					SelectionChange.Validate();
				}
			}
		}
	}

	private bool IsSynchronizedWithCurrentItemPrivate
	{
		get
		{
			return _cacheValid[4];
		}
		set
		{
			_cacheValid[4] = value;
		}
	}

	private bool SkipCoerceSelectedItemCheck
	{
		get
		{
			return _cacheValid[8];
		}
		set
		{
			_cacheValid[8] = value;
		}
	}

	internal SelectionChanger SelectionChange
	{
		get
		{
			if (_selectionChangeInstance == null)
			{
				_selectionChangeInstance = new SelectionChanger(this);
			}
			return _selectionChangeInstance;
		}
	}

	internal object InternalSelectedItem
	{
		get
		{
			if (_selectedItems.Count != 0)
			{
				return _selectedItems[0].Item;
			}
			return null;
		}
	}

	internal ItemInfo InternalSelectedInfo
	{
		get
		{
			if (_selectedItems.Count != 0)
			{
				return _selectedItems[0];
			}
			return null;
		}
	}

	internal int InternalSelectedIndex
	{
		get
		{
			if (_selectedItems.Count == 0)
			{
				return -1;
			}
			int num = _selectedItems[0].Index;
			if (num < 0)
			{
				num = base.Items.IndexOf(_selectedItems[0].Item);
				_selectedItems[0].Index = num;
			}
			return num;
		}
	}

	private object InternalSelectedValue
	{
		get
		{
			object internalSelectedItem = InternalSelectedItem;
			object result;
			if (internalSelectedItem != null)
			{
				BindingExpression bindingExpression = PrepareItemValueBinding(internalSelectedItem);
				if (string.IsNullOrEmpty(SelectedValuePath))
				{
					result = ((!string.IsNullOrEmpty(bindingExpression.ParentBinding.Path.Path)) ? SystemXmlHelper.GetInnerText(internalSelectedItem) : internalSelectedItem);
				}
				else
				{
					bindingExpression.Activate(internalSelectedItem);
					result = bindingExpression.Value;
					bindingExpression.Deactivate();
				}
			}
			else
			{
				result = DependencyProperty.UnsetValue;
			}
			return result;
		}
	}

	/// <summary>Occurs when the selection of a <see cref="T:System.Windows.Controls.Primitives.Selector" /> changes.</summary>
	[Category("Behavior")]
	public event SelectionChangedEventHandler SelectionChanged
	{
		add
		{
			AddHandler(SelectionChangedEvent, value);
		}
		remove
		{
			RemoveHandler(SelectionChangedEvent, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.Selector" /> class. </summary>
	protected Selector()
	{
		base.Items.CurrentChanged += OnCurrentChanged;
		base.ItemContainerGenerator.StatusChanged += OnGeneratorStatusChanged;
		_focusEnterMainFocusScopeEventHandler = OnFocusEnterMainFocusScope;
		KeyboardNavigation.Current.FocusEnterMainFocusScope += _focusEnterMainFocusScopeEventHandler;
		ObservableCollection<object> observableCollection = new SelectedItemCollection(this);
		SetValue(SelectedItemsPropertyKey, observableCollection);
		observableCollection.CollectionChanged += OnSelectedItemsCollectionChanged;
		SetValue(IsSelectionActivePropertyKey, BooleanBoxes.FalseBox);
	}

	static Selector()
	{
		SelectionChangedEvent = EventManager.RegisterRoutedEvent("SelectionChanged", RoutingStrategy.Bubble, typeof(SelectionChangedEventHandler), typeof(Selector));
		SelectedEvent = EventManager.RegisterRoutedEvent("Selected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Selector));
		UnselectedEvent = EventManager.RegisterRoutedEvent("Unselected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Selector));
		IsSelectionActivePropertyKey = DependencyProperty.RegisterAttachedReadOnly("IsSelectionActive", typeof(bool), typeof(Selector), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.Inherits));
		IsSelectionActiveProperty = IsSelectionActivePropertyKey.DependencyProperty;
		IsSelectedProperty = DependencyProperty.RegisterAttached("IsSelected", typeof(bool), typeof(Selector), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
		IsSynchronizedWithCurrentItemProperty = DependencyProperty.Register("IsSynchronizedWithCurrentItem", typeof(bool?), typeof(Selector), new FrameworkPropertyMetadata(null, OnIsSynchronizedWithCurrentItemChanged));
		SelectedIndexProperty = DependencyProperty.Register("SelectedIndex", typeof(int), typeof(Selector), new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal, OnSelectedIndexChanged, CoerceSelectedIndex), ValidateSelectedIndex);
		SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(Selector), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged, CoerceSelectedItem));
		SelectedValueProperty = DependencyProperty.Register("SelectedValue", typeof(object), typeof(Selector), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedValueChanged, CoerceSelectedValue));
		SelectedValuePathProperty = DependencyProperty.Register("SelectedValuePath", typeof(string), typeof(Selector), new FrameworkPropertyMetadata(string.Empty, OnSelectedValuePathChanged));
		SelectedItemsPropertyKey = DependencyProperty.RegisterReadOnly("SelectedItems", typeof(IList), typeof(Selector), new FrameworkPropertyMetadata((object)null));
		SelectedItemsImplProperty = SelectedItemsPropertyKey.DependencyProperty;
		ItemValueBindingExpression = new BindingExpressionUncommonField();
		PendingSelectionByValueField = new UncommonField<ItemInfo>();
		MatchExplicitEqualityComparer = new ItemInfoEqualityComparer(matchUnresolved: false);
		MatchUnresolvedEqualityComparer = new ItemInfoEqualityComparer(matchUnresolved: true);
		ChangeInfoField = new UncommonField<ChangeInfo>();
		EventManager.RegisterClassHandler(typeof(Selector), SelectedEvent, new RoutedEventHandler(OnSelected));
		EventManager.RegisterClassHandler(typeof(Selector), UnselectedEvent, new RoutedEventHandler(OnUnselected));
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Controls.Primitives.Selector.Selected" /> attached event. </summary>
	/// <param name="element">Element that listens to this event.</param>
	/// <param name="handler">Event handler to add.</param>
	public static void AddSelectedHandler(DependencyObject element, RoutedEventHandler handler)
	{
		UIElement.AddHandler(element, SelectedEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Controls.Primitives.Selector.Selected" /> attached event. </summary>
	/// <param name="element">Element that listens to this event.</param>
	/// <param name="handler">Event handler to remove.</param>
	public static void RemoveSelectedHandler(DependencyObject element, RoutedEventHandler handler)
	{
		UIElement.RemoveHandler(element, SelectedEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Controls.Primitives.Selector.Unselected" /> attached event. </summary>
	/// <param name="element">Element that listens to this event.</param>
	/// <param name="handler">Event handler to add.</param>
	public static void AddUnselectedHandler(DependencyObject element, RoutedEventHandler handler)
	{
		UIElement.AddHandler(element, UnselectedEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Controls.Primitives.Selector.Unselected" /> attached event. </summary>
	/// <param name="element">Element that listens to this event.</param>
	/// <param name="handler">Event handler to remove.</param>
	public static void RemoveUnselectedHandler(DependencyObject element, RoutedEventHandler handler)
	{
		UIElement.RemoveHandler(element, UnselectedEvent, handler);
	}

	/// <summary>Gets a value that indicates whether the keyboard focus is within the <see cref="T:System.Windows.Controls.Primitives.Selector" /></summary>
	/// <returns>Value of the property, true if the keyboard focus is within the <see cref="T:System.Windows.Controls.Primitives.Selector" />.</returns>
	/// <param name="element">The element from which to read the attached property.</param>
	public static bool GetIsSelectionActive(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(IsSelectionActiveProperty);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.Primitives.Selector.IsSelected" /> attached property that indicates whether an item is selected. </summary>
	/// <returns>Boolean value, true if the <see cref="P:System.Windows.Controls.Primitives.Selector.IsSelected" /> property is true.</returns>
	/// <param name="element">Object to query concerning the <see cref="P:System.Windows.Controls.Primitives.Selector.IsSelected" /> property.</param>
	[AttachedPropertyBrowsableForChildren]
	public static bool GetIsSelected(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(IsSelectedProperty);
	}

	/// <summary>Sets a property value that indicates whether an item in a <see cref="T:System.Windows.Controls.Primitives.Selector" /> is selected. </summary>
	/// <param name="element">Object on which to set the property.</param>
	/// <param name="isSelected">Value to set.</param>
	public static void SetIsSelected(DependencyObject element, bool isSelected)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(IsSelectedProperty, BooleanBoxes.Box(isSelected));
	}

	private static void OnIsSynchronizedWithCurrentItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Selector)d).SetSynchronizationWithCurrentItem();
	}

	private void SetSynchronizationWithCurrentItem()
	{
		bool? isSynchronizedWithCurrentItem = IsSynchronizedWithCurrentItem;
		bool isSynchronizedWithCurrentItemPrivate = IsSynchronizedWithCurrentItemPrivate;
		bool flag;
		if (isSynchronizedWithCurrentItem.HasValue)
		{
			flag = isSynchronizedWithCurrentItem.Value;
		}
		else
		{
			if (!base.IsInitialized)
			{
				return;
			}
			flag = (SelectionMode)GetValue(ListBox.SelectionModeProperty) == SelectionMode.Single && !CollectionViewSource.IsDefaultView(base.Items.CollectionView);
		}
		IsSynchronizedWithCurrentItemPrivate = flag;
		if (!isSynchronizedWithCurrentItemPrivate && flag)
		{
			if (SelectedItem != null)
			{
				SetCurrentToSelected();
			}
			else
			{
				SetSelectedToCurrent();
			}
		}
	}

	private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Selector selector = (Selector)d;
		if (!selector.SelectionChange.IsActive)
		{
			int index = (int)e.NewValue;
			selector.SelectionChange.SelectJustThisItem(selector.ItemInfoFromIndex(index), assumeInItemsCollection: true);
		}
	}

	private static bool ValidateSelectedIndex(object o)
	{
		return (int)o >= -1;
	}

	private static object CoerceSelectedIndex(DependencyObject d, object value)
	{
		Selector selector = (Selector)d;
		if (value is int && (int)value >= selector.Items.Count)
		{
			return DependencyProperty.UnsetValue;
		}
		return value;
	}

	private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Selector selector = (Selector)d;
		if (!selector.SelectionChange.IsActive)
		{
			selector.SelectionChange.SelectJustThisItem(selector.NewItemInfo(e.NewValue), assumeInItemsCollection: false);
		}
	}

	private static object CoerceSelectedItem(DependencyObject d, object value)
	{
		Selector selector = (Selector)d;
		if (value == null || selector.SkipCoerceSelectedItemCheck)
		{
			return value;
		}
		int selectedIndex = selector.SelectedIndex;
		if ((selectedIndex > -1 && selectedIndex < selector.Items.Count && selector.Items[selectedIndex] == value) || selector.Items.Contains(value))
		{
			return value;
		}
		return DependencyProperty.UnsetValue;
	}

	private static void OnSelectedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (FrameworkAppContextSwitches.SelectionPropertiesCanLagBehindSelectionChangedEvent)
		{
			return;
		}
		Selector selector = (Selector)d;
		ItemInfo value = PendingSelectionByValueField.GetValue(selector);
		if (!(value != null))
		{
			return;
		}
		try
		{
			if (!selector.SelectionChange.IsActive)
			{
				selector._cacheValid[16] = true;
				selector.SelectionChange.SelectJustThisItem(value, assumeInItemsCollection: true);
			}
		}
		finally
		{
			selector._cacheValid[16] = false;
			PendingSelectionByValueField.ClearValue(selector);
		}
	}

	private object SelectItemWithValue(object value, bool selectNow)
	{
		object obj;
		if (FrameworkAppContextSwitches.SelectionPropertiesCanLagBehindSelectionChangedEvent)
		{
			_cacheValid[16] = true;
			if (base.HasItems)
			{
				obj = FindItemWithValue(value, out var index);
				SelectionChange.SelectJustThisItem(NewItemInfo(obj, null, index), assumeInItemsCollection: true);
			}
			else
			{
				obj = DependencyProperty.UnsetValue;
				_cacheValid[32] = true;
			}
			_cacheValid[16] = false;
		}
		else if (base.HasItems)
		{
			obj = FindItemWithValue(value, out var index2);
			ItemInfo itemInfo = NewItemInfo(obj, null, index2);
			if (selectNow)
			{
				try
				{
					_cacheValid[16] = true;
					SelectionChange.SelectJustThisItem(itemInfo, assumeInItemsCollection: true);
				}
				finally
				{
					_cacheValid[16] = false;
				}
			}
			else
			{
				PendingSelectionByValueField.SetValue(this, itemInfo);
			}
		}
		else
		{
			obj = DependencyProperty.UnsetValue;
			_cacheValid[32] = true;
		}
		return obj;
	}

	private object FindItemWithValue(object value, out int index)
	{
		index = -1;
		if (!base.HasItems)
		{
			return DependencyProperty.UnsetValue;
		}
		BindingExpression bindingExpression = PrepareItemValueBinding(base.Items.GetRepresentativeItem());
		if (bindingExpression == null)
		{
			return DependencyProperty.UnsetValue;
		}
		if (string.IsNullOrEmpty(SelectedValuePath))
		{
			if (string.IsNullOrEmpty(bindingExpression.ParentBinding.Path.Path))
			{
				index = base.Items.IndexOf(value);
				if (index >= 0)
				{
					return value;
				}
				return DependencyProperty.UnsetValue;
			}
			return SystemXmlHelper.FindXmlNodeWithInnerText(base.Items, value, out index);
		}
		Type knownType = value?.GetType();
		DynamicValueConverter converter = new DynamicValueConverter(targetToSourceNeeded: false);
		index = 0;
		foreach (object item in (IEnumerable)base.Items)
		{
			bindingExpression.Activate(item);
			object value2 = bindingExpression.Value;
			if (VerifyEqual(value, knownType, value2, converter))
			{
				bindingExpression.Deactivate();
				return item;
			}
			index++;
		}
		bindingExpression.Deactivate();
		index = -1;
		return DependencyProperty.UnsetValue;
	}

	private bool VerifyEqual(object knownValue, Type knownType, object itemValue, DynamicValueConverter converter)
	{
		object obj = knownValue;
		if (knownType != null && itemValue != null)
		{
			Type type = itemValue.GetType();
			if (!knownType.IsAssignableFrom(type))
			{
				obj = converter.Convert(knownValue, type);
				if (obj == DependencyProperty.UnsetValue)
				{
					obj = knownValue;
				}
			}
		}
		return object.Equals(obj, itemValue);
	}

	private static object CoerceSelectedValue(DependencyObject d, object value)
	{
		Selector selector = (Selector)d;
		if (selector.SelectionChange.IsActive)
		{
			selector._cacheValid[16] = false;
		}
		else if (selector.SelectItemWithValue(value, selectNow: false) == DependencyProperty.UnsetValue && selector.HasItems)
		{
			value = null;
		}
		return value;
	}

	private static void OnSelectedValuePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Selector selector = (Selector)d;
		ItemValueBindingExpression.ClearValue(selector);
		if (selector.GetValueEntry(selector.LookupEntry(SelectedValueProperty.GlobalIndex), SelectedValueProperty, null, RequestFlags.RawEntry).IsCoerced || selector.SelectedValue != null)
		{
			selector.CoerceValue(SelectedValueProperty);
		}
	}

	private BindingExpression PrepareItemValueBinding(object item)
	{
		if (item == null)
		{
			return null;
		}
		bool flag = SystemXmlHelper.IsXmlNode(item);
		BindingExpression bindingExpression = ItemValueBindingExpression.GetValue(this);
		if (bindingExpression != null)
		{
			Binding parentBinding = bindingExpression.ParentBinding;
			bool flag2 = parentBinding.XPath != null;
			if ((!flag2 && flag) || (flag2 && !flag))
			{
				ItemValueBindingExpression.ClearValue(this);
				bindingExpression = null;
			}
		}
		if (bindingExpression == null)
		{
			Binding parentBinding = new Binding();
			parentBinding.Source = null;
			if (flag)
			{
				parentBinding.XPath = SelectedValuePath;
				parentBinding.Path = new PropertyPath("/InnerText");
			}
			else
			{
				parentBinding.Path = new PropertyPath(SelectedValuePath);
			}
			bindingExpression = (BindingExpression)BindingExpressionBase.CreateUntargetedBindingExpression(this, parentBinding);
			ItemValueBindingExpression.SetValue(this, bindingExpression);
		}
		return bindingExpression;
	}

	internal bool SetSelectedItemsImpl(IEnumerable selectedItems)
	{
		bool flag = false;
		if (!SelectionChange.IsActive)
		{
			SelectionChange.Begin();
			SelectionChange.CleanupDeferSelection();
			ObservableCollection<object> observableCollection = (ObservableCollection<object>)GetValue(SelectedItemsImplProperty);
			try
			{
				if (observableCollection != null)
				{
					foreach (object item in observableCollection)
					{
						SelectionChange.Unselect(NewUnresolvedItemInfo(item));
					}
				}
				if (selectedItems != null)
				{
					foreach (object selectedItem in selectedItems)
					{
						if (!SelectionChange.Select(NewUnresolvedItemInfo(selectedItem), assumeInItemsCollection: false))
						{
							SelectionChange.Cancel();
							return false;
						}
					}
				}
				SelectionChange.End();
				flag = true;
			}
			finally
			{
				if (!flag)
				{
					SelectionChange.Cancel();
				}
			}
		}
		return flag;
	}

	private void OnSelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (SelectionChange.IsActive)
		{
			return;
		}
		if (!CanSelectMultiple)
		{
			throw new InvalidOperationException(SR.ChangingCollectionNotSupported);
		}
		SelectionChange.Begin();
		bool flag = false;
		try
		{
			switch (e.Action)
			{
			case NotifyCollectionChangedAction.Add:
				if (e.NewItems.Count != 1)
				{
					throw new NotSupportedException(SR.RangeActionsNotSupported);
				}
				SelectionChange.Select(NewUnresolvedItemInfo(e.NewItems[0]), assumeInItemsCollection: false);
				break;
			case NotifyCollectionChangedAction.Remove:
				if (e.OldItems.Count != 1)
				{
					throw new NotSupportedException(SR.RangeActionsNotSupported);
				}
				SelectionChange.Unselect(NewUnresolvedItemInfo(e.OldItems[0]));
				break;
			case NotifyCollectionChangedAction.Reset:
			{
				SelectionChange.CleanupDeferSelection();
				for (int i = 0; i < _selectedItems.Count; i++)
				{
					SelectionChange.Unselect(_selectedItems[i]);
				}
				ObservableCollection<object> observableCollection = (ObservableCollection<object>)sender;
				for (int j = 0; j < observableCollection.Count; j++)
				{
					SelectionChange.Select(NewUnresolvedItemInfo(observableCollection[j]), assumeInItemsCollection: false);
				}
				break;
			}
			case NotifyCollectionChangedAction.Replace:
				if (e.NewItems.Count != 1 || e.OldItems.Count != 1)
				{
					throw new NotSupportedException(SR.RangeActionsNotSupported);
				}
				SelectionChange.Unselect(NewUnresolvedItemInfo(e.OldItems[0]));
				SelectionChange.Select(NewUnresolvedItemInfo(e.NewItems[0]), assumeInItemsCollection: false);
				break;
			default:
				throw new NotSupportedException(SR.Format(SR.UnexpectedCollectionChangeAction, e.Action));
			case NotifyCollectionChangedAction.Move:
				break;
			}
			SelectionChange.End();
			flag = true;
		}
		finally
		{
			if (!flag)
			{
				SelectionChange.Cancel();
			}
		}
	}

	/// <summary>Returns an item container to the state it was in before <see cref="M:System.Windows.Controls.ItemsControl.PrepareContainerForItemOverride(System.Windows.DependencyObject,System.Object)" />.</summary>
	/// <param name="element">The item container element.</param>
	/// <param name="item">The data item.</param>
	protected override void ClearContainerForItemOverride(DependencyObject element, object item)
	{
		base.ClearContainerForItemOverride(element, item);
		if (!((IGeneratorHost)this).IsItemItsOwnContainer(item))
		{
			try
			{
				_clearingContainer = element;
				element.ClearValue(IsSelectedProperty);
			}
			finally
			{
				_clearingContainer = null;
			}
		}
	}

	internal void RaiseIsSelectedChangedAutomationEvent(DependencyObject container, bool isSelected)
	{
		if (UIElementAutomationPeer.FromElement(this) is SelectorAutomationPeer { ItemPeers: not null } selectorAutomationPeer)
		{
			object itemOrContainerFromContainer = GetItemOrContainerFromContainer(container);
			if (itemOrContainerFromContainer != null && selectorAutomationPeer.ItemPeers[itemOrContainerFromContainer] is SelectorItemAutomationPeer selectorItemAutomationPeer)
			{
				selectorItemAutomationPeer.RaiseAutomationIsSelectedChanged(isSelected);
			}
		}
	}

	internal void SetInitialMousePosition()
	{
		_lastMousePosition = Mouse.GetPosition(this);
	}

	internal bool DidMouseMove()
	{
		Point position = Mouse.GetPosition(this);
		if (position != _lastMousePosition)
		{
			_lastMousePosition = position;
			return true;
		}
		return false;
	}

	internal void ResetLastMousePosition()
	{
		_lastMousePosition = default(Point);
	}

	internal virtual void SelectAllImpl()
	{
		SelectionChange.Begin();
		SelectionChange.CleanupDeferSelection();
		try
		{
			int num = 0;
			foreach (object item in (IEnumerable)base.Items)
			{
				ItemInfo info = NewItemInfo(item, null, num++);
				SelectionChange.Select(info, assumeInItemsCollection: true);
			}
		}
		finally
		{
			SelectionChange.End();
		}
	}

	internal virtual void UnselectAllImpl()
	{
		SelectionChange.Begin();
		SelectionChange.CleanupDeferSelection();
		try
		{
			_ = InternalSelectedItem;
			foreach (ItemInfo item in (IEnumerable<ItemInfo>)_selectedItems)
			{
				SelectionChange.Unselect(item);
			}
		}
		finally
		{
			SelectionChange.End();
		}
	}

	/// <summary>Updates the current selection when an item in the <see cref="T:System.Windows.Controls.Primitives.Selector" /> has changed</summary>
	/// <param name="e">The event data.</param>
	protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
	{
		base.OnItemsChanged(e);
		if (base.DataContext == BindingExpressionBase.DisconnectedItem)
		{
			return;
		}
		if (e.Action == NotifyCollectionChangedAction.Reset || (e.Action == NotifyCollectionChangedAction.Add && e.NewStartingIndex == 0))
		{
			ResetSelectedItemsAlgorithm();
		}
		EffectiveValueEntry valueEntry = GetValueEntry(LookupEntry(SelectedIndexProperty.GlobalIndex), SelectedIndexProperty, null, RequestFlags.DeferredReferences);
		if (!valueEntry.IsDeferredReference || !(valueEntry.Value is DeferredSelectedIndexReference))
		{
			CoerceValue(SelectedIndexProperty);
		}
		CoerceValue(SelectedItemProperty);
		if (_cacheValid[32] && !object.Equals(SelectedValue, InternalSelectedValue))
		{
			SelectItemWithValue(SelectedValue, selectNow: true);
		}
		switch (e.Action)
		{
		case NotifyCollectionChangedAction.Add:
			SelectionChange.Begin();
			try
			{
				ItemInfo info = NewItemInfo(e.NewItems[0], null, e.NewStartingIndex);
				if (InfoGetIsSelected(info))
				{
					SelectionChange.Select(info, assumeInItemsCollection: true);
				}
				break;
			}
			finally
			{
				SelectionChange.End();
			}
		case NotifyCollectionChangedAction.Replace:
			ItemSetIsSelected(ItemInfoFromIndex(e.NewStartingIndex), value: false);
			RemoveFromSelection(e);
			break;
		case NotifyCollectionChangedAction.Remove:
			RemoveFromSelection(e);
			break;
		case NotifyCollectionChangedAction.Move:
			AdjustNewContainers();
			SelectionChange.Validate();
			break;
		case NotifyCollectionChangedAction.Reset:
			if (base.Items.IsEmpty)
			{
				SelectionChange.CleanupDeferSelection();
			}
			if (base.Items.CurrentItem != null && IsSynchronizedWithCurrentItemPrivate)
			{
				SetSelectedToCurrent();
				break;
			}
			SelectionChange.Begin();
			try
			{
				LocateSelectedItems(null, deselectMissingItems: true);
				if (base.ItemsSource != null)
				{
					break;
				}
				for (int i = 0; i < base.Items.Count; i++)
				{
					ItemInfo itemInfo = ItemInfoFromIndex(i);
					if (InfoGetIsSelected(itemInfo) && !_selectedItems.Contains(itemInfo))
					{
						SelectionChange.Select(itemInfo, assumeInItemsCollection: true);
					}
				}
				break;
			}
			finally
			{
				SelectionChange.End();
			}
		default:
			throw new NotSupportedException(SR.Format(SR.UnexpectedCollectionChangeAction, e.Action));
		}
	}

	internal override void AdjustItemInfoOverride(NotifyCollectionChangedEventArgs e)
	{
		AdjustItemInfos(e, _selectedItems);
		base.AdjustItemInfoOverride(e);
	}

	private void RemoveFromSelection(NotifyCollectionChangedEventArgs e)
	{
		SelectionChange.Begin();
		try
		{
			ItemInfo itemInfo = NewItemInfo(e.OldItems[0], ItemInfo.SentinelContainer, e.OldStartingIndex);
			itemInfo.Container = null;
			if (_selectedItems.Contains(itemInfo))
			{
				SelectionChange.Unselect(itemInfo);
			}
		}
		finally
		{
			SelectionChange.End();
		}
	}

	/// <summary>Called when the selection changes.</summary>
	/// <param name="e">The event data.</param>
	protected virtual void OnSelectionChanged(SelectionChangedEventArgs e)
	{
		RaiseEvent(e);
	}

	/// <summary>Called when the <see cref="P:System.Windows.UIElement.IsKeyboardFocusWithin" /> property has changed. </summary>
	/// <param name="e">The event data.</param>
	protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnIsKeyboardFocusWithinChanged(e);
		bool flag = false;
		if ((bool)e.NewValue)
		{
			flag = true;
		}
		else if (Keyboard.FocusedElement is DependencyObject element && KeyboardNavigation.GetVisualRoot(this) is UIElement { IsKeyboardFocusWithin: not false } && FocusManager.GetFocusScope(element) != FocusManager.GetFocusScope(this))
		{
			flag = true;
		}
		if (flag)
		{
			SetValue(IsSelectionActivePropertyKey, BooleanBoxes.TrueBox);
		}
		else
		{
			SetValue(IsSelectionActivePropertyKey, BooleanBoxes.FalseBox);
		}
	}

	private void OnFocusEnterMainFocusScope(object sender, EventArgs e)
	{
		if (!base.IsKeyboardFocusWithin)
		{
			ClearValue(IsSelectionActivePropertyKey);
		}
	}

	/// <summary>Called when the source of an item in a selector changes.</summary>
	/// <param name="oldValue">Old value of the source.</param>
	/// <param name="newValue">New value of the source.</param>
	protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
	{
		SetSynchronizationWithCurrentItem();
	}

	/// <summary>Prepares the specified element to display the specified item. </summary>
	/// <param name="element">The element that is used to display the specified item.</param>
	/// <param name="item">The specified item to display.</param>
	protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
	{
		base.PrepareContainerForItemOverride(element, item);
		if (item == SelectedItem)
		{
			KeyboardNavigation.Current.UpdateActiveElement(this, element);
		}
		OnNewContainer();
	}

	/// <summary>Raises the <see cref="E:System.Windows.FrameworkElement.Initialized" /> event. This method is invoked whenever <see cref="P:System.Windows.FrameworkElement.IsInitialized" /> is set to true internally.</summary>
	/// <param name="e">The <see cref="T:System.Windows.RoutedEventArgs" /> that contains the event data.</param>
	protected override void OnInitialized(EventArgs e)
	{
		base.OnInitialized(e);
		SetSynchronizationWithCurrentItem();
	}

	private void SetSelectedHelper(object item, FrameworkElement UI, bool selected)
	{
		if (!ItemGetIsSelectable(item) && selected)
		{
			throw new InvalidOperationException(SR.CannotSelectNotSelectableItem);
		}
		SelectionChange.Begin();
		try
		{
			ItemInfo info = NewItemInfo(item, UI);
			if (selected)
			{
				SelectionChange.Select(info, assumeInItemsCollection: true);
			}
			else
			{
				SelectionChange.Unselect(info);
			}
		}
		finally
		{
			SelectionChange.End();
		}
	}

	private void OnCurrentChanged(object sender, EventArgs e)
	{
		if (IsSynchronizedWithCurrentItemPrivate)
		{
			SetSelectedToCurrent();
		}
	}

	private void OnNewContainer()
	{
		if (!_cacheValid[64])
		{
			_cacheValid[64] = true;
			base.LayoutUpdated += OnLayoutUpdated;
		}
	}

	private void OnLayoutUpdated(object sender, EventArgs e)
	{
		AdjustNewContainers();
	}

	private void OnGeneratorStatusChanged(object sender, EventArgs e)
	{
		if (base.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
		{
			AdjustNewContainers();
		}
	}

	private void AdjustNewContainers()
	{
		if (_cacheValid[64])
		{
			base.LayoutUpdated -= OnLayoutUpdated;
			_cacheValid[64] = false;
		}
		AdjustItemInfosAfterGeneratorChangeOverride();
		if (!base.HasItems)
		{
			return;
		}
		SelectionChange.Begin();
		try
		{
			for (int i = 0; i < _selectedItems.Count; i++)
			{
				ItemSetIsSelected(_selectedItems[i], value: true);
			}
		}
		finally
		{
			SelectionChange.Cancel();
		}
	}

	internal virtual void AdjustItemInfosAfterGeneratorChangeOverride()
	{
		AdjustItemInfosAfterGeneratorChange(_selectedItems, claimUniqueContainer: true);
	}

	private void SetSelectedToCurrent()
	{
		if (_cacheValid[1])
		{
			return;
		}
		_cacheValid[1] = true;
		try
		{
			object currentItem = base.Items.CurrentItem;
			if (currentItem != null && ItemGetIsSelectable(currentItem))
			{
				SelectionChange.SelectJustThisItem(NewItemInfo(currentItem, null, base.Items.CurrentPosition), assumeInItemsCollection: true);
			}
			else
			{
				SelectionChange.SelectJustThisItem(null, assumeInItemsCollection: false);
			}
		}
		finally
		{
			_cacheValid[1] = false;
		}
	}

	private void SetCurrentToSelected()
	{
		if (_cacheValid[1])
		{
			return;
		}
		_cacheValid[1] = true;
		try
		{
			if (_selectedItems.Count == 0)
			{
				base.Items.MoveCurrentToPosition(-1);
				return;
			}
			int index = _selectedItems[0].Index;
			if (index >= 0)
			{
				base.Items.MoveCurrentToPosition(index);
			}
			else
			{
				base.Items.MoveCurrentTo(InternalSelectedItem);
			}
		}
		finally
		{
			_cacheValid[1] = false;
		}
	}

	private void UpdateSelectedItems()
	{
		SelectedItemCollection selectedItemCollection = (SelectedItemCollection)SelectedItemsImpl;
		if (selectedItemCollection == null)
		{
			return;
		}
		InternalSelectedItemsStorage internalSelectedItemsStorage = new InternalSelectedItemsStorage(0, MatchExplicitEqualityComparer);
		InternalSelectedItemsStorage internalSelectedItemsStorage2 = new InternalSelectedItemsStorage(selectedItemCollection.Count, MatchExplicitEqualityComparer);
		internalSelectedItemsStorage.UsesItemHashCodes = _selectedItems.UsesItemHashCodes;
		internalSelectedItemsStorage2.UsesItemHashCodes = _selectedItems.UsesItemHashCodes;
		for (int i = 0; i < selectedItemCollection.Count; i++)
		{
			internalSelectedItemsStorage2.Add(selectedItemCollection[i], ItemInfo.SentinelContainer, ~i);
		}
		using (internalSelectedItemsStorage2.DeferRemove())
		{
			ItemInfo itemInfo = new ItemInfo(null);
			foreach (ItemInfo item in (IEnumerable<ItemInfo>)_selectedItems)
			{
				itemInfo.Reset(item.Item);
				if (internalSelectedItemsStorage2.Contains(itemInfo))
				{
					internalSelectedItemsStorage2.Remove(itemInfo);
				}
				else
				{
					internalSelectedItemsStorage.Add(item);
				}
			}
		}
		if (internalSelectedItemsStorage.Count > 0 || internalSelectedItemsStorage2.Count > 0)
		{
			if (selectedItemCollection.IsChanging)
			{
				ChangeInfoField.SetValue(this, new ChangeInfo(internalSelectedItemsStorage, internalSelectedItemsStorage2));
			}
			else
			{
				UpdateSelectedItems(internalSelectedItemsStorage, internalSelectedItemsStorage2);
			}
		}
	}

	internal void FinishSelectedItemsChange()
	{
		ChangeInfo value = ChangeInfoField.GetValue(this);
		if (value != null)
		{
			bool isActive = SelectionChange.IsActive;
			if (!isActive)
			{
				SelectionChange.Begin();
			}
			UpdateSelectedItems(value.ToAdd, value.ToRemove);
			if (!isActive)
			{
				SelectionChange.End();
			}
		}
	}

	private void UpdateSelectedItems(InternalSelectedItemsStorage toAdd, InternalSelectedItemsStorage toRemove)
	{
		IList selectedItemsImpl = SelectedItemsImpl;
		ChangeInfoField.ClearValue(this);
		for (int i = 0; i < toAdd.Count; i++)
		{
			selectedItemsImpl.Add(toAdd[i].Item);
		}
		for (int num = toRemove.Count - 1; num >= 0; num--)
		{
			selectedItemsImpl.RemoveAt(~toRemove[num].Index);
		}
	}

	internal void UpdatePublicSelectionProperties()
	{
		EffectiveValueEntry valueEntry = GetValueEntry(LookupEntry(SelectedIndexProperty.GlobalIndex), SelectedIndexProperty, null, RequestFlags.DeferredReferences);
		if (!valueEntry.IsDeferredReference)
		{
			int num = (int)valueEntry.Value;
			if (num > base.Items.Count - 1 || (num == -1 && _selectedItems.Count > 0) || (num > -1 && (_selectedItems.Count == 0 || num != _selectedItems[0].Index)))
			{
				SetCurrentDeferredValue(SelectedIndexProperty, new DeferredSelectedIndexReference(this));
			}
		}
		if (SelectedItem != InternalSelectedItem)
		{
			try
			{
				SkipCoerceSelectedItemCheck = true;
				SetCurrentValueInternal(SelectedItemProperty, InternalSelectedItem);
			}
			finally
			{
				SkipCoerceSelectedItemCheck = false;
			}
		}
		if (_selectedItems.Count > 0)
		{
			_cacheValid[32] = false;
		}
		if (!_cacheValid[16] && !_cacheValid[32])
		{
			object obj = InternalSelectedValue;
			if (obj == DependencyProperty.UnsetValue)
			{
				obj = null;
			}
			if (!object.Equals(SelectedValue, obj))
			{
				SetCurrentValueInternal(SelectedValueProperty, obj);
			}
		}
		UpdateSelectedItems();
	}

	private void InvokeSelectionChanged(List<ItemInfo> unselectedInfos, List<ItemInfo> selectedInfos)
	{
		SelectionChangedEventArgs selectionChangedEventArgs = new SelectionChangedEventArgs(unselectedInfos, selectedInfos);
		selectionChangedEventArgs.Source = this;
		OnSelectionChanged(selectionChangedEventArgs);
	}

	private bool InfoGetIsSelected(ItemInfo info)
	{
		DependencyObject container = info.Container;
		if (container != null)
		{
			return (bool)container.GetValue(IsSelectedProperty);
		}
		if (IsItemItsOwnContainerOverride(info.Item) && info.Item is DependencyObject dependencyObject)
		{
			return (bool)dependencyObject.GetValue(IsSelectedProperty);
		}
		return false;
	}

	private void ItemSetIsSelected(ItemInfo info, bool value)
	{
		if (info == null)
		{
			return;
		}
		DependencyObject container = info.Container;
		if (container != null && container != ItemInfo.RemovedContainer)
		{
			if (GetIsSelected(container) != value)
			{
				container.SetCurrentValueInternal(IsSelectedProperty, BooleanBoxes.Box(value));
			}
			return;
		}
		object item = info.Item;
		if (IsItemItsOwnContainerOverride(item) && item is DependencyObject dependencyObject && GetIsSelected(dependencyObject) != value)
		{
			dependencyObject.SetCurrentValueInternal(IsSelectedProperty, BooleanBoxes.Box(value));
		}
	}

	internal static bool ItemGetIsSelectable(object item)
	{
		if (item != null)
		{
			return !(item is Separator);
		}
		return false;
	}

	internal static bool UiGetIsSelectable(DependencyObject o)
	{
		if (o != null)
		{
			if (!ItemGetIsSelectable(o))
			{
				return false;
			}
			ItemsControl itemsControl = ItemsControl.ItemsControlFromItemContainer(o);
			if (itemsControl != null)
			{
				object obj = itemsControl.ItemContainerGenerator.ItemFromContainer(o);
				if (obj != o)
				{
					return ItemGetIsSelectable(obj);
				}
				return true;
			}
		}
		return false;
	}

	private static void OnSelected(object sender, RoutedEventArgs e)
	{
		((Selector)sender).NotifyIsSelectedChanged(e.OriginalSource as FrameworkElement, selected: true, e);
	}

	private static void OnUnselected(object sender, RoutedEventArgs e)
	{
		((Selector)sender).NotifyIsSelectedChanged(e.OriginalSource as FrameworkElement, selected: false, e);
	}

	internal void NotifyIsSelectedChanged(FrameworkElement container, bool selected, RoutedEventArgs e)
	{
		if (SelectionChange.IsActive || container == _clearingContainer)
		{
			e.Handled = true;
		}
		else if (container != null)
		{
			object itemOrContainerFromContainer = GetItemOrContainerFromContainer(container);
			if (itemOrContainerFromContainer != DependencyProperty.UnsetValue)
			{
				SetSelectedHelper(itemOrContainerFromContainer, container, selected);
				e.Handled = true;
			}
		}
	}

	private void ResetSelectedItemsAlgorithm()
	{
		if (!base.Items.IsEmpty)
		{
			_selectedItems.UsesItemHashCodes = base.Items.CollectionView.HasReliableHashCodes();
		}
	}

	internal void LocateSelectedItems(List<Tuple<int, int>> ranges = null, bool deselectMissingItems = false)
	{
		List<int> list = new List<int>(_selectedItems.Count);
		int num = 0;
		foreach (ItemInfo item in (IEnumerable<ItemInfo>)_selectedItems)
		{
			if (item.Index < 0)
			{
				num++;
			}
			else
			{
				list.Add(item.Index);
			}
		}
		int count = list.Count;
		list.Sort();
		ItemInfo itemInfo = new ItemInfo(null, ItemInfo.KeyContainer);
		int num2 = 0;
		while (num > 0 && num2 < base.Items.Count)
		{
			if (list.BinarySearch(0, count, num2, null) < 0)
			{
				itemInfo.Reset(base.Items[num2]);
				itemInfo.Index = num2;
				ItemInfo itemInfo2 = _selectedItems.FindMatch(itemInfo);
				if (itemInfo2 != null)
				{
					itemInfo2.Index = num2;
					list.Add(num2);
					num--;
				}
			}
			num2++;
		}
		if (ranges != null)
		{
			ranges.Clear();
			list.Sort();
			list.Add(-1);
			int num3 = -1;
			int num4 = -2;
			foreach (int item2 in list)
			{
				if (item2 == num4 + 1)
				{
					num4 = item2;
					continue;
				}
				if (num3 >= 0)
				{
					ranges.Add(new Tuple<int, int>(num3, num4 - num3 + 1));
				}
				num3 = (num4 = item2);
			}
		}
		if (!deselectMissingItems)
		{
			return;
		}
		foreach (ItemInfo item3 in (IEnumerable<ItemInfo>)_selectedItems)
		{
			if (item3.Index < 0)
			{
				item3.Container = ItemInfo.RemovedContainer;
				SelectionChange.Unselect(item3);
			}
		}
	}
}
