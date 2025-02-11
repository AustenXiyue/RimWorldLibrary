using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.ToolTip" /> types to UI Automation.</summary>
public class ToolTipAutomationPeer : FrameworkElementAutomationPeer
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.ToolTipAutomationPeer" />.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.ToolTip" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ToolTipAutomationPeer" />.</param>
	public ToolTipAutomationPeer(ToolTip owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.ToolTip" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ToolTipAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "ToolTip".</returns>
	protected override string GetClassNameCore()
	{
		return "ToolTip";
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.ToolTip" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ToolTipAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.ToolTip" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.ToolTip;
	}
}
