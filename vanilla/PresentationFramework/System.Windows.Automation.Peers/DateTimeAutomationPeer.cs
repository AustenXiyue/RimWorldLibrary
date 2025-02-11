using System.Collections.Generic;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Threading;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.Primitives.CalendarDayButton" /> and <see cref="T:System.Windows.Controls.Primitives.CalendarButton" /> types to UI Automation. </summary>
public sealed class DateTimeAutomationPeer : AutomationPeer, IGridItemProvider, ISelectionItemProvider, ITableItemProvider, IInvokeProvider, IVirtualizedItemProvider
{
	internal override bool AncestorsInvalid
	{
		get
		{
			return base.AncestorsInvalid;
		}
		set
		{
			base.AncestorsInvalid = value;
			if (!value)
			{
				AutomationPeer wrapperPeer = WrapperPeer;
				if (wrapperPeer != null)
				{
					wrapperPeer.AncestorsInvalid = false;
				}
			}
		}
	}

	private Calendar OwningCalendar { get; set; }

	internal DateTime Date { get; private set; }

	internal CalendarMode ButtonMode { get; private set; }

	internal bool IsDayButton => ButtonMode == CalendarMode.Month;

	private IRawElementProviderSimple OwningCalendarProvider
	{
		get
		{
			if (OwningCalendar != null)
			{
				AutomationPeer automationPeer = UIElementAutomationPeer.CreatePeerForElement(OwningCalendar);
				if (automationPeer != null)
				{
					return ProviderFromPeer(automationPeer);
				}
			}
			return null;
		}
	}

	internal Button OwningButton
	{
		get
		{
			if (OwningCalendar.DisplayMode != ButtonMode)
			{
				return null;
			}
			if (IsDayButton)
			{
				return OwningCalendar.MonthControl?.GetCalendarDayButton(Date);
			}
			return OwningCalendar.MonthControl?.GetCalendarButton(Date, ButtonMode);
		}
	}

	internal FrameworkElementAutomationPeer WrapperPeer
	{
		get
		{
			Button owningButton = OwningButton;
			if (owningButton != null)
			{
				return UIElementAutomationPeer.CreatePeerForElement(owningButton) as FrameworkElementAutomationPeer;
			}
			return null;
		}
	}

	/// <summary>Gets the ordinal number of the column that contains the cell or item.</summary>
	/// <returns>A zero-based ordinal number that identifies the column containing the cell or item.</returns>
	int IGridItemProvider.Column
	{
		get
		{
			Button owningButton = OwningButton;
			if (owningButton != null)
			{
				return (int)owningButton.GetValue(Grid.ColumnProperty);
			}
			throw new ElementNotAvailableException(SR.VirtualizedElement);
		}
	}

	/// <summary>Gets the number of columns spanned by a cell or item.</summary>
	/// <returns>The number of columns spanned.</returns>
	int IGridItemProvider.ColumnSpan
	{
		get
		{
			Button owningButton = OwningButton;
			if (owningButton != null)
			{
				return (int)owningButton.GetValue(Grid.ColumnSpanProperty);
			}
			throw new ElementNotAvailableException(SR.VirtualizedElement);
		}
	}

	/// <summary>Gets a UI Automation provider that implements <see cref="T:System.Windows.Automation.Provider.IGridProvider" /> and represents the container of the cell or item.</summary>
	/// <returns>A UI Automation provider that represents the cell or item container.</returns>
	IRawElementProviderSimple IGridItemProvider.ContainingGrid => OwningCalendarProvider;

	/// <summary>Gets the ordinal number of the row that contains the cell or item.</summary>
	/// <returns>A zero-based ordinal number that identifies the row containing the cell or item.</returns>
	int IGridItemProvider.Row
	{
		get
		{
			Button owningButton = OwningButton;
			if (owningButton != null)
			{
				if (IsDayButton)
				{
					return (int)owningButton.GetValue(Grid.RowProperty) - 1;
				}
				return (int)owningButton.GetValue(Grid.RowProperty);
			}
			throw new ElementNotAvailableException(SR.VirtualizedElement);
		}
	}

