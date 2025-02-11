using System.Windows.Controls.Primitives;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.Primitives.StatusBarItem" /> types to UI Automation.</summary>
public class StatusBarItemAutomationPeer : FrameworkElementAutomationPeer
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.StatusBarItemAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.Primitives.StatusBarItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.StatusBarItemAutomationPeer" />.</param>
	public StatusBarItemAutomationPeer(StatusBarItem owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.Primitives.StatusBarItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.StatusBarItemAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "StatusBarItem".</returns>
	protected override string GetClassNameCore()
	{
		return "StatusBarItem";
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.Primitives.StatusBarItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.StatusBarItemAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Text" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Text;
	}
}
