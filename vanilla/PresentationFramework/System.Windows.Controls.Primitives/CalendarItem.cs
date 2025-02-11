using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;

namespace System.Windows.Controls.Primitives;

/// <summary>Represents the currently displayed month or year on a <see cref="T:System.Windows.Controls.Calendar" />.</summary>
[TemplatePart(Name = "PART_Root", Type = typeof(FrameworkElement))]
[TemplatePart(Name = "PART_HeaderButton", Type = typeof(Button))]
[TemplatePart(Name = "PART_PreviousButton", Type = typeof(Button))]
[TemplatePart(Name = "PART_NextButton", Type = typeof(Button))]
[TemplatePart(Name = "DayTitleTemplate", Type = typeof(DataTemplate))]
[TemplatePart(Name = "PART_MonthView", Type = typeof(Grid))]
[TemplatePart(Name = "PART_YearView", Type = typeof(Grid))]
[TemplatePart(Name = "PART_DisabledVisual", Type = typeof(FrameworkElement))]
public sealed class CalendarItem : Control
{
	private const string ElementRoot = "PART_Root";

	private const string ElementHeaderButton = "PART_HeaderButton";

	private const string ElementPreviousButton = "PART_PreviousButton";

	private const string ElementNextButton = "PART_NextButton";

	private const string ElementDayTitleTemplate = "DayTitleTemplate";

	private const string ElementMonthView = "PART_MonthView";

	private const string ElementYearView = "PART_YearView";

	private const string ElementDisabledVisual = "PART_DisabledVisual";

	private const int COLS = 7;

	private const int ROWS = 7;

	private const int YEAR_COLS = 4;

	private const int YEAR_ROWS = 3;

	private const int NUMBER_OF_DAYS_IN_WEEK = 7;

	private static ComponentResourceKey _dayTitleTemplateResourceKey;

	private System.Globalization.Calendar _calendar = new GregorianCalendar();

	private DataTemplate _dayTitleTemplate;

	private FrameworkElement _disabledVisual;

	private Button _headerButton;

	private Grid _monthView;

	private Button _nextButton;

	private Button _previousButton;

	private Grid _yearView;

	private bool _isMonthPressed;

	private bool _isDayPressed;

	internal Grid MonthView => _monthView;

	internal Calendar Owner { get; set; }

	internal Grid YearView => _yearView;

	private CalendarMode DisplayMode
	{
		get
		{
			if (Owner == null)
			{
				return CalendarMode.Month;
			}
			return Owner.DisplayMode;
		}
	}

	internal Button HeaderButton => _headerButton;

	internal Button NextButton => _nextButton;

	internal Button PreviousButton => _previousButton;

	private DateTime DisplayDate
	{
		get
		{
			if (Owner == null)
			{
				return DateTime.Today;
			}
			return Owner.DisplayDate;
		}
	}

	/// <summary>Gets or sets the resource key for the <see cref="T:System.Windows.DataTemplate" /> that displays the days of the week.</summary>
	/// <returns>The resource key for the <see cref="T:System.Windows.DataTemplate" /> that displays the days of the week.</returns>
	public static ComponentResourceKey DayTitleTemplateResourceKey
	{
		get
		{
			if (_dayTitleTemplateResourceKey == null)
			{
				_dayTitleTemplateResourceKey = new ComponentResourceKey(typeof(CalendarItem), "DayTitleTemplate");
			}
			return _dayTitleTemplateResourceKey;
		}
	}

