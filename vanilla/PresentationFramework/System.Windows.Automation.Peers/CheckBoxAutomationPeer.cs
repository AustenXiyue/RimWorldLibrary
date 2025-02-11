using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.CheckBox" /> types to UI Automation.</summary>
public class CheckBoxAutomationPeer : ToggleButtonAutomationPeer
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.CheckBoxAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.CheckBox" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.CheckBoxAutomationPeer" />.</param>
	public CheckBoxAutomationPeer(CheckBox owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the element associated with this <see cref="T:System.Windows.Automation.Peers.CheckBoxAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "CheckBox".</returns>
	protected override string GetClassNameCore()
	{
		return "CheckBox";
	}

	/// <summary>Gets the <see cref="T:System.Windows.Automation.Peers.AutomationControlType" /> for the element associated with this <see cref="T:System.Windows.Automation.Peers.CheckBoxAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>
	///   <see cref="F:System.Windows.Automation.Peers.AutomationControlType.CheckBox" />.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.CheckBox;
	}
}
