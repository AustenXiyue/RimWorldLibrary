using System.Collections.Generic;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Media;
using MS.Internal;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes the cells in a <see cref="T:System.Windows.Controls.GridView" /> to UI Automation.</summary>
public class GridViewCellAutomationPeer : FrameworkElementAutomationPeer, ITableItemProvider, IGridItemProvider
{
	private delegate bool IteratorCallback(AutomationPeer peer);

	private ListViewAutomationPeer _listviewAP;

	private int _column;

	private int _row;

	internal int Column
	{
		get
		{
			return _column;
		}
		set
		{
			_column = value;
		}
	}

	internal int Row
	{
		get
		{
			return _row;
		}
		set
		{
			_row = value;
		}
	}

	/// <summary>Gets the ordinal number of the row that contains the cell or item.</summary>
	/// <returns>A zero-based ordinal number that identifies the row containing the cell or item.</returns>
	int IGridItemProvider.Row => Row;

	/// <summary>Gets the ordinal number of the column that contains the cell or item.</summary>
	/// <returns>A zero-based ordinal number that identifies the column containing the cell or item.</returns>
	int IGridItemProvider.Column => Column;

	/// <summary>Gets the number of rows spanned by a cell or item.</summary>
	/// <returns>The number of rows spanned.</returns>
	int IGridItemProvider.RowSpan => 1;

	/// <summary>Gets the number of columns spanned by a cell or item.</summary>
	/// <returns>The number of columns spanned.</returns>
	int IGridItemProvider.ColumnSpan => 1;

	/// <summary>Gets a UI Automation provider that implements <see cref="T:System.Windows.Automation.GridPattern" /> and represents the container of the cell or item.</summary>
	/// <returns>A UI Automation provider that implements the <see cref="T:System.Windows.Automation.GridPattern" /> and represents the cell or item container.</returns>
	IRawElementProviderSimple IGridItemProvider.ContainingGrid => ProviderFromPeer(_listviewAP);

	internal GridViewCellAutomationPeer(ContentPresenter owner, ListViewAutomationPeer parent)
		: base(owner)
	{
		Invariant.Assert(parent != null);
		_listviewAP = parent;
	}

	internal GridViewCellAutomationPeer(TextBlock owner, ListViewAutomationPeer parent)
		: base(owner)
	{
		Invariant.Assert(parent != null);
		_listviewAP = parent;
	}

	/// <summary>Gets the name of the element that is associated with this <see cref="T:System.Windows.Automation.Peers.GridViewCellAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>The name of the element.</returns>
	protected override string GetClassNameCore()
	{
		return base.Owner.GetType().Name;
	}

	/// <summary>Gets the control type for the element that is associated with this <see cref="T:System.Windows.Automation.Peers.GridViewCellAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>If this <see cref="T:System.Windows.Automation.Peers.GridViewCellAutomationPeer" /> is associated with a <see cref="T:System.Windows.Controls.TextBlock" /> element, this method returns <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Text" />; otherwise, this method returns <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Custom" />.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		if (base.Owner is TextBlock)
		{
			return AutomationControlType.Text;
		}
		return AutomationControlType.Custom;
	}

	/// <summary>Gets the control pattern for the element that is associated with this <see cref="T:System.Windows.Automation.Peers.GridViewCellAutomationPeer" />.</summary>
	/// <returns>If <paramref name="patternInterface" /> is <see cref="F:System.Windows.Automation.Peers.PatternInterface.GridItem" /> or <see cref="F:System.Windows.Automation.Peers.PatternInterface.TableItem" />, this method returns the current <see cref="T:System.Windows.Automation.Peers.GridViewCellAutomationPeer" />.</returns>
	/// <param name="patternInterface">One of the enumeration values.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		if (patternInterface == PatternInterface.GridItem || patternInterface == PatternInterface.TableItem)
		{
			return this;
		}
		return base.GetPattern(patternInterface);
	}

	/// <summary>Gets or sets a value that indicates whether the element that is associated with this <see cref="T:System.Windows.Automation.Peers.GridViewCellAutomationPeer" /> is understood by the end user as interactive or the user might understand the element as contributing to the logical structure of the control in the GUI. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsControlElement" />.</summary>
	/// <returns>If this <see cref="T:System.Windows.Automation.Peers.GridViewCellAutomationPeer" /> is associated with a <see cref="T:System.Windows.Controls.TextBlock" /> element, this method returns true; otherwise, this method returns a list of child elements.</returns>
	protected override bool IsControlElementCore()
	{
		bool includeInvisibleElementsInControlView = base.IncludeInvisibleElementsInControlView;
		if (base.Owner is TextBlock)
		{
			if (!includeInvisibleElementsInControlView)
			{
				return base.Owner.IsVisible;
			}
			return true;
		}
		List<AutomationPeer> childrenAutomationPeer = GetChildrenAutomationPeer(base.Owner, includeInvisibleElementsInControlView);
		if (childrenAutomationPeer != null)
		{
			return childrenAutomationPeer.Count >= 1;
		}
		return false;
	}

	/// <summary>Retrieves a collection of UI Automation providers that represent all the row headers associated with a table item or cell.</summary>
	/// <returns>A collection of UI Automation providers.</returns>
	IRawElementProviderSimple[] ITableItemProvider.GetRowHeaderItems()
	{
		return Array.Empty<IRawElementProviderSimple>();
	}

	/// <summary>Retrieves a collection of UI Automation providers that represent all the column headers associated with a table item or cell.</summary>
	/// <returns>A collection of UI Automation providers.</returns>
	IRawElementProviderSimple[] ITableItemProvider.GetColumnHeaderItems()
	{
		if (_listviewAP.Owner is ListView listView && listView.View is GridView)
		{
			GridView gridView = listView.View as GridView;
			if (gridView.HeaderRowPresenter != null && gridView.HeaderRowPresenter.ActualColumnHeaders.Count > Column)
			{
				AutomationPeer automationPeer = UIElementAutomationPeer.FromElement(gridView.HeaderRowPresenter.ActualColumnHeaders[Column]);
				if (automationPeer != null)
				{
					return new IRawElementProviderSimple[1] { ProviderFromPeer(automationPeer) };
				}
			}
		}
		return Array.Empty<IRawElementProviderSimple>();
	}

	private List<AutomationPeer> GetChildrenAutomationPeer(Visual parent, bool includeInvisibleItems)
	{
		Invariant.Assert(parent != null);
		List<AutomationPeer> children = null;
		iterate(parent, includeInvisibleItems, delegate(AutomationPeer peer)
		{
			if (children == null)
			{
				children = new List<AutomationPeer>();
			}
			children.Add(peer);
			return false;
		});
		return children;
	}

	private static bool iterate(Visual parent, bool includeInvisibleItems, IteratorCallback callback)
	{
		bool flag = false;
		AutomationPeer automationPeer = null;
		int internalVisualChildrenCount = parent.InternalVisualChildrenCount;
		for (int i = 0; i < internalVisualChildrenCount; i++)
		{
			if (flag)
			{
				break;
			}
			Visual visual = parent.InternalGetVisualChild(i);
			flag = ((visual == null || !visual.CheckFlagsAnd(VisualFlags.IsUIElement) || (!includeInvisibleItems && !((UIElement)visual).IsVisible) || (automationPeer = UIElementAutomationPeer.CreatePeerForElement((UIElement)visual)) == null) ? iterate(visual, includeInvisibleItems, callback) : callback(automationPeer));
		}
		return flag;
	}
}
