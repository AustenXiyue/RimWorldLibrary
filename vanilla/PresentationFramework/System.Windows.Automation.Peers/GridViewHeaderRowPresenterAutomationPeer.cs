using System.Collections.Generic;
using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.GridViewHeaderRowPresenter" /> types to UI Automation.</summary>
public class GridViewHeaderRowPresenterAutomationPeer : FrameworkElementAutomationPeer
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.GridViewHeaderRowPresenterAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.GridViewHeaderRowPresenter" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.GridViewHeaderRowPresenterAutomationPeer" />.</param>
	public GridViewHeaderRowPresenterAutomationPeer(GridViewHeaderRowPresenter owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.GridViewHeaderRowPresenter" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.GridViewHeaderRowPresenterAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "GridViewHeaderRowPresenter".</returns>
	protected override string GetClassNameCore()
	{
		return "GridViewHeaderRowPresenter";
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.GridViewHeaderRowPresenter" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.GridViewHeaderRowPresenterAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Header" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Header;
	}

	/// <summary>Gets a value that indicates whether the element that is associated with this automation peer contains data that is presented to the user. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsContentElement" />.</summary>
	/// <returns>false in all cases.</returns>
	protected override bool IsContentElementCore()
	{
		return false;
	}

	/// <summary>Gets the collection of child elements of the <see cref="T:System.Windows.Controls.GridViewHeaderRowPresenter" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.GridViewHeaderRowPresenterAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetChildren" />.</summary>
	/// <returns>The collection of child elements.</returns>
	protected override List<AutomationPeer> GetChildrenCore()
	{
		List<AutomationPeer> childrenCore = base.GetChildrenCore();
		List<AutomationPeer> list = null;
		if (childrenCore != null)
		{
			list = new List<AutomationPeer>(childrenCore.Count);
			foreach (AutomationPeer item in childrenCore)
			{
				if (item is UIElementAutomationPeer && ((UIElementAutomationPeer)item).Owner is GridViewColumnHeader { Role: GridViewColumnHeaderRole.Normal })
				{
					list.Insert(0, item);
				}
			}
		}
		return list;
	}
}
