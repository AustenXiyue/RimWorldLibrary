using System.Runtime.CompilerServices;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using MS.Internal.KnownBoxes;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.RadioButton" /> types to UI Automation.</summary>
public class RadioButtonAutomationPeer : ToggleButtonAutomationPeer, ISelectionItemProvider
{
	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if the element is selected; otherwise false.</returns>
	bool ISelectionItemProvider.IsSelected => ((RadioButton)base.Owner).IsChecked == true;

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>The selection container.</returns>
	IRawElementProviderSimple ISelectionItemProvider.SelectionContainer => null;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.RadioButtonAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Controls.RadioButton" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.RadioButtonAutomationPeer" />.</param>
	public RadioButtonAutomationPeer(RadioButton owner)
		: base(owner)
	{
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.ContentElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.ContentElementAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "RadioButton".</returns>
	protected override string GetClassNameCore()
	{
		return "RadioButton";
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Controls.RadioButton" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.RadioButtonAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />..</summary>
	/// <returns>The control type for the <see cref="T:System.Windows.Controls.RadioButton" /> object.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.RadioButton;
	}

	/// <summary>Gets the control pattern for the <see cref="T:System.Windows.Controls.RadioButton" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.RadioButtonAutomationPeer" />.</summary>
	/// <returns>An object that supports the control pattern if <paramref name="patternInterface" /> is a supported value; otherwise, null.</returns>
	/// <param name="patternInterface">A value in the enumeration.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		return patternInterface switch
		{
			PatternInterface.SelectionItem => this, 
			PatternInterface.SynchronizedInput => base.GetPattern(patternInterface), 
			_ => null, 
		};
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void ISelectionItemProvider.Select()
	{
		if (!IsEnabled())
		{
			throw new ElementNotEnabledException();
		}
		((RadioButton)base.Owner).SetCurrentValueInternal(ToggleButton.IsCheckedProperty, BooleanBoxes.TrueBox);
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void ISelectionItemProvider.AddToSelection()
	{
		if (((RadioButton)base.Owner).IsChecked != true)
		{
			throw new InvalidOperationException(SR.UIA_OperationCannotBePerformed);
		}
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void ISelectionItemProvider.RemoveFromSelection()
	{
		if (((RadioButton)base.Owner).IsChecked == true)
		{
			throw new InvalidOperationException(SR.UIA_OperationCannotBePerformed);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal override void RaiseToggleStatePropertyChangedEvent(bool? oldValue, bool? newValue)
	{
		RaisePropertyChangedEvent(SelectionItemPatternIdentifiers.IsSelectedProperty, oldValue == true, newValue == true);
	}
}
