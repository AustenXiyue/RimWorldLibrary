using System.Runtime.CompilerServices;
using System.Windows.Automation.Provider;
using System.Windows.Controls.Primitives;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.Primitives.ToggleButton" /> types to UI Automation.</summary>
public class ToggleButtonAutomationPeer : ButtonBaseAutomationPeer, IToggleProvider
{
	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>the toggle state of the control.</returns>
	ToggleState IToggleProvider.ToggleState => ConvertToToggleState(((ToggleButton)base.Owner).IsChecked);

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.ToggleButtonAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.Primitives.ToggleButton" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ToggleButtonAutomationPeer" />.</param>
	public ToggleButtonAutomationPeer(ToggleButton owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Controls.Primitives.ToggleButton" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ToggleButtonAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "Button".</returns>
	protected override string GetClassNameCore()
	{
		return "Button";
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.Primitives.ToggleButton" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ToggleButtonAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The control type for the <see cref="T:System.Windows.Controls.Primitives.ToggleButton" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ToggleButtonAutomationPeer" />.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Button;
	}

	/// <summary>Gets the control pattern for the <see cref="T:System.Windows.Controls.Primitives.ToggleButton" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ToggleButtonAutomationPeer" />.</summary>
	/// <returns>If <paramref name="patternInterface" /> is <see cref="F:System.Windows.Automation.Peers.PatternInterface.Toggle" />, this method returns the current instance of the <see cref="T:System.Windows.Automation.Peers.ToggleButtonAutomationPeer" />, otherwise null.</returns>
	/// <param name="patternInterface">One of the enumeration values.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		if (patternInterface == PatternInterface.Toggle)
		{
			return this;
		}
		return base.GetPattern(patternInterface);
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void IToggleProvider.Toggle()
	{
		if (!IsEnabled())
		{
			throw new ElementNotEnabledException();
		}
		((ToggleButton)base.Owner).OnToggle();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal virtual void RaiseToggleStatePropertyChangedEvent(bool? oldValue, bool? newValue)
	{
		if (oldValue != newValue)
		{
			RaisePropertyChangedEvent(TogglePatternIdentifiers.ToggleStateProperty, ConvertToToggleState(oldValue), ConvertToToggleState(newValue));
		}
	}

	private static ToggleState ConvertToToggleState(bool? value)
	{
		if (value.HasValue)
		{
			if (value == true)
			{
				return ToggleState.On;
			}
			return ToggleState.Off;
		}
		return ToggleState.Indeterminate;
	}
}
