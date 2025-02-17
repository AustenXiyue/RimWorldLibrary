using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.Frame" /> types to UI Automation.</summary>
public class FrameAutomationPeer : FrameworkElementAutomationPeer
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.FrameAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.Frame" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.FrameAutomationPeer" />.</param>
	public FrameAutomationPeer(Frame owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.Frame" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.FrameAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "Frame".</returns>
	protected override string GetClassNameCore()
	{
		return "Frame";
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.Frame" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.FrameAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Pane" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Pane;
	}
}
