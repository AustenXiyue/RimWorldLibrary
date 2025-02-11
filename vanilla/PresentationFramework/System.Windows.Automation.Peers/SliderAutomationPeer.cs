using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.Slider" /> types to UI Automation.</summary>
public class SliderAutomationPeer : RangeBaseAutomationPeer
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.SliderAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.Slider" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.SliderAutomationPeer" />.</param>
	public SliderAutomationPeer(Slider owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.Slider" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.SliderAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "Slider".</returns>
	protected override string GetClassNameCore()
	{
		return "Slider";
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.Slider" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.SliderAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>
	///   <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Slider" />
	/// </returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Slider;
	}

	/// <summary>Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClickablePoint" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Point" /> containing <see cref="F:System.Double.NaN" />, <see cref="F:System.Double.NaN" />; the only clickable points in a <see cref="T:System.Windows.Controls.TabControl" /> are the child <see cref="T:System.Windows.Controls.TabItem" /> elements.</returns>
	protected override Point GetClickablePointCore()
	{
		return new Point(double.NaN, double.NaN);
	}
}
