using System.Collections.Generic;
using System.Globalization;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using MS.Internal.Automation;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.Calendar" /> types to UI Automation.</summary>
public sealed class CalendarAutomationPeer : FrameworkElementAutomationPeer, IGridProvider, IMultipleViewProvider, ISelectionProvider, ITableProvider, IItemContainerProvider
{
	private Dictionary<DateTimeCalendarModePair, DateTimeAutomationPeer> _dataChildren = new Dictionary<DateTimeCalendarModePair, DateTimeAutomationPeer>();

	private Dictionary<DateTimeCalendarModePair, WeakReference> _weakRefElementProxyStorage = new Dictionary<DateTimeCalendarModePair, WeakReference>();

	private System.Windows.Controls.Calendar OwningCalendar => base.Owner as System.Windows.Controls.Calendar;

	private Grid OwningGrid
	{
		get
		{
			if (OwningCalendar != null && OwningCalendar.MonthControl != null)
			{
				if (OwningCalendar.DisplayMode == CalendarMode.Month)
				{
					return OwningCalendar.MonthControl.MonthView;
				}
				return OwningCalendar.MonthControl.YearView;
			}
			return null;
		}
	}

	/// <summary>Gets the total number of columns in a grid.</summary>
	/// <returns>The total number of columns in a grid.</returns>
	int IGridProvider.ColumnCount
	{
		get
		{
			if (OwningGrid != null)
			{
				return OwningGrid.ColumnDefinitions.Count;
			}
			return 0;
		}
	}

	/// <summary>Gets the total number of rows in a grid.</summary>
	/// <returns>The total number of rows in a grid.</returns>
	int IGridProvider.RowCount
	{
		get
		{
			if (OwningGrid != null)
			{
				if (OwningCalendar.DisplayMode == CalendarMode.Month)
				{
					return Math.Max(0, OwningGrid.RowDefinitions.Count - 1);
				}
				return OwningGrid.RowDefinitions.Count;
			}
			return 0;
		}
	}

	/// <summary>Gets the current control-specific view.</summary>
	/// <returns>The value for the current view of the UI Automation element. </returns>
	int IMultipleViewProvider.CurrentView => (int)OwningCalendar.DisplayMode;

	/// <summary>Gets a value that specifies whether the UI Automation provider allows more than one child element to be selected concurrently.</summary>
	/// <returns>true if multiple selection is allowed; otherwise, false.</returns>
	bool ISelectionProvider.CanSelectMultiple
	{
		get
		{
			if (OwningCalendar.SelectionMode != CalendarSelectionMode.SingleRange)
			{
				return OwningCalendar.SelectionMode == CalendarSelectionMode.MultipleRange;
			}
			return true;
		}
	}

	/// <summary>Gets a value that specifies whether the UI Automation provider requires at least one child element to be selected.</summary>
	/// <returns>false in all cases.</returns>
	bool ISelectionProvider.IsSelectionRequired => false;

	/// <summary>Retrieves the primary direction of traversal for the table.</summary>
	/// <returns>The primary direction of traversal. </returns>
	RowOrColumnMajor ITableProvider.RowOrColumnMajor => RowOrColumnMajor.RowMajor;

	private Dictionary<DateTimeCalendarModePair, DateTimeAutomationPeer> DateTimePeers
	{
		get
		{
			return _dataChildren;
		}
		set
		{
			_dataChildren = value;
		}
	}

	private Dictionary<DateTimeCalendarModePair, WeakReference> WeakRefElementProxyStorage => _weakRefElementProxyStorage;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.CalendarAutomationPeer" /> class. </summary>
	/// <param name="owner">The element associated with this automation peer.</param>
	public CalendarAutomationPeer(System.Windows.Controls.Calendar owner)
		: base(owner)
	{
	}

