using System.Collections;
using System.Collections.Generic;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using MS.Internal.Automation;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.ItemsControl" /> types to UI Automation. </summary>
public abstract class ItemsControlAutomationPeer : FrameworkElementAutomationPeer, IItemContainerProvider
{
	private class UpdateChildrenHelper : IDisposable
	{
		private ItemsControlAutomationPeer _peer;

		private ItemPeersStorage<ItemAutomationPeer> _oldChildren;

		internal UpdateChildrenHelper(ItemsControlAutomationPeer peer)
		{
			_peer = peer;
			_oldChildren = peer.ItemPeers;
		}

		void IDisposable.Dispose()
		{
			if (_peer != null)
			{
				_peer.ClearReusablePeers(_oldChildren);
				_peer = null;
			}
		}
	}

	private ItemPeersStorage<ItemAutomationPeer> _dataChildren = new ItemPeersStorage<ItemAutomationPeer>();

	private ItemPeersStorage<ItemAutomationPeer> _reusablePeers;

	private ItemPeersStorage<WeakReference> _WeakRefElementProxyStorage = new ItemPeersStorage<WeakReference>();

	private List<ItemAutomationPeer> _recentlyRealizedPeers;

	private RecyclableWrapper _recyclableWrapperCache;

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Automation.Peers.ItemsControlAutomationPeer" /> should return <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> objects for child items that are not virtualized. </summary>
	/// <returns>true if the <see cref="T:System.Windows.Automation.Peers.ItemsControlAutomationPeer" /> should return <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> objects for child items that are not virtualized; false if the <see cref="T:System.Windows.Automation.Peers.ItemsControlAutomationPeer" /> should return <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> objects all child items. </returns>
	protected virtual bool IsVirtualized => ItemContainerPatternIdentifiers.Pattern != null;

	internal ItemPeersStorage<ItemAutomationPeer> ItemPeers
	{
		get
		{
			return _dataChildren;
		}
		set
		{
			_dataChildren = value;
		}
	}

	internal ItemPeersStorage<WeakReference> WeakRefElementProxyStorage
	{
		get
		{
			return _WeakRefElementProxyStorage;
		}
		set
		{
			_WeakRefElementProxyStorage = value;
		}
	}

	internal List<ItemAutomationPeer> RecentlyRealizedPeers
	{
		get
		{
			if (_recentlyRealizedPeers == null)
			{
				_recentlyRealizedPeers = new List<ItemAutomationPeer>();
			}
			return _recentlyRealizedPeers;
		}
	}

	/// <summary>Provides initialization for base class values when called by the constructor of a derived class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.ItemsControl" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ItemsControlAutomationPeer" />.</param>
	protected ItemsControlAutomationPeer(ItemsControl owner)
		: base(owner)
	{
	}

