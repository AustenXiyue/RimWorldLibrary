namespace System.Windows.Automation.Peers;

/// <summary>Specifies the event that is raised by the element through the associated <see cref="T:System.Windows.Automation.Peers.AutomationPeer" />.</summary>
public enum AutomationEvents
{
	/// <summary>
	///   <see cref="F:System.Windows.Automation.AutomationElementIdentifiers.ToolTipOpenedEvent" />
	/// </summary>
	ToolTipOpened,
	/// <summary>
	///   <see cref="F:System.Windows.Automation.AutomationElementIdentifiers.ToolTipClosedEvent" />
	/// </summary>
	ToolTipClosed,
	/// <summary>
	///   <see cref="F:System.Windows.Automation.AutomationElementIdentifiers.MenuOpenedEvent" />
	/// </summary>
	MenuOpened,
	/// <summary>
	///   <see cref="F:System.Windows.Automation.AutomationElementIdentifiers.MenuClosedEvent" />
	/// </summary>
	MenuClosed,
	/// <summary>
	///   <see cref="F:System.Windows.Automation.AutomationElementIdentifiers.AutomationFocusChangedEvent" />
	/// </summary>
	AutomationFocusChanged,
	/// <summary>
	///   <see cref="F:System.Windows.Automation.InvokePatternIdentifiers.InvokedEvent" />
	/// </summary>
	InvokePatternOnInvoked,
	/// <summary>
	///   <see cref="F:System.Windows.Automation.SelectionItemPatternIdentifiers.ElementAddedToSelectionEvent" />
	/// </summary>
	SelectionItemPatternOnElementAddedToSelection,
	/// <summary>
	///   <see cref="F:System.Windows.Automation.SelectionItemPatternIdentifiers.ElementRemovedFromSelectionEvent" />
	/// </summary>
	SelectionItemPatternOnElementRemovedFromSelection,
	/// <summary>
	///   <see cref="F:System.Windows.Automation.SelectionItemPatternIdentifiers.ElementSelectedEvent" />
	/// </summary>
	SelectionItemPatternOnElementSelected,
	/// <summary>
	///   <see cref="F:System.Windows.Automation.SelectionPatternIdentifiers.InvalidatedEvent" />
	/// </summary>
	SelectionPatternOnInvalidated,
	/// <summary>
	///   <see cref="F:System.Windows.Automation.TextPatternIdentifiers.TextSelectionChangedEvent" />
	/// </summary>
	TextPatternOnTextSelectionChanged,
	/// <summary>
	///   <see cref="F:System.Windows.Automation.TextPatternIdentifiers.TextChangedEvent" />
	/// </summary>
	TextPatternOnTextChanged,
	/// <summary>
	///   <see cref="F:System.Windows.Automation.AutomationElementIdentifiers.AsyncContentLoadedEvent" />
	/// </summary>
	AsyncContentLoaded,
	/// <summary>Used to raise a notification that a property has changed.</summary>
	PropertyChanged,
	/// <summary>
	///   <see cref="F:System.Windows.Automation.AutomationElementIdentifiers.StructureChangedEvent" />
	/// </summary>
	StructureChanged,
	/// <summary>
	///   <see cref="F:System.Windows.Automation.SynchronizedInputPatternIdentifiers.InputReachedTargetEvent" />
	/// </summary>
	InputReachedTarget,
	/// <summary>
	///   <see cref="F:System.Windows.Automation.SynchronizedInputPatternIdentifiers.InputReachedOtherElementEvent" />
	/// </summary>
	InputReachedOtherElement,
	/// <summary>
	///   <see cref="F:System.Windows.Automation.SynchronizedInputPatternIdentifiers.InputDiscardedEvent" />
	/// </summary>
	InputDiscarded,
	LiveRegionChanged,
	Notification,
	ActiveTextPositionChanged
}
