using System.Runtime.CompilerServices;
using System.Windows.Automation.Provider;
using System.Windows.Controls.Primitives;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes the items in the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection of a <see cref="T:System.Windows.Controls.Primitives.Selector" /> to UI Automation. </summary>
public abstract class SelectorItemAutomationPeer : ItemAutomationPeer, ISelectionItemProvider
{
	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if the element is selected; otherwise false.</returns>
	bool ISelectionItemProvider.IsSelected
	{
		get
		{
			Selector selector = (Selector)base.ItemsControlAutomationPeer.Owner;
			return selector._selectedItems.Contains(selector.NewItemInfo(base.Item));
		}
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>The selection container.</returns>
	IRawElementProviderSimple ISelectionItemProvider.SelectionContainer => ProviderFromPeer(base.ItemsControlAutomationPeer);

	/// <summary>Provides initialization for base class values when they are called by the constructor of a derived class.</summary>
	/// <param name="owner">The item object that is associated with this <see cref="T:System.Windows.Automation.Peers.SelectorItemAutomationPeer" />.</param>
	/// <param name="selectorAutomationPeer">The <see cref="T:System.Windows.Automation.Peers.SelectorAutomationPeer" /> that is associated with the <see cref="T:System.Windows.Controls.Primitives.Selector" /> that holds the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection.</param>
	protected SelectorItemAutomationPeer(object owner, SelectorAutomationPeer selectorAutomationPeer)
		: base(owner, selectorAutomationPeer)
	{
	}

	/// <summary>Gets the control pattern that is associated with the specified <see cref="T:System.Windows.Automation.Peers.PatternInterface" />.</summary>
	/// <returns>If <paramref name="patternInterface" /> is <see cref="F:System.Windows.Automation.Peers.PatternInterface.Selection" />, this method returns the current instance of this <see cref="T:System.Windows.Automation.Peers.SelectorItemAutomationPeer" />.</returns>
	/// <param name="patternInterface">A value in the enumeration.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		if (patternInterface == PatternInterface.SelectionItem)
		{
			return this;
		}
		return base.GetPattern(patternInterface);
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void ISelectionItemProvider.Select()
	{
		if (!IsEnabled())
		{
			throw new ElementNotEnabledException();
		}
		Selector selector = (Selector)base.ItemsControlAutomationPeer.Owner;
		if (selector == null)
		{
			throw new InvalidOperationException(SR.UIA_OperationCannotBePerformed);
		}
		selector.SelectionChange.SelectJustThisItem(selector.NewItemInfo(base.Item), assumeInItemsCollection: true);
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void ISelectionItemProvider.AddToSelection()
	{
		if (!IsEnabled())
		{
			throw new ElementNotEnabledException();
		}
		Selector selector = (Selector)base.ItemsControlAutomationPeer.Owner;
		if (selector == null || (!selector.CanSelectMultiple && selector.SelectedItem != null && selector.SelectedItem != base.Item))
		{
			throw new InvalidOperationException(SR.UIA_OperationCannotBePerformed);
		}
		selector.SelectionChange.Begin();
		selector.SelectionChange.Select(selector.NewItemInfo(base.Item), assumeInItemsCollection: true);
		selector.SelectionChange.End();
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void ISelectionItemProvider.RemoveFromSelection()
	{
		if (!IsEnabled())
		{
			throw new ElementNotEnabledException();
		}
		Selector selector = (Selector)base.ItemsControlAutomationPeer.Owner;
		selector.SelectionChange.Begin();
		selector.SelectionChange.Unselect(selector.NewItemInfo(base.Item));
		selector.SelectionChange.End();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal void RaiseAutomationIsSelectedChanged(bool isSelected)
	{
		RaisePropertyChangedEvent(SelectionItemPatternIdentifiers.IsSelectedProperty, !isSelected, isSelected);
	}
}
