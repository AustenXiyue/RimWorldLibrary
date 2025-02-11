using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.Label" /> types to UI Automation.</summary>
public class LabelAutomationPeer : FrameworkElementAutomationPeer
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.LabelAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.Label" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.LabelAutomationPeer" />.</param>
	public LabelAutomationPeer(Label owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.Label" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.LabelAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "Label".</returns>
	protected override string GetClassNameCore()
	{
		return "Text";
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.Label" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.LabelAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Text" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Text;
	}

	/// <summary>Gets the text label of the <see cref="T:System.Windows.Controls.Label" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.LabelAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetName" />.</summary>
	/// <returns>The string that contains the label.</returns>
	protected override string GetNameCore()
	{
		string nameCore = base.GetNameCore();
		if (!string.IsNullOrEmpty(nameCore) && ((Label)base.Owner).Content is string)
		{
			return AccessText.RemoveAccessKeyMarker(nameCore);
		}
		return nameCore;
	}
}
