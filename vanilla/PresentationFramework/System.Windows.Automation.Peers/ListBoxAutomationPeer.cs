using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.ListBox" /> types to UI Automation.</summary>
public class ListBoxAutomationPeer : SelectorAutomationPeer
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.ListBoxAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.ListBox" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ListBoxAutomationPeer" />.</param>
	public ListBoxAutomationPeer(ListBox owner)
		: base(owner)
	{
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Automation.Peers.ItemAutomationPeer" /> class.</summary>
	/// <returns>A new instance of the <see cref="T:System.Windows.Automation.Peers.ItemAutomationPeer" /> class.</returns>
	/// <param name="item">The <see cref="T:System.Windows.Controls.ListBoxItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ListBoxAutomationPeer" />.</param>
	protected override ItemAutomationPeer CreateItemAutomationPeer(object item)
	{
		return new ListBoxItemAutomationPeer(item, this);
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.ListBox" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ListBoxAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "ListBox".</returns>
	protected override string GetClassNameCore()
	{
		return "ListBox";
	}
}
