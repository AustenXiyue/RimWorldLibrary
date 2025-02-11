using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes the <see cref="T:System.Windows.UIElement" /> sub-tree for the data items in a <see cref="T:System.Windows.Controls.ListBox" /> to UI Automation. </summary>
public class ListBoxItemWrapperAutomationPeer : FrameworkElementAutomationPeer
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.ListBoxItemWrapperAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.ListBoxItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ListBoxItemWrapperAutomationPeer" />.</param>
	public ListBoxItemWrapperAutomationPeer(ListBoxItem owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.ListBoxItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ListBoxItemWrapperAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "ListBoxItem".</returns>
	protected override string GetClassNameCore()
	{
		return "ListBoxItem";
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.ListBoxItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ListBoxItemWrapperAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>
	///   <see cref="F:System.Windows.Automation.Peers.AutomationControlType.ListItem" />
	/// </returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.ListItem;
	}
}
