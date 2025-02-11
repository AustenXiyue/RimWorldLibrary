using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.ToolBar" /> types to UI Automation.</summary>
public class ToolBarAutomationPeer : FrameworkElementAutomationPeer
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.ToolBarAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.ToolBar" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ToolBarAutomationPeer" />.</param>
	public ToolBarAutomationPeer(ToolBar owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.ToolBar" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ToolBarAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains the word "ToolBar".</returns>
	protected override string GetClassNameCore()
	{
		return "ToolBar";
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.ToolBar" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ToolBarAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>
	///   <see cref="F:System.Windows.Automation.Peers.AutomationControlType.ToolBar" />
	/// </returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.ToolBar;
	}
}