	/// <summary>Gets the number of rows spanned by a cell or item.</summary>
	/// <returns>The number of rows spanned.</returns>
	int IGridItemProvider.RowSpan
	{
		get
		{
			Button owningButton = OwningButton;
			if (owningButton != null)
			{
				if (IsDayButton)
				{
					return (int)owningButton.GetValue(Grid.RowSpanProperty);
				}
				return 1;
			}
			throw new ElementNotAvailableException(SR.VirtualizedElement);
		}
	}

	/// <summary>Gets a value that indicates whether an item is selected.</summary>
	/// <returns>true if the element is selected; otherwise, false.</returns>
	bool ISelectionItemProvider.IsSelected
	{
		get
		{
			if (IsDayButton)
			{
				return OwningCalendar.SelectedDates.Contains(Date);
			}
			return false;
		}
	}

	/// <summary>Gets the UI Automation provider that implements <see cref="T:System.Windows.Automation.Provider.ISelectionProvider" /> and acts as the container for the calling object.</summary>
	/// <returns>The provider that acts as the container for the calling object.</returns>
	IRawElementProviderSimple ISelectionItemProvider.SelectionContainer => OwningCalendarProvider;

	internal DateTimeAutomationPeer(DateTime date, Calendar owningCalendar, CalendarMode buttonMode)
	{
		if (owningCalendar == null)
		{
			throw new ArgumentNullException("owningCalendar");
		}
		Date = date;
		ButtonMode = buttonMode;
		OwningCalendar = owningCalendar;
	}

	protected override string GetAcceleratorKeyCore()
	{
		AutomationPeer wrapperPeer = WrapperPeer;
		if (wrapperPeer != null)
		{
			return wrapperPeer.GetAcceleratorKey();
		}
		ThrowElementNotAvailableException();
		return string.Empty;
	}

