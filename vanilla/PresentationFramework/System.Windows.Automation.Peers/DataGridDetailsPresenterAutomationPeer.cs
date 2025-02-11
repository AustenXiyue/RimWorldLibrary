using System.Windows.Controls.Primitives;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.Primitives.DataGridDetailsPresenter" /> types to UI Automation.</summary>
public sealed class DataGridDetailsPresenterAutomationPeer : FrameworkElementAutomationPeer
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.DataGridDetailsPresenterAutomationPeer" /> class. </summary>
	/// <param name="owner">The element associated with this automation peer.</param>
	public DataGridDetailsPresenterAutomationPeer(DataGridDetailsPresenter owner)
		: base(owner)
	{
	}

	protected override string GetClassNameCore()
	{
		return base.Owner.GetType().Name;
	}
}
