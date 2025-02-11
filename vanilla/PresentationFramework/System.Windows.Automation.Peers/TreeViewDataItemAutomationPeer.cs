using System.Runtime.CompilerServices;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.TreeViewItem" /> types containing data items to UI Automation.</summary>
public class TreeViewDataItemAutomationPeer : ItemAutomationPeer, ISelectionItemProvider, IScrollItemProvider, IExpandCollapseProvider
{
	private TreeViewDataItemAutomationPeer _parentDataItemAutomationPeer;

	private ItemPeersStorage<WeakReference> _WeakRefElementProxyStorageCache;

	/// <summary>Gets the <see cref="T:System.Windows.Automation.Peers.TreeViewDataItemAutomationPeer" /> that is the parent to this automation peer.</summary>
	/// <returns>The parent automation peer.</returns>
	public TreeViewDataItemAutomationPeer ParentDataItemAutomationPeer => _parentDataItemAutomationPeer;

	/// <summary>Gets the state, expanded or collapsed, of the control.</summary>
	/// <returns>The state, expanded or collapsed, of the control.</returns>
	ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState
	{
		get
		{
			if (GetWrapperPeer() is TreeViewItemAutomationPeer treeViewItemAutomationPeer)
			{
				return ((IExpandCollapseProvider)treeViewItemAutomationPeer).ExpandCollapseState;
			}
			ThrowElementNotAvailableException();
			return ExpandCollapseState.LeafNode;
		}
	}

	/// <summary>Gets a value that indicates whether an item is selected.</summary>
	/// <returns>true if an item is selected; otherwise, false.</returns>
	bool ISelectionItemProvider.IsSelected
	{
		get
		{
			if (GetWrapperPeer() is TreeViewItemAutomationPeer treeViewItemAutomationPeer)
			{
				return ((ISelectionItemProvider)treeViewItemAutomationPeer).IsSelected;
			}
			return false;
		}
	}

	/// <summary>Gets the UI automation provider that implements <see cref="T:System.Windows.Automation.Provider.ISelectionProvider" /> and acts as the container for the calling object.</summary>
	/// <returns>The UI automation provider.</returns>
	IRawElementProviderSimple ISelectionItemProvider.SelectionContainer
	{
		get
		{
			if (GetWrapperPeer() is TreeViewItemAutomationPeer treeViewItemAutomationPeer)
			{
				return ((ISelectionItemProvider)treeViewItemAutomationPeer).SelectionContainer;
			}
			ThrowElementNotAvailableException();
			return null;
		}
	}

