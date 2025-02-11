using System.Windows.Automation.Peers;

namespace System.Windows.Controls.Primitives;

/// <summary>Represents a day on a <see cref="T:System.Windows.Controls.Calendar" />.</summary>
public sealed class CalendarDayButton : Button
{
	private const int DEFAULTCONTENT = 1;

	internal static readonly DependencyPropertyKey IsTodayPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.CalendarDayButton.IsToday" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.CalendarDayButton.IsToday" /> dependency property.</returns>
	public static readonly DependencyProperty IsTodayProperty;

	internal static readonly DependencyPropertyKey IsSelectedPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.CalendarDayButton.IsSelected" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.CalendarDayButton.IsSelected" /> dependency property.</returns>
	public static readonly DependencyProperty IsSelectedProperty;

	internal static readonly DependencyPropertyKey IsInactivePropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.CalendarDayButton.IsInactive" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.CalendarDayButton.IsInactive" /> dependency property.</returns>
	public static readonly DependencyProperty IsInactiveProperty;

	internal static readonly DependencyPropertyKey IsBlackedOutPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.CalendarDayButton.IsBlackedOut" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.CalendarDayButton.IsBlackedOut" /> dependency property.</returns>
	public static readonly DependencyProperty IsBlackedOutProperty;

	internal static readonly DependencyPropertyKey IsHighlightedPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.CalendarDayButton.IsHighlighted" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.CalendarDayButton.IsHighlighted" /> dependency property.</returns>
	public static readonly DependencyProperty IsHighlightedProperty;

	/// <summary>Gets a value that indicates whether the date represented by this button is the current date.</summary>
	/// <returns>true if the date is the current date; otherwise, false.</returns>
	public bool IsToday => (bool)GetValue(IsTodayProperty);

	/// <summary>Gets a value that indicates whether the date represented by this button is selected.</summary>
	/// <returns>true if the date is selected; otherwise, false.</returns>
	public bool IsSelected => (bool)GetValue(IsSelectedProperty);

	/// <summary>Gets a value that indicates whether this button represents a day that is not in the currently displayed month.</summary>
	/// <returns>true if the button represents a day that is not in the currently displayed month; otherwise, false.</returns>
	public bool IsInactive => (bool)GetValue(IsInactiveProperty);

	/// <summary>Gets a value that indicates whether the date is unavailable.</summary>
	/// <returns>true if the date unavailable; otherwise, false.</returns>
	public bool IsBlackedOut => (bool)GetValue(IsBlackedOutProperty);

	/// <summary>Gets a value that indicates whether this button is highlighted.</summary>
	/// <returns>true if the button is highlighted; otherwise, false.</returns>
	public bool IsHighlighted => (bool)GetValue(IsHighlightedProperty);

	internal Calendar Owner { get; set; }

	static CalendarDayButton()
	{
		IsTodayPropertyKey = DependencyProperty.RegisterReadOnly("IsToday", typeof(bool), typeof(CalendarDayButton), new FrameworkPropertyMetadata(false, Control.OnVisualStatePropertyChanged));
		IsTodayProperty = IsTodayPropertyKey.DependencyProperty;
		IsSelectedPropertyKey = DependencyProperty.RegisterReadOnly("IsSelected", typeof(bool), typeof(CalendarDayButton), new FrameworkPropertyMetadata(false, Control.OnVisualStatePropertyChanged));
		IsSelectedProperty = IsSelectedPropertyKey.DependencyProperty;
		IsInactivePropertyKey = DependencyProperty.RegisterReadOnly("IsInactive", typeof(bool), typeof(CalendarDayButton), new FrameworkPropertyMetadata(false, Control.OnVisualStatePropertyChanged));
		IsInactiveProperty = IsInactivePropertyKey.DependencyProperty;
		IsBlackedOutPropertyKey = DependencyProperty.RegisterReadOnly("IsBlackedOut", typeof(bool), typeof(CalendarDayButton), new FrameworkPropertyMetadata(false, Control.OnVisualStatePropertyChanged));
		IsBlackedOutProperty = IsBlackedOutPropertyKey.DependencyProperty;
		IsHighlightedPropertyKey = DependencyProperty.RegisterReadOnly("IsHighlighted", typeof(bool), typeof(CalendarDayButton), new FrameworkPropertyMetadata(false, Control.OnVisualStatePropertyChanged));
		IsHighlightedProperty = IsHighlightedPropertyKey.DependencyProperty;
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(CalendarDayButton), new FrameworkPropertyMetadata(typeof(CalendarDayButton)));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.CalendarDayButton" /> class. </summary>
	public CalendarDayButton()
	{
	}

	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new CalendarButtonAutomationPeer(this);
	}

	internal override void ChangeVisualState(bool useTransitions)
	{
		VisualStates.GoToState(this, useTransitions, "Active", "Inactive");
		if (IsInactive)
		{
			VisualStateManager.GoToState(this, "Inactive", useTransitions);
		}
		VisualStateManager.GoToState(this, "RegularDay", useTransitions);
		if (IsToday && Owner != null && Owner.IsTodayHighlighted)
		{
			VisualStates.GoToState(this, useTransitions, "Today", "RegularDay");
		}
		VisualStateManager.GoToState(this, "Unselected", useTransitions);
		if (IsSelected || IsHighlighted)
		{
			VisualStates.GoToState(this, useTransitions, "Selected", "Unselected");
		}
		if (IsBlackedOut)
		{
			VisualStates.GoToState(this, useTransitions, "BlackoutDay", "NormalDay");
		}
		else
		{
			VisualStateManager.GoToState(this, "NormalDay", useTransitions);
		}
		if (base.IsKeyboardFocused)
		{
			VisualStates.GoToState(this, useTransitions, "CalendarButtonFocused", "CalendarButtonUnfocused");
		}
		else
		{
			VisualStateManager.GoToState(this, "CalendarButtonUnfocused", useTransitions);
		}
		base.ChangeVisualState(useTransitions);
	}

	internal void NotifyNeedsVisualStateUpdate()
	{
		UpdateVisualState();
	}

	internal void SetContentInternal(string value)
	{
		SetCurrentValueInternal(ContentControl.ContentProperty, value);
	}
}
