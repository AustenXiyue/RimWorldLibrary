using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.Primitives.Selector" /> types to UI Automation.</summary>
public abstract class SelectorAutomationPeer : ItemsControlAutomationPeer, ISelectionProvider
{
	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if multiple selection is allowed; otherwise false.</returns>
	bool ISelectionProvider.CanSelectMultiple => ((Selector)base.Owner).CanSelectMultiple;

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>Returns S_OK if successful, or an error value otherwise.</returns>
	bool ISelectionProvider.IsSelectionRequired => false;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.SelectorAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.Primitives.Selector" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.SelectorAutomationPeer" />.</param>
	protected SelectorAutomationPeer(Selector owner)
		: base(owner)
	{
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.Primitives.Selector" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.SelectorAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.List" /> enumeration value. </returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.List;
	}

	/// <summary>Gets the control pattern for the <see cref="T:System.Windows.Controls.Primitives.Selector" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.SelectorAutomationPeer" />.</summary>
	/// <returns>If <paramref name="patternInterface" /> is <see cref="F:System.Windows.Automation.Peers.PatternInterface.Selection" />, this method returns a pointer to the current instance; otherwise null.</returns>
	/// <param name="patternInterface">A value in the enumeration.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		if (patternInterface == PatternInterface.Selection)
		{
			return this;
		}
		return base.GetPattern(patternInterface);
	}

	internal override bool IsPropertySupportedByControlForFindItem(int id)
	{
		return IsPropertySupportedByControlForFindItemInternal(id);
	}

	internal new static bool IsPropertySupportedByControlForFindItemInternal(int id)
	{
		if (ItemsControlAutomationPeer.IsPropertySupportedByControlForFindItemInternal(id))
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
		return GetSupportedPropertyValueInternal(itemPeer, propertyId);
	}

	internal new static object GetSupportedPropertyValueInternal(AutomationPeer itemPeer, int propertyId)
	{
		if (SelectionItemPatternIdentifiers.IsSelectedProperty.Id == propertyId)
		{
			if (itemPeer.GetPattern(PatternInterface.SelectionItem) is ISelectionItemProvider selectionItemProvider)
			{
				return selectionItemProvider.IsSelected;
			}
			return null;
		}
		return ItemsControlAutomationPeer.GetSupportedPropertyValueInternal(itemPeer, propertyId);
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>A collection of UI Automation providers. true if multiple selection is allowed; otherwise false.</returns>
	IRawElementProviderSimple[] ISelectionProvider.GetSelection()
	{
		Selector selector = (Selector)base.Owner;
		int count = selector._selectedItems.Count;
		int count2 = selector.Items.Count;
		if (count > 0 && count2 > 0)
		{
			List<IRawElementProviderSimple> list = new List<IRawElementProviderSimple>(count);
			for (int i = 0; i < count; i++)
			{
				if (FindOrCreateItemAutomationPeer(selector._selectedItems[i].Item) is SelectorItemAutomationPeer peer)
				{
					list.Add(ProviderFromPeer(peer));
				}
			}
			return list.ToArray();
		}
		return null;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal void RaiseSelectionEvents(SelectionChangedEventArgs e)
	{
		if (base.ItemPeers.Count == 0)
		{
			RaiseAutomationEvent(AutomationEvents.SelectionPatternOnInvalidated);
			return;
		}
		Selector selector = (Selector)base.Owner;
		int count = selector._selectedItems.Count;
		int count2 = e.AddedItems.Count;
		int count3 = e.RemovedItems.Count;
		if (count == 1 && count2 == 1)
		{
			if (FindOrCreateItemAutomationPeer(selector._selectedItems[0].Item) is SelectorItemAutomationPeer selectorItemAutomationPeer)
			{
				selectorItemAutomationPeer.RaiseAutomationEvent(AutomationEvents.SelectionItemPatternOnElementSelected);
			}
			return;
		}
		if (count2 + count3 > 20)
		{
			RaiseAutomationEvent(AutomationEvents.SelectionPatternOnInvalidated);
			return;
		}
		for (int i = 0; i < count2; i++)
		{
			if (FindOrCreateItemAutomationPeer(e.AddedItems[i]) is SelectorItemAutomationPeer selectorItemAutomationPeer2)
			{
				selectorItemAutomationPeer2.RaiseAutomationEvent(AutomationEvents.SelectionItemPatternOnElementAddedToSelection);
			}
		}
		for (int i = 0; i < count3; i++)
		{
			if (FindOrCreateItemAutomationPeer(e.RemovedItems[i]) is SelectorItemAutomationPeer selectorItemAutomationPeer3)
			{
				selectorItemAutomationPeer3.RaiseAutomationEvent(AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection);
			}
		}
	}
}
