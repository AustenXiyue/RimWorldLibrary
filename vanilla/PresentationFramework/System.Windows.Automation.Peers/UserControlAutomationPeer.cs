using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.UserControl" /> types to UI Automation.</summary>
public class UserControlAutomationPeer : FrameworkElementAutomationPeer
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.UserControlAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.UserControl" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UserControlAutomationPeer" />.</param>
	public UserControlAutomationPeer(UserControl owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.UserControl" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UserControlAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>The string that contains the name of the control.</returns>
	protected override string GetClassNameCore()
	{
		return base.Owner.GetType().Name;
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.UserControl" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.UserControlAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Custom" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Custom;
	}
}
