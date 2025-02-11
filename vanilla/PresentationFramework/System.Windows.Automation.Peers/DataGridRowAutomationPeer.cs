using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.DataGridRow" /> types to UI Automation.</summary>
public sealed class DataGridRowAutomationPeer : FrameworkElementAutomationPeer
{
	internal AutomationPeer RowHeaderAutomationPeer
	{
		get
		{
			DataGridRowHeader rowHeader = OwningDataGridRow.RowHeader;
			if (rowHeader != null)
			{
				return UIElementAutomationPeer.CreatePeerForElement(rowHeader);
			}
			return null;
		}
	}

	private AutomationPeer DetailsPresenterAutomationPeer
	{
		get
		{
			DataGridDetailsPresenter detailsPresenter = OwningDataGridRow.DetailsPresenter;
			if (detailsPresenter != null)
			{
				return UIElementAutomationPeer.CreatePeerForElement(detailsPresenter);
			}
			return null;
		}
	}

	private DataGridRow OwningDataGridRow => (DataGridRow)base.Owner;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.DataGridRowAutomationPeer" /> class. </summary>
	/// <param name="owner">The element associated with this automation peer.</param>
	public DataGridRowAutomationPeer(DataGridRow owner)
		: base(owner)
	{
		if (owner == null)
		{
			throw new ArgumentNullException("owner");
		}
	}

	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.DataItem;
	}

	protected override string GetClassNameCore()
	{
		return base.Owner.GetType().Name;
	}

	protected override List<AutomationPeer> GetChildrenCore()
	{
		DataGridCellsPresenter cellsPresenter = OwningDataGridRow.CellsPresenter;
		if (cellsPresenter != null && cellsPresenter.ItemsHost != null)
		{
			List<AutomationPeer> list = new List<AutomationPeer>(3);
			AutomationPeer rowHeaderAutomationPeer = RowHeaderAutomationPeer;
			if (rowHeaderAutomationPeer != null)
			{
				list.Add(rowHeaderAutomationPeer);
			}
			if (base.EventsSource is DataGridItemAutomationPeer dataGridItemAutomationPeer)
			{
				list.AddRange(dataGridItemAutomationPeer.GetCellItemPeers());
			}
			AutomationPeer detailsPresenterAutomationPeer = DetailsPresenterAutomationPeer;
			if (detailsPresenterAutomationPeer != null)
			{
				list.Add(detailsPresenterAutomationPeer);
			}
			return list;
		}
		return base.GetChildrenCore();
	}
}
