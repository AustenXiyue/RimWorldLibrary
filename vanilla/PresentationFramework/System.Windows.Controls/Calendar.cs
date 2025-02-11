using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Represents a control that enables a user to select a date by using a visual calendar display. </summary>
[TemplatePart(Name = "PART_Root", Type = typeof(Panel))]
[TemplatePart(Name = "PART_CalendarItem", Type = typeof(CalendarItem))]
public class Calendar : Control
{
	private const string ElementRoot = "PART_Root";

	private const string ElementMonth = "PART_CalendarItem";

	private const int COLS = 7;

	private const int ROWS = 7;

	private const int YEAR_ROWS = 3;

	private const int YEAR_COLS = 4;

	private const int YEARS_PER_DECADE = 10;

	private DateTime? _hoverStart;

	private DateTime? _hoverEnd;

	private bool _isShiftPressed;

	private DateTime? _currentDate;

	private CalendarItem _monthControl;

	private CalendarBlackoutDatesCollection _blackoutDates;

	private SelectedDatesCollection _selectedDates;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.Calendar.SelectedDatesChanged" /> routed event.</summary>
	public static readonly RoutedEvent SelectedDatesChangedEvent;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Calendar.CalendarButtonStyle" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Calendar.CalendarButtonStyle" /> dependency property.</returns>
	public static readonly DependencyProperty CalendarButtonStyleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Calendar.CalendarDayButtonStyle" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Calendar.CalendarDayButtonStyle" /> dependency property.</returns>
	public static readonly DependencyProperty CalendarDayButtonStyleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Calendar.CalendarItemStyle" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Calendar.CalendarItemStyle" /> dependency property.</returns>
	public static readonly DependencyProperty CalendarItemStyleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Calendar.DisplayDate" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Calendar.DisplayDate" /> dependency property.</returns>
	public static readonly DependencyProperty DisplayDateProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Calendar.DisplayDateEnd" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Calendar.DisplayDateEnd" /> dependency property.</returns>
	public static readonly DependencyProperty DisplayDateEndProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Calendar.DisplayDateStart" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Calendar.DisplayDateStart" /> dependency property.</returns>
	public static readonly DependencyProperty DisplayDateStartProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Calendar.DisplayMode" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Calendar.DisplayMode" /> dependency property.</returns>
	public static readonly DependencyProperty DisplayModeProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Calendar.FirstDayOfWeek" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Calendar.FirstDayOfWeek" /> dependency property.</returns>
	public static readonly DependencyProperty FirstDayOfWeekProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Calendar.IsTodayHighlighted" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Calendar.IsTodayHighlighted" /> dependency property.</returns>
	public static readonly DependencyProperty IsTodayHighlightedProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Calendar.SelectedDate" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Calendar.SelectedDate" /> dependency property.</returns>
	public static readonly DependencyProperty SelectedDateProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Calendar.SelectionMode" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Calendar.SelectionMode" /> dependency property.</returns>
	public static readonly DependencyProperty SelectionModeProperty;