	/// <summary>Gets the object that supports the specified control pattern of the element that is associated with this automation peer.</summary>
	/// <returns>If <paramref name="patternInterface" /> is <see cref="F:System.Windows.Automation.Peers.PatternInterface.Grid" />, <see cref="F:System.Windows.Automation.Peers.PatternInterface.Table" />, <see cref="F:System.Windows.Automation.Peers.PatternInterface.MultipleView" />, or <see cref="F:System.Windows.Automation.Peers.PatternInterface.Selection" />, this method returns a this pointer; otherwise, this method returns null.</returns>
	/// <param name="patternInterface">An enumeration value that specifies the control pattern.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		switch (patternInterface)
		{
		case PatternInterface.Selection:
		case PatternInterface.Grid:
		case PatternInterface.MultipleView:
		case PatternInterface.Table:
		case PatternInterface.ItemContainer:
			if (OwningGrid != null)
			{
				return this;
			}
			break;
		}
		return base.GetPattern(patternInterface);
	}

	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Calendar;
	}

	protected override List<AutomationPeer> GetChildrenCore()
	{
		if (OwningCalendar.MonthControl == null)
		{
			return null;
		}
		List<AutomationPeer> list = new List<AutomationPeer>();
		Dictionary<DateTimeCalendarModePair, DateTimeAutomationPeer> dictionary = new Dictionary<DateTimeCalendarModePair, DateTimeAutomationPeer>();
		AutomationPeer automationPeer = UIElementAutomationPeer.CreatePeerForElement(OwningCalendar.MonthControl.PreviousButton);
		if (automationPeer != null)
		{
			list.Add(automationPeer);
		}
		automationPeer = UIElementAutomationPeer.CreatePeerForElement(OwningCalendar.MonthControl.HeaderButton);
		if (automationPeer != null)
		{
			list.Add(automationPeer);
		}
		automationPeer = UIElementAutomationPeer.CreatePeerForElement(OwningCalendar.MonthControl.NextButton);
		if (automationPeer != null)
		{
			list.Add(automationPeer);
		}
		foreach (UIElement child in OwningGrid.Children)
		{
			int num = (int)child.GetValue(Grid.RowProperty);
			if (OwningCalendar.DisplayMode == CalendarMode.Month && num == 0)
			{
				AutomationPeer automationPeer2 = UIElementAutomationPeer.CreatePeerForElement(child);
				if (automationPeer2 != null)
				{
					list.Add(automationPeer2);
				}
			}
			else if (child is Button button && button.DataContext is DateTime)
			{
				DateTime date = (DateTime)button.DataContext;
				DateTimeAutomationPeer orCreateDateTimeAutomationPeer = GetOrCreateDateTimeAutomationPeer(date, OwningCalendar.DisplayMode, addParentInfo: false);
				list.Add(orCreateDateTimeAutomationPeer);
				DateTimeCalendarModePair key = new DateTimeCalendarModePair(date, OwningCalendar.DisplayMode);
				dictionary.Add(key, orCreateDateTimeAutomationPeer);
			}
		}
		DateTimePeers = dictionary;
		return list;
	}

	protected override string GetClassNameCore()
	{
		return base.Owner.GetType().Name;
	}

	protected override void SetFocusCore()
	{
		System.Windows.Controls.Calendar owningCalendar = OwningCalendar;
		if (owningCalendar.Focusable)
		{
			if (!owningCalendar.Focus())
			{
				DateTime date = ((!owningCalendar.SelectedDate.HasValue || DateTimeHelper.CompareYearMonth(owningCalendar.SelectedDate.Value, owningCalendar.DisplayDateInternal) != 0) ? owningCalendar.DisplayDate : owningCalendar.SelectedDate.Value);
				FrameworkElement owningButton = GetOrCreateDateTimeAutomationPeer(date, owningCalendar.DisplayMode, addParentInfo: false).OwningButton;
				if (owningButton == null || !owningButton.IsKeyboardFocused)
				{
					throw new InvalidOperationException(SR.SetFocusFailed);
				}
			}
			return;
		}
		throw new InvalidOperationException(SR.SetFocusFailed);
	}

	private DateTimeAutomationPeer GetOrCreateDateTimeAutomationPeer(DateTime date, CalendarMode buttonMode)
	{
		return GetOrCreateDateTimeAutomationPeer(date, buttonMode, addParentInfo: true);
	}

	private DateTimeAutomationPeer GetOrCreateDateTimeAutomationPeer(DateTime date, CalendarMode buttonMode, bool addParentInfo)
	{
		DateTimeCalendarModePair dateTimeCalendarModePair = new DateTimeCalendarModePair(date, buttonMode);
		DateTimeAutomationPeer value = null;
		DateTimePeers.TryGetValue(dateTimeCalendarModePair, out value);
		if (value == null)
		{
			value = GetPeerFromWeakRefStorage(dateTimeCalendarModePair);
			if (value != null && !addParentInfo)
			{
				value.AncestorsInvalid = false;
				value.ChildrenValid = false;
			}
		}
		if (value == null)
		{
			value = new DateTimeAutomationPeer(date, OwningCalendar, buttonMode);
			if (addParentInfo)
			{
				value?.TrySetParentInfo(this);
			}
		}
		AutomationPeer wrapperPeer = value.WrapperPeer;
		if (wrapperPeer != null)
		{
			wrapperPeer.EventsSource = value;
		}
		return value;
	}

	private DateTimeAutomationPeer GetPeerFromWeakRefStorage(DateTimeCalendarModePair dateTimeCalendarModePairKey)
	{
		DateTimeAutomationPeer dateTimeAutomationPeer = null;
		WeakReference value = null;
		WeakRefElementProxyStorage.TryGetValue(dateTimeCalendarModePairKey, out value);
		if (value != null)
		{
			if (value.Target is ElementProxy provider)
			{
				dateTimeAutomationPeer = PeerFromProvider(provider) as DateTimeAutomationPeer;
				if (dateTimeAutomationPeer == null)
				{
					WeakRefElementProxyStorage.Remove(dateTimeCalendarModePairKey);
				}
			}
			else
			{
				WeakRefElementProxyStorage.Remove(dateTimeCalendarModePairKey);
			}
		}
		return dateTimeAutomationPeer;
	}

	internal void AddProxyToWeakRefStorage(WeakReference wr, DateTimeAutomationPeer dateTimePeer)
	{
		DateTimeCalendarModePair dateTimeCalendarModePair = new DateTimeCalendarModePair(dateTimePeer.Date, dateTimePeer.ButtonMode);
		if (GetPeerFromWeakRefStorage(dateTimeCalendarModePair) == null)
		{
			WeakRefElementProxyStorage.Add(dateTimeCalendarModePair, wr);
		}
	}

	internal void RaiseSelectionEvents(SelectionChangedEventArgs e)
	{
		int count = OwningCalendar.SelectedDates.Count;
		int count2 = e.AddedItems.Count;
		if (AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementSelected) && count == 1 && count2 == 1)
		{
			GetOrCreateDateTimeAutomationPeer((DateTime)e.AddedItems[0], CalendarMode.Month)?.RaiseAutomationEvent(AutomationEvents.SelectionItemPatternOnElementSelected);
		}
		else if (AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementAddedToSelection))
		{
			foreach (DateTime addedItem in e.AddedItems)
			{
				GetOrCreateDateTimeAutomationPeer(addedItem, CalendarMode.Month)?.RaiseAutomationEvent(AutomationEvents.SelectionItemPatternOnElementAddedToSelection);
			}
		}
		if (!AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection))
		{
			return;
		}
		foreach (DateTime removedItem in e.RemovedItems)
		{
			GetOrCreateDateTimeAutomationPeer(removedItem, CalendarMode.Month)?.RaiseAutomationEvent(AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection);
		}
	}

	/// <summary>Retrieves the UI Automation provider for the specified cell.</summary>
	/// <returns>The UI Automation provider for the specified cell.</returns>
	/// <param name="row">The ordinal number of the row of interest.</param>
	/// <param name="column">The ordinal number of the column of interest.</param>
	IRawElementProviderSimple IGridProvider.GetItem(int row, int column)
	{
		if (OwningCalendar.DisplayMode == CalendarMode.Month)
		{
			row++;
		}
		if (OwningGrid != null && row >= 0 && row < OwningGrid.RowDefinitions.Count && column >= 0 && column < OwningGrid.ColumnDefinitions.Count)
		{
			foreach (UIElement child in OwningGrid.Children)
			{
				int num = (int)child.GetValue(Grid.RowProperty);
				int num2 = (int)child.GetValue(Grid.ColumnProperty);
				if (num == row && num2 == column && (child as FrameworkElement).DataContext is DateTime date)
				{
					AutomationPeer orCreateDateTimeAutomationPeer = GetOrCreateDateTimeAutomationPeer(date, OwningCalendar.DisplayMode);
					return ProviderFromPeer(orCreateDateTimeAutomationPeer);
				}
			}
		}
		return null;
	}

	/// <summary>Retrieves a collection of control-specific view identifiers.</summary>
	/// <returns>A collection of values that identifies the views available for a UI Automation element. </returns>
	int[] IMultipleViewProvider.GetSupportedViews()
	{
		return new int[3] { 0, 1, 2 };
	}

	/// <summary>Retrieves the name of a control-specific view.</summary>
	/// <returns>A localized name for the view.</returns>
	/// <param name="viewId">The view identifier.</param>
	string IMultipleViewProvider.GetViewName(int viewId)
	{
		return viewId switch
		{
			0 => SR.CalendarAutomationPeer_MonthMode, 
			1 => SR.CalendarAutomationPeer_YearMode, 
			2 => SR.CalendarAutomationPeer_DecadeMode, 
			_ => string.Empty, 
		};
	}

	/// <summary>Sets the current control-specific view. </summary>
	/// <param name="viewId">A view identifier.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="viewId" /> is not a member of the supported views collection.</exception>
	void IMultipleViewProvider.SetCurrentView(int viewId)
	{
		OwningCalendar.DisplayMode = (CalendarMode)viewId;
	}

	/// <summary>Retrieves a UI Automation provider for each child element that is selected.</summary>
	/// <returns>A collection of UI Automation providers. </returns>
	IRawElementProviderSimple[] ISelectionProvider.GetSelection()
	{
		List<IRawElementProviderSimple> list = new List<IRawElementProviderSimple>();
		foreach (DateTime selectedDate in OwningCalendar.SelectedDates)
		{
			AutomationPeer orCreateDateTimeAutomationPeer = GetOrCreateDateTimeAutomationPeer(selectedDate, CalendarMode.Month);
			list.Add(ProviderFromPeer(orCreateDateTimeAutomationPeer));
		}
		if (list.Count > 0)
		{
			return list.ToArray();
		}
		return null;
	}

	/// <summary>Retrieves an element by the specified property value.</summary>
	/// <returns>The first item that matches the search criterion; otherwise, null if no items match.</returns>
	/// <param name="startAfterProvider">The item in the container after which to begin the search.</param>
	/// <param name="propertyId">The property that contains the value to retrieve.</param>
	/// <param name="value">The value to retrieve.</param>
	IRawElementProviderSimple IItemContainerProvider.FindItemByProperty(IRawElementProviderSimple startAfterProvider, int propertyId, object value)
	{
		DateTimeAutomationPeer dateTimeAutomationPeer = null;
		if (startAfterProvider != null)
		{
			dateTimeAutomationPeer = PeerFromProvider(startAfterProvider) as DateTimeAutomationPeer;
			if (dateTimeAutomationPeer == null)
			{
				throw new InvalidOperationException(SR.InavalidStartItem);
			}
		}
		DateTime? dateTime = null;
		CalendarMode calendarMode = CalendarMode.Month;
		if (propertyId == SelectionItemPatternIdentifiers.IsSelectedProperty.Id)
		{
			calendarMode = CalendarMode.Month;
			dateTime = GetNextSelectedDate(dateTimeAutomationPeer, (bool)value);
		}
		else if (propertyId == AutomationElementIdentifiers.NameProperty.Id)
		{
			DateTimeFormatInfo currentDateFormat = DateTimeHelper.GetCurrentDateFormat();
			if (DateTime.TryParse(value as string, currentDateFormat, DateTimeStyles.None, out var result))
			{
				dateTime = result;
			}
			if (!dateTime.HasValue || (dateTimeAutomationPeer != null && dateTime <= dateTimeAutomationPeer.Date))
			{
				throw new InvalidOperationException(SR.CalendarNamePropertyValueNotValid);
			}
			calendarMode = dateTimeAutomationPeer?.ButtonMode ?? OwningCalendar.DisplayMode;
		}
		else
		{
			if (propertyId != 0 && propertyId != AutomationElementIdentifiers.ControlTypeProperty.Id)
			{
				throw new ArgumentException(SR.PropertyNotSupported);
			}
			if (propertyId == AutomationElementIdentifiers.ControlTypeProperty.Id && (int)value != ControlType.Button.Id)
			{
				return null;
			}
			calendarMode = dateTimeAutomationPeer?.ButtonMode ?? OwningCalendar.DisplayMode;
			dateTime = GetNextDate(dateTimeAutomationPeer, calendarMode);
		}
		if (dateTime.HasValue)
		{
			AutomationPeer orCreateDateTimeAutomationPeer = GetOrCreateDateTimeAutomationPeer(dateTime.Value, calendarMode);
			if (orCreateDateTimeAutomationPeer != null)
			{
				return ProviderFromPeer(orCreateDateTimeAutomationPeer);
			}
		}
		return null;
	}

	private DateTime? GetNextDate(DateTimeAutomationPeer currentDatePeer, CalendarMode currentMode)
	{
		DateTime? result = null;
		DateTime dateTime = currentDatePeer?.Date ?? OwningCalendar.DisplayDate;
		switch (currentMode)
		{
		case CalendarMode.Month:
			result = dateTime.AddDays(1.0);
			break;
		case CalendarMode.Year:
			result = dateTime.AddMonths(1);
			break;
		case CalendarMode.Decade:
			result = dateTime.AddYears(1);
			break;
		}
		return result;
	}

	private DateTime? GetNextSelectedDate(DateTimeAutomationPeer currentDatePeer, bool isSelected)
	{
		DateTime dateTime = currentDatePeer?.Date ?? OwningCalendar.DisplayDate;
		if (isSelected)
		{
			if (!OwningCalendar.SelectedDates.MaximumDate.HasValue || OwningCalendar.SelectedDates.MaximumDate <= dateTime)
			{
				return null;
			}
			if (OwningCalendar.SelectedDates.MinimumDate.HasValue)
			{
				DateTime value = dateTime;
				DateTime? minimumDate = OwningCalendar.SelectedDates.MinimumDate;
				if (value < minimumDate)
				{
					return OwningCalendar.SelectedDates.MinimumDate;
				}
			}
		}
		do
		{
			dateTime = dateTime.AddDays(1.0);
		}
		while (OwningCalendar.SelectedDates.Contains(dateTime) != isSelected);
		return dateTime;
	}

	/// <summary>Gets a collection of UI Automation providers that represents all the column headers in a table.</summary>
	/// <returns>A collection of UI Automation providers. </returns>
	IRawElementProviderSimple[] ITableProvider.GetColumnHeaders()
	{
		if (OwningCalendar.DisplayMode == CalendarMode.Month)
		{
			List<IRawElementProviderSimple> list = new List<IRawElementProviderSimple>();
			foreach (UIElement child in OwningGrid.Children)
			{
				if ((int)child.GetValue(Grid.RowProperty) == 0)
				{
					AutomationPeer automationPeer = UIElementAutomationPeer.CreatePeerForElement(child);
					if (automationPeer != null)
					{
						list.Add(ProviderFromPeer(automationPeer));
					}
				}
			}
			if (list.Count > 0)
			{
				return list.ToArray();
			}
		}
		return null;
	}

	/// <summary>Retrieves a collection of UI Automation providers that represents all row headers in the table.</summary>
	/// <returns>A collection of UI Automation providers.</returns>
	IRawElementProviderSimple[] ITableProvider.GetRowHeaders()
	{
		return null;
	}
}
