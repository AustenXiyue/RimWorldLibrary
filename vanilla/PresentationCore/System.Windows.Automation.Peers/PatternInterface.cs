namespace System.Windows.Automation.Peers;

/// <summary>Specifies the control pattern that <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetPattern(System.Windows.Automation.Peers.PatternInterface)" /> returns.</summary>
public enum PatternInterface
{
	/// <summary>Value corresponding to the <see cref="T:System.Windows.Automation.Provider.IInvokeProvider" /> control pattern interface.</summary>
	Invoke,
	/// <summary>Value corresponding to the <see cref="T:System.Windows.Automation.Provider.ISelectionProvider" /> control pattern interface.</summary>
	Selection,
	/// <summary>Value corresponding to the <see cref="T:System.Windows.Automation.Provider.IValueProvider" /> control pattern interface.</summary>
	Value,
	/// <summary>Value corresponding to the <see cref="T:System.Windows.Automation.Provider.IRangeValueProvider" /> control pattern interface.</summary>
	RangeValue,
	/// <summary>Value corresponding to the <see cref="T:System.Windows.Automation.Provider.IScrollProvider" /> control pattern interface.</summary>
	Scroll,
	/// <summary>Value corresponding to the <see cref="T:System.Windows.Automation.Provider.IScrollItemProvider" /> control pattern interface.</summary>
	ScrollItem,
	/// <summary>Value corresponding to the <see cref="T:System.Windows.Automation.Provider.IExpandCollapseProvider" /> control pattern interface.</summary>
	ExpandCollapse,
	/// <summary>Value corresponding to the <see cref="T:System.Windows.Automation.Provider.IGridProvider" /> control pattern interface.</summary>
	Grid,
	/// <summary>Value corresponding to the <see cref="T:System.Windows.Automation.Provider.IGridItemProvider" /> control pattern interface.</summary>
	GridItem,
	/// <summary>Value corresponding to the <see cref="T:System.Windows.Automation.Provider.IMultipleViewProvider" /> control pattern interface.</summary>
	MultipleView,
	/// <summary>Value corresponding to the <see cref="T:System.Windows.Automation.Provider.IWindowProvider" /> control pattern interface.</summary>
	Window,
	/// <summary>Value corresponding to the <see cref="T:System.Windows.Automation.Provider.ISelectionItemProvider" /> control pattern interface.</summary>
	SelectionItem,
	/// <summary>Value corresponding to the <see cref="T:System.Windows.Automation.Provider.IDockProvider" /> control pattern interface.</summary>
	Dock,
	/// <summary>Value corresponding to the <see cref="T:System.Windows.Automation.Provider.ITableProvider" /> control pattern interface.</summary>
	Table,
	/// <summary>Value corresponding to the <see cref="T:System.Windows.Automation.Provider.ITableItemProvider" /> control pattern interface.</summary>
	TableItem,
	/// <summary>Value corresponding to the <see cref="T:System.Windows.Automation.Provider.IToggleProvider" /> control pattern interface.</summary>
	Toggle,
	/// <summary>Value corresponding to the <see cref="T:System.Windows.Automation.Provider.ITransformProvider" /> control pattern interface.</summary>
	Transform,
	/// <summary>Value corresponding to the <see cref="T:System.Windows.Automation.Provider.ITextProvider" /> control pattern interface.</summary>
	Text,
	/// <summary>Value corresponding to the <see cref="T:System.Windows.Automation.Provider.IItemContainerProvider" /> control pattern interface.</summary>
	ItemContainer,
	/// <summary>Value corresponding to the <see cref="T:System.Windows.Automation.Provider.IVirtualizedItemProvider" /> control pattern interface.</summary>
	VirtualizedItem,
	/// <summary>Value corresponding to the <see cref="T:System.Windows.Automation.Provider.ISynchronizedInputProvider" /> control pattern interface.</summary>
	SynchronizedInput
}
