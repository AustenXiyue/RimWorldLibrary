using System.Collections.Generic;
using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.TreeView" /> types to UI Automation.</summary>
public class TreeViewAutomationPeer : ItemsControlAutomationPeer, ISelectionProvider
{
	/// <summary>Gets a value that specifies whether the UI Automation provider allows more than one child element to be selected concurrently.</summary>
	/// <returns>true if multiple selection is allowed; otherwise false.</returns>
	bool ISelectionProvider.CanSelectMultiple => false;

	/// <summary>Gets a value that specifies whether the UI Automation provider requires at least one child element to be selected.</summary>
	/// <returns>true if selection is required; otherwise false.</returns>
	bool ISelectionProvider.IsSelectionRequired => false;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.TreeViewAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.TreeView" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TreeViewAutomationPeer" />.</param>
	public TreeViewAutomationPeer(TreeView owner)
		: base(owner)
	{
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>
	///   <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Tree" />.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Tree;
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "TreeView".</returns>
	protected override string GetClassNameCore()
	{
		return "TreeView";
	}

	/// <summary>Gets the control pattern for the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" />.</summary>
	/// <returns>The current instance of the <see cref="T:System.Windows.Automation.Peers.TreeViewItemAutomationPeer" />, or null.</returns>
	/// <param name="patternInterface">One of the enumeration values.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		switch (patternInterface)
		{
		case PatternInterface.Selection:
			return this;
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
		}
		return base.GetPattern(patternInterface);
	}

	/// <summary>Gets the collection of child elements of the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetChildren" />.</summary>
	/// <returns>A collection of <see cref="T:System.Windows.Automation.Peers.TreeViewItemAutomationPeer" /> elements, or null if the <see cref="T:System.Windows.Controls.TreeView" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TreeViewAutomationPeer" /> is empty.</returns>
	protected override List<AutomationPeer> GetChildrenCore()
	{
		if (IsVirtualized)
		{
			return base.GetChildrenCore();
		}
		ItemsControl itemsControl = (ItemsControl)base.Owner;
		ItemCollection items = itemsControl.Items;
		List<AutomationPeer> list = null;
		ItemPeersStorage<ItemAutomationPeer> itemPeers = base.ItemPeers;
		base.ItemPeers = new ItemPeersStorage<ItemAutomationPeer>();
		if (items.Count > 0)
		{
			list = new List<AutomationPeer>(items.Count);
			for (int i = 0; i < items.Count; i++)
			{
				if (!(itemsControl.ItemContainerGenerator.ContainerFromIndex(i) is TreeViewItem))
				{
					continue;
				}
				ItemAutomationPeer itemAutomationPeer = itemPeers[items[i]];
				if (itemAutomationPeer == null)
				{
					itemAutomationPeer = CreateItemAutomationPeer(items[i]);
				}
				if (itemAutomationPeer != null)
				{
					AutomationPeer wrapperPeer = itemAutomationPeer.GetWrapperPeer();
					if (wrapperPeer != null)
					{
						wrapperPeer.EventsSource = itemAutomationPeer;
					}
				}
				if (base.ItemPeers[items[i]] == null)
				{
					list.Add(itemAutomationPeer);
					base.ItemPeers[items[i]] = itemAutomationPeer;
				}
			}
			return list;
		}
		return null;
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Automation.Peers.TreeViewDataItemAutomationPeer" /> for a data item in the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection of this <see cref="T:System.Windows.Controls.TreeView" />.</summary>
	/// <returns>A new instance of the <see cref="T:System.Windows.Automation.Peers.TreeViewDataItemAutomationPeer" /> for <paramref name="item" />.</returns>
	/// <param name="item">The data item that is associated with the <see cref="T:System.Windows.Automation.Peers.TreeViewDataItemAutomationPeer" />.</param>
	protected override ItemAutomationPeer CreateItemAutomationPeer(object item)
	{
		return new TreeViewDataItemAutomationPeer(item, this, null);
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

	/// <summary>Retrieves a UI Automation provider for each child element that is selected.</summary>
	/// <returns>A collection of UI Automation providers. </returns>
	IRawElementProviderSimple[] ISelectionProvider.GetSelection()
	{
		IRawElementProviderSimple[] array = null;
		TreeViewItem selectedContainer = ((TreeView)base.Owner).SelectedContainer;
		if (selectedContainer != null)
		{
			AutomationPeer automationPeer = UIElementAutomationPeer.FromElement(selectedContainer);
			if (automationPeer.EventsSource != null)
			{
				automationPeer = automationPeer.EventsSource;
			}
			if (automationPeer != null)
			{
				array = new IRawElementProviderSimple[1] { ProviderFromPeer(automationPeer) };
			}
		}
		if (array == null)
		{
			array = Array.Empty<IRawElementProviderSimple>();
		}
		return array;
	}
}
