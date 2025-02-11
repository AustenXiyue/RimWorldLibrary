using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.ContextMenu" /> types to UI Automation.</summary>
public class ContextMenuAutomationPeer : FrameworkElementAutomationPeer
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.ContextMenuAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.ContextMenu" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContextMenuAutomationPeer" />.</param>
	public ContextMenuAutomationPeer(ContextMenu owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.ContextMenu" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContextMenuAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "ContextMenu".</returns>
	protected override string GetClassNameCore()
	{
		return "ContextMenu";
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.ContextMenu" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContextMenuAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Menu" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Menu;
	}

	/// <summary>Gets a value that indicates whether the element that is associated with this automation peer contains data that is presented to the user. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsContentElement" />.</summary>
	/// <returns>false in all cases.</returns>
	protected override bool IsContentElementCore()
	{
		return false;
	}
}