	/// <summary>Gets the control pattern for the <see cref="T:System.Windows.Controls.ItemsControl" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ItemsControlAutomationPeer" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Automation.Peers.ScrollViewerAutomationPeer" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ItemsControlAutomationPeer" />.</returns>
	/// <param name="patternInterface">One of the enumeration values.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		switch (patternInterface)
		{
		case PatternInterface.Scroll:
		{
			ItemsControl itemsControl = (ItemsControl)base.Owner;
			if (itemsControl.ScrollHost != null)
			{
				AutomationPeer automationPeer = UIElementAutomationPeer.CreatePeerForElement(itemsControl.ScrollHost);
				if (automationPeer != null && automationPeer is IScrollProvider)
				{
					automationPeer.EventsSource = this;
					return (IScrollProvider)automationPeer;
				}
			}
			break;
		}
		case PatternInterface.ItemContainer:
			if (base.Owner is ItemsControl)
			{
				return this;
			}
			return null;
		}
		return base.GetPattern(patternInterface);
	}

	/// <summary>Gets the collection of child elements of the <see cref="T:System.Windows.Controls.ItemsControl" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ItemsControlAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetChildren" />.</summary>
	/// <returns>The collection of child elements.</returns>
	protected override List<AutomationPeer> GetChildrenCore()
	{
		List<AutomationPeer> list = null;
		ItemPeersStorage<ItemAutomationPeer> dataChildren = _dataChildren;
		_dataChildren = new ItemPeersStorage<ItemAutomationPeer>();
		ItemsControl itemsControl = (ItemsControl)base.Owner;
		ItemCollection items = itemsControl.Items;
		Panel itemsHost = itemsControl.ItemsHost;
		IList list2 = null;
		bool useNetFx472CompatibleAccessibilityFeatures = AccessibilitySwitches.UseNetFx472CompatibleAccessibilityFeatures;
		if (itemsControl.IsGrouping)
		{
			if (itemsHost == null)
			{
				return null;
			}
			if (!useNetFx472CompatibleAccessibilityFeatures)
			{
				_reusablePeers = dataChildren;
			}
			list2 = itemsHost.Children;
			list = new List<AutomationPeer>(list2.Count);
			{
				foreach (UIElement item in list2)
				{
					if (!(item.CreateAutomationPeer() is UIElementAutomationPeer uIElementAutomationPeer))
					{
						continue;
					}
					list.Add(uIElementAutomationPeer);
					if (useNetFx472CompatibleAccessibilityFeatures)
					{
						if (_recentlyRealizedPeers != null && _recentlyRealizedPeers.Count > 0 && AncestorsInvalid && uIElementAutomationPeer is GroupItemAutomationPeer groupItemAutomationPeer)
						{
							groupItemAutomationPeer.InvalidateGroupItemPeersContainingRecentlyRealizedPeers(_recentlyRealizedPeers);
						}
					}
					else if (AncestorsInvalid && uIElementAutomationPeer is GroupItemAutomationPeer groupItemAutomationPeer2)
					{
						groupItemAutomationPeer2.AncestorsInvalid = true;
						groupItemAutomationPeer2.ChildrenValid = true;
					}
				}
				return list;
			}
		}
		if (items.Count > 0)
		{
			if (IsVirtualized)
			{
				if (itemsHost == null)
				{
					return null;
				}
				list2 = itemsHost.Children;
			}
			else
			{
				list2 = items;
			}
			list = new List<AutomationPeer>(list2.Count);
			{
				foreach (object item2 in list2)
				{
					object obj;
					if (IsVirtualized)
					{
						obj = ((item2 is DependencyObject container) ? itemsControl.ItemContainerGenerator.ItemFromContainer(container) : null);
						if (obj == DependencyProperty.UnsetValue)
						{
							continue;
						}
					}
					else
					{
						obj = item2;
					}
					ItemAutomationPeer peer = dataChildren[obj];
					peer = ReusePeerForItem(peer, obj);
					if (peer == null)
					{
						peer = CreateItemAutomationPeer(obj);
					}
					if (peer != null)
					{
						AutomationPeer wrapperPeer = peer.GetWrapperPeer();
						if (wrapperPeer != null)
						{
							wrapperPeer.EventsSource = peer;
						}
					}
					if (peer != null && _dataChildren[obj] == null)
					{
						list.Add(peer);
						_dataChildren[obj] = peer;
					}
				}
				return list;
			}
		}
		return null;
	}

	internal ItemAutomationPeer ReusePeerForItem(ItemAutomationPeer peer, object item)
	{
		if (peer == null)
		{
			peer = GetPeerFromWeakRefStorage(item);
			if (peer != null)
			{
				peer.AncestorsInvalid = false;
				peer.ChildrenValid = false;
			}
		}
		peer?.ReuseForItem(item);
		return peer;
	}

	internal void AddProxyToWeakRefStorage(WeakReference wr, ItemAutomationPeer itemPeer)
	{
		if ((base.Owner as ItemsControl).Items != null && GetPeerFromWeakRefStorage(itemPeer.Item) == null)
		{
			WeakRefElementProxyStorage[itemPeer.Item] = wr;
		}
	}

	/// <summary>Retrieves an element by the specified property value.</summary>
	/// <returns>The first item that matches the search criterion; otherwise, null if no items match.</returns>
	/// <param name="startAfter">The item in the container after which to begin the search.</param>
	/// <param name="propertyId">The property that contains the value to retrieve.</param>
	/// <param name="value">The value to retrieve.</param>
	IRawElementProviderSimple IItemContainerProvider.FindItemByProperty(IRawElementProviderSimple startAfter, int propertyId, object value)
	{
		ResetChildrenCache();
		if (propertyId != 0 && !IsPropertySupportedByControlForFindItem(propertyId))
		{
			throw new ArgumentException(SR.PropertyNotSupported);
		}
		ItemsControl itemsControl = (ItemsControl)base.Owner;
		ItemCollection itemCollection = null;
		if (itemsControl != null)
		{
			itemCollection = itemsControl.Items;
		}
		if (itemCollection != null && itemCollection.Count > 0)
		{
			ItemAutomationPeer itemAutomationPeer = null;
			if (startAfter != null)
			{
				itemAutomationPeer = PeerFromProvider(startAfter) as ItemAutomationPeer;
				if (itemAutomationPeer == null)
				{
					return null;
				}
			}
			int num = 0;
			if (itemAutomationPeer != null)
			{
				if (itemAutomationPeer.Item == null)
				{
					throw new InvalidOperationException(SR.InavalidStartItem);
				}
				num = itemCollection.IndexOf(itemAutomationPeer.Item) + 1;
				if (num == 0 || num == itemCollection.Count)
				{
					return null;
				}
			}
			if (propertyId == 0)
			{
				for (int i = num; i < itemCollection.Count; i++)
				{
					if (itemCollection.IndexOf(itemCollection[i]) == i)
					{
						return ProviderFromPeer(FindOrCreateItemAutomationPeer(itemCollection[i]));
					}
				}
			}
			object obj = null;
			for (int j = num; j < itemCollection.Count; j++)
			{
				ItemAutomationPeer itemAutomationPeer2 = FindOrCreateItemAutomationPeer(itemCollection[j]);
				if (itemAutomationPeer2 == null)
				{
					continue;
				}
				try
				{
					obj = GetSupportedPropertyValue(itemAutomationPeer2, propertyId);
				}
				catch (Exception ex)
				{
					if (ex is ElementNotAvailableException)
					{
						continue;
					}
				}
				if (value == null || obj == null)
				{
					if (obj == null && value == null && itemCollection.IndexOf(itemCollection[j]) == j)
					{
						return ProviderFromPeer(itemAutomationPeer2);
					}
				}
				else if (value.Equals(obj) && itemCollection.IndexOf(itemCollection[j]) == j)
				{
					return ProviderFromPeer(itemAutomationPeer2);
				}
			}
		}
		return null;
	}

	internal virtual bool IsPropertySupportedByControlForFindItem(int id)
	{
		return IsPropertySupportedByControlForFindItemInternal(id);
	}

	internal static bool IsPropertySupportedByControlForFindItemInternal(int id)
	{
		if (AutomationElementIdentifiers.NameProperty.Id == id)
		{
			return true;
		}
		if (AutomationElementIdentifiers.AutomationIdProperty.Id == id)
		{
			return true;
		}
		if (AutomationElementIdentifiers.ControlTypeProperty.Id == id)
		{
			return true;
		}
		return false;
	}

	internal virtual object GetSupportedPropertyValue(ItemAutomationPeer itemPeer, int propertyId)
	{
		return GetSupportedPropertyValueInternal(itemPeer, propertyId);
	}

	internal static object GetSupportedPropertyValueInternal(AutomationPeer itemPeer, int propertyId)
	{
		return itemPeer.GetPropertyValue(propertyId);
	}

	/// <summary>Returns an <see cref="T:System.Windows.Automation.Peers.ItemAutomationPeer" /> for the specified object.</summary>
	/// <returns>An <see cref="T:System.Windows.Automation.Peers.ItemAutomationPeer" /> for the specified object.</returns>
	/// <param name="item">The item to get an <see cref="T:System.Windows.Automation.Peers.ItemAutomationPeer" /> for.</param>
	protected internal virtual ItemAutomationPeer FindOrCreateItemAutomationPeer(object item)
	{
		ItemAutomationPeer itemAutomationPeer = ItemPeers[item];
		if (itemAutomationPeer == null)
		{
			itemAutomationPeer = GetPeerFromWeakRefStorage(item);
		}
		if (itemAutomationPeer == null)
		{
			itemAutomationPeer = CreateItemAutomationPeer(item);
			itemAutomationPeer?.TrySetParentInfo(this);
		}
		if (itemAutomationPeer != null)
		{
			AutomationPeer wrapperPeer = itemAutomationPeer.GetWrapperPeer();
			if (wrapperPeer != null)
			{
				wrapperPeer.EventsSource = itemAutomationPeer;
			}
		}
		return itemAutomationPeer;
	}

	internal ItemAutomationPeer CreateItemAutomationPeerInternal(object item)
	{
		return CreateItemAutomationPeer(item);
	}

	/// <summary>When overridden in a derived class, creates a new instance of the <see cref="T:System.Windows.Automation.Peers.ItemAutomationPeer" /> for a data item in the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection of this <see cref="T:System.Windows.Controls.ItemsControl" />.</summary>
	/// <returns>The new <see cref="T:System.Windows.Automation.Peers.ItemAutomationPeer" /> created.</returns>
	/// <param name="item">The data item that is associated with this <see cref="T:System.Windows.Automation.Peers.ItemAutomationPeer" />.</param>
	protected abstract ItemAutomationPeer CreateItemAutomationPeer(object item);

	internal RecyclableWrapper GetRecyclableWrapperPeer(object item)
	{
		ItemsControl itemsControl = (ItemsControl)base.Owner;
		if (_recyclableWrapperCache == null)
		{
			_recyclableWrapperCache = new RecyclableWrapper(itemsControl, item);
		}
		else
		{
			_recyclableWrapperCache.LinkItem(item);
		}
		return _recyclableWrapperCache;
	}

	internal override IDisposable UpdateChildren()
	{
		UpdateChildrenInternal(5);
		WeakRefElementProxyStorage.PurgeWeakRefCollection();
		if (!AccessibilitySwitches.UseNetFx472CompatibleAccessibilityFeatures)
		{
			return new UpdateChildrenHelper(this);
		}
		return null;
	}

	internal ItemAutomationPeer GetPeerFromWeakRefStorage(object item)
	{
		ItemAutomationPeer itemAutomationPeer = null;
		WeakReference weakReference = WeakRefElementProxyStorage[item];
		if (weakReference != null)
		{
			if (weakReference.Target is ElementProxy provider)
			{
				itemAutomationPeer = PeerFromProvider(provider) as ItemAutomationPeer;
				if (itemAutomationPeer == null)
				{
					WeakRefElementProxyStorage.Remove(item);
				}
			}
			else
			{
				WeakRefElementProxyStorage.Remove(item);
			}
		}
		return itemAutomationPeer;
	}

	internal AutomationPeer GetExistingPeerByItem(object item, bool checkInWeakRefStorage)
	{
		AutomationPeer automationPeer = null;
		if (checkInWeakRefStorage)
		{
			automationPeer = GetPeerFromWeakRefStorage(item);
		}
		if (automationPeer == null)
		{
			automationPeer = ItemPeers[item];
		}
		return automationPeer;
	}

	internal ItemAutomationPeer ReusablePeerFor(object item)
	{
		if (_reusablePeers != null)
		{
			return _reusablePeers[item];
		}
		return ItemPeers[item];
	}

	private void ClearReusablePeers(ItemPeersStorage<ItemAutomationPeer> oldChildren)
	{
		if (_reusablePeers == oldChildren)
		{
			_reusablePeers = null;
		}
	}
}
