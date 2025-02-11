using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls;
using MS.Internal;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes the data items in the collection of <see cref="P:System.Windows.Controls.ItemsControl.Items" /> in <see cref="T:System.Windows.Controls.GridView" /> types to UI Automation.</summary>
public class GridViewItemAutomationPeer : ListBoxItemAutomationPeer
{
	private ListViewAutomationPeer _listviewAP;

	private Hashtable _dataChildren;

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Automation.Peers.GridViewItemAutomationPeer" /> class.</summary>
	/// <param name="owner">The data item that is associated with this <see cref="T:System.Windows.Automation.Peers.GridViewItemAutomationPeer" />.</param>
	/// <param name="listviewAP">The <see cref="T:System.Windows.Automation.Peers.ListViewAutomationPeer" /> that is the parent of this <see cref="T:System.Windows.Automation.Peers.GridViewItemAutomationPeer" />.</param>
	public GridViewItemAutomationPeer(object owner, ListViewAutomationPeer listviewAP)
		: base(owner, listviewAP)
	{
		Invariant.Assert(listviewAP != null);
		_listviewAP = listviewAP;
	}

	/// <summary>Gets the name of the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection that is associated with this <see cref="T:System.Windows.Automation.Peers.GridViewItemAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "ListViewItem".</returns>
	protected override string GetClassNameCore()
	{
		return "ListViewItem";
	}

	/// <summary>Gets the control type for the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection that is associated with this <see cref="T:System.Windows.Automation.Peers.GridViewItemAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>
	///   <see cref="F:System.Windows.Automation.Peers.AutomationControlType.DataItem" />.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.DataItem;
	}

	/// <summary>Gets the collection of child elements of the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection that is associated with this <see cref="T:System.Windows.Automation.Peers.GridViewItemAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetChildren" />.</summary>
	/// <returns>The collection of child elements.</returns>
	protected override List<AutomationPeer> GetChildrenCore()
	{
		ListView listView = _listviewAP.Owner as ListView;
		Invariant.Assert(listView != null);
		object item = base.Item;
		if (listView.ItemContainerGenerator.ContainerFromItem(item) is ListViewItem parent && GridViewAutomationPeer.FindVisualByType(parent, typeof(GridViewRowPresenter)) is GridViewRowPresenter gridViewRowPresenter)
		{
			Hashtable dataChildren = _dataChildren;
			_dataChildren = new Hashtable(gridViewRowPresenter.ActualCells.Count);
			List<AutomationPeer> list = new List<AutomationPeer>();
			int row = listView.Items.IndexOf(item);
			int num = 0;
			{
				foreach (UIElement actualCell in gridViewRowPresenter.ActualCells)
				{
					GridViewCellAutomationPeer gridViewCellAutomationPeer = ((dataChildren == null) ? null : ((GridViewCellAutomationPeer)dataChildren[actualCell]));
					if (gridViewCellAutomationPeer == null)
					{
						if (actualCell is ContentPresenter)
						{
							gridViewCellAutomationPeer = new GridViewCellAutomationPeer((ContentPresenter)actualCell, _listviewAP);
						}
						else if (actualCell is TextBlock)
						{
							gridViewCellAutomationPeer = new GridViewCellAutomationPeer((TextBlock)actualCell, _listviewAP);
						}
						else
						{
							Invariant.Assert(condition: false, "Children of GridViewRowPresenter should be ContentPresenter or TextBlock");
						}
					}
					if (_dataChildren[actualCell] == null)
					{
						gridViewCellAutomationPeer.Column = num;
						gridViewCellAutomationPeer.Row = row;
						list.Add(gridViewCellAutomationPeer);
						_dataChildren.Add(actualCell, gridViewCellAutomationPeer);
						num++;
					}
				}
				return list;
			}
		}
		return null;
	}
}
