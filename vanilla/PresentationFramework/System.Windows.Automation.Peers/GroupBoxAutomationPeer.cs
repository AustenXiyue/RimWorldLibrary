using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.GroupBox" /> types to UI Automation.</summary>
public class GroupBoxAutomationPeer : FrameworkElementAutomationPeer
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.GroupBoxAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.GroupBox" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.GroupBoxAutomationPeer" />.</param>
	public GroupBoxAutomationPeer(GroupBox owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.GroupBox" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.GroupBoxAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "GroupBox".</returns>
	protected override string GetClassNameCore()
	{
		return "GroupBox";
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.GroupBox" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.GroupBoxAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Group" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Group;
	}

	/// <summary>Gets the text label of the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetName" />.</summary>
	/// <returns>The string that contains the label.</returns>
	protected override string GetNameCore()
	{
		string nameCore = base.GetNameCore();
		if (!string.IsNullOrEmpty(nameCore) && ((GroupBox)base.Owner).Header is string)
		{
			return AccessText.RemoveAccessKeyMarker(nameCore);
		}
		return nameCore;
	}
}