	protected override string GetAccessKeyCore()
	{
		AutomationPeer wrapperPeer = WrapperPeer;
		if (wrapperPeer != null)
		{
			return wrapperPeer.GetAccessKey();
		}
		ThrowElementNotAvailableException();
		return string.Empty;
	}

	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Button;
	}

	protected override string GetAutomationIdCore()
	{
		AutomationPeer wrapperPeer = WrapperPeer;
		if (wrapperPeer != null)
		{
			return wrapperPeer.GetAutomationId();
		}
		ThrowElementNotAvailableException();
		return string.Empty;
	}

	protected override Rect GetBoundingRectangleCore()
	{
		AutomationPeer wrapperPeer = WrapperPeer;
		if (wrapperPeer != null)
		{
			return wrapperPeer.GetBoundingRectangle();
		}
		ThrowElementNotAvailableException();
		return default(Rect);
	}

	protected override List<AutomationPeer> GetChildrenCore()
	{
		AutomationPeer wrapperPeer = WrapperPeer;
		if (wrapperPeer != null)
		{
			return wrapperPeer.GetChildren();
		}
		ThrowElementNotAvailableException();
		return null;
	}

	protected override string GetClassNameCore()
	{
		AutomationPeer wrapperPeer = WrapperPeer;
		if (wrapperPeer == null)
		{
			if (!IsDayButton)
			{
				return "CalendarButton";
			}
			return "CalendarDayButton";
		}
		return wrapperPeer.GetClassName();
	}

	protected override Point GetClickablePointCore()
	{
		AutomationPeer wrapperPeer = WrapperPeer;
		if (wrapperPeer != null)
		{
			return wrapperPeer.GetClickablePoint();
		}
		ThrowElementNotAvailableException();
		return new Point(double.NaN, double.NaN);
	}

	protected override string GetHelpTextCore()
	{
		string text = DateTimeHelper.ToLongDateString(Date, DateTimeHelper.GetCulture(OwningCalendar));
		if (IsDayButton && OwningCalendar.BlackoutDates.Contains(Date))
		{
			return string.Format(DateTimeHelper.GetCurrentDateFormat(), SR.CalendarAutomationPeer_BlackoutDayHelpText, text);
		}
		return text;
	}

	protected override string GetItemStatusCore()
	{
		AutomationPeer wrapperPeer = WrapperPeer;
		if (wrapperPeer != null)
		{
			return wrapperPeer.GetItemStatus();
		}
		ThrowElementNotAvailableException();
		return string.Empty;
	}

	protected override string GetItemTypeCore()
	{
		AutomationPeer wrapperPeer = WrapperPeer;
		if (wrapperPeer != null)
		{
			return wrapperPeer.GetItemType();
		}
		ThrowElementNotAvailableException();
		return string.Empty;
	}

	protected override AutomationPeer GetLabeledByCore()
	{
		AutomationPeer wrapperPeer = WrapperPeer;
		if (wrapperPeer != null)
		{
			return wrapperPeer.GetLabeledBy();
		}
		ThrowElementNotAvailableException();
		return null;
	}

	protected override AutomationLiveSetting GetLiveSettingCore()
	{
		AutomationPeer wrapperPeer = WrapperPeer;
		if (wrapperPeer != null)
		{
			return wrapperPeer.GetLiveSetting();
		}
		ThrowElementNotAvailableException();
		return AutomationLiveSetting.Off;
	}

	protected override string GetLocalizedControlTypeCore()
	{
		if (!IsDayButton)
		{
			return SR.CalendarAutomationPeer_CalendarButtonLocalizedControlType;
		}
		return SR.CalendarAutomationPeer_DayButtonLocalizedControlType;
	}

	protected override string GetNameCore()
	{
		string result = "";
		switch (ButtonMode)
		{
		case CalendarMode.Month:
			result = DateTimeHelper.ToLongDateString(Date, DateTimeHelper.GetCulture(OwningCalendar));
			break;
		case CalendarMode.Year:
			result = DateTimeHelper.ToYearMonthPatternString(Date, DateTimeHelper.GetCulture(OwningCalendar));
			break;
		case CalendarMode.Decade:
			result = DateTimeHelper.ToYearString(Date, DateTimeHelper.GetCulture(OwningCalendar));
			break;
		}
		return result;
	}

	protected override AutomationOrientation GetOrientationCore()
	{
		AutomationPeer wrapperPeer = WrapperPeer;
		if (wrapperPeer != null)
		{
			return wrapperPeer.GetOrientation();
		}
		ThrowElementNotAvailableException();
		return AutomationOrientation.None;
	}

	/// <summary>Gets the control pattern implementation for this <see cref="T:System.Windows.Automation.Peers.DateTimeAutomationPeer" />.</summary>
	/// <returns>The object that implements the pattern interface, or null if the specified pattern interface is not implemented by this peer.</returns>
	/// <param name="patternInterface">The type of pattern implemented by the element to retrieve.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		object result = null;
		Button owningButton = OwningButton;
		switch (patternInterface)
		{
		case PatternInterface.Invoke:
		case PatternInterface.GridItem:
			if (owningButton != null)
			{
				result = this;
			}
			break;
		case PatternInterface.TableItem:
			if (IsDayButton && owningButton != null)
			{
				result = this;
			}
			break;
		case PatternInterface.SelectionItem:
			result = this;
			break;
		case PatternInterface.VirtualizedItem:
			if (VirtualizedItemPatternIdentifiers.Pattern != null)
			{
				if (owningButton == null)
				{
					result = this;
				}
				else if (!IsItemInAutomationTree())
				{
					return this;
				}
			}
			break;
		}
		return result;
	}

	protected override int GetPositionInSetCore()
	{
		AutomationPeer wrapperPeer = WrapperPeer;
		if (wrapperPeer != null)
		{
			return wrapperPeer.GetPositionInSet();
		}
		ThrowElementNotAvailableException();
		return -1;
	}

	protected override int GetSizeOfSetCore()
	{
		AutomationPeer wrapperPeer = WrapperPeer;
		if (wrapperPeer != null)
		{
			return wrapperPeer.GetSizeOfSet();
		}
		ThrowElementNotAvailableException();
		return -1;
	}

	protected override AutomationHeadingLevel GetHeadingLevelCore()
	{
		AutomationPeer wrapperPeer = WrapperPeer;
		AutomationHeadingLevel result = AutomationHeadingLevel.None;
		if (wrapperPeer != null)
		{
			result = wrapperPeer.GetHeadingLevel();
		}
		else
		{
			ThrowElementNotAvailableException();
		}
		return result;
	}

	internal override Rect GetVisibleBoundingRectCore()
	{
		return WrapperPeer?.GetVisibleBoundingRect() ?? GetBoundingRectangle();
	}

	protected override bool HasKeyboardFocusCore()
	{
		AutomationPeer wrapperPeer = WrapperPeer;
		if (wrapperPeer != null)
		{
			return wrapperPeer.HasKeyboardFocus();
		}
		ThrowElementNotAvailableException();
		return false;
	}

	protected override bool IsContentElementCore()
	{
		AutomationPeer wrapperPeer = WrapperPeer;
		if (wrapperPeer != null)
		{
			return wrapperPeer.IsContentElement();
		}
		ThrowElementNotAvailableException();
		return true;
	}

	protected override bool IsControlElementCore()
	{
		AutomationPeer wrapperPeer = WrapperPeer;
		if (wrapperPeer != null)
		{
			return wrapperPeer.IsControlElement();
		}
		ThrowElementNotAvailableException();
		return true;
	}

	protected override bool IsDialogCore()
	{
		AutomationPeer wrapperPeer = WrapperPeer;
		if (wrapperPeer != null)
		{
			return wrapperPeer.IsDialog();
		}
		ThrowElementNotAvailableException();
		return false;
	}

	protected override bool IsEnabledCore()
	{
		AutomationPeer wrapperPeer = WrapperPeer;
		if (wrapperPeer != null)
		{
			return wrapperPeer.IsEnabled();
		}
		ThrowElementNotAvailableException();
		return false;
	}

	protected override bool IsKeyboardFocusableCore()
	{
		AutomationPeer wrapperPeer = WrapperPeer;
		if (wrapperPeer != null)
		{
			return wrapperPeer.IsKeyboardFocusable();
		}
		ThrowElementNotAvailableException();
		return false;
	}

	protected override bool IsOffscreenCore()
	{
		AutomationPeer wrapperPeer = WrapperPeer;
		if (wrapperPeer != null)
		{
			return wrapperPeer.IsOffscreen();
		}
		ThrowElementNotAvailableException();
		return true;
	}

	protected override bool IsPasswordCore()
	{
		AutomationPeer wrapperPeer = WrapperPeer;
		if (wrapperPeer != null)
		{
			return wrapperPeer.IsPassword();
		}
		ThrowElementNotAvailableException();
		return false;
	}

	protected override bool IsRequiredForFormCore()
	{
		AutomationPeer wrapperPeer = WrapperPeer;
		if (wrapperPeer != null)
		{
			return wrapperPeer.IsRequiredForForm();
		}
		ThrowElementNotAvailableException();
		return false;
	}

	protected override void SetFocusCore()
	{
		UIElementAutomationPeer wrapperPeer = WrapperPeer;
		if (wrapperPeer != null)
		{
			wrapperPeer.SetFocus();
		}
		else
		{
			ThrowElementNotAvailableException();
		}
	}

	/// <summary>Adds the current element to the collection of selected items.</summary>
	void ISelectionItemProvider.AddToSelection()
	{
		if (!((ISelectionItemProvider)this).IsSelected && IsDayButton && EnsureSelection())
		{
			if (OwningCalendar.SelectionMode == CalendarSelectionMode.SingleDate)
			{
				OwningCalendar.SelectedDate = Date;
			}
			else
			{
				OwningCalendar.SelectedDates.Add(Date);
			}
		}
	}

	/// <summary>Removes the current element from the collection of selected items.</summary>
	void ISelectionItemProvider.RemoveFromSelection()
	{
		if (((ISelectionItemProvider)this).IsSelected && IsDayButton)
		{
			OwningCalendar.SelectedDates.Remove(Date);
		}
	}

	/// <summary>Clears any selected items and then selects the current element.</summary>
	void ISelectionItemProvider.Select()
	{
		Button owningButton = OwningButton;
		if (IsDayButton)
		{
			if (EnsureSelection() && OwningCalendar.SelectionMode == CalendarSelectionMode.SingleDate)
			{
				OwningCalendar.SelectedDate = Date;
			}
		}
		else if (owningButton != null && owningButton.IsEnabled)
		{
			owningButton.Focus();
		}
	}

	/// <summary>Retrieves a collection of UI Automation providers that represents all the column headers associated with a table item or cell.</summary>
	/// <returns>A collection of UI Automation providers.</returns>
	IRawElementProviderSimple[] ITableItemProvider.GetColumnHeaderItems()
	{
		if (IsDayButton && OwningButton != null && OwningCalendar != null && OwningCalendarProvider != null)
		{
			IRawElementProviderSimple[] columnHeaders = ((ITableProvider)UIElementAutomationPeer.CreatePeerForElement(OwningCalendar)).GetColumnHeaders();
			if (columnHeaders != null)
			{
				int column = ((IGridItemProvider)this).Column;
				return new IRawElementProviderSimple[1] { columnHeaders[column] };
			}
		}
		return null;
	}

	/// <summary>Retrieves a collection of UI Automation providers that represents all the row headers associated with a table item or cell.</summary>
	/// <returns>A collection of UI Automation providers.</returns>
	IRawElementProviderSimple[] ITableItemProvider.GetRowHeaderItems()
	{
		return null;
	}

	/// <summary>Sends a request to activate a control and initiate its single, unambiguous action.</summary>
	void IInvokeProvider.Invoke()
	{
		Button owningButton = OwningButton;
		if (owningButton == null || !IsEnabled())
		{
			throw new ElementNotEnabledException();
		}
		base.Dispatcher.BeginInvoke(DispatcherPriority.Input, (DispatcherOperationCallback)delegate
		{
			owningButton.AutomationButtonBaseClick();
			return (object)null;
		}, null);
	}

	/// <summary>Makes the virtual item fully accessible as a UI Automation element.</summary>
	void IVirtualizedItemProvider.Realize()
	{
		if (OwningCalendar.DisplayMode != ButtonMode)
		{
			OwningCalendar.DisplayMode = ButtonMode;
		}
		OwningCalendar.DisplayDate = Date;
	}

	internal override bool IsDataItemAutomationPeer()
	{
		return true;
	}

	internal override void AddToParentProxyWeakRefCache()
	{
		if (UIElementAutomationPeer.CreatePeerForElement(OwningCalendar) is CalendarAutomationPeer calendarAutomationPeer)
		{
			calendarAutomationPeer.AddProxyToWeakRefStorage(base.ElementProxyWeakReference, this);
		}
	}

	private bool EnsureSelection()
	{
		if (OwningCalendar.BlackoutDates.Contains(Date) || OwningCalendar.SelectionMode == CalendarSelectionMode.None)
		{
			return false;
		}
		return true;
	}

	private bool IsItemInAutomationTree()
	{
		AutomationPeer parent = GetParent();
		if (base.Index != -1 && parent != null && parent.Children != null && base.Index < parent.Children.Count && parent.Children[base.Index] == this)
		{
			return true;
		}
		return false;
	}

	private void ThrowElementNotAvailableException()
	{
		if (VirtualizedItemPatternIdentifiers.Pattern != null)
		{
			throw new ElementNotAvailableException(SR.VirtualizedElement);
		}
	}
}
