using System.Collections;
using System.Collections.Generic;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using MS.Internal.Controls;
using MS.Internal.Data;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.GroupItem" /> types to UI Automation.</summary>
public class GroupItemAutomationPeer : FrameworkElementAutomationPeer
{
	private AutomationPeer _expanderPeer;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.GroupItemAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.GroupItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.GroupItemAutomationPeer" />.</param>
	public GroupItemAutomationPeer(GroupItem owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.GroupItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.GroupItemAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "GroupItem".</returns>
	protected override string GetClassNameCore()
	{
		return "GroupItem";
	}

	protected override int GetPositionInSetCore()
	{
		int num = base.GetPositionInSetCore();
		if (num == -1 && ((GroupItem)base.Owner).GetValue(ItemContainerGenerator.ItemForItemContainerProperty) is CollectionViewGroupInternal collectionViewGroupInternal)
		{
			CollectionViewGroup parent = collectionViewGroupInternal.Parent;
			if (parent != null)
			{
				num = parent.Items.IndexOf(collectionViewGroupInternal) + 1;
			}
		}
		return num;
	}

	protected override int GetSizeOfSetCore()
	{
		int num = base.GetSizeOfSetCore();
		if (num == -1 && ((GroupItem)base.Owner).GetValue(ItemContainerGenerator.ItemForItemContainerProperty) is CollectionViewGroupInternal collectionViewGroupInternal)
		{
			CollectionViewGroup parent = collectionViewGroupInternal.Parent;
			if (parent != null)
			{
				num = parent.Items.Count;
			}
		}
		return num;
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.GroupItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.GroupItemAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Group" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Group;
	}

	/// <summary>Gets the control pattern for the <see cref="T:System.Windows.Controls.GroupItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.GroupItemAutomationPeer" />.</summary>
	/// <returns>If <paramref name="pattern" /> is <see cref="F:System.Windows.Automation.Peers.PatternInterface.ExpandCollapse" /> and the <see cref="T:System.Windows.Controls.GroupItem" />that is associated with this <see cref="T:System.Windows.Automation.Peers.GroupItemAutomationPeer" /> contains an <see cref="T:System.Windows.Controls.Expander" />, this method returns a reference to the current instance of the <see cref="T:System.Windows.Automation.Peers.GroupItemAutomationPeer" />.  Otherwise, this method calls the base implementation on <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" /> which returns null.</returns>
	/// <param name="patternInterface">One of the enumeration values.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		if (patternInterface == PatternInterface.ExpandCollapse)
		{
			GroupItem groupItem = (GroupItem)base.Owner;
			if (groupItem.Expander != null)
			{
				AutomationPeer automationPeer = UIElementAutomationPeer.CreatePeerForElement(groupItem.Expander);
				if (automationPeer != null && automationPeer is IExpandCollapseProvider)
				{
					automationPeer.EventsSource = this;
					return (IExpandCollapseProvider)automationPeer;
				}
			}
		}
		return base.GetPattern(patternInterface);
	}

