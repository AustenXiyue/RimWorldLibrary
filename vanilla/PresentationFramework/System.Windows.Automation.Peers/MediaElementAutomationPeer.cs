using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.MediaElement" /> types to UI Automation.</summary>
public class MediaElementAutomationPeer : FrameworkElementAutomationPeer
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.MediaElementAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.MediaElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.MediaElementAutomationPeer" />.</param>
	public MediaElementAutomationPeer(MediaElement owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.MediaElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.MediaElementAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "MediaElement".</returns>
	protected override string GetClassNameCore()
	{
		return "MediaElement";
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.MediaElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.MediaElementAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Custom" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Custom;
	}
}