	static CalendarItem()
	{
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(CalendarItem), new FrameworkPropertyMetadata(typeof(CalendarItem)));
		UIElement.FocusableProperty.OverrideMetadata(typeof(CalendarItem), new FrameworkPropertyMetadata(false));
		KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(CalendarItem), new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));
		KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(CalendarItem), new FrameworkPropertyMetadata(KeyboardNavigationMode.Contained));
		UIElement.IsEnabledProperty.OverrideMetadata(typeof(CalendarItem), new UIPropertyMetadata(Control.OnVisualStatePropertyChanged));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.CalendarItem" /> class. </summary>
	public CalendarItem()
	{
	}

	/// <summary>Builds the visual tree for the <see cref="T:System.Windows.Controls.Primitives.CalendarItem" /> when a new template is applied.</summary>
	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		if (_previousButton != null)
		{
			_previousButton.Click -= PreviousButton_Click;
		}
		if (_nextButton != null)
		{
			_nextButton.Click -= NextButton_Click;
		}
		if (_headerButton != null)
		{
			_headerButton.Click -= HeaderButton_Click;
		}
		_monthView = GetTemplateChild("PART_MonthView") as Grid;
		_yearView = GetTemplateChild("PART_YearView") as Grid;
		_previousButton = GetTemplateChild("PART_PreviousButton") as Button;
		_nextButton = GetTemplateChild("PART_NextButton") as Button;
		_headerButton = GetTemplateChild("PART_HeaderButton") as Button;
		_disabledVisual = GetTemplateChild("PART_DisabledVisual") as FrameworkElement;
		_dayTitleTemplate = null;
		if (base.Template != null && base.Template.Resources.Contains(DayTitleTemplateResourceKey))
		{
			_dayTitleTemplate = base.Template.Resources[DayTitleTemplateResourceKey] as DataTemplate;
		}
		if (_previousButton != null)
		{
			if (_previousButton.Content == null)
			{
				_previousButton.Content = SR.Calendar_PreviousButtonName;
			}
			_previousButton.Click += PreviousButton_Click;
		}
		if (_nextButton != null)
		{
			if (_nextButton.Content == null)
			{
				_nextButton.Content = SR.Calendar_NextButtonName;
			}
			_nextButton.Click += NextButton_Click;
		}
		if (_headerButton != null)
		{
			_headerButton.Click += HeaderButton_Click;
		}
		PopulateGrids();
		if (Owner != null)
		{
			switch (Owner.DisplayMode)
			{
			case CalendarMode.Year:
				UpdateYearMode();
				break;
			case CalendarMode.Decade:
				UpdateDecadeMode();
				break;
			case CalendarMode.Month:
				UpdateMonthMode();
				break;
			}
		}
		else
		{
			UpdateMonthMode();
		}
	}

	internal override void ChangeVisualState(bool useTransitions)
	{
		if (!base.IsEnabled)
		{
			VisualStateManager.GoToState(this, "Disabled", useTransitions);
		}
		else
		{
			VisualStateManager.GoToState(this, "Normal", useTransitions);
		}
		base.ChangeVisualState(useTransitions);
	}

	protected override void OnMouseUp(MouseButtonEventArgs e)
	{
		base.OnMouseUp(e);
		if (base.IsMouseCaptured)
		{
			ReleaseMouseCapture();
		}
		_isMonthPressed = false;
		_isDayPressed = false;
		if (!e.Handled && Owner.DisplayMode == CalendarMode.Month && Owner.HoverEnd.HasValue)
		{
			FinishSelection(Owner.HoverEnd.Value);
		}
	}

	protected override void OnLostMouseCapture(MouseEventArgs e)
	{
		base.OnLostMouseCapture(e);
		if (!base.IsMouseCaptured)
		{
			_isDayPressed = false;
			_isMonthPressed = false;
		}
	}

	internal void UpdateDecadeMode()
	{
		DateTime selectedYear = ((Owner == null) ? DateTime.Today : Owner.DisplayYear);
		int decadeForDecadeMode = GetDecadeForDecadeMode(selectedYear);
		int num = decadeForDecadeMode + 9;
		SetDecadeModeHeaderButton(decadeForDecadeMode);
		SetDecadeModePreviousButton(decadeForDecadeMode);
		SetDecadeModeNextButton(num);
		if (_yearView != null)
		{
			SetYearButtons(decadeForDecadeMode, num);
		}
	}

	internal void UpdateMonthMode()
	{
		SetMonthModeHeaderButton();
		SetMonthModePreviousButton();
		SetMonthModeNextButton();
		if (_monthView != null)
		{
			SetMonthModeDayTitles();
			SetMonthModeCalendarDayButtons();
			AddMonthModeHighlight();
		}
	}

	internal void UpdateYearMode()
	{
		SetYearModeHeaderButton();
		SetYearModePreviousButton();
		SetYearModeNextButton();
		if (_yearView != null)
		{
			SetYearModeMonthButtons();
		}
	}

	internal IEnumerable<CalendarDayButton> GetCalendarDayButtons()
	{
		int count = 49;
		if (MonthView == null)
		{
			yield break;
		}
		UIElementCollection dayButtonsHost = MonthView.Children;
		for (int childIndex = 7; childIndex < count; childIndex++)
		{
			if (dayButtonsHost[childIndex] is CalendarDayButton calendarDayButton)
			{
				yield return calendarDayButton;
			}
		}
	}

	internal CalendarDayButton GetFocusedCalendarDayButton()
	{
		foreach (CalendarDayButton calendarDayButton in GetCalendarDayButtons())
		{
			if (calendarDayButton != null && calendarDayButton.IsFocused)
			{
				return calendarDayButton;
			}
		}
		return null;
	}

	internal CalendarDayButton GetCalendarDayButton(DateTime date)
	{
		foreach (CalendarDayButton calendarDayButton in GetCalendarDayButtons())
		{
			if (calendarDayButton != null && calendarDayButton.DataContext is DateTime && DateTimeHelper.CompareDays(date, (DateTime)calendarDayButton.DataContext) == 0)
			{
				return calendarDayButton;
			}
		}
		return null;
	}

	internal CalendarButton GetCalendarButton(DateTime date, CalendarMode mode)
	{
		foreach (CalendarButton calendarButton in GetCalendarButtons())
		{
			if (calendarButton == null || !(calendarButton.DataContext is DateTime))
			{
				continue;
			}
			if (mode == CalendarMode.Year)
			{
				if (DateTimeHelper.CompareYearMonth(date, (DateTime)calendarButton.DataContext) == 0)
				{
					return calendarButton;
				}
			}
			else if (date.Year == ((DateTime)calendarButton.DataContext).Year)
			{
				return calendarButton;
			}
		}
		return null;
	}

	internal CalendarButton GetFocusedCalendarButton()
	{
		foreach (CalendarButton calendarButton in GetCalendarButtons())
		{
			if (calendarButton != null && calendarButton.IsFocused)
			{
				return calendarButton;
			}
		}
		return null;
	}

	private IEnumerable<CalendarButton> GetCalendarButtons()
	{
		foreach (UIElement child in YearView.Children)
		{
			if (child is CalendarButton calendarButton)
			{
				yield return calendarButton;
			}
		}
	}

	internal void FocusDate(DateTime date)
	{
		FrameworkElement frameworkElement = null;
		switch (DisplayMode)
		{
		case CalendarMode.Month:
			frameworkElement = GetCalendarDayButton(date);
			break;
		case CalendarMode.Year:
		case CalendarMode.Decade:
			frameworkElement = GetCalendarButton(date, DisplayMode);
			break;
		}
		if (frameworkElement != null && !frameworkElement.IsFocused)
		{
			frameworkElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
		}
	}

	private int GetDecadeForDecadeMode(DateTime selectedYear)
	{
		int num = DateTimeHelper.DecadeOfDate(selectedYear);
		if (_isMonthPressed && _yearView != null)
		{
			UIElementCollection children = _yearView.Children;
			int count = children.Count;
			if (count > 0 && children[0] is CalendarButton calendarButton && calendarButton.DataContext is DateTime && ((DateTime)calendarButton.DataContext).Year == selectedYear.Year)
			{
				return num + 10;
			}
			if (count > 1 && children[count - 1] is CalendarButton calendarButton2 && calendarButton2.DataContext is DateTime && ((DateTime)calendarButton2.DataContext).Year == selectedYear.Year)
			{
				return num - 10;
			}
		}
		return num;
	}

	private void EndDrag(bool ctrl, DateTime selectedDate)
	{
		if (Owner == null)
		{
			return;
		}
		Owner.CurrentDate = selectedDate;
		if (Owner.HoverStart.HasValue)
		{
			if (ctrl && DateTime.Compare(Owner.HoverStart.Value, selectedDate) == 0 && (Owner.SelectionMode == CalendarSelectionMode.SingleDate || Owner.SelectionMode == CalendarSelectionMode.MultipleRange))
			{
				Owner.SelectedDates.Toggle(selectedDate);
			}
			else
			{
				Owner.SelectedDates.AddRangeInternal(Owner.HoverStart.Value, selectedDate);
			}
			Owner.OnDayClick(selectedDate);
		}
	}

	private void CellOrMonth_PreviewKeyDown(object sender, RoutedEventArgs e)
	{
		if (Owner != null)
		{
			Owner.OnDayOrMonthPreviewKeyDown(e);
		}
	}

	private void Cell_Clicked(object sender, RoutedEventArgs e)
	{
		if (Owner == null)
		{
			return;
		}
		CalendarDayButton calendarDayButton = sender as CalendarDayButton;
		if (!(calendarDayButton.DataContext is DateTime) || calendarDayButton.IsBlackedOut)
		{
			return;
		}
		DateTime dateTime = (DateTime)calendarDayButton.DataContext;
		CalendarKeyboardHelper.GetMetaKeyState(out var ctrl, out var shift);
		switch (Owner.SelectionMode)
		{
		case CalendarSelectionMode.SingleDate:
			if (!ctrl)
			{
				Owner.SelectedDate = dateTime;
			}
			else
			{
				Owner.SelectedDates.Toggle(dateTime);
			}
			break;
		case CalendarSelectionMode.SingleRange:
		{
			DateTime? dateTime2 = Owner.CurrentDate;
			Owner.SelectedDates.ClearInternal(fireChangeNotification: true);
			if (shift && dateTime2.HasValue)
			{
				Owner.SelectedDates.AddRangeInternal(dateTime2.Value, dateTime);
				break;
			}
			Owner.SelectedDate = dateTime;
			Owner.HoverStart = null;
			Owner.HoverEnd = null;
			break;
		}
		case CalendarSelectionMode.MultipleRange:
			if (!ctrl)
			{
				Owner.SelectedDates.ClearInternal(fireChangeNotification: true);
			}
			if (shift)
			{
				Owner.SelectedDates.AddRangeInternal(Owner.CurrentDate, dateTime);
				break;
			}
			if (!ctrl)
			{
				Owner.SelectedDate = dateTime;
				break;
			}
			Owner.SelectedDates.Toggle(dateTime);
			Owner.HoverStart = null;
			Owner.HoverEnd = null;
			break;
		}
		Owner.OnDayClick(dateTime);
	}

	private void Cell_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (!(sender is CalendarDayButton calendarDayButton) || Owner == null || !(calendarDayButton.DataContext is DateTime))
		{
			return;
		}
		if (calendarDayButton.IsBlackedOut)
		{
			Owner.HoverStart = null;
			return;
		}
		_isDayPressed = true;
		Mouse.Capture(this, CaptureMode.SubTree);
		calendarDayButton.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
		CalendarKeyboardHelper.GetMetaKeyState(out var ctrl, out var shift);
		DateTime dateTime = (DateTime)calendarDayButton.DataContext;
		switch (Owner.SelectionMode)
		{
		case CalendarSelectionMode.SingleDate:
			Owner.DatePickerDisplayDateFlag = true;
			if (!ctrl)
			{
				Owner.SelectedDate = dateTime;
			}
			else
			{
				Owner.SelectedDates.Toggle(dateTime);
			}
			break;
		case CalendarSelectionMode.SingleRange:
			Owner.SelectedDates.ClearInternal();
			if (shift)
			{
				if (!Owner.HoverStart.HasValue)
				{
					Calendar owner3 = Owner;
					DateTime? hoverStart = (Owner.HoverEnd = Owner.CurrentDate);
					owner3.HoverStart = hoverStart;
				}
			}
			else
			{
				Calendar owner4 = Owner;
				DateTime? hoverStart = (Owner.HoverEnd = dateTime);
				owner4.HoverStart = hoverStart;
			}
			break;
		case CalendarSelectionMode.MultipleRange:
			if (!ctrl)
			{
				Owner.SelectedDates.ClearInternal();
			}
			if (shift)
			{
				if (!Owner.HoverStart.HasValue)
				{
					Calendar owner = Owner;
					DateTime? hoverStart = (Owner.HoverEnd = Owner.CurrentDate);
					owner.HoverStart = hoverStart;
				}
			}
			else
			{
				Calendar owner2 = Owner;
				DateTime? hoverStart = (Owner.HoverEnd = dateTime);
				owner2.HoverStart = hoverStart;
			}
			break;
		}
		Owner.CurrentDate = dateTime;
		Owner.UpdateCellItems();
	}

	private void Cell_MouseEnter(object sender, MouseEventArgs e)
	{
		if (!(sender is CalendarDayButton { IsBlackedOut: false } calendarDayButton) || e.LeftButton != MouseButtonState.Pressed || !_isDayPressed)
		{
			return;
		}
		calendarDayButton.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
		if (Owner == null || !(calendarDayButton.DataContext is DateTime))
		{
			return;
		}
		DateTime dateTime = (DateTime)calendarDayButton.DataContext;
		if (Owner.SelectionMode == CalendarSelectionMode.SingleDate)
		{
			Owner.DatePickerDisplayDateFlag = true;
			Calendar owner = Owner;
			DateTime? hoverStart = (Owner.HoverEnd = null);
			owner.HoverStart = hoverStart;
			if (Owner.SelectedDates.Count == 0)
			{
				Owner.SelectedDates.Add(dateTime);
			}
			else
			{
				Owner.SelectedDates[0] = dateTime;
			}
		}
		else
		{
			Owner.HoverEnd = dateTime;
			Owner.CurrentDate = dateTime;
			Owner.UpdateCellItems();
		}
	}

	private void Cell_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (sender is CalendarDayButton calendarDayButton && Owner != null)
		{
			if (!calendarDayButton.IsBlackedOut)
			{
				Owner.OnDayButtonMouseUp(e);
			}
			if (calendarDayButton.DataContext is DateTime)
			{
				FinishSelection((DateTime)calendarDayButton.DataContext);
				e.Handled = true;
			}
		}
	}

	private void FinishSelection(DateTime selectedDate)
	{
		CalendarKeyboardHelper.GetMetaKeyState(out var ctrl, out var _);
		if (Owner.SelectionMode == CalendarSelectionMode.None || Owner.SelectionMode == CalendarSelectionMode.SingleDate)
		{
			Owner.OnDayClick(selectedDate);
		}
		else if (Owner.HoverStart.HasValue)
		{
			switch (Owner.SelectionMode)
			{
			case CalendarSelectionMode.SingleRange:
				Owner.SelectedDates.ClearInternal();
				EndDrag(ctrl, selectedDate);
				break;
			case CalendarSelectionMode.MultipleRange:
				EndDrag(ctrl, selectedDate);
				break;
			}
		}
		else
		{
			CalendarDayButton calendarDayButton = GetCalendarDayButton(selectedDate);
			if (calendarDayButton != null && calendarDayButton.IsInactive && calendarDayButton.IsBlackedOut)
			{
				Owner.OnDayClick(selectedDate);
			}
		}
	}

	private void Month_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (sender is CalendarButton b)
		{
			_isMonthPressed = true;
			Mouse.Capture(this, CaptureMode.SubTree);
			if (Owner != null)
			{
				Owner.OnCalendarButtonPressed(b, switchDisplayMode: false);
			}
		}
	}

	private void Month_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (sender is CalendarButton b && Owner != null)
		{
			Owner.OnCalendarButtonPressed(b, switchDisplayMode: true);
		}
	}

	private void Month_MouseEnter(object sender, MouseEventArgs e)
	{
		if (sender is CalendarButton b && _isMonthPressed && Owner != null)
		{
			Owner.OnCalendarButtonPressed(b, switchDisplayMode: false);
		}
	}

	private void Month_Clicked(object sender, RoutedEventArgs e)
	{
		if (sender is CalendarButton b)
		{
			Owner.OnCalendarButtonPressed(b, switchDisplayMode: true);
		}
	}

	private void HeaderButton_Click(object sender, RoutedEventArgs e)
	{
		if (Owner != null)
		{
			if (Owner.DisplayMode == CalendarMode.Month)
			{
				Owner.SetCurrentValueInternal(Calendar.DisplayModeProperty, CalendarMode.Year);
			}
			else
			{
				Owner.SetCurrentValueInternal(Calendar.DisplayModeProperty, CalendarMode.Decade);
			}
			FocusDate(DisplayDate);
		}
	}

	private void PreviousButton_Click(object sender, RoutedEventArgs e)
	{
		if (Owner != null)
		{
			Owner.OnPreviousClick();
		}
	}

	private void NextButton_Click(object sender, RoutedEventArgs e)
	{
		if (Owner != null)
		{
			Owner.OnNextClick();
		}
	}

	private void PopulateGrids()
	{
		if (_monthView != null)
		{
			for (int i = 0; i < 7; i++)
			{
				FrameworkElement frameworkElement = ((_dayTitleTemplate != null) ? ((FrameworkElement)_dayTitleTemplate.LoadContent()) : new ContentControl());
				frameworkElement.SetValue(Grid.RowProperty, 0);
				frameworkElement.SetValue(Grid.ColumnProperty, i);
				_monthView.Children.Add(frameworkElement);
			}
			for (int j = 1; j < 7; j++)
			{
				for (int k = 0; k < 7; k++)
				{
					CalendarDayButton calendarDayButton = new CalendarDayButton();
					calendarDayButton.Owner = Owner;
					calendarDayButton.SetValue(Grid.RowProperty, j);
					calendarDayButton.SetValue(Grid.ColumnProperty, k);
					calendarDayButton.SetBinding(FrameworkElement.StyleProperty, GetOwnerBinding("CalendarDayButtonStyle"));
					calendarDayButton.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(Cell_MouseLeftButtonDown), handledEventsToo: true);
					calendarDayButton.AddHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(Cell_MouseLeftButtonUp), handledEventsToo: true);
					calendarDayButton.AddHandler(UIElement.MouseEnterEvent, new MouseEventHandler(Cell_MouseEnter), handledEventsToo: true);
					calendarDayButton.Click += Cell_Clicked;
					calendarDayButton.AddHandler(UIElement.PreviewKeyDownEvent, new RoutedEventHandler(CellOrMonth_PreviewKeyDown), handledEventsToo: true);
					_monthView.Children.Add(calendarDayButton);
				}
			}
		}
		if (_yearView == null)
		{
			return;
		}
		int num = 0;
		for (int l = 0; l < 3; l++)
		{
			for (int m = 0; m < 4; m++)
			{
				CalendarButton calendarButton = new CalendarButton();
				calendarButton.Owner = Owner;
				calendarButton.SetValue(Grid.RowProperty, l);
				calendarButton.SetValue(Grid.ColumnProperty, m);
				calendarButton.SetBinding(FrameworkElement.StyleProperty, GetOwnerBinding("CalendarButtonStyle"));
				calendarButton.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(Month_MouseLeftButtonDown), handledEventsToo: true);
				calendarButton.AddHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(Month_MouseLeftButtonUp), handledEventsToo: true);
				calendarButton.AddHandler(UIElement.MouseEnterEvent, new MouseEventHandler(Month_MouseEnter), handledEventsToo: true);
				calendarButton.AddHandler(UIElement.PreviewKeyDownEvent, new RoutedEventHandler(CellOrMonth_PreviewKeyDown), handledEventsToo: true);
				calendarButton.Click += Month_Clicked;
				_yearView.Children.Add(calendarButton);
				num++;
			}
		}
	}

	private void SetMonthModeDayTitles()
	{
		if (_monthView == null)
		{
			return;
		}
		string[] shortestDayNames = DateTimeHelper.GetDateFormat(DateTimeHelper.GetCulture(this)).ShortestDayNames;
		for (int i = 0; i < 7; i++)
		{
			if (_monthView.Children[i] is FrameworkElement frameworkElement && shortestDayNames != null && shortestDayNames.Length != 0)
			{
				if (Owner != null)
				{
					frameworkElement.DataContext = shortestDayNames[(int)(i + Owner.FirstDayOfWeek) % shortestDayNames.Length];
				}
				else
				{
					frameworkElement.DataContext = shortestDayNames[(int)(i + DateTimeHelper.GetDateFormat(DateTimeHelper.GetCulture(this)).FirstDayOfWeek) % shortestDayNames.Length];
				}
			}
		}
	}

	private void SetMonthModeCalendarDayButtons()
	{
		DateTime dateTime = DateTimeHelper.DiscardDayTime(DisplayDate);
		int numberOfDisplayedDaysFromPreviousMonth = GetNumberOfDisplayedDaysFromPreviousMonth(dateTime);
		bool flag = DateTimeHelper.CompareYearMonth(dateTime, DateTime.MinValue) <= 0;
		bool flag2 = DateTimeHelper.CompareYearMonth(dateTime, DateTime.MaxValue) >= 0;
		int daysInMonth = _calendar.GetDaysInMonth(dateTime.Year, dateTime.Month);
		CultureInfo culture = DateTimeHelper.GetCulture(this);
		int num = 49;
		for (int i = 7; i < num; i++)
		{
			CalendarDayButton calendarDayButton = _monthView.Children[i] as CalendarDayButton;
			int num2 = i - numberOfDisplayedDaysFromPreviousMonth - 7;
			if ((!flag || num2 >= 0) && (!flag2 || num2 < daysInMonth))
			{
				DateTime dateTime2 = _calendar.AddDays(dateTime, num2);
				SetMonthModeDayButtonState(calendarDayButton, dateTime2);
				calendarDayButton.DataContext = dateTime2;
				calendarDayButton.SetContentInternal(DateTimeHelper.ToDayString(dateTime2, culture));
			}
			else
			{
				SetMonthModeDayButtonState(calendarDayButton, null);
				calendarDayButton.DataContext = null;
				calendarDayButton.SetContentInternal(DateTimeHelper.ToDayString(null, culture));
			}
		}
	}

	private void SetMonthModeDayButtonState(CalendarDayButton childButton, DateTime? dateToAdd)
	{
		if (Owner == null)
		{
			return;
		}
		if (dateToAdd.HasValue)
		{
			childButton.Visibility = Visibility.Visible;
			if (DateTimeHelper.CompareDays(dateToAdd.Value, Owner.DisplayDateStartInternal) < 0 || DateTimeHelper.CompareDays(dateToAdd.Value, Owner.DisplayDateEndInternal) > 0)
			{
				childButton.IsEnabled = false;
				childButton.Visibility = Visibility.Hidden;
				return;
			}
			childButton.IsEnabled = true;
			childButton.SetValue(CalendarDayButton.IsBlackedOutPropertyKey, Owner.BlackoutDates.Contains(dateToAdd.Value));
			childButton.SetValue(CalendarDayButton.IsInactivePropertyKey, DateTimeHelper.CompareYearMonth(dateToAdd.Value, Owner.DisplayDateInternal) != 0);
			if (DateTimeHelper.CompareDays(dateToAdd.Value, DateTime.Today) == 0)
			{
				childButton.SetValue(CalendarDayButton.IsTodayPropertyKey, value: true);
			}
			else
			{
				childButton.SetValue(CalendarDayButton.IsTodayPropertyKey, value: false);
			}
			childButton.NotifyNeedsVisualStateUpdate();
			bool flag = false;
			foreach (DateTime selectedDate in Owner.SelectedDates)
			{
				flag |= DateTimeHelper.CompareDays(dateToAdd.Value, selectedDate) == 0;
			}
			childButton.SetValue(CalendarDayButton.IsSelectedPropertyKey, flag);
		}
		else
		{
			childButton.Visibility = Visibility.Hidden;
			childButton.IsEnabled = false;
			childButton.SetValue(CalendarDayButton.IsBlackedOutPropertyKey, value: false);
			childButton.SetValue(CalendarDayButton.IsInactivePropertyKey, value: true);
			childButton.SetValue(CalendarDayButton.IsTodayPropertyKey, value: false);
			childButton.SetValue(CalendarDayButton.IsSelectedPropertyKey, value: false);
		}
	}

	private void AddMonthModeHighlight()
	{
		Calendar owner = Owner;
		if (owner == null)
		{
			return;
		}
		if (owner.HoverStart.HasValue && owner.HoverEnd.HasValue)
		{
			DateTime value = owner.HoverEnd.Value;
			DateTime value2 = owner.HoverEnd.Value;
			int num = DateTimeHelper.CompareDays(owner.HoverEnd.Value, owner.HoverStart.Value);
			if (num < 0)
			{
				value2 = owner.HoverStart.Value;
			}
			else
			{
				value = owner.HoverStart.Value;
			}
			int num2 = 49;
			for (int i = 7; i < num2; i++)
			{
				CalendarDayButton calendarDayButton = _monthView.Children[i] as CalendarDayButton;
				if (calendarDayButton.DataContext is DateTime)
				{
					DateTime date = (DateTime)calendarDayButton.DataContext;
					calendarDayButton.SetValue(CalendarDayButton.IsHighlightedPropertyKey, num != 0 && DateTimeHelper.InRange(date, value, value2));
				}
				else
				{
					calendarDayButton.SetValue(CalendarDayButton.IsHighlightedPropertyKey, value: false);
				}
			}
		}
		else
		{
			int num3 = 49;
			for (int j = 7; j < num3; j++)
			{
				(_monthView.Children[j] as CalendarDayButton).SetValue(CalendarDayButton.IsHighlightedPropertyKey, value: false);
			}
		}
	}

	private void SetMonthModeHeaderButton()
	{
		if (_headerButton != null)
		{
			_headerButton.Content = DateTimeHelper.ToYearMonthPatternString(DisplayDate, DateTimeHelper.GetCulture(this));
			if (Owner != null)
			{
				_headerButton.IsEnabled = true;
			}
		}
	}

	private void SetMonthModeNextButton()
	{
		if (Owner != null && _nextButton != null)
		{
			DateTime dateTime = DateTimeHelper.DiscardDayTime(DisplayDate);
			if (DateTimeHelper.CompareYearMonth(dateTime, DateTime.MaxValue) == 0)
			{
				_nextButton.IsEnabled = false;
				return;
			}
			DateTime dt = _calendar.AddMonths(dateTime, 1);
			_nextButton.IsEnabled = DateTimeHelper.CompareDays(Owner.DisplayDateEndInternal, dt) > -1;
		}
	}

	private void SetMonthModePreviousButton()
	{
		if (Owner != null && _previousButton != null)
		{
			DateTime dt = DateTimeHelper.DiscardDayTime(DisplayDate);
			_previousButton.IsEnabled = DateTimeHelper.CompareDays(Owner.DisplayDateStartInternal, dt) < 0;
		}
	}

	private void SetYearButtons(int decade, int decadeEnd)
	{
		int num = -1;
		foreach (object child in _yearView.Children)
		{
			CalendarButton calendarButton = child as CalendarButton;
			int num2 = decade + num;
			if (num2 <= DateTime.MaxValue.Year && num2 >= DateTime.MinValue.Year)
			{
				DateTime dateTime = new DateTime(num2, 1, 1);
				calendarButton.DataContext = dateTime;
				calendarButton.SetContentInternal(DateTimeHelper.ToYearString(dateTime, DateTimeHelper.GetCulture(this)));
				calendarButton.Visibility = Visibility.Visible;
				if (Owner != null)
				{
					calendarButton.HasSelectedDays = Owner.DisplayDate.Year == num2;
					if (num2 < Owner.DisplayDateStartInternal.Year || num2 > Owner.DisplayDateEndInternal.Year)
					{
						calendarButton.IsEnabled = false;
						calendarButton.Opacity = 0.0;
					}
					else
					{
						calendarButton.IsEnabled = true;
						calendarButton.Opacity = 1.0;
					}
				}
				calendarButton.IsInactive = num2 < decade || num2 > decadeEnd;
			}
			else
			{
				calendarButton.DataContext = null;
				calendarButton.IsEnabled = false;
				calendarButton.Opacity = 0.0;
			}
			num++;
		}
	}

	private void SetYearModeMonthButtons()
	{
		int num = 0;
		foreach (object child in _yearView.Children)
		{
			CalendarButton calendarButton = child as CalendarButton;
			DateTime dateTime = new DateTime(DisplayDate.Year, num + 1, 1);
			calendarButton.DataContext = dateTime;
			calendarButton.SetContentInternal(DateTimeHelper.ToAbbreviatedMonthString(dateTime, DateTimeHelper.GetCulture(this)));
			calendarButton.Visibility = Visibility.Visible;
			if (Owner != null)
			{
				calendarButton.HasSelectedDays = DateTimeHelper.CompareYearMonth(dateTime, Owner.DisplayDateInternal) == 0;
				if (DateTimeHelper.CompareYearMonth(dateTime, Owner.DisplayDateStartInternal) < 0 || DateTimeHelper.CompareYearMonth(dateTime, Owner.DisplayDateEndInternal) > 0)
				{
					calendarButton.IsEnabled = false;
					calendarButton.Opacity = 0.0;
				}
				else
				{
					calendarButton.IsEnabled = true;
					calendarButton.Opacity = 1.0;
				}
			}
			calendarButton.IsInactive = false;
			num++;
		}
	}

	private void SetYearModeHeaderButton()
	{
		if (_headerButton != null)
		{
			_headerButton.IsEnabled = true;
			_headerButton.Content = DateTimeHelper.ToYearString(DisplayDate, DateTimeHelper.GetCulture(this));
		}
	}

	private void SetYearModeNextButton()
	{
		if (Owner != null && _nextButton != null)
		{
			_nextButton.IsEnabled = Owner.DisplayDateEndInternal.Year != DisplayDate.Year;
		}
	}

	private void SetYearModePreviousButton()
	{
		if (Owner != null && _previousButton != null)
		{
			_previousButton.IsEnabled = Owner.DisplayDateStartInternal.Year != DisplayDate.Year;
		}
	}

	private void SetDecadeModeHeaderButton(int decade)
	{
		if (_headerButton != null)
		{
			_headerButton.Content = DateTimeHelper.ToDecadeRangeString(decade, this);
			_headerButton.IsEnabled = false;
		}
	}

	private void SetDecadeModeNextButton(int decadeEnd)
	{
		if (Owner != null && _nextButton != null)
		{
			_nextButton.IsEnabled = Owner.DisplayDateEndInternal.Year > decadeEnd;
		}
	}

	private void SetDecadeModePreviousButton(int decade)
	{
		if (Owner != null && _previousButton != null)
		{
			_previousButton.IsEnabled = decade > Owner.DisplayDateStartInternal.Year;
		}
	}

	private int GetNumberOfDisplayedDaysFromPreviousMonth(DateTime firstOfMonth)
	{
		DayOfWeek dayOfWeek = _calendar.GetDayOfWeek(firstOfMonth);
		int num = ((Owner == null) ? ((dayOfWeek - DateTimeHelper.GetDateFormat(DateTimeHelper.GetCulture(this)).FirstDayOfWeek + 7) % 7) : ((dayOfWeek - Owner.FirstDayOfWeek + 7) % 7));
		if (num == 0)
		{
			return 7;
		}
		return num;
	}

	private BindingBase GetOwnerBinding(string propertyName)
	{
		return new Binding(propertyName)
		{
			Source = Owner
		};
	}
}
