using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.Expander" /> types to UI Automation.</summary>
public class ExpanderAutomationPeer : FrameworkElementAutomationPeer, IExpandCollapseProvider
{
	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>The state (expanded or collapsed) of the control.</returns>
	ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState
	{
		get
		{
			if (!((Expander)Owner).IsExpanded)
			{
				return ExpandCollapseState.Collapsed;
			}
			return ExpandCollapseState.Expanded;
		}
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Automation.Peers.ExpanderAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.Expander" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ExpanderAutomationPeer" />.</param>
	public ExpanderAutomationPeer(Expander owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.Expander" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ExpanderAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "Expander".</returns>
	protected override string GetClassNameCore()
	{
		return "Expander";
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.Expander" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ExpanderAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>
	///   <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Group" />.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Group;
	}

	protected override List<AutomationPeer> GetChildrenCore()
	{
		List<AutomationPeer> childrenCore = base.GetChildrenCore();
		ToggleButton expanderToggleButton = ((Expander)base.Owner).ExpanderToggleButton;
		if (!AccessibilitySwitches.UseNetFx47CompatibleAccessibilityFeatures && childrenCore != null)
		{
			foreach (UIElementAutomationPeer item in childrenCore)
			{
				if (item.Owner == expanderToggleButton)
				{
					item.EventsSource = ((!AccessibilitySwitches.UseNetFx472CompatibleAccessibilityFeatures && base.EventsSource != null) ? base.EventsSource : this);
					break;
				}
			}
		}
		return childrenCore;
	}

	protected override bool HasKeyboardFocusCore()
	{
		if (AccessibilitySwitches.UseNetFx47CompatibleAccessibilityFeatures || !((Expander)base.Owner).IsExpanderToggleButtonFocused)
		{
			return base.HasKeyboardFocusCore();
		}
		return true;
	}

	/// <summary>Gets the control pattern for the <see cref="T:System.Windows.Controls.Expander" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ExpanderAutomationPeer" />. </summary>
	/// <returns>If <paramref name="pattern" /> is <see cref="F:System.Windows.Automation.Peers.PatternInterface.ExpandCollapse" />, this method returns a reference to the current instance of the <see cref="T:System.Windows.Automation.Peers.ExpanderAutomationPeer" />, otherwise this method calls the base implementation on <see cref="T:System.Windows.Automation.Peers.UIElementAutomationPeer" /> which returns null.</returns>
	/// <param name="pattern">One of the enumeration values.</param>
	public override object GetPattern(PatternInterface pattern)
	{
		object obj = null;
		if (pattern == PatternInterface.ExpandCollapse)
		{
			return this;
		}
		return base.GetPattern(pattern);
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void IExpandCollapseProvider.Expand()
	{
		if (!IsEnabled())
		{
			throw new ElementNotEnabledException();
		}
		((Expander)Owner).IsExpanded = true;
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void IExpandCollapseProvider.Collapse()
	{
		if (!IsEnabled())
		{
			throw new ElementNotEnabledException();
		}
		((Expander)Owner).IsExpanded = false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal void RaiseExpandCollapseAutomationEvent(bool oldValue, bool newValue)
	{
		RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty, oldValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed, newValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed);
	}
}
