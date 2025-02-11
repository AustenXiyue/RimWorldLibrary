using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.DatePicker" /> types to UI Automation.</summary>
public sealed class DatePickerAutomationPeer : FrameworkElementAutomationPeer, IExpandCollapseProvider, IValueProvider
{
	private DatePicker OwningDatePicker => base.Owner as DatePicker;

	/// <summary>Gets the state (expanded or collapsed) of the control.</summary>
	/// <returns>The state (expanded or collapsed) of the control.</returns>
	ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState
	{
		get
		{
			if (OwningDatePicker.IsDropDownOpen)
			{
				return ExpandCollapseState.Expanded;
			}
			return ExpandCollapseState.Collapsed;
		}
	}

	/// <summary>Gets a value that specifies whether the value of a control is read-only. </summary>
	/// <returns>true if the value is read-only; false if it can be modified. </returns>
	bool IValueProvider.IsReadOnly => false;

	/// <summary>Gets the value of the control.</summary>
	/// <returns>A string that represents the value of the control. </returns>
	string IValueProvider.Value => OwningDatePicker.ToString();

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.DatePickerAutomationPeer" /> class. </summary>
	/// <param name="owner">The element associated with this automation peer.</param>
	public DatePickerAutomationPeer(DatePicker owner)
		: base(owner)
	{
	}

	/// <summary>Returns the object that supports the specified control pattern of the element that is associated with this automation peer.</summary>
	/// <returns>If <paramref name="patternInterface" /> is <see cref="F:System.Windows.Automation.Peers.PatternInterface.ExpandCollapse" /> or <see cref="F:System.Windows.Automation.Peers.PatternInterface.Value" />, this method returns a this pointer; otherwise, this method returns null.</returns>
	/// <param name="patternInterface">An enumeration value that specifies the control pattern.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		if (patternInterface == PatternInterface.ExpandCollapse || patternInterface == PatternInterface.Value)
		{
			return this;
		}
		return base.GetPattern(patternInterface);
	}

	protected override void SetFocusCore()
	{
		DatePicker owningDatePicker = OwningDatePicker;
		if (owningDatePicker.Focusable)
		{
			if (!owningDatePicker.Focus())
			{
				TextBox textBox = owningDatePicker.TextBox;
				if (textBox == null || !textBox.IsKeyboardFocused)
				{
					throw new InvalidOperationException(SR.SetFocusFailed);
				}
			}
			return;
		}
		throw new InvalidOperationException(SR.SetFocusFailed);
	}

	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Custom;
	}

	protected override List<AutomationPeer> GetChildrenCore()
	{
		List<AutomationPeer> childrenCore = base.GetChildrenCore();
		if (OwningDatePicker.IsDropDownOpen && OwningDatePicker.Calendar != null && UIElementAutomationPeer.CreatePeerForElement(OwningDatePicker.Calendar) is CalendarAutomationPeer item)
		{
			childrenCore.Add(item);
		}
		return childrenCore;
	}

	protected override string GetClassNameCore()
	{
		return base.Owner.GetType().Name;
	}

	protected override string GetLocalizedControlTypeCore()
	{
		return SR.DatePickerAutomationPeer_LocalizedControlType;
	}

	/// <summary>Hides all nodes, controls, or content that are descendants of the control.</summary>
	void IExpandCollapseProvider.Collapse()
	{
		OwningDatePicker.IsDropDownOpen = false;
	}

	/// <summary>Displays all child nodes, controls, or content of the control.</summary>
	void IExpandCollapseProvider.Expand()
	{
		OwningDatePicker.IsDropDownOpen = true;
	}

	/// <summary>Sets the value of a control.</summary>
	/// <param name="value">The value to set. The provider is responsible for converting the value to the appropriate data type.</param>
	void IValueProvider.SetValue(string value)
	{
		OwningDatePicker.Text = value;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal void RaiseValuePropertyChangedEvent(string oldValue, string newValue)
	{
		if (oldValue != newValue)
		{
			RaisePropertyChangedEvent(ValuePatternIdentifiers.ValueProperty, oldValue, newValue);
		}
	}
}
