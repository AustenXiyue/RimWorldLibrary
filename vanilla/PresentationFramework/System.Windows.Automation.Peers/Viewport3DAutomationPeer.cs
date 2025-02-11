using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.Viewport3D" /> types to UI Automation.</summary>
public class Viewport3DAutomationPeer : FrameworkElementAutomationPeer
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.Viewport3DAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.Viewport3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.Viewport3DAutomationPeer" />.</param>
	public Viewport3DAutomationPeer(Viewport3D owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.Viewport3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.Viewport3DAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "Viewport3D".</returns>
	protected override string GetClassNameCore()
	{
		return "Viewport3D";
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.Viewport3D" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.Viewport3DAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>
	///   <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Custom" />
	/// </returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Custom;
	}
}
