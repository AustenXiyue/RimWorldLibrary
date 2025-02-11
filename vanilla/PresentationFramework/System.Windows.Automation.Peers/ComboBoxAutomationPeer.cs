using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using MS.Internal.KnownBoxes;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.ComboBox" /> types to UI Automation.</summary>
public class ComboBoxAutomationPeer : SelectorAutomationPeer, IValueProvider, IExpandCollapseProvider
{
	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>A string value of the control.</returns>
	string IValueProvider.Value => ((ComboBox)Owner).Text;

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if the value is read-only; false if it can be modified.</returns>
	bool IValueProvider.IsReadOnly
	{
		get
		{
			ComboBox comboBox = (ComboBox)base.Owner;
			if (comboBox.IsEnabled)
			{
				return comboBox.IsReadOnly;
			}
			return true;
		}
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>The <see cref="T:System.Windows.Automation.ExpandCollapseState" /> for the current element.</returns>
	ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState
	{
		get
		{
			if (!((ComboBox)Owner).IsDropDownOpen)
			{
				return ExpandCollapseState.Collapsed;
			}
			return ExpandCollapseState.Expanded;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.ComboBoxAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.ComboBox" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ComboBoxAutomationPeer" />.</param>
	public ComboBoxAutomationPeer(ComboBox owner)
		: base(owner)
	{
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Automation.Peers.ItemAutomationPeer" /> class.</summary>
	/// <returns>A new instance of the <see cref="T:System.Windows.Automation.Peers.ItemAutomationPeer" /> class.</returns>
	/// <param name="item">The <see cref="T:System.Windows.Controls.ComboBoxItem" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ComboBoxAutomationPeer" />.</param>
	protected override ItemAutomationPeer CreateItemAutomationPeer(object item)
	{
		return new ListBoxItemAutomationPeer(item, this);
	}

	/// <summary>Gets the control type for this <see cref="T:System.Windows.Automation.Peers.ComboBoxAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.ComboBox" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.ComboBox;
	}

	/// <summary>Gets the name of the class that defines the type that is associated with this <see cref="T:System.Windows.Automation.Peers.ComboBoxAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "ComboBox".</returns>
	protected override string GetClassNameCore()
	{
		return "ComboBox";
	}

	/// <summary>Gets the control pattern for this <see cref="T:System.Windows.Automation.Peers.ComboBoxAutomationPeer" />.</summary>
	/// <returns>If <paramref name="pattern" /> is <see cref="F:System.Windows.Automation.Peers.PatternInterface.Value" /> or <see cref="F:System.Windows.Automation.Peers.PatternInterface.ExpandCollapse" />, this method returns a reference to the current instance of the <see cref="T:System.Windows.Automation.Peers.ComboBoxAutomationPeer" />; otherwise, this method calls the base implementation on <see cref="T:System.Windows.Automation.Peers.SelectorAutomationPeer" />. </returns>
	/// <param name="pattern">One of the enumeration values.</param>
	public override object GetPattern(PatternInterface pattern)
	{
		object result = null;
		ComboBox comboBox = (ComboBox)base.Owner;
		if (pattern != PatternInterface.Value)
		{
			result = ((pattern == PatternInterface.ExpandCollapse) ? this : ((pattern != PatternInterface.Scroll || comboBox.IsDropDownOpen) ? base.GetPattern(pattern) : this));
		}
		else if (comboBox.IsEditable)
		{
			result = this;
		}
		return result;
	}

	/// <summary>Gets a collection of child elements. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetChildren" />.</summary>
	/// <returns>A collection of child elements.</returns>
	protected override List<AutomationPeer> GetChildrenCore()
	{
		List<AutomationPeer> list = base.GetChildrenCore();
		TextBox editableTextBoxSite = ((ComboBox)base.Owner).EditableTextBoxSite;
		if (editableTextBoxSite != null)
		{
			AutomationPeer automationPeer = UIElementAutomationPeer.CreatePeerForElement(editableTextBoxSite);
			if (automationPeer != null)
			{
				if (list == null)
				{
					list = new List<AutomationPeer>();
				}
				list.Insert(0, automationPeer);
			}
		}
		return list;
	}

	/// <summary>Sets the keyboard input focus on the <see cref="T:System.Windows.Controls.ComboBox" /> control that is associated with this <see cref="T:System.Windows.Automation.Peers.ComboBoxAutomationPeer" /> object. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.SetFocus" />.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Controls.ComboBox" /> control that is associated with this <see cref="T:System.Windows.Automation.Peers.ComboBoxAutomationPeer" /> object cannot receive focus.</exception>
	protected override void SetFocusCore()
	{
		ComboBox comboBox = (ComboBox)base.Owner;
		if (comboBox.Focusable)
		{
			if (!comboBox.Focus())
			{
				if (!comboBox.IsEditable)
				{
					throw new InvalidOperationException(SR.SetFocusFailed);
				}
				TextBox editableTextBoxSite = comboBox.EditableTextBoxSite;
				if (editableTextBoxSite == null || !editableTextBoxSite.IsKeyboardFocused)
				{
					throw new InvalidOperationException(SR.SetFocusFailed);
				}
			}
			return;
		}
		throw new InvalidOperationException(SR.SetFocusFailed);
	}

	internal void ScrollItemIntoView(object item)
	{
		if (((IExpandCollapseProvider)this).ExpandCollapseState == ExpandCollapseState.Expanded)
		{
			ComboBox comboBox = (ComboBox)base.Owner;
			if (comboBox.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
			{
				comboBox.OnBringItemIntoView(item);
			}
			else
			{
				base.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new DispatcherOperationCallback(comboBox.OnBringItemIntoView), item);
			}
		}
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="val"> The string value of a control.</param>
	void IValueProvider.SetValue(string val)
	{
		if (val == null)
		{
			throw new ArgumentNullException("val");
		}
		ComboBox obj = (ComboBox)base.Owner;
		if (!obj.IsEnabled)
		{
			throw new ElementNotEnabledException();
		}
		obj.SetCurrentValueInternal(ComboBox.TextProperty, val);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal void RaiseValuePropertyChangedEvent(string oldValue, string newValue)
	{
		if (oldValue != newValue)
		{
			RaisePropertyChangedEvent(ValuePatternIdentifiers.ValueProperty, oldValue, newValue);
		}
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void IExpandCollapseProvider.Expand()
	{
		if (!IsEnabled())
		{
			throw new ElementNotEnabledException();
		}
		((ComboBox)Owner).SetCurrentValueInternal(ComboBox.IsDropDownOpenProperty, BooleanBoxes.TrueBox);
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void IExpandCollapseProvider.Collapse()
	{
		if (!IsEnabled())
		{
			throw new ElementNotEnabledException();
		}
		((ComboBox)Owner).SetCurrentValueInternal(ComboBox.IsDropDownOpenProperty, BooleanBoxes.FalseBox);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal void RaiseExpandCollapseAutomationEvent(bool oldValue, bool newValue)
	{
		RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty, oldValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed, newValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed);
	}
}
