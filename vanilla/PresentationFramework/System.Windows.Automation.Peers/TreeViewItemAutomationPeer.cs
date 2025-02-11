using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Media;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.TreeViewItem" /> types to UI Automation.</summary>
public class TreeViewItemAutomationPeer : ItemsControlAutomationPeer, IExpandCollapseProvider, ISelectionItemProvider, IScrollItemProvider
{
	private delegate bool IteratorCallback(AutomationPeer peer);

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>The state (expanded or collapsed) of the control.</returns>
	ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState
	{
		get
		{
			TreeViewItem treeViewItem = (TreeViewItem)base.Owner;
			if (treeViewItem.HasItems)
			{
				if (!treeViewItem.IsExpanded)
				{
					return ExpandCollapseState.Collapsed;
				}
				return ExpandCollapseState.Expanded;
			}
			return ExpandCollapseState.LeafNode;
		}
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if the element is selected; otherwise false.</returns>
	bool ISelectionItemProvider.IsSelected => ((TreeViewItem)base.Owner).IsSelected;

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>The selection container.</returns>
	IRawElementProviderSimple ISelectionItemProvider.SelectionContainer
	{
		get
		{
			ItemsControl parentItemsControl = ((TreeViewItem)base.Owner).ParentItemsControl;
			if (parentItemsControl != null)
			{
				AutomationPeer automationPeer = UIElementAutomationPeer.FromElement(parentItemsControl);
				if (automationPeer != null)
				{
					return ProviderFromPeer(automationPeer);
				}
			}
			return null;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.TreeViewItemAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.TreeViewItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TreeViewItemAutomationPeer" />.</param>
	public TreeViewItemAutomationPeer(TreeViewItem owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.TreeViewItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TreeViewItemAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "TreeViewItem".</returns>
	protected override string GetClassNameCore()
	{
		return "TreeViewItem";
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.TreeViewItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TreeViewItemAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.TreeItem" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.TreeItem;
	}

	/// <summary>Gets the control pattern for the <see cref="T:System.Windows.Controls.TreeViewItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TreeViewItemAutomationPeer" />.</summary>
	/// <returns>If <paramref name="patternInterface" /> is <see cref="F:System.Windows.Automation.Peers.PatternInterface.ScrollItem" />, <see cref="F:System.Windows.Automation.Peers.PatternInterface.SelectionItem" />, or <see cref="F:System.Windows.Automation.Peers.PatternInterface.ExpandCollapse" />, this method returns the current instance of the <see cref="T:System.Windows.Automation.Peers.TreeViewItemAutomationPeer" />; otherwise, it returns null.</returns>
	/// <param name="patternInterface">One of the enumeration values.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		return patternInterface switch
		{
			PatternInterface.ExpandCollapse => this, 
			PatternInterface.SelectionItem => this, 
			PatternInterface.ScrollItem => this, 
			_ => base.GetPattern(patternInterface), 
		};
	}

	/// <summary>Gets the collection of child elements of the <see cref="T:System.Windows.Controls.TreeViewItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TreeViewItemAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetChildren" />.</summary>
	/// <returns>The collection of child elements.</returns>
	protected override List<AutomationPeer> GetChildrenCore()
	{
		List<AutomationPeer> children = null;
		ItemPeersStorage<ItemAutomationPeer> itemPeers = base.ItemPeers;
		base.ItemPeers = new ItemPeersStorage<ItemAutomationPeer>();
		if (base.Owner is TreeViewItem parent)
		{
			iterate(this, parent, delegate(AutomationPeer peer)
			{
				if (children == null)
				{
					children = new List<AutomationPeer>();
				}
				children.Add(peer);
				return false;
			}, base.ItemPeers, itemPeers);
		}
		return children;
	}

	private static bool iterate(TreeViewItemAutomationPeer logicalParentAp, DependencyObject parent, IteratorCallback callback, ItemPeersStorage<ItemAutomationPeer> dataChildren, ItemPeersStorage<ItemAutomationPeer> oldChildren)
	{
		bool flag = false;
		if (parent != null)
		{
			AutomationPeer automationPeer = null;
			int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
			for (int i = 0; i < childrenCount; i++)
			{
				if (flag)
				{
					break;
				}
				DependencyObject child = VisualTreeHelper.GetChild(parent, i);
				if (child != null && child is UIElement)
				{
					if (child is TreeViewItem)
					{
						object item = ((child is UIElement) ? (logicalParentAp.Owner as ItemsControl).GetItemOrContainerFromContainer(child as UIElement) : child);
						automationPeer = oldChildren[item];
						if (automationPeer == null)
						{
							automationPeer = logicalParentAp.GetPeerFromWeakRefStorage(item);
							if (automationPeer != null)
							{
								automationPeer.AncestorsInvalid = false;
								automationPeer.ChildrenValid = false;
							}
						}
						if (automationPeer == null)
						{
							automationPeer = logicalParentAp.CreateItemAutomationPeer(item);
						}
						if (automationPeer != null)
						{
							AutomationPeer wrapperPeer = (automationPeer as ItemAutomationPeer).GetWrapperPeer();
							if (wrapperPeer != null)
							{
								wrapperPeer.EventsSource = automationPeer;
							}
							if (dataChildren[item] == null && automationPeer is ItemAutomationPeer)
							{
								callback(automationPeer);
								dataChildren[item] = automationPeer as ItemAutomationPeer;
							}
						}
					}
					else
					{
						automationPeer = UIElementAutomationPeer.CreatePeerForElement((UIElement)child);
						if (automationPeer != null)
						{
							flag = callback(automationPeer);
						}
					}
					if (automationPeer == null)
					{
						flag = iterate(logicalParentAp, child, callback, dataChildren, oldChildren);
					}
				}
				else
				{
					flag = iterate(logicalParentAp, child, callback, dataChildren, oldChildren);
				}
			}
		}
		return flag;
	}

	/// <summary>Returns an <see cref="T:System.Windows.Automation.Peers.ItemAutomationPeer" /> for the specified object.</summary>
	/// <returns>An <see cref="T:System.Windows.Automation.Peers.ItemAutomationPeer" /> for the specified object.</returns>
	/// <param name="item">The item to get an <see cref="T:System.Windows.Automation.Peers.ItemAutomationPeer" /> for.</param>
	protected internal override ItemAutomationPeer FindOrCreateItemAutomationPeer(object item)
	{
		ItemAutomationPeer itemAutomationPeer = base.ItemPeers[item];
		AutomationPeer peer = this;
		if (base.EventsSource is TreeViewDataItemAutomationPeer)
		{
			peer = base.EventsSource as TreeViewDataItemAutomationPeer;
		}
		if (itemAutomationPeer == null)
		{
			itemAutomationPeer = GetPeerFromWeakRefStorage(item);
		}
		if (itemAutomationPeer == null)
		{
			itemAutomationPeer = CreateItemAutomationPeer(item);
			itemAutomationPeer?.TrySetParentInfo(peer);
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

	internal override bool IsPropertySupportedByControlForFindItem(int id)
	{
		if (base.IsPropertySupportedByControlForFindItem(id))
		{
			return true;
		}
		if (SelectionItemPatternIdentifiers.IsSelectedProperty.Id == id)
		{
			return true;
		}
		return false;
	}

	internal override object GetSupportedPropertyValue(ItemAutomationPeer itemPeer, int propertyId)
	{
		if (SelectionItemPatternIdentifiers.IsSelectedProperty.Id == propertyId)
		{
			if (itemPeer.GetPattern(PatternInterface.SelectionItem) is ISelectionItemProvider selectionItemProvider)
			{
				return selectionItemProvider.IsSelected;
			}
			return null;
		}
		return base.GetSupportedPropertyValue(itemPeer, propertyId);
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Automation.Peers.TreeViewDataItemAutomationPeer" /> for a data item in the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection of this <see cref="T:System.Windows.Controls.TreeView" />.</summary>
	/// <returns>A new instance of the <see cref="T:System.Windows.Automation.Peers.TreeViewDataItemAutomationPeer" /> for <paramref name="item" />.</returns>
	/// <param name="item">The data item that is associated with the <see cref="T:System.Windows.Automation.Peers.TreeViewDataItemAutomationPeer" />.</param>
	protected override ItemAutomationPeer CreateItemAutomationPeer(object item)
	{
		return new TreeViewDataItemAutomationPeer(item, this, base.EventsSource as TreeViewDataItemAutomationPeer);
	}

	internal override IDisposable UpdateChildren()
	{
		if (base.EventsSource is TreeViewDataItemAutomationPeer treeViewDataItemAutomationPeer)
		{
			treeViewDataItemAutomationPeer.UpdateChildrenInternal(5);
		}
		else
		{
			UpdateChildrenInternal(5);
		}
		base.WeakRefElementProxyStorage.PurgeWeakRefCollection();
		return null;
	}

	internal void AddDataPeerInfo(TreeViewDataItemAutomationPeer dataPeer)
	{
		base.EventsSource = dataPeer;
		UpdateWeakRefStorageFromDataPeer();
	}

	internal void UpdateWeakRefStorageFromDataPeer()
	{
		if (base.EventsSource is TreeViewDataItemAutomationPeer)
		{
			if ((base.EventsSource as TreeViewDataItemAutomationPeer).WeakRefElementProxyStorageCache == null)
			{
				(base.EventsSource as TreeViewDataItemAutomationPeer).WeakRefElementProxyStorageCache = base.WeakRefElementProxyStorage;
			}
			else if (base.WeakRefElementProxyStorage.Count == 0)
			{
				base.WeakRefElementProxyStorage = (base.EventsSource as TreeViewDataItemAutomationPeer).WeakRefElementProxyStorageCache;
			}
		}
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void IExpandCollapseProvider.Expand()
	{
		if (!IsEnabled())
		{
			throw new ElementNotEnabledException();
		}
		TreeViewItem obj = (TreeViewItem)base.Owner;
		if (!obj.HasItems)
		{
			throw new InvalidOperationException(SR.UIA_OperationCannotBePerformed);
		}
		obj.IsExpanded = true;
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void IExpandCollapseProvider.Collapse()
	{
		if (!IsEnabled())
		{
			throw new ElementNotEnabledException();
		}
		TreeViewItem obj = (TreeViewItem)base.Owner;
		if (!obj.HasItems)
		{
			throw new InvalidOperationException(SR.UIA_OperationCannotBePerformed);
		}
		obj.IsExpanded = false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal void RaiseExpandCollapseAutomationEvent(bool oldValue, bool newValue)
	{
		if (base.EventsSource is TreeViewDataItemAutomationPeer)
		{
			(base.EventsSource as TreeViewDataItemAutomationPeer).RaiseExpandCollapseAutomationEvent(oldValue, newValue);
		}
		else
		{
			RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty, oldValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed, newValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed);
		}
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void ISelectionItemProvider.Select()
	{
		((TreeViewItem)base.Owner).IsSelected = true;
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void ISelectionItemProvider.AddToSelection()
	{
		TreeView parentTreeView = ((TreeViewItem)base.Owner).ParentTreeView;
		if (parentTreeView == null || (parentTreeView.SelectedItem != null && parentTreeView.SelectedContainer != base.Owner))
		{
			throw new InvalidOperationException(SR.UIA_OperationCannotBePerformed);
		}
		((TreeViewItem)base.Owner).IsSelected = true;
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void ISelectionItemProvider.RemoveFromSelection()
	{
		((TreeViewItem)base.Owner).IsSelected = false;
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void IScrollItemProvider.ScrollIntoView()
	{
		((TreeViewItem)base.Owner).BringIntoView();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal void RaiseAutomationIsSelectedChanged(bool isSelected)
	{
		if (base.EventsSource is TreeViewDataItemAutomationPeer)
		{
			(base.EventsSource as TreeViewDataItemAutomationPeer).RaiseAutomationIsSelectedChanged(isSelected);
		}
		else
		{
			RaisePropertyChangedEvent(SelectionItemPatternIdentifiers.IsSelectedProperty, !isSelected, isSelected);
		}
	}

	internal void RaiseAutomationSelectionEvent(AutomationEvents eventId)
	{
		if (base.EventsSource != null)
		{
			base.EventsSource.RaiseAutomationEvent(eventId);
		}
		else
		{
			RaiseAutomationEvent(eventId);
		}
	}
}
