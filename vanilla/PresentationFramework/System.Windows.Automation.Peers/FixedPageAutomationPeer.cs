using System.Windows.Documents;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Documents.FixedPage" /> types to UI Automation.</summary>
public class FixedPageAutomationPeer : FrameworkElementAutomationPeer
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.FixedPageAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Documents.FixedPage" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.FixedPageAutomationPeer" />.</param>
	public FixedPageAutomationPeer(FixedPage owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Documents.FixedPage" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.FixedPageAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "FixedPage".</returns>
	protected override string GetClassNameCore()
	{
		return "FixedPage";
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Documents.FixedPage" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.FixedPageAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Pane" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Pane;
	}
}
