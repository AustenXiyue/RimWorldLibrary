using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Automation;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.GridView" /> types to UI Automation.</summary>
public class GridViewAutomationPeer : IViewAutomationPeer, ITableProvider, IGridProvider
{
	private GridView _owner;

	private ListView _listview;

	private int _oldItemsCount;

	private int _oldColumnsCount;

	/// <summary>Gets the primary direction of traversal for the table.</summary>
	/// <returns>The primary direction of traversal.  </returns>
	RowOrColumnMajor ITableProvider.RowOrColumnMajor => RowOrColumnMajor.RowMajor;

	/// <summary>Gets the total number of columns in a grid.</summary>
	/// <returns>The total number of columns in a grid.</returns>
	int IGridProvider.ColumnCount
	{
		get
		{
			if (_owner.HeaderRowPresenter != null)
			{
				return _owner.HeaderRowPresenter.ActualColumnHeaders.Count;
			}
			return _owner.Columns.Count;
		}
	}

	/// <summary>Gets the total number of rows in a grid.</summary>
	/// <returns>The total number of rows in a grid.</returns>
	int IGridProvider.RowCount => _listview.Items.Count;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.GridViewAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.GridView" /> associated with this <see cref="T:System.Windows.Automation.Peers.GridViewAutomationPeer" />.</param>
	/// <param name="listview">The <see cref="T:System.Windows.Controls.ListView" /> that the <see cref="T:System.Windows.Controls.GridView" /> is using as a view mode.</param>
	public GridViewAutomationPeer(GridView owner, ListView listview)
	{
		Invariant.Assert(owner != null);
		Invariant.Assert(listview != null);
		_owner = owner;
		_listview = listview;
		_oldItemsCount = _listview.Items.Count;
		_oldColumnsCount = _owner.Columns.Count;
		((INotifyCollectionChanged)_owner.Columns).CollectionChanged += OnColumnCollectionChanged;
	}

	/// <summary>Gets the control type for the element that is associated with this <see cref="T:System.Windows.Automation.Peers.GridViewAutomationPeer" />.</summary>
	/// <returns>A value in the enumeration.</returns>
	AutomationControlType IViewAutomationPeer.GetAutomationControlType()
	{
		return AutomationControlType.DataGrid;
	}

	/// <summary>Gets the control pattern that is associated with the specified <paramref name="patternInterface" />.</summary>
	/// <returns>If <paramref name="patternInterface" /> is <see cref="F:System.Windows.Automation.Peers.PatternInterface.Grid" /> or <see cref="F:System.Windows.Automation.Peers.PatternInterface.Table" />, this method returns an object that implements the control pattern, otherwise this method returns null.</returns>
	/// <param name="patternInterface">A value in the enumeration.</param>
	object IViewAutomationPeer.GetPattern(PatternInterface patternInterface)
	{
		object result = null;
		if (patternInterface == PatternInterface.Grid || patternInterface == PatternInterface.Table)
		{
			result = this;
		}
		return result;
	}