	/// <summary>Gets a collection of dates that are marked as not selectable.</summary>
	/// <returns>A collection of dates that cannot be selected. The default value is an empty collection.</returns>
	public CalendarBlackoutDatesCollection BlackoutDates => _blackoutDates;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Style" /> associated with the control's internal <see cref="T:System.Windows.Controls.Primitives.CalendarButton" /> object.</summary>
	/// <returns>The current style of the <see cref="T:System.Windows.Controls.Primitives.CalendarButton" /> object.</returns>
	public Style CalendarButtonStyle
	{
		get
		{
			return (Style)GetValue(CalendarButtonStyleProperty);
		}
		set
		{
			SetValue(CalendarButtonStyleProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Style" /> associated with the control's internal <see cref="T:System.Windows.Controls.Primitives.CalendarDayButton" /> object.</summary>
	/// <returns>The current style of the <see cref="T:System.Windows.Controls.Primitives.CalendarDayButton" /> object.</returns>
	public Style CalendarDayButtonStyle
	{
		get
		{
			return (Style)GetValue(CalendarDayButtonStyleProperty);
		}
		set
		{
			SetValue(CalendarDayButtonStyleProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Style" /> associated with the control's internal <see cref="T:System.Windows.Controls.Primitives.CalendarItem" /> object.</summary>
	/// <returns>The current style of the <see cref="T:System.Windows.Controls.Primitives.CalendarItem" /> object.</returns>
	public Style CalendarItemStyle
	{
		get
		{
			return (Style)GetValue(CalendarItemStyleProperty);
		}
		set
		{
			SetValue(CalendarItemStyleProperty, value);
		}
	}

	/// <summary>Gets or sets the date to display.</summary>
	/// <returns>The date to display. The default is <see cref="P:System.DateTime.Today" />.</returns>
	public DateTime DisplayDate
	{
		get
		{
			return (DateTime)GetValue(DisplayDateProperty);
		}
		set
		{
			SetValue(DisplayDateProperty, value);
		}
	}

	/// <summary>Gets or sets the last date in the date range that is available in the calendar.</summary>
	/// <returns>The last date that is available in the calendar.</returns>
	public DateTime? DisplayDateEnd
	{
		get
		{
			return (DateTime?)GetValue(DisplayDateEndProperty);
		}
		set
		{
			SetValue(DisplayDateEndProperty, value);
		}
	}

	/// <summary>Gets or sets the first date that is available in the calendar.</summary>
	/// <returns>The first date that is available in the calendar. The default is null.</returns>
	public DateTime? DisplayDateStart
	{
		get
		{
			return (DateTime?)GetValue(DisplayDateStartProperty);
		}
		set
		{
			SetValue(DisplayDateStartProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the calendar displays a month, year, or decade.</summary>
	/// <returns>A value that indicates what length of time the <see cref="T:System.Windows.Controls.Calendar" /> should display.</returns>
	public CalendarMode DisplayMode
	{
		get
		{
			return (CalendarMode)GetValue(DisplayModeProperty);
		}
		set
		{
			SetValue(DisplayModeProperty, value);
		}
	}

	/// <summary>Gets or sets the day that is considered the beginning of the week.</summary>
	/// <returns>A <see cref="T:System.DayOfWeek" /> that represents the beginning of the week. The default is the <see cref="P:System.Globalization.DateTimeFormatInfo.FirstDayOfWeek" /> that is determined by the current culture.</returns>
	public DayOfWeek FirstDayOfWeek
	{
		get
		{
			return (DayOfWeek)GetValue(FirstDayOfWeekProperty);
		}
		set
		{
			SetValue(FirstDayOfWeekProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the current date is highlighted.</summary>
	/// <returns>true if the current date is highlighted; otherwise, false. The default is true. </returns>
	public bool IsTodayHighlighted
	{
		get
		{
			return (bool)GetValue(IsTodayHighlightedProperty);
		}
		set
		{
			SetValue(IsTodayHighlightedProperty, value);
		}
	}

	/// <summary>Gets or sets the currently selected date.</summary>
	/// <returns>The date currently selected. The default is null.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The specified date is outside the range specified by <see cref="P:System.Windows.Controls.Calendar.DisplayDateStart" /> and <see cref="P:System.Windows.Controls.Calendar.DisplayDateEnd" />-or-The specified date is in the <see cref="P:System.Windows.Controls.Calendar.BlackoutDates" /> collection.</exception>
	/// <exception cref="T:System.InvalidOperationException">If set to anything other than null when <see cref="P:System.Windows.Controls.Calendar.SelectionMode" /> is set to <see cref="F:System.Windows.Controls.CalendarSelectionMode.None" />.</exception>
	public DateTime? SelectedDate
	{
		get
		{
			return (DateTime?)GetValue(SelectedDateProperty);
		}
		set
		{
			SetValue(SelectedDateProperty, value);
		}
	}

	/// <summary>Gets a collection of selected dates.</summary>
	/// <returns>A <see cref="T:System.Windows.Controls.SelectedDatesCollection" /> object that contains the currently selected dates. The default is an empty collection.</returns>
	public SelectedDatesCollection SelectedDates => _selectedDates;

	/// <summary>Gets or sets a value that indicates what kind of selections are allowed.</summary>
	/// <returns>A value that indicates the current selection mode. The default is <see cref="F:System.Windows.Controls.CalendarSelectionMode.SingleDate" />.</returns>
	public CalendarSelectionMode SelectionMode
	{
		get
		{
			return (CalendarSelectionMode)GetValue(SelectionModeProperty);
		}
		set
		{
			SetValue(SelectionModeProperty, value);
		}
	}

	internal bool DatePickerDisplayDateFlag { get; set; }

	internal DateTime DisplayDateInternal { get; private set; }

	internal DateTime DisplayDateEndInternal => DisplayDateEnd.GetValueOrDefault(DateTime.MaxValue);

	internal DateTime DisplayDateStartInternal => DisplayDateStart.GetValueOrDefault(DateTime.MinValue);

	internal DateTime CurrentDate
	{
		get
		{
			return _currentDate.GetValueOrDefault(DisplayDateInternal);
		}
		set
		{
			_currentDate = value;
		}
	}

	internal DateTime? HoverStart
	{
		get
		{
			if (SelectionMode != CalendarSelectionMode.None)
			{
				return _hoverStart;
			}
			return null;
		}
		set
		{
			_hoverStart = value;
		}
	}

	internal DateTime? HoverEnd
	{
		get
		{
			if (SelectionMode != CalendarSelectionMode.None)
			{
				return _hoverEnd;
			}
			return null;
		}
		set
		{
			_hoverEnd = value;
		}
	}

	internal CalendarItem MonthControl => _monthControl;

	internal DateTime DisplayMonth => DateTimeHelper.DiscardDayTime(DisplayDate);

	internal DateTime DisplayYear => new DateTime(DisplayDate.Year, 1, 1);

	/// <summary>Occurs when the collection returned by the <see cref="P:System.Windows.Controls.Calendar.SelectedDates" /> property is changed. </summary>
	public event EventHandler<SelectionChangedEventArgs> SelectedDatesChanged
	{
		add
		{
			AddHandler(SelectedDatesChangedEvent, value);
		}
		remove
		{
			RemoveHandler(SelectedDatesChangedEvent, value);
		}
	}

	/// <summary>Occurs when the <see cref="P:System.Windows.Controls.Calendar.DisplayDate" /> property is changed.</summary>
	public event EventHandler<CalendarDateChangedEventArgs> DisplayDateChanged;

	/// <summary>Occurs when the <see cref="P:System.Windows.Controls.Calendar.DisplayMode" /> property is changed. </summary>
	public event EventHandler<CalendarModeChangedEventArgs> DisplayModeChanged;

	/// <summary>Occurs when the <see cref="P:System.Windows.Controls.Calendar.SelectionMode" /> changes.</summary>
	public event EventHandler<EventArgs> SelectionModeChanged;

	internal event MouseButtonEventHandler DayButtonMouseUp;

	internal event RoutedEventHandler DayOrMonthPreviewKeyDown;

	static Calendar()
	{
		SelectedDatesChangedEvent = EventManager.RegisterRoutedEvent("SelectedDatesChanged", RoutingStrategy.Direct, typeof(EventHandler<SelectionChangedEventArgs>), typeof(Calendar));
		CalendarButtonStyleProperty = DependencyProperty.Register("CalendarButtonStyle", typeof(Style), typeof(Calendar));
		CalendarDayButtonStyleProperty = DependencyProperty.Register("CalendarDayButtonStyle", typeof(Style), typeof(Calendar));
		CalendarItemStyleProperty = DependencyProperty.Register("CalendarItemStyle", typeof(Style), typeof(Calendar));
		DisplayDateProperty = DependencyProperty.Register("DisplayDate", typeof(DateTime), typeof(Calendar), new FrameworkPropertyMetadata(DateTime.MinValue, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDisplayDateChanged, CoerceDisplayDate));
		DisplayDateEndProperty = DependencyProperty.Register("DisplayDateEnd", typeof(DateTime?), typeof(Calendar), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDisplayDateEndChanged, CoerceDisplayDateEnd));
		DisplayDateStartProperty = DependencyProperty.Register("DisplayDateStart", typeof(DateTime?), typeof(Calendar), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDisplayDateStartChanged, CoerceDisplayDateStart));
		DisplayModeProperty = DependencyProperty.Register("DisplayMode", typeof(CalendarMode), typeof(Calendar), new FrameworkPropertyMetadata(CalendarMode.Month, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDisplayModePropertyChanged), IsValidDisplayMode);
		FirstDayOfWeekProperty = DependencyProperty.Register("FirstDayOfWeek", typeof(DayOfWeek), typeof(Calendar), new FrameworkPropertyMetadata(DateTimeHelper.GetCurrentDateFormat().FirstDayOfWeek, OnFirstDayOfWeekChanged), IsValidFirstDayOfWeek);
		IsTodayHighlightedProperty = DependencyProperty.Register("IsTodayHighlighted", typeof(bool), typeof(Calendar), new FrameworkPropertyMetadata(true, OnIsTodayHighlightedChanged));
		SelectedDateProperty = DependencyProperty.Register("SelectedDate", typeof(DateTime?), typeof(Calendar), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedDateChanged));
		SelectionModeProperty = DependencyProperty.Register("SelectionMode", typeof(CalendarSelectionMode), typeof(Calendar), new FrameworkPropertyMetadata(CalendarSelectionMode.SingleDate, OnSelectionModeChanged), IsValidSelectionMode);
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(Calendar), new FrameworkPropertyMetadata(typeof(Calendar)));
		KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(Calendar), new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));
		KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(Calendar), new FrameworkPropertyMetadata(KeyboardNavigationMode.Contained));
		FrameworkElement.LanguageProperty.OverrideMetadata(typeof(Calendar), new FrameworkPropertyMetadata(OnLanguageChanged));
		EventManager.RegisterClassHandler(typeof(Calendar), UIElement.GotFocusEvent, new RoutedEventHandler(OnGotFocus));
		ControlsTraceLogger.AddControl(TelemetryControls.Calendar);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Calendar" /> class. </summary>
	public Calendar()
	{
		_blackoutDates = new CalendarBlackoutDatesCollection(this);
		_selectedDates = new SelectedDatesCollection(this);
		SetCurrentValueInternal(DisplayDateProperty, DateTime.Today);
	}

	private static void OnDisplayDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Calendar obj = d as Calendar;
		obj.DisplayDateInternal = DateTimeHelper.DiscardDayTime((DateTime)e.NewValue);
		obj.UpdateCellItems();
		obj.OnDisplayDateChanged(new CalendarDateChangedEventArgs((DateTime)e.OldValue, (DateTime)e.NewValue));
	}

	private static object CoerceDisplayDate(DependencyObject d, object value)
	{
		Calendar calendar = d as Calendar;
		DateTime dateTime = (DateTime)value;
		if (calendar.DisplayDateStart.HasValue && dateTime < calendar.DisplayDateStart.Value)
		{
			value = calendar.DisplayDateStart.Value;
		}
		else if (calendar.DisplayDateEnd.HasValue && dateTime > calendar.DisplayDateEnd.Value)
		{
			value = calendar.DisplayDateEnd.Value;
		}
		return value;
	}

	private static void OnDisplayDateEndChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Calendar obj = d as Calendar;
		obj.CoerceValue(DisplayDateProperty);
		obj.UpdateCellItems();
	}

	private static object CoerceDisplayDateEnd(DependencyObject d, object value)
	{
		Calendar calendar = d as Calendar;
		DateTime? dateTime = (DateTime?)value;
		if (dateTime.HasValue)
		{
			if (calendar.DisplayDateStart.HasValue && dateTime.Value < calendar.DisplayDateStart.Value)
			{
				value = calendar.DisplayDateStart;
			}
			DateTime? maximumDate = calendar.SelectedDates.MaximumDate;
			if (maximumDate.HasValue && dateTime.Value < maximumDate.Value)
			{
				value = maximumDate;
			}
		}
		return value;
	}

	private static void OnDisplayDateStartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Calendar obj = d as Calendar;
		obj.CoerceValue(DisplayDateEndProperty);
		obj.CoerceValue(DisplayDateProperty);
		obj.UpdateCellItems();
	}

	private static object CoerceDisplayDateStart(DependencyObject d, object value)
	{
		Calendar calendar = d as Calendar;
		DateTime? dateTime = (DateTime?)value;
		if (dateTime.HasValue)
		{
			DateTime? minimumDate = calendar.SelectedDates.MinimumDate;
			if (minimumDate.HasValue && dateTime.Value > minimumDate.Value)
			{
				value = minimumDate;
			}
		}
		return value;
	}

	private static void OnDisplayModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Calendar calendar = d as Calendar;
		CalendarMode calendarMode = (CalendarMode)e.NewValue;
		CalendarMode calendarMode2 = (CalendarMode)e.OldValue;
		_ = calendar.MonthControl;
		switch (calendarMode)
		{
		case CalendarMode.Month:
			if (calendarMode2 == CalendarMode.Year || calendarMode2 == CalendarMode.Decade)
			{
				DateTime? hoverStart = (calendar.HoverEnd = null);
				calendar.HoverStart = hoverStart;
				calendar.CurrentDate = calendar.DisplayDate;
			}
			calendar.UpdateCellItems();
			break;
		case CalendarMode.Year:
		case CalendarMode.Decade:
			if (calendarMode2 == CalendarMode.Month)
			{
				calendar.SetCurrentValueInternal(DisplayDateProperty, calendar.CurrentDate);
			}
			calendar.UpdateCellItems();
			break;
		}
		calendar.OnDisplayModeChanged(new CalendarModeChangedEventArgs((CalendarMode)e.OldValue, calendarMode));
	}

	private static void OnFirstDayOfWeekChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		(d as Calendar).UpdateCellItems();
	}

	private static void OnIsTodayHighlightedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Calendar calendar = d as Calendar;
		int num = DateTimeHelper.CompareYearMonth(calendar.DisplayDateInternal, DateTime.Today);
		if (num > -2 && num < 2)
		{
			calendar.UpdateCellItems();
		}
	}

