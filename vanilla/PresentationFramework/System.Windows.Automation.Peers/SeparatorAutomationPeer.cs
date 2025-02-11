using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.Separator" /> types to UI Automation.</summary>
public class SeparatorAutomationPeer : FrameworkElementAutomationPeer
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.SeparatorAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.Separator" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.SeparatorAutomationPeer" />.</param>
	public SeparatorAutomationPeer(Separator owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.Separator" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.SeparatorAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "Separator".</returns>
	protected override string GetClassNameCore()
	{
		return "Separator";
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.Separator" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.SeparatorAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>
	///   <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Separator" />
	/// </returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Separator;
	}

	/// <summary>Gets a value that indicates whether the element that is associated with this automation peer contains data that is presented to the user. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsContentElement" />.</summary>
	/// <returns>false in all cases.</returns>
	protected override bool IsContentElementCore()
	{
		return false;
	}
}