	internal ItemPeersStorage<WeakReference> WeakRefElementProxyStorageCache
	{
		get
		{
			return _WeakRefElementProxyStorageCache;
		}
		set
		{
			_WeakRefElementProxyStorageCache = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.TreeViewDataItemAutomationPeer" /> class. </summary>
	/// <param name="item">The data item in the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection that is associated with this <see cref="T:System.Windows.Automation.Peers.TreeViewDataItemAutomationPeer" />.</param>
	/// <param name="itemsControlAutomationPeer">The <see cref="T:System.Windows.Automation.Peers.ItemsControlAutomationPeer" /> that is associated with the <see cref="T:System.Windows.Controls.ItemsControl" /> that holds the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection.</param>
	/// <param name="parentDataItemAutomationPeer">The <see cref="T:System.Windows.Automation.Peers.TreeViewDataItemAutomationPeer" /> that is the parent to this <see cref="T:System.Windows.Automation.Peers.TreeViewDataItemAutomationPeer" />.</param>
	public TreeViewDataItemAutomationPeer(object item, ItemsControlAutomationPeer itemsControlAutomationPeer, TreeViewDataItemAutomationPeer parentDataItemAutomationPeer)
		: base(item, null)
	{
		if (itemsControlAutomationPeer.Owner is TreeView || parentDataItemAutomationPeer == null)
		{
			base.ItemsControlAutomationPeer = itemsControlAutomationPeer;
		}
		_parentDataItemAutomationPeer = parentDataItemAutomationPeer;
	}

	/// <summary>Gets the control pattern for the element that is associated with this <see cref="T:System.Windows.Automation.Peers.TreeViewDataItemAutomationPeer" />.</summary>
	/// <returns>The object that implements the pattern interface, or null if the specified pattern interface is not implemented by this peer.</returns>
	/// <param name="patternInterface">The type of pattern implemented by the element to retrieve.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		switch (patternInterface)
		{
		case PatternInterface.ExpandCollapse:
			return this;
		case PatternInterface.SelectionItem:
			return this;
		case PatternInterface.ScrollItem:
			return this;
		case PatternInterface.ItemContainer:
		case PatternInterface.SynchronizedInput:
			if (GetWrapperPeer() is TreeViewItemAutomationPeer treeViewItemAutomationPeer)
			{
				if (patternInterface == PatternInterface.SynchronizedInput)
				{
					return treeViewItemAutomationPeer.GetPattern(patternInterface);
				}
				return treeViewItemAutomationPeer;
			}
			break;
		}
		return base.GetPattern(patternInterface);
	}

	internal override AutomationPeer GetWrapperPeer()
	{
		AutomationPeer wrapperPeer = base.GetWrapperPeer();
		if (wrapperPeer is TreeViewItemAutomationPeer treeViewItemAutomationPeer)
		{
			treeViewItemAutomationPeer.AddDataPeerInfo(this);
		}
		return wrapperPeer;
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.TreeViewItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TreeViewDataItemAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string containing the value TreeViewItem.</returns>
	protected override string GetClassNameCore()
	{
		return "TreeViewItem";
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.TreeViewItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TreeViewDataItemAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>
	///   <see cref="F:System.Windows.Automation.Peers.AutomationControlType.TreeItem" /> in all cases.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.TreeItem;
	}

	internal override ItemsControlAutomationPeer GetItemsControlAutomationPeer()
	{
		if (_parentDataItemAutomationPeer == null)
		{
			return base.GetItemsControlAutomationPeer();
		}
		return _parentDataItemAutomationPeer.GetWrapperPeer() as ItemsControlAutomationPeer;
	}

	internal override void RealizeCore()
	{
		RecursiveScrollIntoView();
	}

	/// <summary>Displays all child nodes, controls, or content of the control.</summary>
	void IExpandCollapseProvider.Expand()
	{
		if (GetWrapperPeer() is TreeViewItemAutomationPeer treeViewItemAutomationPeer)
		{
			((IExpandCollapseProvider)treeViewItemAutomationPeer).Expand();
		}
		else
		{
			ThrowElementNotAvailableException();
		}
	}

	/// <summary>Hides all nodes, controls, or content that are descendants of the control.</summary>
	void IExpandCollapseProvider.Collapse()
	{
		if (GetWrapperPeer() is TreeViewItemAutomationPeer treeViewItemAutomationPeer)
		{
			((IExpandCollapseProvider)treeViewItemAutomationPeer).Collapse();
		}
		else
		{
			ThrowElementNotAvailableException();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal void RaiseExpandCollapseAutomationEvent(bool oldValue, bool newValue)
	{
		RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty, oldValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed, newValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed);
	}

	/// <summary>Clears any selection and then selects the current element.</summary>
	/// <exception cref="T:System.Windows.Automation.ElementNotAvailableException">UI Automation element is no longer available.</exception>
	void ISelectionItemProvider.Select()
	{
		if (GetWrapperPeer() is TreeViewItemAutomationPeer treeViewItemAutomationPeer)
		{
			((ISelectionItemProvider)treeViewItemAutomationPeer).Select();
		}
		else
		{
			ThrowElementNotAvailableException();
		}
	}

	/// <summary>Adds the current element to the collection of selected items.</summary>
	/// <exception cref="T:System.Windows.Automation.ElementNotAvailableException">UI Automation element is no longer available.</exception>
	void ISelectionItemProvider.AddToSelection()
	{
		if (GetWrapperPeer() is TreeViewItemAutomationPeer treeViewItemAutomationPeer)
		{
			((ISelectionItemProvider)treeViewItemAutomationPeer).AddToSelection();
		}
		else
		{
			ThrowElementNotAvailableException();
		}
	}

	/// <summary>Removes the current element from the collection of selected items.</summary>
	/// <exception cref="T:System.Windows.Automation.ElementNotAvailableException">UI Automation element is no longer available.</exception>
	void ISelectionItemProvider.RemoveFromSelection()
	{
		if (GetWrapperPeer() is TreeViewItemAutomationPeer treeViewItemAutomationPeer)
		{
			((ISelectionItemProvider)treeViewItemAutomationPeer).RemoveFromSelection();
		}
		else
		{
			ThrowElementNotAvailableException();
		}
	}

	/// <summary>Scrolls the content area of a container object in order to display the control within the visible region (viewport) of the container.</summary>
	void IScrollItemProvider.ScrollIntoView()
	{
		if (GetWrapperPeer() is TreeViewItemAutomationPeer treeViewItemAutomationPeer)
		{
			((IScrollItemProvider)treeViewItemAutomationPeer).ScrollIntoView();
		}
		else
		{
			RecursiveScrollIntoView();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal void RaiseAutomationIsSelectedChanged(bool isSelected)
	{
		RaisePropertyChangedEvent(SelectionItemPatternIdentifiers.IsSelectedProperty, !isSelected, isSelected);
	}

	private void RecursiveScrollIntoView()
	{
		ItemsControlAutomationPeer itemsControlAutomationPeer = base.ItemsControlAutomationPeer;
		if (ParentDataItemAutomationPeer != null && itemsControlAutomationPeer == null)
		{
			ParentDataItemAutomationPeer.RecursiveScrollIntoView();
			itemsControlAutomationPeer = base.ItemsControlAutomationPeer;
		}
		if (itemsControlAutomationPeer == null)
		{
			return;
		}
		if (itemsControlAutomationPeer is TreeViewItemAutomationPeer { ExpandCollapseState: ExpandCollapseState.Collapsed } treeViewItemAutomationPeer)
		{
			((IExpandCollapseProvider)treeViewItemAutomationPeer).Expand();
		}
		if (itemsControlAutomationPeer.Owner is ItemsControl itemsControl)
		{
			if (itemsControl.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
			{
				itemsControl.OnBringItemIntoView(base.Item);
			}
			else
			{
				base.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new DispatcherOperationCallback(itemsControl.OnBringItemIntoView), base.Item);
			}
		}
	}
}
