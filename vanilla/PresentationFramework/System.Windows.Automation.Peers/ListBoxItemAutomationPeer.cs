using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes the items in the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection of a <see cref="T:System.Windows.Controls.ListBox" /> to UI Automation.</summary>
public class ListBoxItemAutomationPeer : SelectorItemAutomationPeer, IScrollItemProvider
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.ListBoxItemAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.ListBoxItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ListBoxItemAutomationPeer" />.</param>
	/// <param name="selectorAutomationPeer">The <see cref="T:System.Windows.Automation.Peers.SelectorAutomationPeer" /> that is the parent of this <see cref="T:System.Windows.Automation.Peers.ListBoxItemAutomationPeer" />.</param>
	public ListBoxItemAutomationPeer(object owner, SelectorAutomationPeer selectorAutomationPeer)
		: base(owner, selectorAutomationPeer)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.ListBoxItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ListBoxItemAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "ListBoxItem".</returns>
	protected override string GetClassNameCore()
	{
		return "ListBoxItem";
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.ListBoxItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ListBoxItemAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.ListItem" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.ListItem;
	}

	/// <summary>Gets the control pattern for the <see cref="T:System.Windows.Controls.ListBoxItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ListBoxItemAutomationPeer" />.</summary>
	/// <returns>If <paramref name="patternInterface" /> is <see cref="F:System.Windows.Automation.Peers.PatternInterface.ScrollItem" />, this method returns the current instance of this <see cref="T:System.Windows.Automation.Peers.ListBoxItemAutomationPeer" />.</returns>
	/// <param name="patternInterface">One of the enumeration values.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		if (patternInterface == PatternInterface.ScrollItem)
		{
			return this;
		}
		return base.GetPattern(patternInterface);
	}

	internal override void RealizeCore()
	{
		if (base.ItemsControlAutomationPeer.Owner is ComboBox element)
		{
			IExpandCollapseProvider expandCollapseProvider = ((IExpandCollapseProvider)UIElementAutomationPeer.FromElement(element)) as ComboBoxAutomationPeer;
			if (expandCollapseProvider.ExpandCollapseState != ExpandCollapseState.Expanded)
			{
				expandCollapseProvider.Expand();
			}
		}
		base.RealizeCore();
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void IScrollItemProvider.ScrollIntoView()
	{
		if (base.ItemsControlAutomationPeer.Owner is ListBox listBox)
		{
			listBox.ScrollIntoView(base.Item);
		}
		else if (base.ItemsControlAutomationPeer is ComboBoxAutomationPeer comboBoxAutomationPeer)
		{
			comboBoxAutomationPeer.ScrollItemIntoView(base.Item);
		}
	}
}