	/// <summary>Gets the collection of immediate child elements of the specified UI Automation peer.</summary>
	/// <returns>Collection of child objects.</returns>
	/// <param name="children">Collection of child objects you want to get.</param>
	List<AutomationPeer> IViewAutomationPeer.GetChildren(List<AutomationPeer> children)
	{
		if (_owner.HeaderRowPresenter != null)
		{
			AutomationPeer automationPeer = UIElementAutomationPeer.CreatePeerForElement(_owner.HeaderRowPresenter);
			if (automationPeer != null)
			{
				if (children == null)
				{
					children = new List<AutomationPeer>();
				}
				children.Insert(0, automationPeer);
			}
		}
		return children;
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Automation.Peers.ItemAutomationPeer" /> class.</summary>
	/// <returns>The item created.</returns>
	/// <param name="item">Item to create.</param>
	ItemAutomationPeer IViewAutomationPeer.CreateItemAutomationPeer(object item)
	{
		ListViewAutomationPeer listviewAP = UIElementAutomationPeer.FromElement(_listview) as ListViewAutomationPeer;
		return new GridViewItemAutomationPeer(item, listviewAP);
	}

	/// <summary>Called when the collection of items changes.</summary>
	/// <param name="e">Data associated with the event.</param>
	void IViewAutomationPeer.ItemsChanged(NotifyCollectionChangedEventArgs e)
	{
		if (UIElementAutomationPeer.FromElement(_listview) is ListViewAutomationPeer listViewAutomationPeer)
		{
			if (_oldItemsCount != _listview.Items.Count)
			{
				listViewAutomationPeer.RaisePropertyChangedEvent(GridPatternIdentifiers.RowCountProperty, _oldItemsCount, _listview.Items.Count);
			}
			_oldItemsCount = _listview.Items.Count;
		}
	}

	/// <summary>Called when the custom view is no longer applied to the control.</summary>
	[MethodImpl(MethodImplOptions.NoInlining)]
	void IViewAutomationPeer.ViewDetached()
	{
		((INotifyCollectionChanged)_owner.Columns).CollectionChanged -= OnColumnCollectionChanged;
	}

	/// <summary>Returns a collection of UI Automation providers that represents all the column headers in a table.</summary>
	/// <returns>A collection of UI Automation providers.</returns>
	IRawElementProviderSimple[] ITableProvider.GetColumnHeaders()
	{
		if (_owner.HeaderRowPresenter != null)
		{
			List<IRawElementProviderSimple> list = new List<IRawElementProviderSimple>(_owner.HeaderRowPresenter.ActualColumnHeaders.Count);
			if (UIElementAutomationPeer.FromElement(_listview) is ListViewAutomationPeer referencePeer)
			{
				foreach (GridViewColumnHeader actualColumnHeader in _owner.HeaderRowPresenter.ActualColumnHeaders)
				{
					AutomationPeer automationPeer = UIElementAutomationPeer.CreatePeerForElement(actualColumnHeader);
					if (automationPeer != null)
					{
						list.Add(ElementProxy.StaticWrap(automationPeer, referencePeer));
					}
				}
			}
			return list.ToArray();
		}
		return new IRawElementProviderSimple[0];
	}

	/// <summary>Returns a collection of UI Automation providers that represents all row headers in the table.</summary>
	/// <returns>A collection of UI Automation providers.</returns>
	IRawElementProviderSimple[] ITableProvider.GetRowHeaders()
	{
		return Array.Empty<IRawElementProviderSimple>();
	}

	/// <summary>Returns the UI Automation provider for the specified cell.</summary>
	/// <returns>The UI Automation provider for the specified cell.</returns>
	/// <param name="row">The ordinal number of the row of interest.</param>
	/// <param name="column">The ordinal number of the column of interest.</param>
	IRawElementProviderSimple IGridProvider.GetItem(int row, int column)
	{
		if (row < 0 || row >= ((IGridProvider)this).RowCount)
		{
			throw new ArgumentOutOfRangeException("row");
		}
		if (column < 0 || column >= ((IGridProvider)this).ColumnCount)
		{
			throw new ArgumentOutOfRangeException("column");
		}
		ListViewItem listViewItem = _listview.ItemContainerGenerator.ContainerFromIndex(row) as ListViewItem;
		if (listViewItem == null)
		{
			if (_listview.ItemsHost is VirtualizingPanel virtualizingPanel)
			{
				virtualizingPanel.BringIndexIntoView(row);
			}
			listViewItem = _listview.ItemContainerGenerator.ContainerFromIndex(row) as ListViewItem;
			if (listViewItem != null)
			{
				_listview.Dispatcher.Invoke(DispatcherPriority.Loaded, (DispatcherOperationCallback)((object arg) => (object)null), null);
			}
		}
		if (listViewItem != null)
		{
			AutomationPeer automationPeer = UIElementAutomationPeer.FromElement(_listview);
			if (automationPeer != null)
			{
				AutomationPeer automationPeer2 = UIElementAutomationPeer.FromElement(listViewItem);
				if (automationPeer2 != null)
				{
					AutomationPeer eventsSource = automationPeer2.EventsSource;
					if (eventsSource != null)
					{
						automationPeer2 = eventsSource;
					}
					List<AutomationPeer> children = automationPeer2.GetChildren();
					if (children.Count > column)
					{
						return ElementProxy.StaticWrap(children[column], automationPeer);
					}
				}
			}
		}
		return null;
	}

	private void OnColumnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (_oldColumnsCount != _owner.Columns.Count)
		{
			ListViewAutomationPeer listViewAutomationPeer = UIElementAutomationPeer.FromElement(_listview) as ListViewAutomationPeer;
			Invariant.Assert(listViewAutomationPeer != null);
			listViewAutomationPeer?.RaisePropertyChangedEvent(GridPatternIdentifiers.ColumnCountProperty, _oldColumnsCount, _owner.Columns.Count);
		}
		_oldColumnsCount = _owner.Columns.Count;
		AutomationPeer automationPeer = UIElementAutomationPeer.FromElement(_listview);
		if (automationPeer == null)
		{
			return;
		}
		List<AutomationPeer> children = automationPeer.GetChildren();
		if (children == null)
		{
			return;
		}
		foreach (AutomationPeer item in children)
		{
			item.InvalidatePeer();
		}
	}

	internal static Visual FindVisualByType(Visual parent, Type type)
	{
		if (parent != null)
		{
			int internalVisualChildrenCount = parent.InternalVisualChildrenCount;
			for (int i = 0; i < internalVisualChildrenCount; i++)
			{
				Visual visual = parent.InternalGetVisualChild(i);
				if (!type.IsInstanceOfType(visual))
				{
					visual = FindVisualByType(visual, type);
				}
				if (visual != null)
				{
					return visual;
				}
			}
		}
		return null;
	}
}
