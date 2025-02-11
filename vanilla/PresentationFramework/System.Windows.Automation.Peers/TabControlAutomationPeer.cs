using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.TabControl" /> types to UI Automation.</summary>
public class TabControlAutomationPeer : SelectorAutomationPeer, ISelectionProvider
{
	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>Returns S_OK if successful, or an error value otherwise.</returns>
	bool ISelectionProvider.IsSelectionRequired => true;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.TabControlAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.TabControl" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TabControlAutomationPeer" />.</param>
	public TabControlAutomationPeer(TabControl owner)
		: base(owner)
	{
	}

	/// <summary>Creates a new <see cref="T:System.Windows.Automation.Peers.TabItemAutomationPeer" />.</summary>
	/// <returns>The created <see cref="T:System.Windows.Automation.Peers.TabItemAutomationPeer" /> object.</returns>
	/// <param name="item">The <see cref="T:System.Windows.Controls.TabItem" /> that is associated with the new <see cref="T:System.Windows.Automation.Peers.TabItemAutomationPeer" />.</param>
	protected override ItemAutomationPeer CreateItemAutomationPeer(object item)
	{
		return new TabItemAutomationPeer(item, this);
	}

	/// <summary>Gets the collection of child elements for the <see cref="T:System.Windows.Controls.TabItem" /> that is associated with the new <see cref="T:System.Windows.Automation.Peers.TabItemAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetChildren" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Tab" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Tab;
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.TabItem" /> that is associated with the new <see cref="T:System.Windows.Automation.Peers.TabItemAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "TabControl".</returns>
	protected override string GetClassNameCore()
	{
		return "TabControl";
	}

	/// <summary>This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClickablePoint" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Point" /> containing <see cref="F:System.Double.NaN" />, <see cref="F:System.Double.NaN" />; the only clickable points in a <see cref="T:System.Windows.Controls.TabControl" /> are the child <see cref="T:System.Windows.Controls.TabItem" /> elements. </returns>
	protected override Point GetClickablePointCore()
	{
		return new Point(double.NaN, double.NaN);
	}
}
