using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.DataGridCell" /> types to UI Automation.</summary>
public sealed class DataGridCellAutomationPeer : FrameworkElementAutomationPeer
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.DataGridCellAutomationPeer" /> class. </summary>
	/// <param name="owner">An enumeration value that specifies the control pattern.</param>
	public DataGridCellAutomationPeer(DataGridCell owner)
		: base(owner)
	{
		if (owner == null)
		{
			throw new ArgumentNullException("owner");
		}
	}

	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Custom;
	}

	protected override string GetClassNameCore()
	{
		return base.Owner.GetType().Name;
	}
}
