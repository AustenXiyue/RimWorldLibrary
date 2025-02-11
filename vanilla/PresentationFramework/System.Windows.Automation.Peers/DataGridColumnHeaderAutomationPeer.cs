using System.Windows.Controls.Primitives;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.Primitives.DataGridColumnHeader" /> types to UI Automation.</summary>
public sealed class DataGridColumnHeaderAutomationPeer : ButtonBaseAutomationPeer
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.DataGridColumnHeaderAutomationPeer" /> class. </summary>
	/// <param name="owner">The element associated with this automation peer.</param>
	public DataGridColumnHeaderAutomationPeer(DataGridColumnHeader owner)
		: base(owner)
	{
	}

	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.HeaderItem;
	}

	protected override string GetClassNameCore()
	{
		return base.Owner.GetType().Name;
	}

	protected override bool IsContentElementCore()
	{
		return false;
	}
}