	private static void OnLanguageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Calendar calendar = d as Calendar;
		if (DependencyPropertyHelper.GetValueSource(d, FirstDayOfWeekProperty).BaseValueSource == BaseValueSource.Default)
		{
			calendar.SetCurrentValueInternal(FirstDayOfWeekProperty, DateTimeHelper.GetDateFormat(DateTimeHelper.GetCulture(calendar)).FirstDayOfWeek);
			calendar.UpdateCellItems();
		}
	}

	private static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Calendar calendar = d as Calendar;
		if (calendar.SelectionMode != CalendarSelectionMode.None || e.NewValue == null)
		{
			DateTime? dateTime = (DateTime?)e.NewValue;
			if (IsValidDateSelection(calendar, dateTime))
			{
				if (!dateTime.HasValue)
				{
					calendar.SelectedDates.ClearInternal(fireChangeNotification: true);
				}
				else if (dateTime.HasValue && (calendar.SelectedDates.Count <= 0 || !(calendar.SelectedDates[0] == dateTime.Value)))
				{
					calendar.SelectedDates.ClearInternal();
					calendar.SelectedDates.Add(dateTime.Value);
				}
				if (calendar.SelectionMode == CalendarSelectionMode.SingleDate)
				{
					if (dateTime.HasValue)
					{
						calendar.CurrentDate = dateTime.Value;
					}
					calendar.UpdateCellItems();
				}
				return;
			}
			throw new ArgumentOutOfRangeException("d", SR.Calendar_OnSelectedDateChanged_InvalidValue);
		}
		throw new InvalidOperationException(SR.Calendar_OnSelectedDateChanged_InvalidOperation);
	}

	private static void OnSelectionModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Calendar obj = d as Calendar;
		DateTime? hoverStart = (obj.HoverEnd = null);
		obj.HoverStart = hoverStart;
		obj.SelectedDates.ClearInternal(fireChangeNotification: true);
		obj.OnSelectionModeChanged(EventArgs.Empty);
	}

	/// <summary>Builds the visual tree for the <see cref="T:System.Windows.Controls.Calendar" /> control when a new template is applied.</summary>
	public override void OnApplyTemplate()
	{
		if (_monthControl != null)
		{
			_monthControl.Owner = null;
		}
		base.OnApplyTemplate();
		_monthControl = GetTemplateChild("PART_CalendarItem") as CalendarItem;
		if (_monthControl != null)
		{
			_monthControl.Owner = this;
		}
		CurrentDate = DisplayDate;
		UpdateCellItems();
	}

	/// <summary>Provides a text representation of the selected date.</summary>
	/// <returns>A text representation of the selected date, or an empty string if <see cref="P:System.Windows.Controls.Calendar.SelectedDate" /> is null.</returns>
	public override string ToString()
	{
		if (SelectedDate.HasValue)
		{
			return SelectedDate.Value.ToString(DateTimeHelper.GetDateFormat(DateTimeHelper.GetCulture(this)));
		}
		return string.Empty;
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.Calendar.SelectedDatesChanged" /> routed event. </summary>
	/// <param name="e">The data for the event. </param>
	protected virtual void OnSelectedDatesChanged(SelectionChangedEventArgs e)
	{
		RaiseEvent(e);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.Calendar.DisplayDateChanged" /> event. </summary>
	/// <param name="e">The data for the event. </param>
	protected virtual void OnDisplayDateChanged(CalendarDateChangedEventArgs e)
	{
		this.DisplayDateChanged?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.Calendar.DisplayModeChanged" /> event. </summary>
	/// <param name="e">The data for the event. </param>
	protected virtual void OnDisplayModeChanged(CalendarModeChangedEventArgs e)
	{
		this.DisplayModeChanged?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.Calendar.SelectionModeChanged" /> event. </summary>
	/// <param name="e">The data for the event. </param>
	protected virtual void OnSelectionModeChanged(EventArgs e)
	{
		this.SelectionModeChanged?.Invoke(this, e);
	}

	/// <summary>Returns a <see cref="T:System.Windows.Automation.Peers.CalendarAutomationPeer" /> for use by the  automation infrastructure.</summary>
	/// <returns>A <see cref="T:System.Windows.Automation.Peers.CalendarAutomationPeer" /> for the <see cref="T:System.Windows.Controls.Calendar" /> object.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new CalendarAutomationPeer(this);
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.KeyDown" /> routed event that occurs when the user presses a key while this control has focus.</summary>
	/// <param name="e">The data for the event. </param>
	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (!e.Handled)
		{
			e.Handled = ProcessCalendarKey(e);
		}
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.KeyUp" /> routed event that occurs when the user releases a key while this control has focus.</summary>
	/// <param name="e">The data for the event. </param>
	protected override void OnKeyUp(KeyEventArgs e)
	{
		if (!e.Handled && (e.Key == Key.LeftShift || e.Key == Key.RightShift))
		{
			ProcessShiftKeyUp();
		}
	}

	internal CalendarDayButton FindDayButtonFromDay(DateTime day)
	{
		if (MonthControl != null)
		{
			foreach (CalendarDayButton calendarDayButton in MonthControl.GetCalendarDayButtons())
			{
				if (calendarDayButton.DataContext is DateTime && DateTimeHelper.CompareDays((DateTime)calendarDayButton.DataContext, day) == 0)
				{
					return calendarDayButton;
				}
			}
		}
		return null;
	}

	internal static bool IsValidDateSelection(Calendar cal, object value)
	{
		if (value != null)
		{
			return !cal.BlackoutDates.Contains((DateTime)value);
		}
		return true;
	}

	internal void OnDayButtonMouseUp(MouseButtonEventArgs e)
	{
		this.DayButtonMouseUp?.Invoke(this, e);
	}

	internal void OnDayOrMonthPreviewKeyDown(RoutedEventArgs e)
	{
		this.DayOrMonthPreviewKeyDown?.Invoke(this, e);
	}

	internal void OnDayClick(DateTime selectedDate)
	{
		if (SelectionMode == CalendarSelectionMode.None)
		{
			CurrentDate = selectedDate;
		}
		if (DateTimeHelper.CompareYearMonth(selectedDate, DisplayDateInternal) != 0)
		{
			MoveDisplayTo(selectedDate);
			return;
		}
		UpdateCellItems();
		FocusDate(selectedDate);
	}

	internal void OnCalendarButtonPressed(CalendarButton b, bool switchDisplayMode)
	{
		if (!(b.DataContext is DateTime))
		{
			return;
		}
		DateTime yearMonth = (DateTime)b.DataContext;
		DateTime? dateTime = null;
		CalendarMode calendarMode = CalendarMode.Month;
		switch (DisplayMode)
		{
		case CalendarMode.Year:
			dateTime = DateTimeHelper.SetYearMonth(DisplayDate, yearMonth);
			calendarMode = CalendarMode.Month;
			break;
		case CalendarMode.Decade:
			dateTime = DateTimeHelper.SetYear(DisplayDate, yearMonth.Year);
			calendarMode = CalendarMode.Year;
			break;
		}
		if (dateTime.HasValue)
		{
			DisplayDate = dateTime.Value;
			if (switchDisplayMode)
			{
				SetCurrentValueInternal(DisplayModeProperty, calendarMode);
				FocusDate((DisplayMode == CalendarMode.Month) ? CurrentDate : DisplayDate);
			}
		}
	}

	private DateTime? GetDateOffset(DateTime date, int offset, CalendarMode displayMode)
	{
		DateTime? dateTime = null;
		return displayMode switch
		{
			CalendarMode.Month => DateTimeHelper.AddMonths(date, offset), 
			CalendarMode.Year => DateTimeHelper.AddYears(date, offset), 
			CalendarMode.Decade => DateTimeHelper.AddYears(DisplayDate, offset * 10), 
			_ => dateTime, 
		};
	}

	private void MoveDisplayTo(DateTime? date)
	{
		if (date.HasValue)
		{
			DateTime date2 = date.Value.Date;
			switch (DisplayMode)
			{
			case CalendarMode.Month:
				SetCurrentValueInternal(DisplayDateProperty, DateTimeHelper.DiscardDayTime(date2));
				CurrentDate = date2;
				UpdateCellItems();
				break;
			case CalendarMode.Year:
			case CalendarMode.Decade:
				SetCurrentValueInternal(DisplayDateProperty, date2);
				UpdateCellItems();
				break;
			}
			FocusDate(date2);
		}
	}

	internal void OnNextClick()
	{
		DateTime? dateOffset = GetDateOffset(DisplayDate, 1, DisplayMode);
		if (dateOffset.HasValue)
		{
			MoveDisplayTo(DateTimeHelper.DiscardDayTime(dateOffset.Value));
		}
	}

	internal void OnPreviousClick()
	{
		DateTime? dateOffset = GetDateOffset(DisplayDate, -1, DisplayMode);
		if (dateOffset.HasValue)
		{
			MoveDisplayTo(DateTimeHelper.DiscardDayTime(dateOffset.Value));
		}
	}

	internal void OnSelectedDatesCollectionChanged(SelectionChangedEventArgs e)
	{
		if (IsSelectionChanged(e))
		{
			if ((AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementSelected) || AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementAddedToSelection) || AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection)) && UIElementAutomationPeer.FromElement(this) is CalendarAutomationPeer calendarAutomationPeer)
			{
				calendarAutomationPeer.RaiseSelectionEvents(e);
			}
			CoerceFromSelection();
			OnSelectedDatesChanged(e);
		}
	}

	internal void UpdateCellItems()
	{
		CalendarItem monthControl = MonthControl;
		if (monthControl != null)
		{
			switch (DisplayMode)
			{
			case CalendarMode.Month:
				monthControl.UpdateMonthMode();
				break;
			case CalendarMode.Year:
				monthControl.UpdateYearMode();
				break;
			case CalendarMode.Decade:
				monthControl.UpdateDecadeMode();
				break;
			}
		}
	}

	private void CoerceFromSelection()
	{
		CoerceValue(DisplayDateStartProperty);
		CoerceValue(DisplayDateEndProperty);
		CoerceValue(DisplayDateProperty);
	}

	private void AddKeyboardSelection()
	{
		if (HoverStart.HasValue)
		{
			SelectedDates.ClearInternal();
			SelectedDates.AddRange(HoverStart.Value, CurrentDate);
		}
	}

	private static bool IsSelectionChanged(SelectionChangedEventArgs e)
	{
		if (e.AddedItems.Count != e.RemovedItems.Count)
		{
			return true;
		}
		foreach (DateTime addedItem in e.AddedItems)
		{
			if (!e.RemovedItems.Contains(addedItem))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsValidDisplayMode(object value)
	{
		CalendarMode calendarMode = (CalendarMode)value;
		if (calendarMode != 0 && calendarMode != CalendarMode.Year)
		{
			return calendarMode == CalendarMode.Decade;
		}
		return true;
	}

	internal static bool IsValidFirstDayOfWeek(object value)
	{
		DayOfWeek dayOfWeek = (DayOfWeek)value;
		if (dayOfWeek != 0 && dayOfWeek != DayOfWeek.Monday && dayOfWeek != DayOfWeek.Tuesday && dayOfWeek != DayOfWeek.Wednesday && dayOfWeek != DayOfWeek.Thursday && dayOfWeek != DayOfWeek.Friday)
		{
			return dayOfWeek == DayOfWeek.Saturday;
		}
		return true;
	}

	private static bool IsValidKeyboardSelection(Calendar cal, object value)
	{
		if (value == null)
		{
			return true;
		}
		if (cal.BlackoutDates.Contains((DateTime)value))
		{
			return false;
		}
		if (DateTime.Compare((DateTime)value, cal.DisplayDateStartInternal) >= 0)
		{
			return DateTime.Compare((DateTime)value, cal.DisplayDateEndInternal) <= 0;
		}
		return false;
	}

	private static bool IsValidSelectionMode(object value)
	{
		CalendarSelectionMode calendarSelectionMode = (CalendarSelectionMode)value;
		if (calendarSelectionMode != 0 && calendarSelectionMode != CalendarSelectionMode.SingleRange && calendarSelectionMode != CalendarSelectionMode.MultipleRange)
		{
			return calendarSelectionMode == CalendarSelectionMode.None;
		}
		return true;
	}

	private void OnSelectedMonthChanged(DateTime? selectedMonth)
	{
		if (selectedMonth.HasValue)
		{
			SetCurrentValueInternal(DisplayDateProperty, selectedMonth.Value);
			UpdateCellItems();
			FocusDate(selectedMonth.Value);
		}
	}

	private void OnSelectedYearChanged(DateTime? selectedYear)
	{
		if (selectedYear.HasValue)
		{
			SetCurrentValueInternal(DisplayDateProperty, selectedYear.Value);
			UpdateCellItems();
			FocusDate(selectedYear.Value);
		}
	}

	internal void FocusDate(DateTime date)
	{
		if (MonthControl != null)
		{
			MonthControl.FocusDate(date);
		}
	}

	private static void OnGotFocus(object sender, RoutedEventArgs e)
	{
		Calendar calendar = (Calendar)sender;
		if (!e.Handled && e.OriginalSource == calendar)
		{
			if (calendar.SelectedDate.HasValue && DateTimeHelper.CompareYearMonth(calendar.SelectedDate.Value, calendar.DisplayDateInternal) == 0)
			{
				calendar.FocusDate(calendar.SelectedDate.Value);
			}
			else
			{
				calendar.FocusDate(calendar.DisplayDate);
			}
			e.Handled = true;
		}
	}

	private bool ProcessCalendarKey(KeyEventArgs e)
	{
		if (DisplayMode == CalendarMode.Month)
		{
			CalendarDayButton calendarDayButton = ((MonthControl != null) ? MonthControl.GetCalendarDayButton(CurrentDate) : null);
			if (DateTimeHelper.CompareYearMonth(CurrentDate, DisplayDateInternal) != 0 && calendarDayButton != null && !calendarDayButton.IsInactive)
			{
				return false;
			}
		}
		CalendarKeyboardHelper.GetMetaKeyState(out var ctrl, out var shift);
		switch (e.Key)
		{
		case Key.Up:
			ProcessUpKey(ctrl, shift);
			return true;
		case Key.Down:
			ProcessDownKey(ctrl, shift);
			return true;
		case Key.Left:
			ProcessLeftKey(shift);
			return true;
		case Key.Right:
			ProcessRightKey(shift);
			return true;
		case Key.Next:
			ProcessPageDownKey(shift);
			return true;
		case Key.Prior:
			ProcessPageUpKey(shift);
			return true;
		case Key.Home:
			ProcessHomeKey(shift);
			return true;
		case Key.End:
			ProcessEndKey(shift);
			return true;
		case Key.Return:
		case Key.Space:
			return ProcessEnterKey();
		default:
			return false;
		}
	}

	private void ProcessDownKey(bool ctrl, bool shift)
	{
		switch (DisplayMode)
		{
		case CalendarMode.Month:
			if (!ctrl || shift)
			{
				DateTime? nonBlackoutDate = _blackoutDates.GetNonBlackoutDate(DateTimeHelper.AddDays(CurrentDate, 7), 1);
				ProcessSelection(shift, nonBlackoutDate);
			}
			break;
		case CalendarMode.Year:
			if (ctrl)
			{
				SetCurrentValueInternal(DisplayModeProperty, CalendarMode.Month);
				FocusDate(DisplayDate);
			}
			else
			{
				DateTime? selectedMonth = DateTimeHelper.AddMonths(DisplayDate, 4);
				OnSelectedMonthChanged(selectedMonth);
			}
			break;
		case CalendarMode.Decade:
			if (ctrl)
			{
				SetCurrentValueInternal(DisplayModeProperty, CalendarMode.Year);
				FocusDate(DisplayDate);
			}
			else
			{
				DateTime? selectedYear = DateTimeHelper.AddYears(DisplayDate, 4);
				OnSelectedYearChanged(selectedYear);
			}
			break;
		}
	}

	private void ProcessEndKey(bool shift)
	{
		switch (DisplayMode)
		{
		case CalendarMode.Month:
		{
			DateTime? lastSelectedDate = new DateTime(DisplayDateInternal.Year, DisplayDateInternal.Month, 1);
			if (DateTimeHelper.CompareYearMonth(DateTime.MaxValue, lastSelectedDate.Value) > 0)
			{
				lastSelectedDate = DateTimeHelper.AddMonths(lastSelectedDate.Value, 1).Value;
				lastSelectedDate = DateTimeHelper.AddDays(lastSelectedDate.Value, -1).Value;
			}
			else
			{
				lastSelectedDate = DateTime.MaxValue;
			}
			ProcessSelection(shift, lastSelectedDate);
			break;
		}
		case CalendarMode.Year:
		{
			DateTime value = new DateTime(DisplayDate.Year, 12, 1);
			OnSelectedMonthChanged(value);
			break;
		}
		case CalendarMode.Decade:
		{
			DateTime? selectedYear = new DateTime(DateTimeHelper.EndOfDecade(DisplayDate), 1, 1);
			OnSelectedYearChanged(selectedYear);
			break;
		}
		}
	}

	private bool ProcessEnterKey()
	{
		switch (DisplayMode)
		{
		case CalendarMode.Year:
			SetCurrentValueInternal(DisplayModeProperty, CalendarMode.Month);
			FocusDate(DisplayDate);
			return true;
		case CalendarMode.Decade:
			SetCurrentValueInternal(DisplayModeProperty, CalendarMode.Year);
			FocusDate(DisplayDate);
			return true;
		default:
			return false;
		}
	}

	private void ProcessHomeKey(bool shift)
	{
		switch (DisplayMode)
		{
		case CalendarMode.Month:
		{
			DateTime? lastSelectedDate = new DateTime(DisplayDateInternal.Year, DisplayDateInternal.Month, 1);
			ProcessSelection(shift, lastSelectedDate);
			break;
		}
		case CalendarMode.Year:
		{
			DateTime value = new DateTime(DisplayDate.Year, 1, 1);
			OnSelectedMonthChanged(value);
			break;
		}
		case CalendarMode.Decade:
		{
			DateTime? selectedYear = new DateTime(DateTimeHelper.DecadeOfDate(DisplayDate), 1, 1);
			OnSelectedYearChanged(selectedYear);
			break;
		}
		}
	}

	private void ProcessLeftKey(bool shift)
	{
		int num = (base.IsRightToLeft ? 1 : (-1));
		switch (DisplayMode)
		{
		case CalendarMode.Month:
		{
			DateTime? nonBlackoutDate = _blackoutDates.GetNonBlackoutDate(DateTimeHelper.AddDays(CurrentDate, num), num);
			ProcessSelection(shift, nonBlackoutDate);
			break;
		}
		case CalendarMode.Year:
		{
			DateTime? selectedMonth = DateTimeHelper.AddMonths(DisplayDate, num);
			OnSelectedMonthChanged(selectedMonth);
			break;
		}
		case CalendarMode.Decade:
		{
			DateTime? selectedYear = DateTimeHelper.AddYears(DisplayDate, num);
			OnSelectedYearChanged(selectedYear);
			break;
		}
		}
	}

	private void ProcessPageDownKey(bool shift)
	{
		switch (DisplayMode)
		{
		case CalendarMode.Month:
		{
			DateTime? nonBlackoutDate = _blackoutDates.GetNonBlackoutDate(DateTimeHelper.AddMonths(CurrentDate, 1), 1);
			ProcessSelection(shift, nonBlackoutDate);
			break;
		}
		case CalendarMode.Year:
		{
			DateTime? selectedMonth = DateTimeHelper.AddYears(DisplayDate, 1);
			OnSelectedMonthChanged(selectedMonth);
			break;
		}
		case CalendarMode.Decade:
		{
			DateTime? selectedYear = DateTimeHelper.AddYears(DisplayDate, 10);
			OnSelectedYearChanged(selectedYear);
			break;
		}
		}
	}

	private void ProcessPageUpKey(bool shift)
	{
		switch (DisplayMode)
		{
		case CalendarMode.Month:
		{
			DateTime? nonBlackoutDate = _blackoutDates.GetNonBlackoutDate(DateTimeHelper.AddMonths(CurrentDate, -1), -1);
			ProcessSelection(shift, nonBlackoutDate);
			break;
		}
		case CalendarMode.Year:
		{
			DateTime? selectedMonth = DateTimeHelper.AddYears(DisplayDate, -1);
			OnSelectedMonthChanged(selectedMonth);
			break;
		}
		case CalendarMode.Decade:
		{
			DateTime? selectedYear = DateTimeHelper.AddYears(DisplayDate, -10);
			OnSelectedYearChanged(selectedYear);
			break;
		}
		}
	}

	private void ProcessRightKey(bool shift)
	{
		int num = ((!base.IsRightToLeft) ? 1 : (-1));
		switch (DisplayMode)
		{
		case CalendarMode.Month:
		{
			DateTime? nonBlackoutDate = _blackoutDates.GetNonBlackoutDate(DateTimeHelper.AddDays(CurrentDate, num), num);
			ProcessSelection(shift, nonBlackoutDate);
			break;
		}
		case CalendarMode.Year:
		{
			DateTime? selectedMonth = DateTimeHelper.AddMonths(DisplayDate, num);
			OnSelectedMonthChanged(selectedMonth);
			break;
		}
		case CalendarMode.Decade:
		{
			DateTime? selectedYear = DateTimeHelper.AddYears(DisplayDate, num);
			OnSelectedYearChanged(selectedYear);
			break;
		}
		}
	}

	private void ProcessSelection(bool shift, DateTime? lastSelectedDate)
	{
		if (SelectionMode == CalendarSelectionMode.None && lastSelectedDate.HasValue)
		{
			OnDayClick(lastSelectedDate.Value);
		}
		else
		{
			if (!lastSelectedDate.HasValue || !IsValidKeyboardSelection(this, lastSelectedDate.Value))
			{
				return;
			}
			if (SelectionMode == CalendarSelectionMode.SingleRange || SelectionMode == CalendarSelectionMode.MultipleRange)
			{
				SelectedDates.ClearInternal();
				if (shift)
				{
					_isShiftPressed = true;
					if (!HoverStart.HasValue)
					{
						DateTime? hoverStart = (HoverEnd = CurrentDate);
						HoverStart = hoverStart;
					}
					CalendarDateRange range = ((DateTime.Compare(HoverStart.Value, lastSelectedDate.Value) >= 0) ? new CalendarDateRange(lastSelectedDate.Value, HoverStart.Value) : new CalendarDateRange(HoverStart.Value, lastSelectedDate.Value));
					if (!BlackoutDates.ContainsAny(range))
					{
						_currentDate = lastSelectedDate;
						HoverEnd = lastSelectedDate;
					}
					OnDayClick(CurrentDate);
				}
				else
				{
					DateTime value2 = (CurrentDate = lastSelectedDate.Value);
					DateTime? hoverStart = value2;
					HoverEnd = hoverStart;
					HoverStart = hoverStart;
					AddKeyboardSelection();
					OnDayClick(lastSelectedDate.Value);
				}
			}
			else
			{
				CurrentDate = lastSelectedDate.Value;
				DateTime? hoverStart = (HoverEnd = null);
				HoverStart = hoverStart;
				if (SelectedDates.Count > 0)
				{
					SelectedDates[0] = lastSelectedDate.Value;
				}
				else
				{
					SelectedDates.Add(lastSelectedDate.Value);
				}
				OnDayClick(lastSelectedDate.Value);
			}
			UpdateCellItems();
		}
	}

	private void ProcessShiftKeyUp()
	{
		if (_isShiftPressed && (SelectionMode == CalendarSelectionMode.SingleRange || SelectionMode == CalendarSelectionMode.MultipleRange))
		{
			AddKeyboardSelection();
			_isShiftPressed = false;
			DateTime? hoverStart = (HoverEnd = null);
			HoverStart = hoverStart;
		}
	}

	private void ProcessUpKey(bool ctrl, bool shift)
	{
		switch (DisplayMode)
		{
		case CalendarMode.Month:
			if (ctrl)
			{
				SetCurrentValueInternal(DisplayModeProperty, CalendarMode.Year);
				FocusDate(DisplayDate);
			}
			else
			{
				DateTime? nonBlackoutDate = _blackoutDates.GetNonBlackoutDate(DateTimeHelper.AddDays(CurrentDate, -7), -1);
				ProcessSelection(shift, nonBlackoutDate);
			}
			break;
		case CalendarMode.Year:
			if (ctrl)
			{
				SetCurrentValueInternal(DisplayModeProperty, CalendarMode.Decade);
				FocusDate(DisplayDate);
			}
			else
			{
				DateTime? selectedMonth = DateTimeHelper.AddMonths(DisplayDate, -4);
				OnSelectedMonthChanged(selectedMonth);
			}
			break;
		case CalendarMode.Decade:
			if (!ctrl)
			{
				DateTime? selectedYear = DateTimeHelper.AddYears(DisplayDate, -4);
				OnSelectedYearChanged(selectedYear);
			}
			break;
		}
	}
}