	/// <summary>Gets the collection of child elements of the <see cref="T:System.Windows.Controls.GroupItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.GroupItemAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetChildren" />.</summary>
	/// <returns>The collection of child elements.</returns>
	protected override List<AutomationPeer> GetChildrenCore()
	{
		GroupItem groupItem = (GroupItem)base.Owner;
		ItemsControl itemsControl = ItemsControl.ItemsControlFromItemContainer(base.Owner);
		if (itemsControl != null && itemsControl.CreateAutomationPeer() is ItemsControlAutomationPeer itemsControlAutomationPeer)
		{
			List<AutomationPeer> list = new List<AutomationPeer>();
			bool useNetFx472CompatibleAccessibilityFeatures = AccessibilitySwitches.UseNetFx472CompatibleAccessibilityFeatures;
			if (!useNetFx472CompatibleAccessibilityFeatures && groupItem.Expander != null)
			{
				_expanderPeer = UIElementAutomationPeer.CreatePeerForElement(groupItem.Expander);
				if (_expanderPeer != null)
				{
					_expanderPeer.EventsSource = this;
					_expanderPeer.GetChildren();
				}
			}
			Panel itemsHost = groupItem.ItemsHost;
			if (itemsHost == null)
			{
				if (_expanderPeer == null)
				{
					return null;
				}
				list.Add(_expanderPeer);
				return list;
			}
			UIElementCollection children = itemsHost.Children;
			ItemPeersStorage<ItemAutomationPeer> itemPeersStorage = new ItemPeersStorage<ItemAutomationPeer>();
			{
				foreach (UIElement item in (IEnumerable)children)
				{
					if (!((IGeneratorHost)itemsControl).IsItemItsOwnContainer((object)item))
					{
						if (!(item.CreateAutomationPeer() is UIElementAutomationPeer uIElementAutomationPeer))
						{
							continue;
						}
						list.Add(uIElementAutomationPeer);
						if (useNetFx472CompatibleAccessibilityFeatures)
						{
							if (itemsControlAutomationPeer.RecentlyRealizedPeers.Count > 0 && AncestorsInvalid && uIElementAutomationPeer is GroupItemAutomationPeer groupItemAutomationPeer)
							{
								groupItemAutomationPeer.InvalidateGroupItemPeersContainingRecentlyRealizedPeers(itemsControlAutomationPeer.RecentlyRealizedPeers);
							}
						}
						else if (AncestorsInvalid && uIElementAutomationPeer is GroupItemAutomationPeer groupItemAutomationPeer2)
						{
							groupItemAutomationPeer2.AncestorsInvalid = true;
							groupItemAutomationPeer2.ChildrenValid = true;
						}
						continue;
					}
					object obj = itemsControl.ItemContainerGenerator.ItemFromContainer(item);
					if (obj == DependencyProperty.UnsetValue)
					{
						continue;
					}
					ItemAutomationPeer peer = (useNetFx472CompatibleAccessibilityFeatures ? itemsControlAutomationPeer.ItemPeers[obj] : itemsControlAutomationPeer.ReusablePeerFor(obj));
					peer = itemsControlAutomationPeer.ReusePeerForItem(peer, obj);
					if (peer != null)
					{
						if (useNetFx472CompatibleAccessibilityFeatures)
						{
							int num = itemsControlAutomationPeer.RecentlyRealizedPeers.IndexOf(peer);
							if (num >= 0)
							{
								itemsControlAutomationPeer.RecentlyRealizedPeers.RemoveAt(num);
							}
						}
					}
					else
					{
						peer = itemsControlAutomationPeer.CreateItemAutomationPeerInternal(obj);
					}
					if (peer != null)
					{
						AutomationPeer wrapperPeer = peer.GetWrapperPeer();
						if (wrapperPeer != null)
						{
							wrapperPeer.EventsSource = peer;
							if (peer.ChildrenValid && peer.Children == null && AncestorsInvalid)
							{
								peer.AncestorsInvalid = true;
								wrapperPeer.AncestorsInvalid = true;
							}
						}
					}
					bool flag = itemsControlAutomationPeer.ItemPeers[obj] == null;
					if (peer != null && (flag || (peer.GetParent() == this && itemPeersStorage[obj] == null)))
					{
						list.Add(peer);
						itemPeersStorage[obj] = peer;
						if (flag)
						{
							itemsControlAutomationPeer.ItemPeers[obj] = peer;
						}
					}
				}
				return list;
			}
		}
		return null;
	}

	internal void InvalidateGroupItemPeersContainingRecentlyRealizedPeers(List<ItemAutomationPeer> recentlyRealizedPeers)
	{
		ItemsControl itemsControl = ItemsControl.ItemsControlFromItemContainer(base.Owner);
		if (itemsControl == null || !(itemsControl.ItemContainerGenerator.ItemFromContainer(base.Owner) is CollectionViewGroupInternal collectionViewGroupInternal))
		{
			return;
		}
		for (int i = 0; i < recentlyRealizedPeers.Count; i++)
		{
			object item = recentlyRealizedPeers[i].Item;
			if (collectionViewGroupInternal.LeafIndexOf(item) >= 0)
			{
				AncestorsInvalid = true;
				base.ChildrenValid = true;
			}
		}
	}

	protected override void SetFocusCore()
	{
		GroupItem groupItem = (GroupItem)base.Owner;
		if (!AccessibilitySwitches.UseNetFx472CompatibleAccessibilityFeatures && groupItem.Expander != null)
		{
			ToggleButton expanderToggleButton = groupItem.Expander.ExpanderToggleButton;
			if (expanderToggleButton == null || !expanderToggleButton.Focus())
			{
				throw new InvalidOperationException(SR.SetFocusFailed);
			}
		}
		else
		{
			base.SetFocusCore();
		}
	}

	protected override bool IsKeyboardFocusableCore()
	{
		if (_expanderPeer != null)
		{
			return _expanderPeer.IsKeyboardFocusable();
		}
		return base.IsKeyboardFocusableCore();
	}

	protected override bool HasKeyboardFocusCore()
	{
		if (_expanderPeer != null)
		{
			return _expanderPeer.HasKeyboardFocus();
		}
		return base.HasKeyboardFocusCore();
	}
}
