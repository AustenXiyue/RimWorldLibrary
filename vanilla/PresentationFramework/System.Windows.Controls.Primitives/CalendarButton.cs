using System.Windows.Automation.Peers;

namespace System.Windows.Controls.Primitives;

/// <summary>Represents a month or year on a <see cref="T:System.Windows.Controls.Calendar" /> object.</summary>
public sealed class CalendarButton : Button
{
	internal static readonly DependencyPropertyKey HasSelectedDaysPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.CalendarButton.HasSelectedDays" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.CalendarButton.HasSelectedDays" /> dependency property.</returns>
	public static readonly DependencyProperty HasSelectedDaysProperty;

	internal static readonly DependencyPropertyKey IsInactivePropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.CalendarButton.IsInactive" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.CalendarButton.IsInactive" /> dependency property.</returns>
	public static readonly DependencyProperty IsInactiveProperty;

	/// <summary>Gets a value that indicates whether this button represents a year or month that contains selected dates.</summary>
	/// <returns>true if this button represents a year or month that contains selected dates; otherwise, false.</returns>
	public bool HasSelectedDays
	{
		get
		{
			return (bool)GetValue(HasSelectedDaysProperty);
		}
		internal set
		{
			SetValue(HasSelectedDaysPropertyKey, value);
		}
	}

	/// <summary>Gets a value that indicates whether this button represents a year that is not in the currently displayed decade.</summary>
	/// <returns>true if this button represents a day that is not in the currently displayed month, or a year that is not in the currently displayed decade; otherwise, false.</returns>
	public bool IsInactive
	{
		get
		{
			return (bool)GetValue(IsInactiveProperty);
		}
		internal set
		{
			SetValue(IsInactivePropertyKey, value);
		}
	}

	internal Calendar Owner { get; set; }

	static CalendarButton()
	{
		HasSelectedDaysPropertyKey = DependencyProperty.RegisterReadOnly("HasSelectedDays", typeof(bool), typeof(CalendarButton), new FrameworkPropertyMetadata(false, Control.OnVisualStatePropertyChanged));
		HasSelectedDaysProperty = HasSelectedDaysPropertyKey.DependencyProperty;
		IsInactivePropertyKey = DependencyProperty.RegisterReadOnly("IsInactive", typeof(bool), typeof(CalendarButton), new FrameworkPropertyMetadata(false, Control.OnVisualStatePropertyChanged));
		IsInactiveProperty = IsInactivePropertyKey.DependencyProperty;
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(CalendarButton), new FrameworkPropertyMetadata(typeof(CalendarButton)));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.CalendarButton" /> class. </summary>
	public CalendarButton()
	{
	}

	internal override void ChangeVisualState(bool useTransitions)
	{
		if (HasSelectedDays)
		{
			VisualStates.GoToState(this, useTransitions, "Selected", "Unselected");
		}
		else
		{
			VisualStateManager.GoToState(this, "Unselected", useTransitions);
		}
		if (!IsInactive)
		{
			VisualStates.GoToState(this, useTransitions, "Active", "Inactive");
		}
		else
		{
			VisualStateManager.GoToState(this, "Inactive", useTransitions);
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

	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new CalendarButtonAutomationPeer(this);
	}

	internal void SetContentInternal(string value)
	{
		SetCurrentValueInternal(ContentControl.ContentProperty, value);
	}
}
