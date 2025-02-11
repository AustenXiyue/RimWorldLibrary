using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.InkCanvas" /> types to UI Automation.</summary>
public class InkCanvasAutomationPeer : FrameworkElementAutomationPeer
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.InkCanvasAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.InkCanvas" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.InkCanvasAutomationPeer" />.</param>
	public InkCanvasAutomationPeer(InkCanvas owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.InkCanvas" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.InkCanvasAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "InkCanvas".</returns>
	protected override string GetClassNameCore()
	{
		return "InkCanvas";
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.InkCanvas" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.InkCanvasAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Custom" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Custom;
	}
}
