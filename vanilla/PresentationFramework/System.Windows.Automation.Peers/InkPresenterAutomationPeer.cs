using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.InkPresenter" /> types to UI Automation.</summary>
public class InkPresenterAutomationPeer : FrameworkElementAutomationPeer
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.InkPresenterAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.InkPresenter" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.InkPresenterAutomationPeer" />.</param>
	public InkPresenterAutomationPeer(InkPresenter owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.InkPresenter" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.InkPresenterAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "InkPresenter".</returns>
	protected override string GetClassNameCore()
	{
		return "InkPresenter";
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.InkPresenter" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.InkPresenterAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Custom" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Custom;
	}
}
