using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal.KnownBoxes;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Represents a control that allows the user to select a date.</summary>
[TemplatePart(Name = "PART_Root", Type = typeof(Grid))]
[TemplatePart(Name = "PART_TextBox", Type = typeof(DatePickerTextBox))]
[TemplatePart(Name = "PART_Button", Type = typeof(Button))]
[TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
public class DatePicker : Control
{
	private const string ElementRoot = "PART_Root";

	private const string ElementTextBox = "PART_TextBox";

	private const string ElementButton = "PART_Button";

	private const string ElementPopup = "PART_Popup";

	private Calendar _calendar;

	private string _defaultText;

	private ButtonBase _dropDownButton;

	private Popup _popUp;

	private bool _disablePopupReopen;

	private DatePickerTextBox _textBox;

	private IDictionary<DependencyProperty, bool> _isHandlerSuspended;

	private DateTime? _originalSelectedDate;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.DatePicker.SelectedDateChanged" /> routed event.</summary>
	public static readonly RoutedEvent SelectedDateChangedEvent;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DatePicker.CalendarStyle" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DatePicker.CalendarStyle" /> dependency property.</returns>
	public static readonly DependencyProperty CalendarStyleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DatePicker.DisplayDate" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DatePicker.DisplayDate" /> dependency property.</returns>
	public static readonly DependencyProperty DisplayDateProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DatePicker.DisplayDateEnd" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DatePicker.DisplayDateEnd" /> dependency property.</returns>
	public static readonly DependencyProperty DisplayDateEndProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DatePicker.DisplayDateStart" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DatePicker.DisplayDateStart" /> dependency property.</returns>
	public static readonly DependencyProperty DisplayDateStartProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DatePicker.FirstDayOfWeek" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DatePicker.FirstDayOfWeek" /> dependency property.</returns>
	public static readonly DependencyProperty FirstDayOfWeekProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DatePicker.IsDropDownOpen" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DatePicker.IsDropDownOpen" /> dependency property.</returns>
	public static readonly DependencyProperty IsDropDownOpenProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DatePicker.IsTodayHighlighted" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DatePicker.IsTodayHighlighted" /> dependency property.</returns>
	public static readonly DependencyProperty IsTodayHighlightedProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DatePicker.SelectedDate" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DatePicker.SelectedDate" /> dependency property.</returns>
	public static readonly DependencyProperty SelectedDateProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DatePicker.SelectedDateFormat" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DatePicker.SelectedDateFormat" /> dependency property.</returns>
	public static readonly DependencyProperty SelectedDateFormatProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DatePicker.Text" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DatePicker.Text" /> dependency property.</returns>
	public static readonly DependencyProperty TextProperty;

	/// <summary>Gets or sets a collection of dates that are marked as not selectable.</summary>
	/// <returns>A collection of dates that cannot be selected. The default value is an empty collection.</returns>
	public CalendarBlackoutDatesCollection BlackoutDates => _calendar.BlackoutDates;

	/// <summary>Gets or sets the style that is used when rendering the calendar.</summary>
	/// <returns>The style that is used when rendering the calendar.</returns>
	public Style CalendarStyle
	{
		get
		{
			return (Style)GetValue(CalendarStyleProperty);
		}
		set
		{
			SetValue(CalendarStyleProperty, value);
		}
	}

	/// <summary>Gets or sets the date to display.</summary>
	/// <returns>The date to display. The default is <see cref="P:System.DateTime.Today" />.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The specified date is not in the range defined by <see cref="P:System.Windows.Controls.DatePicker.DisplayDateStart" />. and <see cref="P:System.Windows.Controls.DatePicker.DisplayDateEnd" />.</exception>
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

	/// <summary>Gets or sets the last date to be displayed.</summary>
	/// <returns>The last date to display.</returns>
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

	/// <summary>Gets or sets the first date to be displayed.</summary>
	/// <returns>The first date to display.</returns>
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

	/// <summary>Gets or sets a value that indicates whether the drop-down <see cref="T:System.Windows.Controls.Calendar" /> is open or closed.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.Calendar" /> is open; otherwise, false. The default is false.</returns>
	public bool IsDropDownOpen
	{
		get
		{
			return (bool)GetValue(IsDropDownOpenProperty);
		}
		set
		{
			SetValue(IsDropDownOpenProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the current date will be highlighted.</summary>
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
	/// <exception cref="T:System.ArgumentOutOfRangeException">The specified date is not in the range defined by <see cref="P:System.Windows.Controls.DatePicker.DisplayDateStart" /> and <see cref="P:System.Windows.Controls.DatePicker.DisplayDateEnd" />, or the specified date is in the <see cref="P:System.Windows.Controls.DatePicker.BlackoutDates" /> collection. </exception>
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

	/// <summary>Gets or sets the format that is used to display the selected date.</summary>
	/// <returns>The format that is used to display the selected date. The default is <see cref="F:System.Windows.Controls.DatePickerFormat.Long" />.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The specified format is not valid.</exception>
	public DatePickerFormat SelectedDateFormat
	{
		get
		{
			return (DatePickerFormat)GetValue(SelectedDateFormatProperty);
		}
		set
		{
			SetValue(SelectedDateFormatProperty, value);
		}
	}

	/// <summary>Gets the text that is displayed by the <see cref="T:System.Windows.Controls.DatePicker" />, or sets the selected date.</summary>
	/// <returns>The text displayed by the <see cref="T:System.Windows.Controls.DatePicker" />. The default is an empty string.</returns>
	public string Text
	{
		get
		{
			return (string)GetValue(TextProperty);
		}
		set
		{
			SetValue(TextProperty, value);
		}
	}

	internal Calendar Calendar => _calendar;

	internal TextBox TextBox => _textBox;

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Controls.DatePicker" /> has focus.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.DatePicker" /> has focus; otherwise, false.</returns>
	protected override bool HasEffectiveKeyboardFocus
	{
		protected internal get
		{
			if (_textBox != null)
			{
				return _textBox.HasEffectiveKeyboardFocus;
			}
			return base.HasEffectiveKeyboardFocus;
		}
	}

	/// <summary>Occurs when the drop-down <see cref="T:System.Windows.Controls.Calendar" /> is closed.</summary>
	public event RoutedEventHandler CalendarClosed;

	/// <summary>Occurs when the drop-down <see cref="T:System.Windows.Controls.Calendar" /> is opened.</summary>
	public event RoutedEventHandler CalendarOpened;

	/// <summary>Occurs when <see cref="P:System.Windows.Controls.DatePicker.Text" /> is set to a value that cannot be interpreted as a date or when the date cannot be selected.</summary>
	public event EventHandler<DatePickerDateValidationErrorEventArgs> DateValidationError;

	/// <summary>Occurs when the <see cref="P:System.Windows.Controls.DatePicker.SelectedDate" /> property is changed.</summary>
	public event EventHandler<SelectionChangedEventArgs> SelectedDateChanged
	{
		add
		{
			AddHandler(SelectedDateChangedEvent, value);
		}
		remove
		{
			RemoveHandler(SelectedDateChangedEvent, value);
		}
	}

	static DatePicker()
	{
		SelectedDateChangedEvent = EventManager.RegisterRoutedEvent("SelectedDateChanged", RoutingStrategy.Direct, typeof(EventHandler<SelectionChangedEventArgs>), typeof(DatePicker));
		CalendarStyleProperty = DependencyProperty.Register("CalendarStyle", typeof(Style), typeof(DatePicker));
		DisplayDateProperty = DependencyProperty.Register("DisplayDate", typeof(DateTime), typeof(DatePicker), new FrameworkPropertyMetadata(DateTime.Now, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, null, CoerceDisplayDate));
		DisplayDateEndProperty = DependencyProperty.Register("DisplayDateEnd", typeof(DateTime?), typeof(DatePicker), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDisplayDateEndChanged, CoerceDisplayDateEnd));
		DisplayDateStartProperty = DependencyProperty.Register("DisplayDateStart", typeof(DateTime?), typeof(DatePicker), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDisplayDateStartChanged, CoerceDisplayDateStart));
		FirstDayOfWeekProperty = DependencyProperty.Register("FirstDayOfWeek", typeof(DayOfWeek), typeof(DatePicker), null, Calendar.IsValidFirstDayOfWeek);
		IsDropDownOpenProperty = DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(DatePicker), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsDropDownOpenChanged, OnCoerceIsDropDownOpen));
		IsTodayHighlightedProperty = DependencyProperty.Register("IsTodayHighlighted", typeof(bool), typeof(DatePicker));
		SelectedDateProperty = DependencyProperty.Register("SelectedDate", typeof(DateTime?), typeof(DatePicker), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedDateChanged, CoerceSelectedDate));
		SelectedDateFormatProperty = DependencyProperty.Register("SelectedDateFormat", typeof(DatePickerFormat), typeof(DatePicker), new FrameworkPropertyMetadata(DatePickerFormat.Long, OnSelectedDateFormatChanged), IsValidSelectedDateFormat);
		TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(DatePicker), new FrameworkPropertyMetadata(string.Empty, OnTextChanged));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(DatePicker), new FrameworkPropertyMetadata(typeof(DatePicker)));
		EventManager.RegisterClassHandler(typeof(DatePicker), UIElement.GotFocusEvent, new RoutedEventHandler(OnGotFocus));
		KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(DatePicker), new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));
		KeyboardNavigation.IsTabStopProperty.OverrideMetadata(typeof(DatePicker), new FrameworkPropertyMetadata(false));
		UIElement.IsEnabledProperty.OverrideMetadata(typeof(DatePicker), new UIPropertyMetadata(OnIsEnabledChanged));
		FrameworkElement.LanguageProperty.OverrideMetadata(typeof(DatePicker), new FrameworkPropertyMetadata(OnLanguageChanged));
		ControlsTraceLogger.AddControl(TelemetryControls.DatePicker);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DatePicker" /> class. </summary>
	public DatePicker()
	{
		InitializeCalendar();
		_defaultText = string.Empty;
		SetCurrentValueInternal(FirstDayOfWeekProperty, DateTimeHelper.GetCurrentDateFormat().FirstDayOfWeek);
		SetCurrentValueInternal(DisplayDateProperty, DateTime.Today);
	}

	private static object CoerceDisplayDate(DependencyObject d, object value)
	{
		DatePicker obj = d as DatePicker;
		obj._calendar.DisplayDate = (DateTime)value;
		return obj._calendar.DisplayDate;
	}

	private static void OnDisplayDateEndChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		(d as DatePicker).CoerceValue(DisplayDateProperty);
	}

	private static object CoerceDisplayDateEnd(DependencyObject d, object value)
	{
		DatePicker obj = d as DatePicker;
		obj._calendar.DisplayDateEnd = (DateTime?)value;
		return obj._calendar.DisplayDateEnd;
	}

	private static void OnDisplayDateStartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DatePicker obj = d as DatePicker;
		obj.CoerceValue(DisplayDateEndProperty);
		obj.CoerceValue(DisplayDateProperty);
	}

	private static object CoerceDisplayDateStart(DependencyObject d, object value)
	{
		DatePicker obj = d as DatePicker;
		obj._calendar.DisplayDateStart = (DateTime?)value;
		return obj._calendar.DisplayDateStart;
	}

	private static object OnCoerceIsDropDownOpen(DependencyObject d, object baseValue)
	{
		if (!(d as DatePicker).IsEnabled)
		{
			return false;
		}
		return baseValue;
	}

	private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DatePicker dp = d as DatePicker;
		bool flag = (bool)e.NewValue;
		if (dp._popUp == null || dp._popUp.IsOpen == flag)
		{
			return;
		}
		dp._popUp.IsOpen = flag;
		if (flag)
		{
			dp._originalSelectedDate = dp.SelectedDate;
			dp.Dispatcher.BeginInvoke(DispatcherPriority.Input, (Action)delegate
			{
				dp._calendar.Focus();
			});
		}
	}

	private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		(d as DatePicker).CoerceValue(IsDropDownOpenProperty);
		Control.OnVisualStatePropertyChanged(d, e);
	}

	private static void OnLanguageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DatePicker datePicker = (DatePicker)d;
		if (DependencyPropertyHelper.GetValueSource(datePicker, FirstDayOfWeekProperty).BaseValueSource == BaseValueSource.Default)
		{
			datePicker.SetCurrentValueInternal(FirstDayOfWeekProperty, DateTimeHelper.GetDateFormat(DateTimeHelper.GetCulture(datePicker)).FirstDayOfWeek);
		}
	}

	private static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DatePicker datePicker = d as DatePicker;
		Collection<DateTime> collection = new Collection<DateTime>();
		Collection<DateTime> collection2 = new Collection<DateTime>();
		datePicker.CoerceValue(DisplayDateStartProperty);
		datePicker.CoerceValue(DisplayDateEndProperty);
		datePicker.CoerceValue(DisplayDateProperty);
		DateTime? dateTime = (DateTime?)e.NewValue;
		DateTime? dateTime2 = (DateTime?)e.OldValue;
		if (datePicker.SelectedDate.HasValue)
		{
			DateTime value = datePicker.SelectedDate.Value;
			datePicker.SetTextInternal(datePicker.DateTimeToString(value));
			if ((value.Month != datePicker.DisplayDate.Month || value.Year != datePicker.DisplayDate.Year) && !datePicker._calendar.DatePickerDisplayDateFlag)
			{
				datePicker.SetCurrentValueInternal(DisplayDateProperty, value);
			}
			datePicker._calendar.DatePickerDisplayDateFlag = false;
		}
		else
		{
			datePicker.SetWaterMarkText();
		}
		if (dateTime.HasValue)
		{
			collection.Add(dateTime.Value);
		}
		if (dateTime2.HasValue)
		{
			collection2.Add(dateTime2.Value);
		}
		datePicker.OnSelectedDateChanged(new CalendarSelectionChangedEventArgs(SelectedDateChangedEvent, collection2, collection));
		if (UIElementAutomationPeer.FromElement(datePicker) is DatePickerAutomationPeer datePickerAutomationPeer)
		{
			string newValue = (dateTime.HasValue ? datePicker.DateTimeToString(dateTime.Value) : "");
			string oldValue = (dateTime2.HasValue ? datePicker.DateTimeToString(dateTime2.Value) : "");
			datePickerAutomationPeer.RaiseValuePropertyChangedEvent(oldValue, newValue);
		}
	}

	private static object CoerceSelectedDate(DependencyObject d, object value)
	{
		DatePicker obj = d as DatePicker;
		obj._calendar.SelectedDate = (DateTime?)value;
		return obj._calendar.SelectedDate;
	}

	private static void OnSelectedDateFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DatePicker datePicker = d as DatePicker;
		if (datePicker._textBox == null)
		{
			return;
		}
		if (string.IsNullOrEmpty(datePicker._textBox.Text))
		{
			datePicker.SetWaterMarkText();
			return;
		}
		DateTime? dateTime = datePicker.ParseText(datePicker._textBox.Text);
		if (dateTime.HasValue)
		{
			datePicker.SetTextInternal(datePicker.DateTimeToString(dateTime.Value));
		}
	}

	private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DatePicker datePicker = d as DatePicker;
		if (datePicker.IsHandlerSuspended(TextProperty))
		{
			return;
		}
		if (e.NewValue is string text)
		{
			if (datePicker._textBox != null)
			{
				datePicker._textBox.Text = text;
			}
			else
			{
				datePicker._defaultText = text;
			}
			datePicker.SetSelectedDate();
		}
		else
		{
			datePicker.SetValueNoCallback(SelectedDateProperty, null);
		}
	}

	private void SetTextInternal(string value)
	{
		SetCurrentValueInternal(TextProperty, value);
	}

	/// <summary>Builds the visual tree for the <see cref="T:System.Windows.Controls.DatePicker" /> control when a new template is applied.</summary>
	public override void OnApplyTemplate()
	{
		if (_popUp != null)
		{
			_popUp.RemoveHandler(UIElement.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(PopUp_PreviewMouseLeftButtonDown));
			_popUp.Opened -= PopUp_Opened;
			_popUp.Closed -= PopUp_Closed;
			_popUp.Child = null;
		}
		if (_dropDownButton != null)
		{
			_dropDownButton.Click -= DropDownButton_Click;
			_dropDownButton.RemoveHandler(UIElement.MouseLeaveEvent, new MouseEventHandler(DropDownButton_MouseLeave));
		}
		if (_textBox != null)
		{
			_textBox.RemoveHandler(UIElement.KeyDownEvent, new KeyEventHandler(TextBox_KeyDown));
			_textBox.RemoveHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(TextBox_TextChanged));
			_textBox.RemoveHandler(UIElement.LostFocusEvent, new RoutedEventHandler(TextBox_LostFocus));
		}
		base.OnApplyTemplate();
		_popUp = GetTemplateChild("PART_Popup") as Popup;
		if (_popUp != null)
		{
			_popUp.AddHandler(UIElement.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(PopUp_PreviewMouseLeftButtonDown));
			_popUp.Opened += PopUp_Opened;
			_popUp.Closed += PopUp_Closed;
			_popUp.Child = _calendar;
			if (IsDropDownOpen)
			{
				_popUp.IsOpen = true;
			}
		}
		_dropDownButton = GetTemplateChild("PART_Button") as Button;
		if (_dropDownButton != null)
		{
			_dropDownButton.Click += DropDownButton_Click;
			_dropDownButton.AddHandler(UIElement.MouseLeaveEvent, new MouseEventHandler(DropDownButton_MouseLeave), handledEventsToo: true);
			if (_dropDownButton.Content == null)
			{
				_dropDownButton.Content = SR.DatePicker_DropDownButtonName;
			}
		}
		_textBox = GetTemplateChild("PART_TextBox") as DatePickerTextBox;
		if (!SelectedDate.HasValue)
		{
			SetWaterMarkText();
		}
		if (_textBox == null)
		{
			return;
		}
		_textBox.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(TextBox_KeyDown), handledEventsToo: true);
		_textBox.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(TextBox_TextChanged), handledEventsToo: true);
		_textBox.AddHandler(UIElement.LostFocusEvent, new RoutedEventHandler(TextBox_LostFocus), handledEventsToo: true);
		if (!SelectedDate.HasValue)
		{
			if (!string.IsNullOrEmpty(_defaultText))
			{
				_textBox.Text = _defaultText;
				SetSelectedDate();
			}
		}
		else
		{
			_textBox.Text = DateTimeToString(SelectedDate.Value);
		}
	}

	/// <summary>Provides a text representation of the selected date.</summary>
	/// <returns>A text representation of the selected date, or an empty string if <see cref="P:System.Windows.Controls.DatePicker.SelectedDate" /> is null.</returns>
	public override string ToString()
	{
		if (SelectedDate.HasValue)
		{
			return SelectedDate.Value.ToString(DateTimeHelper.GetDateFormat(DateTimeHelper.GetCulture(this)));
		}
		return string.Empty;
	}

	internal override void ChangeVisualState(bool useTransitions)
	{
		if (!base.IsEnabled)
		{
			VisualStates.GoToState(this, useTransitions, "Disabled", "Normal");
		}
		else
		{
			VisualStateManager.GoToState(this, "Normal", useTransitions);
		}
		base.ChangeVisualState(useTransitions);
	}

	/// <summary>Returns a <see cref="T:System.Windows.Automation.Peers.DatePickerAutomationPeer" /> for use by the automation infrastructure.</summary>
	/// <returns>A <see cref="T:System.Windows.Automation.Peers.DatePickerAutomationPeer" /> for the <see cref="T:System.Windows.Controls.DatePicker" /> object.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new DatePickerAutomationPeer(this);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DatePicker.CalendarClosed" /> routed event. </summary>
	/// <param name="e">The data for the event. </param>
	protected virtual void OnCalendarClosed(RoutedEventArgs e)
	{
		this.CalendarClosed?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DatePicker.CalendarOpened" /> routed event. </summary>
	/// <param name="e">The data for the event. </param>
	protected virtual void OnCalendarOpened(RoutedEventArgs e)
	{
		this.CalendarOpened?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DatePicker.SelectedDateChanged" /> routed event. </summary>
	/// <param name="e">The data for the event. </param>
	protected virtual void OnSelectedDateChanged(SelectionChangedEventArgs e)
	{
		RaiseEvent(e);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DatePicker.DateValidationError" /> event.</summary>
	/// <param name="e">A <see cref="T:System.Windows.Controls.DatePickerDateValidationErrorEventArgs" /> that contains the event data.</param>
	protected virtual void OnDateValidationError(DatePickerDateValidationErrorEventArgs e)
	{
		this.DateValidationError?.Invoke(this, e);
	}

	private static void OnGotFocus(object sender, RoutedEventArgs e)
	{
		DatePicker datePicker = (DatePicker)sender;
		if (!e.Handled && datePicker._textBox != null)
		{
			if (e.OriginalSource == datePicker)
			{
				datePicker._textBox.Focus();
				e.Handled = true;
			}
			else if (e.OriginalSource == datePicker._textBox)
			{
				datePicker._textBox.SelectAll();
				e.Handled = true;
			}
		}
	}

	private void SetValueNoCallback(DependencyProperty property, object value)
	{
		SetIsHandlerSuspended(property, value: true);
		try
		{
			SetCurrentValue(property, value);
		}
		finally
		{
			SetIsHandlerSuspended(property, value: false);
		}
	}

	private bool IsHandlerSuspended(DependencyProperty property)
	{
		if (_isHandlerSuspended != null)
		{
			return _isHandlerSuspended.ContainsKey(property);
		}
		return false;
	}

	private void SetIsHandlerSuspended(DependencyProperty property, bool value)
	{
		if (value)
		{
			if (_isHandlerSuspended == null)
			{
				_isHandlerSuspended = new Dictionary<DependencyProperty, bool>(2);
			}
			_isHandlerSuspended[property] = true;
		}
		else if (_isHandlerSuspended != null)
		{
			_isHandlerSuspended.Remove(property);
		}
	}

	private void PopUp_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (sender is Popup { StaysOpen: false } && _dropDownButton != null && _dropDownButton.InputHitTest(e.GetPosition(_dropDownButton)) != null)
		{
			_disablePopupReopen = true;
		}
	}

	private void PopUp_Opened(object sender, EventArgs e)
	{
		if (!IsDropDownOpen)
		{
			SetCurrentValueInternal(IsDropDownOpenProperty, BooleanBoxes.TrueBox);
		}
		if (_calendar != null)
		{
			_calendar.DisplayMode = CalendarMode.Month;
			_calendar.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
		}
		OnCalendarOpened(new RoutedEventArgs());
	}

	private void PopUp_Closed(object sender, EventArgs e)
	{
		if (IsDropDownOpen)
		{
			SetCurrentValueInternal(IsDropDownOpenProperty, BooleanBoxes.FalseBox);
		}
		if (_calendar.IsKeyboardFocusWithin)
		{
			MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
		}
		OnCalendarClosed(new RoutedEventArgs());
	}

	private void Calendar_DayButtonMouseUp(object sender, MouseButtonEventArgs e)
	{
		SetCurrentValueInternal(IsDropDownOpenProperty, BooleanBoxes.FalseBox);
	}

	private void CalendarDayOrMonthButton_PreviewKeyDown(object sender, RoutedEventArgs e)
	{
		Calendar calendar = sender as Calendar;
		KeyEventArgs keyEventArgs = (KeyEventArgs)e;
		if (keyEventArgs.Key == Key.Escape || ((keyEventArgs.Key == Key.Return || keyEventArgs.Key == Key.Space) && calendar.DisplayMode == CalendarMode.Month))
		{
			SetCurrentValueInternal(IsDropDownOpenProperty, BooleanBoxes.FalseBox);
			if (keyEventArgs.Key == Key.Escape)
			{
				SetCurrentValueInternal(SelectedDateProperty, _originalSelectedDate);
			}
		}
	}

	private void Calendar_DisplayDateChanged(object sender, CalendarDateChangedEventArgs e)
	{
		if (e.AddedDate != DisplayDate)
		{
			SetCurrentValueInternal(DisplayDateProperty, e.AddedDate.Value);
		}
	}

	private void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
	{
		if (e.AddedItems.Count > 0 && SelectedDate.HasValue && DateTime.Compare((DateTime)e.AddedItems[0], SelectedDate.Value) != 0)
		{
			SetCurrentValueInternal(SelectedDateProperty, (DateTime?)e.AddedItems[0]);
		}
		else if (e.AddedItems.Count == 0)
		{
			SetCurrentValueInternal(SelectedDateProperty, null);
		}
		else if (!SelectedDate.HasValue && e.AddedItems.Count > 0)
		{
			SetCurrentValueInternal(SelectedDateProperty, (DateTime?)e.AddedItems[0]);
		}
	}

	private string DateTimeToString(DateTime d)
	{
		DateTimeFormatInfo dateFormat = DateTimeHelper.GetDateFormat(DateTimeHelper.GetCulture(this));
		return SelectedDateFormat switch
		{
			DatePickerFormat.Short => string.Format(CultureInfo.CurrentCulture, d.ToString(dateFormat.ShortDatePattern, dateFormat)), 
			DatePickerFormat.Long => string.Format(CultureInfo.CurrentCulture, d.ToString(dateFormat.LongDatePattern, dateFormat)), 
			_ => null, 
		};
	}

	private static DateTime DiscardDayTime(DateTime d)
	{
		int year = d.Year;
		int month = d.Month;
		return new DateTime(year, month, 1, 0, 0, 0);
	}

	private static DateTime? DiscardTime(DateTime? d)
	{
		if (!d.HasValue)
		{
			return null;
		}
		DateTime value = d.Value;
		int year = value.Year;
		int month = value.Month;
		int day = value.Day;
		return new DateTime(year, month, day, 0, 0, 0);
	}

	private void DropDownButton_Click(object sender, RoutedEventArgs e)
	{
		TogglePopUp();
	}

	private void DropDownButton_MouseLeave(object sender, MouseEventArgs e)
	{
		_disablePopupReopen = false;
	}

	private void TogglePopUp()
	{
		if (IsDropDownOpen)
		{
			SetCurrentValueInternal(IsDropDownOpenProperty, BooleanBoxes.FalseBox);
			return;
		}
		if (_disablePopupReopen)
		{
			_disablePopupReopen = false;
			return;
		}
		SetSelectedDate();
		SetCurrentValueInternal(IsDropDownOpenProperty, BooleanBoxes.TrueBox);
	}

	private void InitializeCalendar()
	{
		_calendar = new Calendar();
		_calendar.DayButtonMouseUp += Calendar_DayButtonMouseUp;
		_calendar.DisplayDateChanged += Calendar_DisplayDateChanged;
		_calendar.SelectedDatesChanged += Calendar_SelectedDatesChanged;
		_calendar.DayOrMonthPreviewKeyDown += CalendarDayOrMonthButton_PreviewKeyDown;
		_calendar.HorizontalAlignment = HorizontalAlignment.Left;
		_calendar.VerticalAlignment = VerticalAlignment.Top;
		_calendar.SelectionMode = CalendarSelectionMode.SingleDate;
		_calendar.SetBinding(Control.ForegroundProperty, GetDatePickerBinding(Control.ForegroundProperty));
		_calendar.SetBinding(FrameworkElement.StyleProperty, GetDatePickerBinding(CalendarStyleProperty));
		_calendar.SetBinding(Calendar.IsTodayHighlightedProperty, GetDatePickerBinding(IsTodayHighlightedProperty));
		_calendar.SetBinding(Calendar.FirstDayOfWeekProperty, GetDatePickerBinding(FirstDayOfWeekProperty));
		_calendar.SetBinding(FrameworkElement.FlowDirectionProperty, GetDatePickerBinding(FrameworkElement.FlowDirectionProperty));
		RenderOptions.SetClearTypeHint(_calendar, ClearTypeHint.Enabled);
	}

	private BindingBase GetDatePickerBinding(DependencyProperty property)
	{
		return new Binding(property.Name)
		{
			Source = this
		};
	}

	private static bool IsValidSelectedDateFormat(object value)
	{
		DatePickerFormat datePickerFormat = (DatePickerFormat)value;
		if (datePickerFormat != 0)
		{
			return datePickerFormat == DatePickerFormat.Short;
		}
		return true;
	}

	private DateTime? ParseText(string text)
	{
		try
		{
			DateTime dateTime = DateTime.Parse(text, DateTimeHelper.GetDateFormat(DateTimeHelper.GetCulture(this)));
			if (Calendar.IsValidDateSelection(_calendar, dateTime))
			{
				return dateTime;
			}
			DatePickerDateValidationErrorEventArgs datePickerDateValidationErrorEventArgs = new DatePickerDateValidationErrorEventArgs(new ArgumentOutOfRangeException("text", SR.Calendar_OnSelectedDateChanged_InvalidValue), text);
			OnDateValidationError(datePickerDateValidationErrorEventArgs);
			if (datePickerDateValidationErrorEventArgs.ThrowException)
			{
				throw datePickerDateValidationErrorEventArgs.Exception;
			}
		}
		catch (FormatException exception)
		{
			DatePickerDateValidationErrorEventArgs datePickerDateValidationErrorEventArgs2 = new DatePickerDateValidationErrorEventArgs(exception, text);
			OnDateValidationError(datePickerDateValidationErrorEventArgs2);
			if (datePickerDateValidationErrorEventArgs2.ThrowException && datePickerDateValidationErrorEventArgs2.Exception != null)
			{
				throw datePickerDateValidationErrorEventArgs2.Exception;
			}
		}
		return null;
	}

	private bool ProcessDatePickerKey(KeyEventArgs e)
	{
		switch (e.Key)
		{
		case Key.System:
			if (e.SystemKey == Key.Down && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
			{
				TogglePopUp();
				return true;
			}
			break;
		case Key.Return:
			SetSelectedDate();
			return true;
		}
		return false;
	}

	private void SetSelectedDate()
	{
		if (_textBox != null)
		{
			if (!string.IsNullOrEmpty(_textBox.Text))
			{
				string text = _textBox.Text;
				if (!SelectedDate.HasValue || string.Compare(DateTimeToString(SelectedDate.Value), text, StringComparison.Ordinal) != 0)
				{
					DateTime? dateTime = SetTextBoxValue(text);
					if (!SelectedDate.Equals(dateTime))
					{
						SetCurrentValueInternal(SelectedDateProperty, dateTime);
						SetCurrentValueInternal(DisplayDateProperty, dateTime);
					}
				}
			}
			else if (SelectedDate.HasValue)
			{
				SetCurrentValueInternal(SelectedDateProperty, null);
			}
		}
		else
		{
			DateTime? dateTime2 = SetTextBoxValue(_defaultText);
			if (!SelectedDate.Equals(dateTime2))
			{
				SetCurrentValueInternal(SelectedDateProperty, dateTime2);
			}
		}
	}

	private void SafeSetText(string s)
	{
		if (string.Compare(Text, s, StringComparison.Ordinal) != 0)
		{
			SetCurrentValueInternal(TextProperty, s);
		}
	}

	private DateTime? SetTextBoxValue(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			SafeSetText(s);
			return SelectedDate;
		}
		DateTime? result = ParseText(s);
		if (result.HasValue)
		{
			SafeSetText(DateTimeToString(result.Value));
			return result;
		}
		if (SelectedDate.HasValue)
		{
			string s2 = DateTimeToString(SelectedDate.Value);
			SafeSetText(s2);
			return SelectedDate;
		}
		SetWaterMarkText();
		return null;
	}

	private void SetWaterMarkText()
	{
		if (_textBox != null)
		{
			DateTimeFormatInfo dateFormat = DateTimeHelper.GetDateFormat(DateTimeHelper.GetCulture(this));
			SetTextInternal(string.Empty);
			_defaultText = string.Empty;
			switch (SelectedDateFormat)
			{
			case DatePickerFormat.Long:
				_textBox.Watermark = string.Format(CultureInfo.CurrentCulture, SR.DatePicker_WatermarkText, dateFormat.LongDatePattern.ToString());
				break;
			case DatePickerFormat.Short:
				_textBox.Watermark = string.Format(CultureInfo.CurrentCulture, SR.DatePicker_WatermarkText, dateFormat.ShortDatePattern.ToString());
				break;
			}
		}
	}

	private void TextBox_LostFocus(object sender, RoutedEventArgs e)
	{
		SetSelectedDate();
	}

	private void TextBox_KeyDown(object sender, KeyEventArgs e)
	{
		e.Handled = ProcessDatePickerKey(e) || e.Handled;
	}

	private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
	{
		SetValueNoCallback(TextProperty, _textBox.Text);
	}
}
